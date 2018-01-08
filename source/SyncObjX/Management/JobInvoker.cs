using System;
using System.Linq;
using SyncObjX.Configuration;
using SyncObjX.Exceptions;
using SyncObjX.Logging;

namespace SyncObjX.Management
{
    public static class JobInvoker
    {
        public static void ExecuteJob(JobInstance jobInstance, ISyncEngineConfigurator configurator)
        {
            jobInstance.ActualStartTime = DateTime.Now;

            if (jobInstance.MaxDelayedStartExceeded)
                jobInstance.Status = JobQueueStatus.InProgress_MaxDelayExceeded;
            else
                jobInstance.Status = JobQueueStatus.InProgress;

            bool terminateNextStepsOnStepFailure;
            bool isSafeJobStopViaException = false; // used to stop job execution without logging an error

            switch (jobInstance.Job.TerminateOnErrorType)
            {
                case JobTerminateOnErrorType.NeverTerminateOnError:
                case JobTerminateOnErrorType.TerminateExecutingStepOnly:
                    terminateNextStepsOnStepFailure = false;
                    break;

                case JobTerminateOnErrorType.TerminateExecutingDataSourceOnly:
                case JobTerminateOnErrorType.TerminateJob:
                    terminateNextStepsOnStepFailure = true;
                    break;

                default:
                    throw new EnumValueNotImplementedException<JobTerminateOnErrorType>(jobInstance.Job.TerminateOnErrorType);
            }

            bool previousJobStepHasRuntimeErrors = false;

            JobStepInstance previousJobStepInstance = null;

            foreach (var jobStepInstance in jobInstance.JobStepInstances)
            {
                // handle potential job instances that may have been partially executed prior to machine or service restart
                if (jobStepInstance.Status == JobStepQueueStatus.Completed)
                {
                    continue;
                }
                else if (jobStepInstance.Status == JobStepQueueStatus.Completed_WithError ||
                         jobStepInstance.Status == JobStepQueueStatus.DidNotRun_JobError)
                {
                    previousJobStepHasRuntimeErrors = true;
                    jobInstance.HasRuntimeErrors = true;
                    continue;
                }
                else if (isSafeJobStopViaException)
                {
                    continue;
                }

                if (previousJobStepHasRuntimeErrors && terminateNextStepsOnStepFailure)
                {
                    jobStepInstance.Status = JobStepQueueStatus.DidNotRun_JobError;
                }
                else
                {
                    jobStepInstance.ActualStartTime = DateTime.Now;

                    jobStepInstance.Status = JobStepQueueStatus.InProgress;

                    jobStepInstance.HasRuntimeErrors = false;

                    try
                    {
                        jobInstance.RunningJobStepInstance = jobStepInstance;

                        JobStepInvoker.ExecuteStep(jobInstance, previousJobStepInstance, jobStepInstance, configurator);

                        jobStepInstance.ActualEndTime = DateTime.Now;

                        if (jobStepInstance.HasRecordErrors)
                            jobInstance.HasRecordErrors = true;

                        if (jobStepInstance.HasRuntimeErrors)
                        {
                            jobStepInstance.Status = JobStepQueueStatus.Completed_WithError;

                            jobInstance.HasRuntimeErrors = true;

                            previousJobStepHasRuntimeErrors = true;
                        }
                        else
                            jobStepInstance.Status = JobStepQueueStatus.Completed;
                    }
                    catch (SafeJobStopException ex)
                    {
                        jobStepInstance.ActualEndTime = DateTime.Now;

                        isSafeJobStopViaException = true;

                        SyncEngineLogger.WriteByParallelTaskContext(LogEntryType.Info, () =>
                            {
                                return string.Format("Job step '{0}' of job '{1}' was terminated safely with message: {2}",
                                                     jobStepInstance.JobStep.Name, jobStepInstance.AssociatedJobInstance.Job.Name, ex.Message);
                            });

                        continue;
                    }
                    catch (Exception ex)
                    {
                        jobStepInstance.ActualEndTime = DateTime.Now;

                        jobStepInstance.HasRuntimeErrors = true;

                        jobStepInstance.Status = JobStepQueueStatus.Completed_WithError;

                        jobInstance.HasRuntimeErrors = true;

                        jobInstance.Exceptions.Add(ex);

                        previousJobStepHasRuntimeErrors = true;

                        SyncEngineLogger.WriteByParallelTaskContext(ex);

                        jobInstance.RunningJobStepInstance = null;

                        continue;
                    }
                }

                previousJobStepInstance = jobStepInstance;
            }

            jobInstance.ActualEndTime = jobInstance.JobStepInstances.Select(d => d.ActualEndTime).Max();

            jobInstance.RunningJobStepInstance = null;

            if (jobInstance.HasRuntimeErrors && terminateNextStepsOnStepFailure)
                jobInstance.Status = JobQueueStatus.Terminated_WithError;
            else if (jobInstance.HasRuntimeErrors)
                jobInstance.Status = JobQueueStatus.Completed_WithError;
            else
                jobInstance.Status = JobQueueStatus.Completed;
        }
    }
}

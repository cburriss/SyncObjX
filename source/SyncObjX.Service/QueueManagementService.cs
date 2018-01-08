using System;
using System.Collections.Generic;
using System.Linq;
using SyncObjX.Logging;
using SyncObjX.Management;
using SyncObjX.SyncObjects;

namespace SyncObjX.Service
{
    public class QueueManagementService : IJobQueueManagement
    {
        private const string WEB_SERVICE_EXCEPTION_MESSAGE = "An exception occurred within the queue management web service.";

        public void Start(double intervalInSeconds)
        {
            try
            {
                JobQueueManager.Start(intervalInSeconds);
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        public void Restart()
        {
            try
            {
                JobQueueManager.Restart();
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        public void Stop()
        {
            try
            {
                JobQueueManager.Stop();
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        public JobQueueManagerStatus GetStatus()
        {
            try
            {
                return JobQueueManager.Status;
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        public double GetIntervalInSeconds()
        {
            try
            {
                return JobQueueManager.IntervalInSeconds;
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        public List<KeyValuePair<JobPriority, TimeSpan>> GetMaxDelayedStartByJobPriority()
        {
            try
            {
                return JobQueueManager.MaxDelayedStartByJobPriority.ToList();
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        public TimeSpan GetMaxDelayedStartForJobPriority(JobPriority jobPriority)
        {
            try
            {
                return JobQueueManager.GetMaxDelayedStartForJobPriority(jobPriority);
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        public ConcurrentThreadState GetThreadUsage(Guid integrationId)
        {
            try
            {
                return JobQueueManager.GetThreadUsage(integrationId);
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        public List<KeyValuePair<Guid, ConcurrentThreadState>> GetThreadUsagesByIntegrationId()
        {
            try
            {
                return JobQueueManager.GetThreadUsagesByIntegrationId().ToList();
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        public bool HasWaitingJobs()
        {
            try
            {
                return JobQueueManager.HasWaitingJobs;
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        public bool HasRunningJobs()
        {
            try
            {
                return JobQueueManager.HasRunningJobs;
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        public List<KeyValuePair<Guid, List<JobInstance>>> GetWaitingJobsByIntegrationId()
        {
            try
            {
                return JobQueueManager.WaitingJobsByIntegrationId.ToList();
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        public List<KeyValuePair<Guid, List<JobInstance>>> GetRunningJobsByIntegrationId()
        {
            try
            {
                return JobQueueManager.RunningJobsByIntegrationId.ToList();
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        public List<JobInstance> QueueJob(Integration integration, IEnumerable<DataSource> sourceSideDataSourcesToRun, Job jobToRun, 
            DateTime scheduledStartTime, string invocationSource, JobInvocationSourceType invocationType)
        {
            try
            {
                return JobQueueManager.QueueJob(integration, sourceSideDataSourcesToRun, jobToRun, scheduledStartTime, invocationSource, invocationType);
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        public JobInstance QueueJobWithFilters(Integration integration, DataSource sourceSideDataSourceToRun, Job jobToRun,
            DateTime scheduledStartTime, string invocationSource, JobInvocationSourceType invocationType, List<JobFilter> filters)
        {
            try
            {
                return JobQueueManager.QueueJob(integration, sourceSideDataSourceToRun, jobToRun, scheduledStartTime, invocationSource, invocationType, filters);
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        public bool? QueueRequestIsComplete(Guid queueRequestId)
        {
            try
            {
                return JobQueueManager.QueueRequestIsComplete(queueRequestId);
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        public bool? JobInstanceIsComplete(Guid jobInstanceId)
        {
            try
            {
                return JobQueueManager.JobInstanceIsComplete(jobInstanceId);
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        public JobQueueRequestResult GetQueueRequestResult(Guid queueRequestId)
        {
            try
            {
                return JobQueueManager.GetQueueRequestResult(queueRequestId);
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        public JobInstanceResult GetJobInstanceResult(Guid jobInstanceId)
        {
            try
            {
                return JobQueueManager.GetJobInstanceResult(jobInstanceId);
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        public List<JobInstance> DequeueAllJobs()
        {
            try
            {
                return JobQueueManager.DequeueAllJobs();
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        public List<JobInstance> DequeueOnDemandJobs()
        {
            try
            {
                return JobQueueManager.DequeueOnDemandJobs();
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        public List<JobInstance> DequeueScheduledJobs()
        {
            try
            {
                return JobQueueManager.DequeueScheduledJobs();
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        public bool DequeueJob(Guid jobInstanceGuid)
        {
            try
            {
                return JobQueueManager.DequeueJob(jobInstanceGuid);
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }
    }
}
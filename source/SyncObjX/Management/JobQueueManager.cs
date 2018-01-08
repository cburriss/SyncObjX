using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using SyncObjX.Collections.Generic;
using SyncObjX.Configuration;
using SyncObjX.Exceptions;
using SyncObjX.Logging;
using SyncObjX.SyncObjects;

namespace SyncObjX.Management
{
    public static class JobQueueManager
    {
        private static System.Timers.Timer timer;

        private static double _intervalInSeconds = 0.0d;

        private static Dictionary<Guid, List<JobInstance>> waitingJobsByIntegrationId = new Dictionary<Guid, List<JobInstance>>();

        private static Dictionary<Guid, List<JobInstance>> runningJobsByIntegrationId = new Dictionary<Guid, List<JobInstance>>();

        // maintains collection of previous X on-demand job instances; maintained in memory for web service performance
        private static FixedSizeDictionary<Guid, JobInstance> jobInstancesById = new FixedSizeDictionary<Guid, JobInstance>(50);

        private static FixedSizeDictionary<Guid, JobQueueRequest> queueRequestsById = new FixedSizeDictionary<Guid, JobQueueRequest>(50);

        private static Dictionary<Guid, ConcurrentThreadState> threadsInUseByIntegrationId = new Dictionary<Guid, ConcurrentThreadState>();

        private static Dictionary<int, JobInstance> jobInstancesByParallelTaskId = new Dictionary<int, JobInstance>();

        private static Dictionary<JobPriority, TimeSpan> maxDelayedStartByJobPriority = new Dictionary<JobPriority, TimeSpan>()
        {
            { JobPriority.VeryLow, new TimeSpan(0, 0, 1800) },
            { JobPriority.Low, new TimeSpan(0, 0, 900) },
            { JobPriority.Normal, new TimeSpan(0, 0, 300) },
            { JobPriority.High, new TimeSpan(0, 0, 150) },
            { JobPriority.VeryHigh, new TimeSpan(0, 0, 0) }
        };

        private static bool suspendJobInvocation = false;

        private static JobQueueManagerStatus status = JobQueueManagerStatus.Stopped;

        private static ISyncEngineConfigurator _configurator;

        public static event EventHandler<JobQueueManagerStatusChangedArgs> RunStateChanged;

        public static event EventHandler<JobQueuedArgs> JobQueued;

        public static event EventHandler<JobDequeuedArgs> JobDequeued;

        public static event EventHandler<JobQueueRequestStatusChangedArgs> JobQueueRequestStatusChanged;

        public static event EventHandler<JobQueueStatusChangedArgs> JobInstanceStatusChanged;

        public static event EventHandler<JobStepQueueStatusChangedArgs> JobStepInstanceStatusChanged;

        public static event EventHandler<ConcurrentThreadUsageChangedArgs> ThreadUsageChanged;

        public static IEnumerable<KeyValuePair<JobPriority, TimeSpan>> MaxDelayedStartByJobPriority
        {
            get
            {
                return maxDelayedStartByJobPriority;
            }

            set
            {
                if (value == null)
                    return;

                foreach (var jobPriority in value)
                {
                    // this case does not have a max delayed start
                    if (jobPriority.Key == JobPriority.AlwaysRunOtherJobsFirst)
                        continue;

                    if (jobPriority.Value.TotalSeconds < 0)
                        throw new Exception(string.Format("Job priority '{0}' max delay must be greater than or equal to 0.", Enum.GetName(typeof(JobPriority), jobPriority.Key)));

                    maxDelayedStartByJobPriority[jobPriority.Key] = jobPriority.Value;
                }
            }
        }

        public static bool HasWaitingJobs
        {
            get
            {
                foreach (var waitingJobsForIntegration in waitingJobsByIntegrationId.ToList())
                {
                    if (waitingJobsForIntegration.Value != null && waitingJobsForIntegration.Value.Count > 0)
                        return true;
                }
                
                return false;
            }
        }

        public static bool HasRunningJobs
        {
            get
            {
                foreach (var runningJobsForIntegration in runningJobsByIntegrationId.ToList())
                {
                    if (runningJobsForIntegration.Value != null && runningJobsForIntegration.Value.Count > 0)
                        return true;
                }

                return false;
            }
        }

        public static bool IsSafeForQueueLogRetrieval
        {
            get
            {
                if (HasRunningJobs || (Status == JobQueueManagerStatus.Started && HasWaitingJobs))
                    return false;
                else
                    return true;
            }
        }

        public static double IntervalInSeconds
        {
            get
            {
                return _intervalInSeconds;
            }
        }

        public static JobQueueManagerStatus Status
        {
            get
            {
                return status;
            }

            private set
            {
                status = value;

                if (RunStateChanged != null)
                    RunStateChanged(null, new JobQueueManagerStatusChangedArgs(status));
            }
        }

        public static ISyncEngineConfigurator Configurator
        {
            get
            {
                return _configurator;
            }

            set
            {
                _configurator = value;
            }
        }

        static JobQueueManager()
        {
            WireQueueManagementEventsForLogging();
        }

        private static void WireQueueManagementEventsForLogging()
        {
            JobQueueManager.RunStateChanged += new EventHandler<JobQueueManagerStatusChangedArgs>((s, e) =>
            {
                SyncEngineLogger.WriteToLog(LogEntryType.Info, "Job queue manager run state changed to '{0}'.", e.NewStatus);
            });

            JobQueueManager.JobQueued += new EventHandler<JobQueuedArgs>((s, e) =>
            {
                SyncEngineLogger.WriteToLog(LogEntryType.Info, e.QueuedJobInstance, "Job '{0}' ({1}) queued and scheduled to execute at {2}.",
                                                e.QueuedJobInstance.Job.Name, e.QueuedJobInstance.Id, e.QueuedJobInstance.ScheduledStartTime);
            });

            JobQueueManager.JobDequeued += new EventHandler<JobDequeuedArgs>((s, e) =>
            {
                SyncEngineLogger.WriteToLog(LogEntryType.Info, e.DequeuedJobInstance, "Job '{0}' ({1}) dequeued.",
                                                e.DequeuedJobInstance.Job.Name, e.DequeuedJobInstance.Id);
            });

            JobQueueManager.JobQueueRequestStatusChanged += new EventHandler<JobQueueRequestStatusChangedArgs>((s, e) =>
            {
                SyncEngineLogger.WriteToLog(LogEntryType.Info, e.QueueRequest.Integration, "Job '{0}' queue request ({1}) status changed to '{2}'.",
                                                e.QueueRequest.Job.Name, e.QueueRequest.Id, e.QueueRequest.Status);
            });

            JobQueueManager.JobInstanceStatusChanged += new EventHandler<JobQueueStatusChangedArgs>((s, e) =>
            {
                SyncEngineLogger.WriteToLog(LogEntryType.Info, e.UpdatedJobInstance, "Job '{0}' ({1}) status changed to '{2}'.",
                                                e.UpdatedJobInstance.Job.Name, e.UpdatedJobInstance.Id, e.UpdatedJobInstance.Status);
            });

            JobQueueManager.JobStepInstanceStatusChanged += new EventHandler<JobStepQueueStatusChangedArgs>((s, e) =>
            {
                SyncEngineLogger.WriteToLog(LogEntryType.Info, e.AssociatedJobInstance, e.UpdatedJobStepInstance, "Job step '{0}' ({1}) for Job '{2}' ({3}) status changed to '{4}'.",
                                                e.UpdatedJobStepInstance.JobStep.Name, e.UpdatedJobStepInstance.Id,
                                                e.AssociatedJobInstance == null ? "N/A" : e.AssociatedJobInstance.Job.Name,
                                                e.AssociatedJobInstance == null ? "N/A" : e.AssociatedJobInstance.Id.ToString(),
                                                e.UpdatedJobStepInstance.Status);
            });

            JobQueueManager.ThreadUsageChanged += new EventHandler<ConcurrentThreadUsageChangedArgs>((s, e) =>
            {
                SyncEngineLogger.WriteToLog(LogEntryType.Info, e.Integration, "Integration '{0}' using {1}/{2} available threads.",
                                                e.Integration.Name, e.NewConcurrentThreadState.ThreadsInUse, e.NewConcurrentThreadState.MaxConcurrentThreads);
            });
        }

        public static TimeSpan GetMaxDelayedStartForJobPriority(JobPriority jobPriority)
        {
            return maxDelayedStartByJobPriority[jobPriority];
        }

        public static JobInstance GetJobInstanceByParallelTaskId(int taskId)
        {
            JobInstance jobInstance;

            jobInstancesByParallelTaskId.TryGetValue(taskId, out jobInstance);

            return jobInstance;
        }

        public static ConcurrentThreadState GetThreadUsage(Guid integrationId)
        {
            if (integrationId == Guid.Empty)
                throw new Exception("Integration ID can not be empty.");

            if (!threadsInUseByIntegrationId.ContainsKey(integrationId))
                threadsInUseByIntegrationId[integrationId] = new ConcurrentThreadState(1);

            return threadsInUseByIntegrationId[integrationId];
        }

        public static IEnumerable<KeyValuePair<Guid, ConcurrentThreadState>> GetThreadUsagesByIntegrationId()
        {
            return threadsInUseByIntegrationId;
        }

        public static IEnumerable<KeyValuePair<Guid, List<JobInstance>>> WaitingJobsByIntegrationId
        {
            get { return waitingJobsByIntegrationId; }
        }

        /// <summary>
        /// Gets a copy of the collection that maintains running jobs.
        /// </summary>
        public static Dictionary<Guid, List<JobInstance>> RunningJobsByIntegrationId
        {
            get 
            {
                Dictionary<Guid, List<JobInstance>> copy = new Dictionary<Guid,List<JobInstance>>();

                foreach (var integrationId in runningJobsByIntegrationId.Keys.ToList())
                {
                    // ToList will copy the list, necessary because the running jobs object change via event handlers
                    copy.Add(integrationId, new List<JobInstance>(runningJobsByIntegrationId[integrationId]));
                }

                return copy;
            }
        }

        public static void Start(double intervalInSeconds)
        {
            if (intervalInSeconds <= 0)
                throw new Exception("Interval (in seconds) must be greater than 0.");

            _intervalInSeconds = intervalInSeconds;

            timer = new System.Timers.Timer(IntervalInSeconds * 1000);

            // http://stackoverflow.com/questions/18280330/system-timers-timer-elapsed-event-executing-after-timer-stop-is-called
            // http://msdn.microsoft.com/en-us/library/system.timers.timer.autoreset(v=vs.110).aspx
            timer.AutoReset = false; // informs the timer to only invoke the Elapsed event once; timer.Start() is required to fire the event on the next interval

            timer.Elapsed += new System.Timers.ElapsedEventHandler(RunJobsFromQueue);

            timer.Start();

            Status = JobQueueManagerStatus.Started;
        }

        public static void Restart()
        {
            if (IntervalInSeconds == 0.0d)
                throw new Exception("Queue manager has never started so can not be restarted.");

            Status = JobQueueManagerStatus.Started;
        }

        public static void Stop()
        {
            Status = JobQueueManagerStatus.Stopped;
        }

        public static JobInstance QueueJob(Integration integration, DataSource sourceSideDataSourceToRun, Job jobToRun, 
            DateTime scheduledStartTime, string invocationSource, JobInvocationSourceType invocationType, List<JobFilter> filters = null)
        {
            var jobInstances = QueueJob(integration, new List<DataSource>() { sourceSideDataSourceToRun }, jobToRun, 
                scheduledStartTime, invocationSource, invocationType, filters);

            if (jobInstances == null || jobInstances.Count == 0)
                return null;
            else
                return jobInstances[0];
        }

        public static List<JobInstance> QueueJob(Integration integration, IEnumerable<DataSource> sourceSideDataSourcesToRun, Job jobToRun, 
            DateTime scheduledStartTime, string invocationSource, JobInvocationSourceType invocationSourceType, List<JobFilter> filters = null)
        {
            if (integration == null)
                throw new Exception("Integration is missing or empty.");

            if (sourceSideDataSourcesToRun == null || sourceSideDataSourcesToRun.Where(d => d != null).Count() == 0)
                throw new Exception(string.Format("At least one source-side data source is required to queue job '{0}'.", jobToRun.Name));

            foreach(var dataSource in sourceSideDataSourcesToRun)
            {
                if (!jobToRun.SourceDataSources.ContainsKey(dataSource.Name))
                    throw new Exception(string.Format("Source-side data source '{0}' does not exist for job '{1}'. This job will not be queued for execution.", dataSource.Name, jobToRun.Name));
            }

            if (String.IsNullOrWhiteSpace(invocationSource))
                throw new Exception("Invocation source is missing or empty.");

            if (!integration.IsEnabled)
                throw new Exception(string.Format("Job '{0}' could not be queued for execution because integration '{1}' is disabled.", jobToRun.Name, integration.Name));

            if (!jobToRun.IsEnabled)
                throw new Exception(string.Format("Job '{0}' could not be queued for execution because it is disabled.", jobToRun.Name));

            if (jobToRun.Steps.Where(d => d.IsEnabled).Count() == 0)
                throw new Exception(string.Format("Job '{0}' could not be queued for execution because no job steps exist or all are disabled.", jobToRun.Name));

            SetMaxConcurrentThreadsForIntegration(integration);

            List<JobInstance> queuedJobs = new List<JobInstance>();

            JobInstance jobInstance;
            JobStepInstance jobStepInstance;
            List<JobStepInstance> jobStepInstances;

            var queueRequest = new JobQueueRequest(integration, jobToRun, invocationSourceType);

            queueRequest.StatusChanged += new EventHandler<JobQueueRequestStatusChangedArgs>(HandleChangedQueueRequestStatus);

            queueRequest.Status = JobQueueRequestStatus.Waiting;

            foreach (var sourceSideDataSource in sourceSideDataSourcesToRun)
            {
                byte jobStepIdx = 0;

                jobStepInstances = new List<JobStepInstance>();

                foreach (var jobStep in jobToRun.Steps)
                {
                    if (jobStep.IsEnabled)
                    {
                        jobStepInstance = new JobStepInstance(jobStep, jobStepIdx);

                        jobStepInstance.StatusChanged += new EventHandler<JobStepQueueStatusChangedArgs>(HandleChangedJobStepStatus);

                        jobStepInstances.Add(jobStepInstance);

                        //jobStepInstance.Status = JobStepQueueStatus.Waiting;

                        jobStepIdx++;
                    }
                }

                jobInstance = new JobInstance(queueRequest, integration, jobToRun, sourceSideDataSource, jobToRun.TargetDataSource, jobStepInstances, 
                                                    scheduledStartTime, invocationSource, invocationSourceType, filters);

                queueRequest.JobInstances.Add(jobInstance);

                // change the status here to avoid the job step not having an association with the job instance when status changes are logged
                foreach (var jobStepInstanceToFireStatusChangeFor in jobStepInstances)
                {
                    jobStepInstanceToFireStatusChangeFor.Status = JobStepQueueStatus.Waiting;
                }

                jobInstance.StatusChanged += new EventHandler<JobQueueStatusChangedArgs>(HandleChangedJobStatus);

                if (waitingJobsByIntegrationId.ContainsKey(jobInstance.Integration.Id))
                    waitingJobsByIntegrationId[jobInstance.Integration.Id].Add(jobInstance);
                else
                    waitingJobsByIntegrationId.Add(jobInstance.Integration.Id, new List<JobInstance>() { jobInstance });

                // cache the job instance in memory if on-demand (temporary to get around OutOfMemoryException)
                if (jobInstance.InvocationSourceType == JobInvocationSourceType.OnDemand)
                {
                    jobInstancesById.Add(jobInstance.Id, jobInstance);
                    queueRequestsById.Add(queueRequest.Id, queueRequest);
                }

                jobInstance.Status = JobQueueStatus.Scheduled;

                queuedJobs.Add(jobInstance);

                if (JobQueued != null)
                    JobQueued(null, new JobQueuedArgs(jobInstance));
            }

            return queuedJobs;
        }

        public static void QueueJob(JobInstance jobInstance)
        {
            JobInstance existingJobInstance;

            if (runningJobsByIntegrationId.ContainsKey(jobInstance.Integration.Id))
            {
                existingJobInstance = runningJobsByIntegrationId[jobInstance.Integration.Id].Where(d => d.Id == jobInstance.Id).FirstOrDefault();

                if (existingJobInstance != null)
                    throw new Exception(string.Format("A job instance with Id '{0}' is already running.", jobInstance.Id));
            }

            if (waitingJobsByIntegrationId.ContainsKey(jobInstance.Integration.Id))
            {
                existingJobInstance = waitingJobsByIntegrationId[jobInstance.Integration.Id].Where(d => d.Id == jobInstance.Id).FirstOrDefault();

                if (existingJobInstance != null)
                    throw new Exception(string.Format("A job instance with Id '{0}' is already waiting for execution.", jobInstance.Id));

                waitingJobsByIntegrationId[jobInstance.Integration.Id].Add(jobInstance);

            }
            else
            {
                SetMaxConcurrentThreadsForIntegration(jobInstance.Integration);

                waitingJobsByIntegrationId.Add(jobInstance.Integration.Id, new List<JobInstance>() { jobInstance });
            }

            // cache the job instance in memory if on-demand (temporary to get around OutOfMemoryException)
            if (jobInstance.InvocationSourceType == JobInvocationSourceType.OnDemand)
            {
                jobInstancesById.Add(jobInstance.Id, jobInstance);
                queueRequestsById.Add(jobInstance.QueueRequest.Id, jobInstance.QueueRequest);
            }

            jobInstance.StatusChanged += new EventHandler<JobQueueStatusChangedArgs>(HandleChangedJobStatus);

            foreach (var jobStepInstance in jobInstance.JobStepInstances)
            {
                jobStepInstance.StatusChanged += new EventHandler<JobStepQueueStatusChangedArgs>(HandleChangedJobStepStatus);
            }

            jobInstance.QueueRequest.Status = JobQueueRequestStatus.Waiting;

            jobInstance.Status = JobQueueStatus.Scheduled;

            jobInstance.RunningJobStepInstance = null;

            if (JobQueued != null)
                JobQueued(null, new JobQueuedArgs(jobInstance));
        }

        private static void SetMaxConcurrentThreadsForIntegration(Integration integration)
        {
            if (!threadsInUseByIntegrationId.ContainsKey(integration.Id))
            {
                threadsInUseByIntegrationId.Add(integration.Id, new ConcurrentThreadState(integration.MaxConcurrentThreads));
            }
            // allow max concurrent threads to be changed if no jobs with that integration name are awaiting execution
            else if (threadsInUseByIntegrationId[integration.Id].MaxConcurrentThreads != integration.MaxConcurrentThreads &&
                     waitingJobsByIntegrationId.ContainsKey(integration.Id))
                     //TODO: Keep commented? - && waitingJobsByIntegrationName[integration.Name].Count == 0)
            {
                var threadStateForIntegration = threadsInUseByIntegrationId[integration.Id];
                threadStateForIntegration.MaxConcurrentThreads = integration.MaxConcurrentThreads;
            }
        }

        private static void RunJobsFromQueue(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                // invoke the event on the next time interval if the queue manager is started
                if (Status != JobQueueManagerStatus.Stopped)
                    timer.Start();

                if (!suspendJobInvocation)
                {
                    // suspend job invocation for the next timed cycle(s) so that jobs aren't inadvertently executed more than once
                    suspendJobInvocation = true;

                    foreach (var integrationId in waitingJobsByIntegrationId.Keys.ToList())
                    {
                        // get all job instances where the scheduled start time is now or in the past
                        var waitingJobInstancesReadyForExecution = waitingJobsByIntegrationId[integrationId]
                                                                        .Where(d => d.ScheduledStartTime <= DateTime.Now)
                                                                        .ToList();

                        if (waitingJobsByIntegrationId[integrationId].Count > 0)
                        {
                            // set the boolean for each job instance indicating if the max delayed start has been exceeded
                            waitingJobInstancesReadyForExecution.ForEach(d => d.MaxDelayedStartExceeded = HasMaxDelayedStartBeenExceeded(d.Job.Priority, d.ScheduledStartTime));

                            if (threadsInUseByIntegrationId[integrationId].HasAvailableThread && waitingJobInstancesReadyForExecution.Count > 0)
                            {
                                var jobInstancesToStartNext = GetNextJobInstancesToInvoke(waitingJobInstancesReadyForExecution,
                                                                                  threadsInUseByIntegrationId[integrationId].CountOfAvailableThreads);

                                InvokeJobInstancesUsingParallelTasks(integrationId, jobInstancesToStartNext);

                                // remove invoked jobs from waiting list and updated the queue status if needed
                                foreach (var jobInstanceToRemove in jobInstancesToStartNext)
                                {
                                    waitingJobInstancesReadyForExecution.Remove(jobInstanceToRemove);

                                    if (jobInstanceToRemove.QueueRequest.Status != JobQueueRequestStatus.InProgress)
                                        jobInstanceToRemove.QueueRequest.Status = JobQueueRequestStatus.InProgress;
                                }

                                // change status for tasks that were unable to be invoked due to a thread shortage
                                foreach (var jobInstanceToUpdateStatusFor in waitingJobInstancesReadyForExecution)
                                {
                                    jobInstanceToUpdateStatusFor.Status = JobQueueStatus.Delayed_NoAvailableThread;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex);
            }
            finally
            {
                suspendJobInvocation = false;
            }
        }

        private static void InvokeJobInstancesUsingParallelTasks(Guid integrationId, IEnumerable<JobInstance> jobInstancesToStartNext)
        {
            foreach (var jobInstanceToStartNext in jobInstancesToStartNext)
            {
                waitingJobsByIntegrationId[integrationId].Remove(jobInstanceToStartNext);

                if (runningJobsByIntegrationId.ContainsKey(jobInstanceToStartNext.Integration.Id))
                    runningJobsByIntegrationId[jobInstanceToStartNext.Integration.Id].Add(jobInstanceToStartNext);
                else
                    runningJobsByIntegrationId.Add(jobInstanceToStartNext.Integration.Id, new List<JobInstance>() { jobInstanceToStartNext });

                // invoke the job using a parallel job
                // NOTE: code called downstream but within this block will have a unique Task ID that is used
                //       to refer back to the associated integration within the sync engine logger
                var workerThread = new Thread(() =>
                {
                    try
                    {
                        // this is an unlikely scenario but may occur if the Task IDs start over
                        if (jobInstancesByParallelTaskId.ContainsKey(Thread.CurrentThread.ManagedThreadId))
                            jobInstancesByParallelTaskId[Thread.CurrentThread.ManagedThreadId] = jobInstanceToStartNext;
                        else
                            jobInstancesByParallelTaskId.Add(Thread.CurrentThread.ManagedThreadId, jobInstanceToStartNext);

                        SyncEngineLogger.WriteToLog(LogEntryType.Info, jobInstanceToStartNext, "Job instance '{0}' is starting.", jobInstanceToStartNext.Id);

                        JobInvoker.ExecuteJob(jobInstanceToStartNext, Configurator);

                        SyncEngineLogger.WriteToLog(LogEntryType.Info, jobInstanceToStartNext, "Job instance '{0}' has ended.", jobInstanceToStartNext.Id);
                    }
                    catch (Exception ex)
                    {
                        SyncEngineLogger.WriteExceptionToLog(jobInstanceToStartNext, ex);
                    }
                });
                workerThread.Start();

                var threadStateForIntegration = threadsInUseByIntegrationId[jobInstanceToStartNext.Integration.Id];

                threadStateForIntegration.ThreadsInUse++;

                if (ThreadUsageChanged != null)
                    ThreadUsageChanged(null, new ConcurrentThreadUsageChangedArgs(jobInstanceToStartNext.Integration, threadStateForIntegration));
            }
        }

        private static bool HasMaxDelayedStartBeenExceeded(JobPriority jobPriority, DateTime scheduledStartTime)
        {
            if (jobPriority == JobPriority.AlwaysRunOtherJobsFirst)
                return false;
            else
                return maxDelayedStartByJobPriority[jobPriority].TotalSeconds < (DateTime.Now - scheduledStartTime).TotalSeconds;
        }

        private static IEnumerable<JobInstance> GetNextJobInstancesToInvoke(IEnumerable<JobInstance> waitingJobInstancesReadyForExecution, int countOfJobInstances)
        {
            var now = DateTime.Now;

            return waitingJobInstancesReadyForExecution
                        .OrderByDescending(d => d.MaxDelayedStartExceeded) // returns true if past max delay, which is ordered on top
                        .ThenByDescending(d => d.Job.Priority) // higher # is higher priority
                        .ThenBy(d => d.ScheduledStartTime)
                        .Take(countOfJobInstances)
                        .ToList();
        }

        private static bool QueueRequestExistsInQueue(Guid requestGuid)
        {
            foreach (var integrationName in waitingJobsByIntegrationId.Keys.ToList())
            {
                if (waitingJobsByIntegrationId[integrationName].Exists(d => d.QueueRequest.Id == requestGuid))
                    return true;
            }

            foreach (var integrationName in runningJobsByIntegrationId.Keys.ToList())
            {
                if (runningJobsByIntegrationId[integrationName].Exists(d => d.QueueRequest.Id == requestGuid))
                    return true;
            }

            return false;
        }

        private static void HandleChangedQueueRequestStatus(object sender, JobQueueRequestStatusChangedArgs args)
        {
            try
            {
                if (JobQueueRequestStatusChanged != null)
                    JobQueueRequestStatusChanged(null, args);
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(args.QueueRequest.Integration, ex);
            }
        }

        private static void HandleChangedJobStatus(object sender, JobQueueStatusChangedArgs args)
        {
            try
            {
                if (args.UpdatedJobInstance.Status == JobQueueStatus.Completed ||
                    args.UpdatedJobInstance.Status == JobQueueStatus.Completed_WithError ||
                    args.UpdatedJobInstance.Status == JobQueueStatus.Terminated_WithError)
                {
                    var threadStateForIntegration = threadsInUseByIntegrationId[args.UpdatedJobInstance.Integration.Id];

                    threadStateForIntegration.ThreadsInUse--;

                    if (ThreadUsageChanged != null)
                        ThreadUsageChanged(null, new ConcurrentThreadUsageChangedArgs(args.UpdatedJobInstance.Integration, threadStateForIntegration));
                }

                // if the entire job should be terminated, remove all job instances within the same queue request GUID
                if (args.UpdatedJobInstance.Job.TerminateOnErrorType == JobTerminateOnErrorType.TerminateJob &&
                    (args.UpdatedJobInstance.Status == JobQueueStatus.Completed_WithError ||
                     args.UpdatedJobInstance.Status == JobQueueStatus.Terminated_WithError))
                {
                    suspendJobInvocation = true;

                    var waitingJobInstancesWithinSameQueueRequest = waitingJobsByIntegrationId[args.UpdatedJobInstance.Integration.Id]
                                                                        .Where(d => d.QueueRequest == args.UpdatedJobInstance.QueueRequest);

                    foreach (var jobInstanceWithinSameQueueRequest in waitingJobInstancesWithinSameQueueRequest)
                    {
                        foreach (var jobStepInstance in jobInstanceWithinSameQueueRequest.JobStepInstances)
                        {
                            jobStepInstance.Status = JobStepQueueStatus.DidNotRun_JobError;
                        }

                        jobInstanceWithinSameQueueRequest.Status = JobQueueStatus.DidNotRun_ErrorWithinQueueRequest;
                    }

                    waitingJobsByIntegrationId[args.UpdatedJobInstance.Integration.Id].RemoveAll(d => d.QueueRequest == args.UpdatedJobInstance.QueueRequest);

                    args.UpdatedJobInstance.QueueRequest.Status = JobQueueRequestStatus.Completed;

                    suspendJobInvocation = false;
                }

                if (JobInstanceStatusChanged != null)
                    JobInstanceStatusChanged(null, args);

                // remove from running job list last to allow previous code to be executed first 
                // (e.g. to ensure the HasRunningJobs property isn't set to false prematurely, which is checked on service shutdown)
                if (args.UpdatedJobInstance.Status == JobQueueStatus.Completed ||
                    args.UpdatedJobInstance.Status == JobQueueStatus.Completed_WithError ||
                    args.UpdatedJobInstance.Status == JobQueueStatus.Terminated_WithError)
                {
                    runningJobsByIntegrationId[args.UpdatedJobInstance.Integration.Id].Remove(args.UpdatedJobInstance);

                    if (!QueueRequestExistsInQueue(args.UpdatedJobInstance.QueueRequest.Id))
                        args.UpdatedJobInstance.QueueRequest.Status = JobQueueRequestStatus.Completed;
                }
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(args.UpdatedJobInstance, ex);
            }
            finally
            {
                suspendJobInvocation = false;
            }
        }

        private static void HandleChangedJobStepStatus(object sender, JobStepQueueStatusChangedArgs args)
        {
            try
            {
                if (JobStepInstanceStatusChanged != null)
                    JobStepInstanceStatusChanged(null, args);
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(args.AssociatedJobInstance, ex);
            }
        }

        public static bool? JobInstanceIsComplete(Guid jobInstanceId)
        {
            object _lock = new object();

            lock (_lock)
            {
                JobInstance jobInstance;

                jobInstancesById.TryGetValue(jobInstanceId, out jobInstance);

                if (jobInstance == null)
                    return null;
                else
                {
                    switch(jobInstance.Status)
                    {
                        case JobQueueStatus.Scheduled:
                        case JobQueueStatus.Delayed_NoAvailableThread:
                        case JobQueueStatus.InProgress:
                        case JobQueueStatus.InProgress_MaxDelayExceeded:
                            return false;

                        case JobQueueStatus.DidNotRun_ErrorWithinQueueRequest:
                        case JobQueueStatus.Terminated_WithError:
                        case JobQueueStatus.Completed_WithError:
                        case JobQueueStatus.Completed:
                            return true;

                        default:
                            throw new EnumValueNotImplementedException<JobQueueStatus>(jobInstance.Status);
                    }
                }
            }
        }

        public static bool? QueueRequestIsComplete(Guid queueRequestId)
        {
            object _lock = new object();

            lock (_lock)
            {
                JobQueueRequest queueRequest;

                queueRequestsById.TryGetValue(queueRequestId, out queueRequest);

                if (queueRequest == null)
                    return null;
                else
                {
                    switch (queueRequest.Status)
                    {
                        case JobQueueRequestStatus.Waiting:
                        case JobQueueRequestStatus.InProgress:
                            return false;

                        case JobQueueRequestStatus.Completed:
                            return true;

                        default:
                            throw new EnumValueNotImplementedException<JobQueueRequestStatus>(queueRequest.Status);
                    }
                }
            }
        }

        public static JobInstanceResult GetJobInstanceResult(Guid jobInstanceId)
        {
            JobInstanceResult jobInstanceResult = null;
            JobInstance jobInstance;

            object _lock = new object();

            lock (_lock)
            {
                jobInstancesById.TryGetValue(jobInstanceId, out jobInstance);

                if (jobInstance != null)
                {
                    jobInstanceResult = JobQueueResult.GetResult(jobInstance);
                }
            }

            return jobInstanceResult;
        }

        public static JobQueueRequestResult GetQueueRequestResult(Guid queueRequestId)
        {
            JobQueueRequestResult queueRequestResult = null;
            JobQueueRequest queueRequest;

            object _lock = new object();

            lock (_lock)
            {
                queueRequestsById.TryGetValue(queueRequestId, out queueRequest);

                if (queueRequest != null)
                {
                    queueRequestResult = JobQueueResult.GetResult(queueRequest);
                }
            }

            return queueRequestResult;
        }

        public static List<JobInstance> DequeueAllJobs()
        {
            return DequeueJobs(removeScheduled: true, removeOnDemand: true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Returns a list of removed job instances.</returns>
        public static List<JobInstance> DequeueOnDemandJobs()
        {
            return DequeueJobs(removeScheduled: false, removeOnDemand: true);
        }

        public static List<JobInstance> DequeueScheduledJobs()
        {
            return DequeueJobs(removeScheduled: true, removeOnDemand: false);
        }

        private static List<JobInstance> DequeueJobs(bool removeScheduled, bool removeOnDemand)
        {
            suspendJobInvocation = true;

            List<JobInstance> jobInstancesToDequeue = new List<JobInstance>();

            foreach (var integrationName in waitingJobsByIntegrationId.Keys)
            {
                 jobInstancesToDequeue.AddRange(waitingJobsByIntegrationId[integrationName]
                                                    .Where(d => (removeScheduled && d.InvocationSourceType == JobInvocationSourceType.Scheduled) ||
                                                                (removeOnDemand && d.InvocationSourceType == JobInvocationSourceType.OnDemand)));
            }

            foreach (var jobInstanceToDequeue in jobInstancesToDequeue)
            {
                bool wasRemoved = waitingJobsByIntegrationId[jobInstanceToDequeue.Integration.Id].Remove(jobInstanceToDequeue);

                if (!wasRemoved)
                    throw new Exception(string.Format("Job '{0}' of integration '{1}' (Instance GUID: {2}) was not removed but should have been.",
                        jobInstanceToDequeue.Job.Name, jobInstanceToDequeue.Integration.Name, jobInstanceToDequeue.Id));

                if (JobDequeued != null)
                    JobDequeued(null, new JobDequeuedArgs(jobInstanceToDequeue));
            }

            suspendJobInvocation = false;

            return jobInstancesToDequeue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="jobInstanceGuid"></param>
        /// <returns>Returns true if the job was removed.</returns>
        public static bool DequeueJob(Guid jobInstanceGuid)
        {
            suspendJobInvocation = true;

            JobInstance jobInstanceToDequeue;

            foreach (var integrationName in waitingJobsByIntegrationId.Keys)
            {
                jobInstanceToDequeue = waitingJobsByIntegrationId[integrationName].Where(d => d.Id == jobInstanceGuid).FirstOrDefault();

                if (jobInstanceToDequeue != null)
                {
                    bool wasRemoved = waitingJobsByIntegrationId[integrationName].Remove(jobInstanceToDequeue);

                    if (!wasRemoved)
                        throw new Exception(string.Format("Job '{0}' of integration '{1}' (Instance GUID: {2}) was not removed but should have been.",
                            jobInstanceToDequeue.Job.Name, jobInstanceToDequeue.Integration.Name, jobInstanceToDequeue.Id));

                    if (JobDequeued != null)
                        JobDequeued(null, new JobDequeuedArgs(jobInstanceToDequeue));

                    suspendJobInvocation = false;

                    return true;
                }
            }

            suspendJobInvocation = false;

            return false;
        }
    }
}

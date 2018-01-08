using System;
using System.Collections.Generic;
using System.ServiceModel;
using SyncObjX.SyncObjects;

namespace SyncObjX.Management
{
    [ServiceContract]
    [ServiceKnownType(typeof(JobInstance))]
    [ServiceKnownType(typeof(JobInstanceResult))]
    [ServiceKnownType(typeof(JobQueueResult))]
    [ServiceKnownType(typeof(JobFilter))]
    [ServiceKnownType(typeof(Exception))]
    public interface IJobQueueManagement
    {
        [OperationContract]
        void Start(double intervalInSeconds);

        [OperationContract]
        void Restart();

        [OperationContract]
        void Stop();

        [OperationContract]
        JobQueueManagerStatus GetStatus();

        [OperationContract]
        double GetIntervalInSeconds();

        [OperationContract]
        List<KeyValuePair<JobPriority, TimeSpan>> GetMaxDelayedStartByJobPriority();

        [OperationContract]
        TimeSpan GetMaxDelayedStartForJobPriority(JobPriority jobPriority);

        [OperationContract]
        ConcurrentThreadState GetThreadUsage(Guid integrationId);

        [OperationContract]
        List<KeyValuePair<Guid, ConcurrentThreadState>> GetThreadUsagesByIntegrationId();

        [OperationContract]
        bool HasWaitingJobs();

        [OperationContract]
        bool HasRunningJobs();

        [OperationContract]
        List<KeyValuePair<Guid, List<JobInstance>>> GetWaitingJobsByIntegrationId();

        [OperationContract]
        List<KeyValuePair<Guid, List<JobInstance>>> GetRunningJobsByIntegrationId();

        [OperationContract]
        List<JobInstance> QueueJob(Integration integration, IEnumerable<DataSource> sourceSideDataSourcesToRun, Job jobToRun, DateTime scheduledRunTime, string invocationSource, JobInvocationSourceType invocationType);

        [OperationContract]
        JobInstance QueueJobWithFilters(Integration integration, DataSource sourceSideDataSourcesToRun, Job jobToRun, DateTime scheduledRunTime, string invocationSource, JobInvocationSourceType invocationType, List<JobFilter> filters);

        [OperationContract]
        bool? QueueRequestIsComplete(Guid queueRequestId);

        [OperationContract]
        bool? JobInstanceIsComplete(Guid jobInstanceId);

        [OperationContract]
        JobQueueRequestResult GetQueueRequestResult(Guid queueRequestId);

        [OperationContract]
        JobInstanceResult GetJobInstanceResult(Guid jobInstanceId);

        [OperationContract]
        List<JobInstance> DequeueAllJobs();

        [OperationContract]
        List<JobInstance> DequeueOnDemandJobs();

        [OperationContract]
        List<JobInstance> DequeueScheduledJobs();

        [OperationContract]
        bool DequeueJob(Guid jobInstanceGuid);
    }
}

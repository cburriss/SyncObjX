using System;
using System.Collections.Generic;

namespace SyncObjX.Management
{
    public class JobQueueResult
    {
        public static JobQueueRequestResult GetResult(JobQueueRequest queueRequest)
        {
            if (queueRequest == null)
                return null;

            object _lock = new object();

            lock (_lock)
            {
                var queueRequestResult = new JobQueueRequestResult();

                queueRequestResult.IntegrationId = queueRequest.JobInstances.Count > 0 ? queueRequest.JobInstances[0].Integration.Id : Guid.Empty;
                queueRequestResult.JobId = queueRequest.Job.Id;
                queueRequestResult.QueueRequestId = queueRequest.Id;
                queueRequestResult.InvocationSourceType = queueRequest.InvocationSourceType;
                queueRequestResult.Status = queueRequest.Status;
               
                var jobResults = new List<JobInstanceResult>();

                foreach (var jobInstance in queueRequest.JobInstances)
                {
                    jobResults.Add(GetResult(jobInstance));
                }

                queueRequestResult.JobInstanceResults = jobResults;

                return queueRequestResult;
            }
        }

        public static JobInstanceResult GetResult(JobInstance jobInstance)
        {
            if (jobInstance == null)
                return null;

            object _lock = new object();

            lock (_lock)
            {
                var jobInstanceResult = new JobInstanceResult();

                jobInstanceResult.IntegrationId = jobInstance.Integration.Id;
                jobInstanceResult.JobId = jobInstance.Job.Id;
                jobInstanceResult.QueueRequestId = jobInstance.QueueRequest.Id;
                jobInstanceResult.JobInstanceId = jobInstance.Id;
                jobInstanceResult.Filters = jobInstance.Filters;
                jobInstanceResult.SourceDataSourceId = jobInstance.SourceDataSource.DataSource.Id;
                jobInstanceResult.TargetDataSourceId = jobInstance.TargetDataSource.DataSource.Id;
                jobInstanceResult.InvocationSource = jobInstance.InvocationSource;
                jobInstanceResult.InvocationSourceType = jobInstance.InvocationSourceType;
                jobInstanceResult.Status = jobInstance.Status;
                jobInstanceResult.ScheduledStartTime = jobInstance.ScheduledStartTime;
                jobInstanceResult.TimeToStartDelay = jobInstance.TimeToStartDelay;
                jobInstanceResult.ActualStartTime = jobInstance.ActualStartTime;
                jobInstanceResult.ActualEndTime = jobInstance.ActualEndTime;
                jobInstanceResult.ActualDuration = jobInstance.ActualDuration;
                jobInstanceResult.HasRecordErrors = jobInstance.HasRecordErrors;
                jobInstanceResult.HasRuntimeErrors = jobInstance.HasRuntimeErrors;
                jobInstanceResult.Exceptions = WebServiceException.Convert(jobInstance.Exceptions);

                var jobStepResults = new List<JobStepInstanceResult>();

                foreach (var jobStepInstance in jobInstance.JobStepInstances)
                {
                    jobStepResults.Add(GetResult(jobStepInstance));
                }

                jobInstanceResult.JobStepInstanceResults = jobStepResults;

                return jobInstanceResult;
            }
        }

        public static JobStepInstanceResult GetResult(JobStepInstance jobStepInstance)
        {
            if (jobStepInstance == null)
                return null;

            object _lock = new object();

            lock (_lock)
            {
                var jobStepResult = new JobStepInstanceResult();

                jobStepResult.JobStepId = jobStepInstance.JobStep.Id;
                jobStepResult.JobInstanceId = jobStepInstance.AssociatedJobInstance.Id;
                jobStepResult.JobStepInstanceId = jobStepInstance.Id;
                jobStepResult.OrderIndex = jobStepInstance.OrderIndex;
                jobStepResult.Status = jobStepInstance.Status;
                jobStepResult.ActualStartTime = jobStepInstance.ActualStartTime;
                jobStepResult.ActualEndTime = jobStepInstance.ActualEndTime;
                jobStepResult.ActualDuration = jobStepInstance.ActualDuration;
                jobStepResult.HasRecordErrors = jobStepInstance.HasRecordErrors;
                jobStepResult.HasRuntimeErrors = jobStepInstance.HasRuntimeErrors;
                jobStepResult.Exceptions = WebServiceException.Convert(jobStepInstance.Exceptions);

                return jobStepResult;
            }
        }
    }
}

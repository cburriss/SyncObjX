using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using SyncObjX.Configuration;
using SyncObjX.Exceptions;
using ls = SyncObjX.Management.LinqToSql;

namespace SyncObjX.Management
{
    public class JobQueueDatabaseLogger : IJobQueueManagementLogger
    {
        private string _connectionString;

        public JobQueueDatabaseLogger(string connectionString)
        {
            if (String.IsNullOrWhiteSpace(connectionString))
                throw new Exception("Connection string is null or empty.");

            // test connection to configuration database; if a failure occurs, an exception will be thrown
            SqlConnection conn = new SqlConnection(connectionString);
            conn.Open();
            conn.Close();

            _connectionString = connectionString;
        }

        public void AddToQueueLog(JobInstance jobInstance)
        {
            if (jobInstance == null)
                throw new Exception("Job instance can not be null.");

            using (var dbContext = new ls.QueueManagementDataContext(_connectionString))
            {
                var now = DateTime.Now;

                var queuedJobInstance = new ls.JobQueue();

                queuedJobInstance.JobInstanceId = jobInstance.Id;
                queuedJobInstance.QueueRequestId = jobInstance.QueueRequest.Id;
                queuedJobInstance.JobId = jobInstance.Job.Id;
                queuedJobInstance.CreatedDate = now;
                queuedJobInstance.UpdatedDate = now;
                queuedJobInstance.SourceDataSourceId = jobInstance.SourceDataSource.DataSource.Id;
                queuedJobInstance.TargetDataSourceId = jobInstance.TargetDataSource.DataSource.Id;
                queuedJobInstance.Filters = JobFilterHelper.GetTextForDatabase(jobInstance.Filters);
                queuedJobInstance.InvocationSource = jobInstance.InvocationSource;
                queuedJobInstance.ScheduledStartTime = jobInstance.ScheduledStartTime;
                queuedJobInstance.ActualStartTime = jobInstance.ActualStartTime.HasValue ? jobInstance.ActualStartTime.Value : new DateTime?();
                queuedJobInstance.IsOnDemand = jobInstance.InvocationSourceType == JobInvocationSourceType.OnDemand ? true : false;
                queuedJobInstance.JobQueueStatusId = (byte)jobInstance.Status;

                dbContext.JobQueues.InsertOnSubmit(queuedJobInstance);

                dbContext.SubmitChanges();

                foreach (var jobStepInstance in jobInstance.JobStepInstances)
                {
                    var queuedJobStepInstance = new ls.JobStepQueue();

                    queuedJobStepInstance.JobStepInstanceId = jobStepInstance.Id;
                    queuedJobStepInstance.JobInstanceId = jobInstance.Id;
                    queuedJobStepInstance.JobStepId = jobStepInstance.JobStep.Id;
                    queuedJobStepInstance.CreatedDate = now;
                    queuedJobStepInstance.UpdatedDate = now;
                    queuedJobStepInstance.ActualStartTime = jobStepInstance.ActualStartTime.HasValue ? jobStepInstance.ActualStartTime.Value : new DateTime?();
                    queuedJobStepInstance.ActualEndTime = jobStepInstance.ActualEndTime.HasValue ? jobStepInstance.ActualEndTime.Value : new DateTime?();
                    queuedJobStepInstance.OrderIndex = (byte)jobStepInstance.OrderIndex;
                    queuedJobStepInstance.JobStepQueueStatusId = (byte)jobStepInstance.Status;

                    dbContext.JobStepQueues.InsertOnSubmit(queuedJobStepInstance);
                }

                dbContext.SubmitChanges();
            }
        }

        public void UpdateJobStatusInQueueLog(JobInstance jobInstance)
        {
            if (jobInstance == null)
                throw new Exception("Job instance can not be null.");

            using (var dbContext = new ls.QueueManagementDataContext(_connectionString))
            {
                var queuedJobInstance = dbContext.JobQueues.Where(d => d.JobInstanceId == jobInstance.Id).FirstOrDefault();

                if (queuedJobInstance != null)
                {
                    queuedJobInstance.UpdatedDate = DateTime.Now;
                    queuedJobInstance.ActualStartTime = jobInstance.ActualStartTime.HasValue ? jobInstance.ActualStartTime.Value : new DateTime?();
                    queuedJobInstance.JobQueueStatusId = (byte)jobInstance.Status;

                    dbContext.SubmitChanges();
                }
            }
        }

        public void UpdateJobStepStatusInQueueLog(JobStepInstance jobStepInstance)
        {
            if (jobStepInstance == null)
                throw new Exception("Job step instance can not be null.");

            using (var dbContext = new ls.QueueManagementDataContext(_connectionString))
            {
                var queuedJobStepInstance = dbContext.JobStepQueues.Where(d => d.JobStepInstanceId == jobStepInstance.Id).FirstOrDefault();

                if (queuedJobStepInstance != null)
                {
                    queuedJobStepInstance.UpdatedDate = DateTime.Now;
                    queuedJobStepInstance.ActualStartTime = jobStepInstance.ActualStartTime.HasValue ? jobStepInstance.ActualStartTime.Value : new DateTime?();
                    queuedJobStepInstance.ActualEndTime = jobStepInstance.ActualEndTime.HasValue ? jobStepInstance.ActualEndTime.Value : new DateTime?();
                    queuedJobStepInstance.JobStepQueueStatusId = (byte)jobStepInstance.Status;

                    dbContext.SubmitChanges();
                }
            }
        }

        public void DeleteAllFromQueueLog()
        {
            using (var dbContext = new ls.QueueManagementDataContext(_connectionString))
            {
                dbContext.ExecuteCommand("DELETE FROM JobStepQueue");

                dbContext.ExecuteCommand("DELETE FROM JobQueue");
            }
        }

        public void DeleteFromQueueLog(IEnumerable<Guid> jobInstanceIds)
        {
            foreach (var jobInstanceGuid in jobInstanceIds)
            {
                DeleteFromQueueLog(jobInstanceGuid);
            }
        }

        public void DeleteFromQueueLog(Guid jobInstanceId)
        {
            using (var dbContext = new ls.QueueManagementDataContext(_connectionString))
            {
                var queuedJobStepInstances = dbContext.JobStepQueues.Where(d => d.JobInstanceId == jobInstanceId).ToList();

                dbContext.JobStepQueues.DeleteAllOnSubmit(queuedJobStepInstances);

                dbContext.SubmitChanges();

                var queuedJobInstances = dbContext.JobQueues.Where(d => d.JobInstanceId == jobInstanceId).ToList();

                dbContext.JobQueues.DeleteAllOnSubmit(queuedJobInstances);

                dbContext.SubmitChanges();
            }
        }

        public void UpdateJobDataSourceHistory(JobInstance jobInstance)
        {
            if (jobInstance == null)
                throw new Exception("Job instance can not be null.");

            if (!jobInstance.ActualStartTime.HasValue)
                throw new Exception("Job instance is missing an actual start time.");

            if (!jobInstance.ActualEndTime.HasValue)
                throw new Exception("Job instance is missing an actual end time.");
          
            using (var dbContext = new ls.QueueManagementDataContext(_connectionString))
            {
                dbContext.UpdateJobDataSourceHistory(jobInstance.Job.Id, jobInstance.SourceDataSource.DataSource.Id, jobInstance.TargetDataSource.DataSource.Id,
                                                     jobInstance.QueueRequest.Id, jobInstance.Id, jobInstance.ActualStartTime.Value, jobInstance.ActualEndTime.Value,
                                                     jobInstance.HasRecordErrors, jobInstance.HasRuntimeErrors);
            }
        }

        public void MoveToHistoryLog(JobInstance jobInstance)
        {
            if (jobInstance == null)
                throw new Exception("Job instance can not be null.");

            using (var dbContext = new ls.QueueManagementDataContext(_connectionString))
            {
                var now = DateTime.Now;

                var jobHistory = new ls.JobHistory();

                jobHistory.JobInstanceId = jobInstance.Id;
                jobHistory.QueueRequestId = jobInstance.QueueRequest.Id;
                jobHistory.JobId = jobInstance.Job.Id;
                jobHistory.CreatedDate = now;
                jobHistory.UpdatedDate = now;
                jobHistory.SourceDataSourceId = jobInstance.SourceDataSource.DataSource.Id;
                jobHistory.TargetDataSourceId = jobInstance.TargetDataSource.DataSource.Id;
                jobHistory.Filters = JobFilterHelper.GetTextForDatabase(jobInstance.Filters);
                jobHistory.InvocationSource = jobInstance.InvocationSource;
                jobHistory.ScheduledStartTime = jobInstance.ScheduledStartTime;
                jobHistory.ActualStartTime = jobInstance.ActualStartTime.HasValue ? jobInstance.ActualStartTime.Value : new DateTime?();

                if (jobInstance.TimeToStartDelay.HasValue && jobInstance.TimeToStartDelay.Value.Hours > 23)
                    jobHistory.TimeToStartDelay = new TimeSpan(23, 59, 59);
                else if (jobInstance.TimeToStartDelay.HasValue)
                    jobHistory.TimeToStartDelay = jobInstance.TimeToStartDelay.Value;
                else
                    jobHistory.TimeToStartDelay = new TimeSpan(0, 0, 0);

                jobHistory.ActualEndTime = jobInstance.ActualEndTime.HasValue ? jobInstance.ActualEndTime.Value : new DateTime?();

                if (jobInstance.ActualDuration.HasValue && jobInstance.ActualDuration.Value.Hours > 23)
                    jobHistory.ActualDuration = new TimeSpan(23, 59, 59);
                else if (jobInstance.ActualDuration.HasValue)
                    jobHistory.ActualDuration = jobInstance.ActualDuration.Value;
                else
                    jobHistory.ActualDuration = new TimeSpan(0, 0, 0);

                jobHistory.IsOnDemand = jobInstance.InvocationSourceType == JobInvocationSourceType.OnDemand ? true : false;
                jobHistory.JobQueueStatusId = (byte)jobInstance.Status;
                jobHistory.HasRecordErrors = jobInstance.HasRecordErrors;
                jobHistory.HasRuntimeErrors = jobInstance.HasRuntimeErrors;

                dbContext.JobHistories.InsertOnSubmit(jobHistory);

                dbContext.SubmitChanges();

                foreach (var jobStepInstance in jobInstance.JobStepInstances)
                {
                    var jobStepHistory = new ls.JobStepHistory();

                    jobStepHistory.JobHistoryId = jobHistory.JobHistoryId;
                    jobStepHistory.JobStepInstanceId = jobStepInstance.Id;
                    jobStepHistory.JobInstanceId = jobInstance.Id;
                    jobStepHistory.JobStepId = jobStepInstance.JobStep.Id;
                    jobStepHistory.CreatedDate = now;
                    jobStepHistory.UpdatedDate = now;
                    jobStepHistory.ActualStartTime = jobStepInstance.ActualStartTime.HasValue ? jobStepInstance.ActualStartTime.Value : new DateTime?();
                    jobStepHistory.ActualEndTime = jobStepInstance.ActualEndTime.HasValue ? jobStepInstance.ActualEndTime.Value : new DateTime?();

                    if (jobStepInstance.ActualDuration.HasValue && jobStepInstance.ActualDuration.Value.Hours > 23)
                        jobStepHistory.ActualDuration = new TimeSpan(23, 59, 59);
                    else if (jobStepInstance.ActualDuration.HasValue)
                        jobStepHistory.ActualDuration = jobStepInstance.ActualDuration.Value;
                    else
                        jobStepHistory.ActualDuration = new TimeSpan(0, 0, 0);
                    
                    jobStepHistory.OrderIndex = (byte)jobStepInstance.OrderIndex;
                    jobStepHistory.JobStepQueueStatusId = (byte)jobStepInstance.Status;
                    jobStepHistory.HasRecordErrors = jobStepInstance.HasRecordErrors;
                    jobStepHistory.HasRuntimeErrors = jobStepInstance.HasRuntimeErrors;

                    dbContext.JobStepHistories.InsertOnSubmit(jobStepHistory);
                }

                dbContext.SubmitChanges();

                DeleteFromQueueLog(jobInstance.Id);
            }
        }

        /// <summary>
        /// Clears the in-memory queue and refreshes from the database as best as possible.
        /// 
        /// NOTE: Job configurations may have changed and new changes will be reflected within the job instances.
        /// </summary>
        /// <returns>Returns a list of queued job instances.</returns>
        public List<JobInstance> RecoverJobInstancesFromQueueLog()
        {
            if (!JobQueueManager.IsSafeForQueueLogRetrieval)
                throw new Exception("The Job Queue Manager is in an unsafe state as job statuses may change. Queue retrieval from the database is disallowed.");

            bool queueManagerWasStarted = JobQueueManager.Status == JobQueueManagerStatus.Started ? true : false;

            if (queueManagerWasStarted)
                JobQueueManager.Stop();

            // clear the in-memory queue
            JobQueueManager.DequeueAllJobs();

            List<JobInstance> queuedJobInstances = new List<JobInstance>();

            using (var dbContext = new ls.QueueManagementDataContext(_connectionString))
            {
                foreach (var jobInstanceGuid in dbContext.JobQueues.Select(d => d.JobInstanceId).ToList())
                {
                    var jobInstance = GetJobInstanceFromQueueLog(jobInstanceGuid, throwExceptionIfSyncObjectsAreDeletedOrDisabled: false);

                    if (jobInstance == null)
                    {
                        DeleteFromQueueLog(jobInstanceGuid);
                        continue;
                    }
                    else
                    {
                        switch (jobInstance.Status)
	                    {
                            case JobQueueStatus.Scheduled:
                            case JobQueueStatus.Delayed_NoAvailableThread:
                            case JobQueueStatus.InProgress:
                            case JobQueueStatus.InProgress_MaxDelayExceeded:

                                DeleteFromQueueLog(jobInstanceGuid);

                                // all job instances will start over with JobQueueStatus = Scheduled
                                JobQueueManager.QueueJob(jobInstance);

                                queuedJobInstances.Add(jobInstance);

                                break;

                            case JobQueueStatus.Completed:
                            case JobQueueStatus.Completed_WithError:
                            case JobQueueStatus.Terminated_WithError:
                            case JobQueueStatus.DidNotRun_ErrorWithinQueueRequest:

                                MoveToHistoryLog(jobInstance);

                                break;

                            default:
                                throw new EnumValueNotImplementedException<JobQueueStatus>(jobInstance.Status);
	                    }
                    }
                }
            }

            if (queueManagerWasStarted)
                JobQueueManager.Restart();

            return queuedJobInstances;
        }

        public List<JobInstance> GetJobInstancesFromQueueLog(bool throwExceptionIfJobInstanceSyncObjectsHaveChanged = false)
        {
            if (!JobQueueManager.IsSafeForQueueLogRetrieval)
                throw new Exception("The Job Queue Manager is in an unsafe state as job statuses may change. Queue retrieval from the database is disallowed.");

            List<JobInstance> jobInstancesFromDbQueue = new List<JobInstance>();

            using (var dbContext = new ls.QueueManagementDataContext(_connectionString))
            {
                foreach (var jobInstanceGuid in dbContext.JobQueues.Select(d => d.JobInstanceId).ToList())
                {
                    var jobInstanceObj = GetJobInstanceFromQueueLog(jobInstanceGuid, throwExceptionIfJobInstanceSyncObjectsHaveChanged);

                    if (jobInstanceObj != null)
                        jobInstancesFromDbQueue.Add(jobInstanceObj);
                }
            }

            return jobInstancesFromDbQueue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="jobInstanceId"></param>
        /// <returns>Returns null if a "sync object" no longer exists.</returns>
        public JobInstance GetJobInstanceFromQueueLog(Guid jobInstanceId, bool throwExceptionIfSyncObjectsAreDeletedOrDisabled = false)
        {
            if (!JobQueueManager.IsSafeForQueueLogRetrieval)
                throw new Exception("The Job Queue Manager is in an unsafe state as job statuses may change. Queue retrieval from the database is disallowed.");

            var configurator = new SyncEngineDatabaseConfigurator(_connectionString);

            using (var dbContext = new ls.QueueManagementDataContext(_connectionString))
            {
                var jobInstanceInDbQueue = dbContext.JobQueues.Where(d => d.JobInstanceId == jobInstanceId).FirstOrDefault();

                var integration = configurator.GetIntegrationByJobId(jobInstanceInDbQueue.JobId);

                if (integration == null || !integration.IsEnabled)
                    goto SyncObjectsForJobInstanceHaveChanged;

                var job = integration.Jobs.Where(d => d.Id == jobInstanceInDbQueue.JobId).FirstOrDefault();

                if (job == null || !job.IsEnabled)
                    goto SyncObjectsForJobInstanceHaveChanged;

                var sourceDataSource = job.SourceDataSources.Values.Where(d => d.Id == jobInstanceInDbQueue.SourceDataSourceId).FirstOrDefault();

                if (sourceDataSource == null)
                    goto SyncObjectsForJobInstanceHaveChanged;

                if (job.TargetDataSource.Id != jobInstanceInDbQueue.TargetDataSourceId)
                    goto SyncObjectsForJobInstanceHaveChanged;

                var jobStepInstancesInDbQueue = dbContext.JobStepQueues.Where(d => d.JobInstanceId == jobInstanceInDbQueue.JobInstanceId).ToList();

                if (jobStepInstancesInDbQueue.Count == 0)
                    goto SyncObjectsForJobInstanceHaveChanged;

                List<JobStepInstance> jobStepInstanceObjs = new List<JobStepInstance>();

                foreach (var jobStepInstanceInDbQueue in jobStepInstancesInDbQueue)
                {
                    var jobStep = job.Steps.Where(d => d.Id == jobStepInstanceInDbQueue.JobStepId).FirstOrDefault();

                    if (jobStep == null || !jobStep.IsEnabled)
                        goto SyncObjectsForJobInstanceHaveChanged;

                    var jobStepInstanceObj = new JobStepInstance(jobStepInstanceInDbQueue.JobStepInstanceId, jobStep, jobStepInstanceInDbQueue.OrderIndex);

                    jobStepInstanceObj.Status = (JobStepQueueStatus)jobStepInstanceInDbQueue.JobStepQueueStatusId;

                    jobStepInstanceObjs.Add(jobStepInstanceObj);
                }

                var filters = JobFilterHelper.ParseFromDatabaseText(jobInstanceInDbQueue.Filters);

                var invocationSourceType = jobInstanceInDbQueue.IsOnDemand == true ? JobInvocationSourceType.OnDemand : JobInvocationSourceType.Scheduled;

                var queueRequest = new JobQueueRequest(jobInstanceInDbQueue.QueueRequestId, integration, job, invocationSourceType);

                var jobInstanceObj = new JobInstance(queueRequest, jobInstanceInDbQueue.JobInstanceId, integration, job, sourceDataSource, job.TargetDataSource,
                                                           jobStepInstanceObjs, jobInstanceInDbQueue.ScheduledStartTime, 
                                                           jobInstanceInDbQueue.InvocationSource, invocationSourceType, filters.ToList());

                queueRequest.JobInstances.Add(jobInstanceObj);

                jobInstanceObj.Status = (JobQueueStatus)jobInstanceInDbQueue.JobQueueStatusId;

                return jobInstanceObj;

            SyncObjectsForJobInstanceHaveChanged:

                if (throwExceptionIfSyncObjectsAreDeletedOrDisabled)
                    throw new Exception(string.Format("Queued job '{0}' in database logger has changed sync objects.", jobInstanceInDbQueue.JobInstanceId));
                else
                    return null;
                
            }
        }
    }
}

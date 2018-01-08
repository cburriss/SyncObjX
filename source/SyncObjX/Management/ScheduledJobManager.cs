using System;
using System.Collections.Generic;
using System.Linq;
using SyncObjX.Configuration;

namespace SyncObjX.Management
{
    public class ScheduledJobManager
    {
        private ISyncEngineConfigurator _configurator;

        public ScheduledJobManager(ISyncEngineConfigurator configurator)
        {
            if (configurator == null)
                throw new Exception("Configurator can not be null.");

            _configurator = configurator;
        }
        
        public List<JobInstance> QueueScheduledJobs(bool clearExistingScheduledJobInstancesFromWaitingQueue = true)
        {
            if (clearExistingScheduledJobInstancesFromWaitingQueue)
                JobQueueManager.DequeueScheduledJobs();

            List<JobInstance> jobInstances = new List<JobInstance>();

            var integrations = _configurator.GetIntegrations().Where(d => d.IsEnabled);

            foreach (var integration in integrations)
            {
                var jobs = integration.Jobs.Where(d => d.IsEnabled);

                foreach (var job in jobs)
                {
                    if (job.Steps.Where(d => d.IsEnabled).Count() > 0 && job.SourceDataSources.Values.Count > 0)
                    {
                        var nextSoonestRunTime = GetNextScheduledRunTimeForJob(job.Id);

                        if (nextSoonestRunTime.HasValue)
                        {
                            var addedJobInstances = JobQueueManager.QueueJob(integration, job.SourceDataSources.Values, job, nextSoonestRunTime.Value, "Sync Engine Scheduler", JobInvocationSourceType.Scheduled);

                            jobInstances.AddRange(addedJobInstances);
                        }
                    }
                }
            }

            return jobInstances;
        }

        public List<JobInstance> QueueJobForNextScheduledRunTime(string integrationName, string jobName)
        {
            return QueueJobForNextScheduledRunTime(GetJobId(integrationName, jobName));
        }

        public List<JobInstance> QueueJobForNextScheduledRunTime(Guid jobId)
        {
            var nextSoonestRunTime = GetNextScheduledRunTimeForJob(jobId);

            var integration = _configurator.GetIntegrationByJobId(jobId);

            var job = _configurator.GetJobById(jobId);

            if (nextSoonestRunTime.HasValue)
                return JobQueueManager.QueueJob(integration, job.SourceDataSources.Values, job, nextSoonestRunTime.Value, "Sync Engine Scheduler", JobInvocationSourceType.Scheduled);
            else
                return new List<JobInstance>();
        }

        public DateTime? GetNextScheduledRunTimeForJob(string integrationName, string jobName)
        {
            return GetNextScheduledRunTimeForJob(GetJobId(integrationName, jobName));
        }

        public DateTime? GetNextScheduledRunTimeForJob(Guid jobId)
        {
            var jobSchedules = _configurator.GetJobSchedulesByJobId(jobId).Where(d => d.IsEnabled);

            DateTime? nextSoonestRunTime = null;

            foreach (var jobSchedule in jobSchedules)
            {
                var nextScheduledRunTime = jobSchedule.GetNextRunTime();

                if (nextScheduledRunTime.HasValue)
                {
                    if (!nextSoonestRunTime.HasValue)
                        nextSoonestRunTime = nextScheduledRunTime;
                    else if (nextScheduledRunTime < nextSoonestRunTime)
                        nextSoonestRunTime = nextScheduledRunTime;
                }
            }

            return nextSoonestRunTime;
        }

        private Guid GetJobId(string integrationName, string jobName)
        {
            if (String.IsNullOrWhiteSpace(integrationName))
                throw new Exception("Integration name is missing or empty.");

            if (String.IsNullOrWhiteSpace(jobName))
                throw new Exception("Job name is missing or empty.");

            var job = _configurator.GetJobByName(integrationName, jobName);

            if (job == null)
                throw new RecordNotFoundException(string.Format("Job with name '{0}' in integration '{1}' does not exist.", jobName, integrationName));

            return job.Id;
        }
    }
}

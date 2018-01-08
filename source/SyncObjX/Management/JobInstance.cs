using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Serialization;
using SyncObjX.Core;
using SyncObjX.Logging;
using SyncObjX.SyncObjects;

namespace SyncObjX.Management
{
    [DataContract(IsReference=true)]
    public class JobInstance
    {
        private JobQueueStatus status;

        private List<JobFilter> _filters = new List<JobFilter>();

        [DataMember]
        private List<JobStepInstance> _jobStepInstances = new List<JobStepInstance>();

        private JobStepInstance _runningJobStepInstance;

        [DataMember]
        public readonly Guid Id;

        [DataMember]
        public readonly JobQueueRequest QueueRequest;

        [DataMember]
        public JobQueueStatus Status
        {
            get
            {
                return status;
            }

            set
            {
                bool statusHasChanged = false;

                if (status != value)
                    statusHasChanged = true;

                status = value;

                if (StatusChanged != null && statusHasChanged)
                    StatusChanged(null, new JobQueueStatusChangedArgs(this));
            }
        }

        public event EventHandler<JobQueueStatusChangedArgs> StatusChanged;

        [DataMember]
        public readonly Integration Integration;

        [DataMember]
        public readonly Job Job;

        [DataMember]
        public readonly JobDataSource SourceDataSource;

        [DataMember]
        public readonly JobDataSource TargetDataSource;

        [DataMember]
        public List<JobStepInstance> JobStepInstances
        {
            get { return _jobStepInstances; }

            set { _jobStepInstances = value; }
        }

        [DataMember]
        public IEnumerable<JobFilter> Filters
        {
            get { return _filters; }

            private set { }
        }

        [DataMember]
        public JobStepInstance RunningJobStepInstance
        {
            get { return _runningJobStepInstance; }
            set { _runningJobStepInstance = value; }
        }

        [DataMember]
        public readonly string InvocationSource;

        [DataMember]
        public readonly JobInvocationSourceType InvocationSourceType;

        [DataMember]
        public readonly DateTime ScheduledStartTime;

        [DataMember]
        public bool MaxDelayedStartExceeded = false;

        [DataMember]
        public DateTime? ActualStartTime;

        [DataMember]
        public DateTime? ActualEndTime;

        [DataMember]
        public TimeSpan? TimeToStartDelay
        {
            get
            {
                if (!ActualStartTime.HasValue)
                    return null;
                else
                    return ActualStartTime - ScheduledStartTime;
            }

            private set { }
        }

        [DataMember]
        public TimeSpan? ActualDuration
        {
            get
            {
                if (!(ActualStartTime.HasValue && ActualEndTime.HasValue))
                    return null;
                else
                    return ActualEndTime - ActualStartTime;
            }

            private set { }
        }

        [DataMember]
        public bool HasRuntimeErrors = false;

        [DataMember]
        public bool HasRecordErrors = false;

        [DataMember]
        public List<Exception> Exceptions = new List<Exception>();

        /// <summary>
        /// Used to share source-side data between job steps.
        /// </summary>
        public DataSet SourceData = new DataSet();

        /// <summary>
        /// Used to share target-side data between job steps.
        /// </summary>
        public DataSet TargetData = new DataSet();

        /// <summary>
        /// Used to share 1-way field maps between job steps.
        /// </summary>
        public Dictionary<string, Dictionary<string, OneWayFieldMap>> OneWayFieldMaps_SourceToTarget;

        /// <summary>
        /// Used to share 1-way field maps between job steps.
        /// </summary>
        public Dictionary<string, Dictionary<string, OneWayFieldMap>> OneWayFieldMaps_TargetToSource;

        /// <summary>
        /// Used to share 2-way field maps between job steps.
        /// </summary>
        public Dictionary<string, Dictionary<string, TwoWayFieldMap>> TwoWayFieldMaps = new Dictionary<string,Dictionary<string,TwoWayFieldMap>>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Used to share objects across job steps.
        /// </summary>
        public Dictionary<string, object> SharedObjects = new Dictionary<string,object>(StringComparer.OrdinalIgnoreCase);

        public JobInstance(JobQueueRequest queueRequest, Integration integration, Job jobToRun, DataSource sourceDataSource, DataSource targetDataSource, List<JobStepInstance> jobStepInstances,
                                 DateTime scheduledStartTime, string invocationSource, JobInvocationSourceType invocationSourceType, List<JobFilter> filters = null)
            : this(queueRequest, Guid.Empty, integration, jobToRun, sourceDataSource, targetDataSource, jobStepInstances,
                   scheduledStartTime, invocationSource, invocationSourceType, filters) { }

        public JobInstance(JobQueueRequest queueRequest, Guid jobInstanceGuid, Integration integration, Job jobToRun, DataSource sourceDataSource, DataSource targetDataSource, List<JobStepInstance> jobStepInstances,
                                 DateTime scheduledStartTime, string invocationSource, JobInvocationSourceType invocationSourceType, List<JobFilter> filters = null)
        {
            if (integration == null)
                throw new Exception("Integration can not be null.");

            if (jobToRun == null)
                throw new Exception("Job to run can not be null.");

            if (sourceDataSource == null)
                throw new Exception("Source-side data source can not be null.");

            if (jobToRun.SourceDataSources.Where(d => d.Value.Id == sourceDataSource.Id).Count() == 0)
                throw new Exception(string.Format("'{0}' ({1}) is not associated with job '{2}' ({3}) as a source-side data source.", 
                                                    sourceDataSource.Name, sourceDataSource.Id, jobToRun.Name, jobToRun.Id));

            if (targetDataSource == null)
                throw new Exception("Target-side data source can not be null.");

            if (jobToRun.TargetDataSource.Id != targetDataSource.Id)
                throw new Exception(string.Format("'{0}' ({1}) is not associated with job '{2}' ({3}) as the target-side data source.",
                                                    targetDataSource.Name, targetDataSource.Id, jobToRun.Name, jobToRun.Id));

            if (jobStepInstances == null || jobStepInstances.Count == 0)
                throw new Exception(string.Format("At least one job step is required for queued job '{0}'.  The job will not be executed.", jobToRun.Name));

            QueueRequest = queueRequest;

            if (jobInstanceGuid == Guid.Empty)
                Id = Guid.NewGuid();
            else
                Id = jobInstanceGuid;

            Integration = integration;

            Job = jobToRun;

            if (JobQueueManager.Configurator == null)
            {
                SourceDataSource = new JobDataSource(SyncSide.Source, sourceDataSource);
                TargetDataSource = new JobDataSource(SyncSide.Target, targetDataSource);

                SyncEngineLogger.WriteToLog(LogEntryType.Warning, integration, "Update history could not be set for source and target data sources. No configurator has been set for the JobQueueManager.");
            }
            else
            {
                var runHistory = JobQueueManager.Configurator.GetJobDataSourceRunHistory(jobToRun.Id, sourceDataSource.Id, targetDataSource.Id);

                SourceDataSource = new JobDataSource(SyncSide.Source, sourceDataSource, runHistory);
                TargetDataSource = new JobDataSource(SyncSide.Target, targetDataSource, runHistory);
            }

            if (filters != null)
                _filters = filters;

            _jobStepInstances = jobStepInstances.OrderBy(d => d.OrderIndex).ToList();

            ScheduledStartTime = scheduledStartTime;

            InvocationSource = invocationSource;

            InvocationSourceType = invocationSourceType;

            Id = Guid.NewGuid();

            foreach (var jobStepInstance in _jobStepInstances)
            {
                jobStepInstance.AssociatedJobInstance = this;
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using SyncObjX.Core;
using SyncObjX.Exceptions;
using SyncObjX.Logging;
using SyncObjX.Management;

namespace SyncObjX.SyncObjects
{
    [DataContract]
    public class Job : JobDefinition
    {
        private Dictionary<string, string> _extendedProperties = new Dictionary<string, string>();

        [DataMember]
        private List<JobStep> _steps = new List<JobStep>();

        [DataMember]
        private Dictionary<string, DataSource> _sourceDataSources = new Dictionary<string, DataSource>(StringComparer.OrdinalIgnoreCase);

        [DataMember]
        private DataSource _targetDataSource;

        [DataMember]
        public Dictionary<string, string> ExtendedProperties
        {
            get { return _extendedProperties; }
            set { _extendedProperties = value; }
        } 

        [DataMember]
        public Dictionary<string, DataSource> SourceDataSources
        {
            get { return _sourceDataSources; }
            private set { }
        }

        [DataMember]
        public DataSource TargetDataSource
        {
            get { return _targetDataSource; }
            private set { }
        }

        [DataMember]
        public List<JobStep> Steps
        {
            get { return _steps; }
            private set { }
        }

        public Job(Guid jobId, string name, bool isEnabled = true)
            : base(jobId, name, isEnabled) { }

        public Job(Guid jobId, string name, DataSource sourceDataSource, DataSource targetDataSource, JobStep step, bool isEnabled = true)
            : this(jobId, name, isEnabled)
        {
            AddDataSource(sourceDataSource, SyncSide.Source);

            AddDataSource(targetDataSource, SyncSide.Target);

            AddStep(step);
        }

        public Job(Guid jobId, string name, DataSource sourceDataSource, DataSource targetDataSource, List<JobStep> steps, bool isEnabled = true)
            : this(jobId, name, isEnabled)
        {
            AddDataSource(sourceDataSource, SyncSide.Source);

            AddDataSource(targetDataSource, SyncSide.Target);

            AddSteps(steps);
        }

        public Job(Guid jobId, string name, List<DataSource> sourceDataSources, DataSource targetDataSource, List<JobStep> steps, bool isEnabled = true)
            : this(jobId, name, isEnabled)
        {
            AddDataSources(sourceDataSources, SyncSide.Source);

            AddDataSource(targetDataSource, SyncSide.Target);

            AddSteps(steps);
        }

        public Job(Guid jobId, string name, List<DataSource> sourceDataSources, DataSource targetDataSource, List<JobStep> steps, JobPriority priority, JobTerminateOnErrorType terminateOnErrorType, LoggingLevel loggingLevel, bool isEnabled = true)
            : this(jobId, name, sourceDataSources, targetDataSource, steps, isEnabled)
        {
            Priority = priority;

            TerminateOnErrorType = terminateOnErrorType;

            LoggingLevel = loggingLevel;
        }

        public void AddDataSources(IEnumerable<DataSource> dataSources, SyncSide side)
        {
            if (dataSources != null)
            {
                foreach (var dataSource in dataSources)
                {
                    AddDataSource(dataSource, side);
                }
            }
        }

        public void AddDataSource(DataSource dataSource, SyncSide side)
        {
            switch (side)
            {
                case SyncSide.Source:

                    if (dataSource == null)
                        throw new Exception("Source-side data source can not be null.");
                    else if (SourceDataSources.Keys.Where(d => d.ToLower() == dataSource.Name.ToLower()).Count() > 0)
                        throw new Exception(string.Format("A source-side data source with name '{0}' already exists.", dataSource.Name));
                    else
                        _sourceDataSources.Add(dataSource.Name, dataSource);

                    break;

                case SyncSide.Target:

                    if (dataSource == null)
                        throw new Exception("Target-side data source can not be null.");
                    else
                        _targetDataSource = dataSource;

                    break;

                default:
                    throw new EnumValueNotImplementedException<SyncSide>(side);
            }
        }

        public void AddSteps(IEnumerable<JobStep> steps)
        {
            if (steps != null)
            {
                foreach (var step in steps)
                {
                    AddStep(step);
                }
            }
        }

        public void AddStep(JobStep step)
        {
            if (step == null)
                throw new Exception("Job step can not be null.");
            else
                _steps.Add(step);
        }
    }
}
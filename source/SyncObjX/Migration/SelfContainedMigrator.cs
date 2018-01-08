using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using SyncObjX.Adapters;
using SyncObjX.Core;
using SyncObjX.Exceptions;
using SyncObjX.Logging;
using SyncObjX.Management;
using SyncObjX.SyncObjects;

namespace SyncObjX.Migration
{
    public class SelfContainedMigrator
    {
        private DataSource _sourceSideDataSource;

        private DataSource _targetSideDataSource;

        private Integration _integration;

        public LoggingLevel LoggingLevel = LoggingLevel.ErrorsWarningsInfoDebugAndTrace;

        public DataSource SourceSideDataSource
        {
            get { return _sourceSideDataSource; }
        }

        public DataSource TargetSideDataSource
        {
            get { return _targetSideDataSource; }
        }

        public SelfContainedMigrator(string migrationName, Guid migrationId, string sourceName, string targetName)
        {
            JobQueueManager.Start(1);

            _integration = new Integration(migrationId, migrationName, Assembly.GetCallingAssembly().Location, sourceName, targetName);

            var loggingPath = Path.GetDirectoryName(Assembly.GetCallingAssembly().Location) + @"\Logs";

            var textFileLogger = new TextFileLogger(loggingPath);
            SyncEngineLogger.RegisterLogger(textFileLogger, LoggingLevel.ErrorsWarningsInfoDebugAndTrace, -1);
        }

        public void AddDataSource(SyncSide syncSide, string name, Type adapterType, Dictionary<string, string> properties)
        {
            if (adapterType.BaseType != typeof(AdapterInstance))
            {
                throw new Exception(string.Format("Type '{0}' is not a valid adapter. Class must derive from base class '{1}'.",
                                                   adapterType.FullName, typeof(AdapterInstance).FullName));
            }

            var adapter = new Adapter(Guid.NewGuid(), adapterType.Name, adapterType.Assembly.Location, adapterType.FullName);

            if (syncSide == SyncSide.Source)
            {
                if (_sourceSideDataSource != null)
                    throw new Exception("A source-side data source already exists.");
                else
                {
                    _sourceSideDataSource = new DataSource(Guid.NewGuid(), name, adapter);
                    _sourceSideDataSource.ExtendedProperties = properties;
                }
            }
            else if (syncSide == SyncSide.Target)
            {
                if (_targetSideDataSource != null)
                    throw new Exception("A target-side data source already exists.");
                else
                {
                    _targetSideDataSource = new DataSource(Guid.NewGuid(), name, adapter);
                    _targetSideDataSource.ExtendedProperties = properties;
                }
            }
            else
                throw new EnumValueNotImplementedException<SyncSide>(syncSide);
        }

        public void ExecuteJob(string jobName, JobTerminateOnErrorType jobTerminateOnErrorType, Type[] jobStepTypes)
        {
            var job = new Job(Guid.NewGuid(), jobName);
            job.TerminateOnErrorType = jobTerminateOnErrorType;

            job.AddDataSource(_sourceSideDataSource, SyncSide.Source);
            job.AddDataSource(_targetSideDataSource, SyncSide.Target);

            foreach (var jobStepType in jobStepTypes)
            {
                var jobStep = new JobStep(Guid.NewGuid(), jobStepType.Name, jobStepType.FullName);

                job.AddStep(jobStep);
            }

            JobQueueManager.QueueJob(_integration, _sourceSideDataSource, job, DateTime.Now, Path.GetFileName(Assembly.GetCallingAssembly().Location), JobInvocationSourceType.OnDemand);

            while (JobQueueManager.HasWaitingJobs || JobQueueManager.HasRunningJobs)
            {
                Thread.Sleep(500);
            }
        }
    }
}

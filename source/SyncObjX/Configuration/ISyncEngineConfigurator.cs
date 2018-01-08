using System;
using System.Collections.Generic;
using System.ServiceModel;
using SyncObjX.Core;
using SyncObjX.Management;
using SyncObjX.Service;
using SyncObjX.SyncObjects;

namespace SyncObjX.Configuration
{
    [ServiceContract]
    public interface ISyncEngineConfigurator
    {
        /* Global */

        [OperationContract]
        void UpdateServiceConfig(ServiceConfig serviceConfig);

        [OperationContract]
        ServiceConfig GetServiceConfig();

        [OperationContract]
        void SyncJobPriorityConfigChanges(Dictionary<JobPriority, TimeSpan> jobPriorityConfig);

        [OperationContract]
        Dictionary<JobPriority, TimeSpan> GetJobPriorityConfig();

        /* Extended Properties */

        [OperationContract]
        void SyncExtendedPropertyChanges(SyncObjectType objectType, Guid objectId, Dictionary<string, string> extendedPropertiesToApply);

        [OperationContract]
        int AddExtendedProperty(SyncObjectType objectType, Guid objectId, string key, string value);

        [OperationContract]
        void UpdateExtendedProperty(SyncObjectType objectType, Guid objectId, string key, string value);

        [OperationContract]
        void DeleteExtendedProperty(SyncObjectType objectType, Guid objectId, string key);
            
        [OperationContract]
        void DeleteAllExtendedProperties(SyncObjectType objectType, Guid objectId);

        [OperationContract]
        string GetExtendedProperty(SyncObjectType objectType, Guid objectId, string key);

        [OperationContract]
        Dictionary<string, string> GetExtendedProperties(SyncObjectType objectType, Guid objectId);

        /* Adapter */

        [OperationContract]
        void AddAdapter(AdapterDefinition adapterDefinition);

        [OperationContract]
        [ServiceKnownType(typeof(Adapter))]
        void UpdateAdapter(AdapterDefinition adapterDefinition);

        [OperationContract]
        void DeleteAdapter(Guid adapterId);

        [OperationContract]
        bool AdapterExistsById(Guid adapterId);

        [OperationContract]
        bool AdapterExistsByName(string adapterName);

        [OperationContract]
        Adapter GetAdapterById(Guid adapterId);

        [OperationContract]
        Adapter GetAdapterByName(string adapterName);

        [OperationContract]
        List<Adapter> GetAdapters();

        /* Data Source */

        [OperationContract]
        void AddDataSource(DataSourceDefinition dataSourceDefinition);

        [OperationContract]
        [ServiceKnownType(typeof(DataSource))]
        void UpdateDataSource(DataSourceDefinition dataSourceDefinition);

        [OperationContract]
        void DeleteDataSource(Guid dataSourceId);

        [OperationContract]
        bool DataSourceExistsById(Guid dataSourceId);

        [OperationContract]
        bool DataSourceExistsByName(string dataSourceName);

        [OperationContract]
        DataSource GetDataSourceById(Guid dataSourceId);

        [OperationContract]
        DataSource GetDataSourceByName(string dataSourceName);

        [OperationContract]
        List<DataSource> GetDataSources();

        [OperationContract]
        List<DataSource> GetDataSourcesByAdapterId(Guid adapterId);

        [OperationContract]
        List<DataSource> GetDataSourcesByAdapterName(string adapterName);

        /* Integration */

        [OperationContract]
        void AddIntegration(IntegrationDefinition integrationDefinition);

        [OperationContract]
        [ServiceKnownType(typeof(Integration))]
        void UpdateIntegration(IntegrationDefinition integrationDefinition);

        [OperationContract]
        void DeleteIntegration(Guid integrationId, bool deleteHistory);

        [OperationContract]
        bool IntegrationExistsById(Guid integrationId);

        [OperationContract]
        bool IntegrationExistsByName(string integrationName);

        [OperationContract]
        [ServiceKnownType(typeof(Integration))]
        Integration GetIntegrationById(Guid integrationId);

        [OperationContract]
        Integration GetIntegrationByName(string integrationName);

        [OperationContract]
        Integration GetIntegrationByJobId(Guid jobId);

        [OperationContract]
        List<Integration> GetIntegrations();

        /* Job */

        [OperationContract]
        void AddJob(Guid integrationId, JobDefinition jobDefinition);

        [OperationContract]
        [ServiceKnownType(typeof(Job))]
        void UpdateJob(JobDefinition jobDefinition);

        [OperationContract]
        void DeleteJob(Guid jobId);

        [OperationContract]
        bool JobExistsById(Guid jobId);

        [OperationContract]
        bool JobExistsByName(string integrationName, string jobName);

        [OperationContract]
        Job GetJobById(Guid jobId);

        [OperationContract]
        Job GetJobByName(string integrationName, string jobName);

        [OperationContract]
        List<Job> GetJobsByIntegrationId(Guid integrationId);

        [OperationContract]
        List<Job> GetJobsByIntegrationName(string integrationName);

        [OperationContract]
        List<Job> GetJobsByDataSourceId(Guid dataSourceId);

        [OperationContract]
        List<Job> GetJobsByDataSourceName(string dataSourceName);

        /* Job Data Source */

        [OperationContract]
        void AddJobDataSource(Guid jobId, Guid dataSourceId, SyncSide syncSide);

        [OperationContract]
        void DeleteJobDataSource(Guid jobId, Guid dataSourceId, SyncSide syncSide);

        [OperationContract]
        void DeleteJobDataSources(Guid jobId, SyncSide syncSide);

        [OperationContract]
        void DeleteAllJobDataSources(Guid jobId);

        [OperationContract]
        bool JobDataSourceExists(Guid jobId, Guid dataSourceId, SyncSide syncSide);

        [OperationContract]
        DataSource GetJobDataSource(Guid jobId, Guid dataSourceId, SyncSide syncSide);

        [OperationContract]
        RunHistory GetJobDataSourceRunHistory(Guid jobId, Guid sourceDataSourceId, Guid targetDataSourceId);

        [OperationContract]
        List<DataSource> GetJobDataSourcesByJobId(Guid jobId, SyncSide syncSide);

        [OperationContract]
        List<DataSource> GetJobDataSourcesByJobName(string integrationName, string jobName, SyncSide syncSide);

        /* Job Schedule */

        [OperationContract]
        void AddJobSchedule(Guid jobId, JobSchedule jobSchedule);

        [OperationContract]
        void UpdateJobSchedule(JobSchedule jobSchedule);

        [OperationContract]
        void DeleteJobSchedule(Guid jobScheduleId);

        [OperationContract]
        void DeleteAllJobSchedules(Guid jobId);

        [OperationContract]
        bool JobScheduleExistsById(Guid jobScheduleId);

        [OperationContract]
        JobSchedule GetJobScheduleById(Guid jobScheduleId);

        [OperationContract]
        List<JobSchedule> GetJobSchedulesByJobId(Guid jobId);

        [OperationContract]
        List<JobSchedule> GetJobSchedulesByJobName(string integrationName, string jobName);

        /* Job Steps */

        [OperationContract]
        void AddJobStep(Guid jobId, JobStepDefinition jobStepDefinition);

        [OperationContract]
        [ServiceKnownType(typeof(JobStep))]
        void UpdateJobStep(JobStepDefinition jobStepDefinition);

        [OperationContract]
        void DeleteJobStep(Guid jobStepId);

        [OperationContract]
        void DeleteAllJobSteps(Guid jobId);

        [OperationContract]
        bool JobStepExistsById(Guid jobStepId);

        [OperationContract]
        bool JobStepExistsByName(string integrationName, string jobName, string jobStepName);

        [OperationContract]
        JobStep GetJobStepById(Guid jobStepId);

        [OperationContract]
        JobStep GetJobStepByName(string integrationName, string jobName, string jobStepName);

        [OperationContract]
        List<JobStep> GetJobStepsByJobId(Guid jobId);

        [OperationContract]
        List<JobStep> GetJobStepsByJobName(string integrationName, string jobName);

        [OperationContract]
        void MoveJobStepUp(Guid jobStepId);

        [OperationContract]
        void MoveJobStepDown(Guid jobStepId);
    }
}

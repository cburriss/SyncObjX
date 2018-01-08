using System.Collections.Generic;
using System.ServiceModel;
using SyncEngine.Core;
using SyncEngine.SyncObjects;
using SyncEngine.Management;

namespace SyncEngine.Configuration
{
    public interface ISyncEngineConfiguration
    {
        /* Extended Properties */

        [OperationContract]
        void SyncExtendedPropertyChanges(SyncObjectType objectType, int objectId, Dictionary<string, string> extendedPropertiesToApply);

        [OperationContract]
        int AddExtendedProperty(SyncObjectType objectType, int objectId, string key, string value);

        [OperationContract]
        void UpdateExtendedProperty(SyncObjectType objectType, int objectId, string key, string value);

        [OperationContract]
        void DeleteExtendedProperty(SyncObjectType objectType, int objectId, string key);
            
        [OperationContract]
        void DeleteAllExtendedProperties(SyncObjectType objectType, int objectId);

        [OperationContract]
        string GetExtendedProperty(SyncObjectType objectType, int objectId, string key);

        [OperationContract]
        Dictionary<string, string> GetExtendedProperties(SyncObjectType objectType, int objectId);

        /* Adapter */

        [OperationContract]
        int AddAdapter(AdapterDefinition adapterDefinition);

        [OperationContract]
        Adapter UpdateAdapter(AdapterDefinition adapterDefinition);

        [OperationContract]
        void DeleteAdapter(int adapterId);

        [OperationContract]
        bool AdapterExistsById(int adapterId);

        [OperationContract]
        bool AdapterExistsByName(string adapterName);

        [OperationContract]
        Adapter GetAdapterById(int adapterId);

        [OperationContract]
        Adapter GetAdapterByName(string adapterName);

        [OperationContract]
        List<Adapter> GetAdapters();

        /* Data Source */

        [OperationContract]
        int AddDataSource(DataSourceDefinition dataSourceDefinition);

        [OperationContract]
        DataSource UpdateDataSource(DataSourceDefinition dataSourceDefinition);

        [OperationContract]
        void DeleteDataSource(int dataSourceId);

        [OperationContract]
        bool DataSourceExistsById(int dataSourceId);

        [OperationContract]
        bool DataSourceExistsByName(string dataSourceName);

        [OperationContract]
        DataSource GetDataSourceById(int dataSourceId);

        [OperationContract]
        DataSource GetDataSourceByName(string dataSourceName);

        [OperationContract]
        List<DataSource> GetDataSources();

        [OperationContract]
        List<DataSource> GetDataSourcesByAdapterId(int adapterId);

        [OperationContract]
        List<DataSource> GetDataSourcesByAdapterName(string adapterName);

        /* Integration */

        [OperationContract]
        int AddIntegration(IntegrationDefinition integrationDefinition);

        [OperationContract]
        Integration UpdateIntegration(IntegrationDefinition integrationDefinition);

        [OperationContract]
        void DeleteIntegration(int integrationId);

        [OperationContract]
        bool IntegrationExistsById(int integrationId);

        [OperationContract]
        bool IntegrationExistsByName(string integrationName);

        [OperationContract]
        Integration GetIntegrationById(int integrationId);

        [OperationContract]
        Integration GetIntegrationByName(string integrationName);

        [OperationContract]
        List<Integration> GetIntegrations();

        /* Job */

        [OperationContract]
        int AddJob(int integrationId, JobDefinition jobDefinition);

        [OperationContract]
        Job UpdateJob(JobDefinition jobDefinition);

        [OperationContract]
        void DeleteJob(int jobId);

        [OperationContract]
        bool JobExistsById(int jobId);

        [OperationContract]
        bool JobExistsByName(string integrationName, string jobName);

        [OperationContract]
        Job GetJobById(int jobId);

        [OperationContract]
        Job GetJobByName(string integrationName, string jobName);

        [OperationContract]
        List<Job> GetJobsByIntegrationId(int integrationId);

        [OperationContract]
        List<Job> GetJobsByIntegrationName(string integrationName);

        [OperationContract]
        List<Job> GetJobsByDataSourceId(int dataSourceId);

        [OperationContract]
        List<Job> GetJobsByDataSourceName(string dataSourceName);

        /* Job Data Source */

        [OperationContract]
        void AddJobDataSource(int jobId, int dataSourceId, SyncSide syncSide);

        [OperationContract]
        void DeleteJobDataSource(int jobId, int dataSourceId, SyncSide syncSide);

        [OperationContract]
        void DeleteJobDataSources(int jobId, SyncSide syncSide);

        [OperationContract]
        void DeleteAllJobDataSources(int jobId);

        [OperationContract]
        bool JobDataSourceExists(int jobId, int dataSourceId, SyncSide syncSide);

        [OperationContract]
        DataSource GetJobDataSource(int jobId, int dataSourceId, SyncSide syncSide);

        [OperationContract]
        List<DataSource> GetJobDataSourcesByJobId(int jobId, SyncSide syncSide);

        [OperationContract]
        List<DataSource> GetJobDataSourcesByJobName(string integrationName, string jobName, SyncSide syncSide);

        /* Job Schedule */

        [OperationContract]
        int AddJobSchedule(int jobId, JobSchedule jobSchedule);

        [OperationContract]
        JobSchedule UpdateJobSchedule(JobSchedule jobSchedule);

        [OperationContract]
        void DeleteJobSchedule(int jobScheduleId);

        [OperationContract]
        bool JobScheduleExistsById(int jobScheduleId);

        [OperationContract]
        JobSchedule GetJobScheduleById(int jobScheduleId);

        [OperationContract]
        List<JobSchedule> GetJobSchedulesByJobId(int jobId);

        [OperationContract]
        List<JobSchedule> GetJobSchedulesByJobName(string integrationName, string jobName);

        /* Job Steps */

        [OperationContract]
        int AddJobStep(int jobId, JobStepDefinition jobStepDefinition);

        [OperationContract]
        JobStep UpdateJobStep(JobStepDefinition jobStepDefinition);

        [OperationContract]
        void DeleteJobStep(int jobStepId);

        [OperationContract]
        void DeleteAllJobSteps(int jobId);

        [OperationContract]
        bool JobStepExistsById(int jobStepId);

        [OperationContract]
        bool JobStepExistsByName(string integrationName, string jobName, string jobStepName);

        [OperationContract]
        JobStep GetJobStepById(int jobStepId);

        [OperationContract]
        JobStep GetJobStepByName(string integrationName, string jobName, string jobStepName);

        [OperationContract]
        List<JobStep> GetJobStepsByJobId(int jobId);

        [OperationContract]
        List<JobStep> GetJobStepsByJobName(string integrationName, string jobName);

        [OperationContract]
        void MoveJobStepUp(int jobStepId);

        [OperationContract]
        void MoveJobStepDown(int jobStepId);
    }
}

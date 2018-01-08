using System;
using System.Collections.Generic;
using SyncObjX.Configuration;
using SyncObjX.Core;
using SyncObjX.Logging;
using SyncObjX.Management;
using SyncObjX.SyncObjects;

/* http://msdn.microsoft.com/en-us/library/ms733069(v=vs.110).aspx */

namespace SyncObjX.Service
{
    public class ConfigurationService : ISyncEngineConfigurator
    {
        private SyncEngineDatabaseConfigurator configurator;

        private ScheduledJobManager scheduledJobManager;

        private const string WEB_SERVICE_EXCEPTION_MESSAGE = "An exception occurred within the configuration web service.";

        public ConfigurationService()
        {
            configurator = new SyncEngineDatabaseConfigurator(IntegrationWindowsService.IntegrationDbConnectionString);

            scheduledJobManager = new ScheduledJobManager(configurator);
        }

        public void UpdateServiceConfig(ServiceConfig serviceConfig)
        {
            try
            {
                configurator.UpdateServiceConfig(serviceConfig);

                IntegrationWindowsService.RefreshServiceConfig();
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        public ServiceConfig GetServiceConfig()
        {
            try
            {
                var serviceConfig = configurator.GetServiceConfig();

                // remove password if retrieved via web service
                // TODO: Add encryption for service config and adapter properties
                serviceConfig.SmtpPassword = "";

                return serviceConfig;
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }
        
        public void SyncJobPriorityConfigChanges(Dictionary<JobPriority, TimeSpan> jobPriorityConfig)
        {
            try
            {
                configurator.SyncJobPriorityConfigChanges(jobPriorityConfig);
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        public Dictionary<JobPriority, TimeSpan> GetJobPriorityConfig()
        {
            try
            {
                return configurator.GetJobPriorityConfig();
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        public void SyncExtendedPropertyChanges(SyncObjectType objectType, Guid objectId, Dictionary<string, string> extendedPropertiesToApply)
        {
            try
            {
                configurator.SyncExtendedPropertyChanges(objectType, objectId, extendedPropertiesToApply);

                scheduledJobManager.QueueScheduledJobs(clearExistingScheduledJobInstancesFromWaitingQueue: true);
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        public int AddExtendedProperty(SyncObjectType objectType, Guid objectId, string key, string value)
        {
            try
            {
                var extendedPropertyId = configurator.AddExtendedProperty(objectType, objectId, key, value);

                scheduledJobManager.QueueScheduledJobs(clearExistingScheduledJobInstancesFromWaitingQueue: true);

                return extendedPropertyId;
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        public void UpdateExtendedProperty(SyncObjectType objectType, Guid objectId, string key, string value)
        {
            try
            {
                configurator.UpdateExtendedProperty(objectType, objectId, key, value);

                scheduledJobManager.QueueScheduledJobs(clearExistingScheduledJobInstancesFromWaitingQueue: true);
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        public void DeleteExtendedProperty(SyncObjectType objectType, Guid objectId, string key)
        {
            try
            {
                configurator.DeleteExtendedProperty(objectType, objectId, key);

                scheduledJobManager.QueueScheduledJobs(clearExistingScheduledJobInstancesFromWaitingQueue: true);
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        public void DeleteAllExtendedProperties(SyncObjectType objectType, Guid objectId)
        {
            try
            {
                configurator.DeleteAllExtendedProperties(objectType, objectId);

                scheduledJobManager.QueueScheduledJobs(clearExistingScheduledJobInstancesFromWaitingQueue: true);
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        public string GetExtendedProperty(SyncObjectType objectType, Guid objectId, string key)
        {
            try
            {
                return configurator.GetExtendedProperty(objectType, objectId, key);
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        public Dictionary<string, string> GetExtendedProperties(SyncObjectType objectType, Guid objectId)
        {
            try
            {
                return configurator.GetExtendedProperties(objectType, objectId);
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        public void AddAdapter(AdapterDefinition adapterDefinition)
        {
            try
            {
                configurator.AddAdapter(adapterDefinition);

                scheduledJobManager.QueueScheduledJobs(clearExistingScheduledJobInstancesFromWaitingQueue: true);
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        public void UpdateAdapter(AdapterDefinition adapterDefinition)
        {
            try
            {
                configurator.UpdateAdapter(adapterDefinition);

                scheduledJobManager.QueueScheduledJobs(clearExistingScheduledJobInstancesFromWaitingQueue: true);
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        public void DeleteAdapter(Guid adapterId)
        {
            try
            {
                configurator.DeleteAdapter(adapterId);

                scheduledJobManager.QueueScheduledJobs(clearExistingScheduledJobInstancesFromWaitingQueue: true);
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        public bool AdapterExistsById(Guid adapterId)
        {
            try
            {
                return configurator.AdapterExistsById(adapterId);
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        public bool AdapterExistsByName(string adapterName)
        {
            try
            {
                return configurator.AdapterExistsByName(adapterName);
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        public Adapter GetAdapterById(Guid adapterId)
        {
            try
            {
                return configurator.GetAdapterById(adapterId);
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        public Adapter GetAdapterByName(string adapterName)
        {
            try
            {
                return configurator.GetAdapterByName(adapterName);
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        public List<Adapter> GetAdapters()
        {
            try
            {
                return configurator.GetAdapters();
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        public void AddDataSource(DataSourceDefinition dataSourceDefinition)
        {
            try
            {
                configurator.AddDataSource(dataSourceDefinition);

                scheduledJobManager.QueueScheduledJobs(clearExistingScheduledJobInstancesFromWaitingQueue: true);
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        public void UpdateDataSource(DataSourceDefinition dataSourceDefinition)
        {
            try
            {
                configurator.UpdateDataSource(dataSourceDefinition);

                scheduledJobManager.QueueScheduledJobs(clearExistingScheduledJobInstancesFromWaitingQueue: true);
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        public void DeleteDataSource(Guid dataSourceId)
        {
            try
            {
                configurator.DeleteDataSource(dataSourceId);

                scheduledJobManager.QueueScheduledJobs(clearExistingScheduledJobInstancesFromWaitingQueue: true);
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        public bool DataSourceExistsById(Guid dataSourceId)
        {
            try
            {
                return configurator.DataSourceExistsById(dataSourceId);
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        public bool DataSourceExistsByName(string dataSourceName)
        {
            try
            {
                return configurator.DataSourceExistsByName(dataSourceName);
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        public DataSource GetDataSourceById(Guid dataSourceId)
        {
            try
            {
                return configurator.GetDataSourceById(dataSourceId);
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        public DataSource GetDataSourceByName(string dataSourceName)
        {
            try
            {
                return configurator.GetDataSourceByName(dataSourceName);
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        public List<DataSource> GetDataSources()
        {
            try
            {
                return configurator.GetDataSources();
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        public List<DataSource> GetDataSourcesByAdapterId(Guid adapterId)
        {
            try
            {
                return configurator.GetDataSourcesByAdapterId(adapterId);
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        public List<DataSource> GetDataSourcesByAdapterName(string adapterName)
        {
            try
            {
                return configurator.GetDataSourcesByAdapterName(adapterName);
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        public void AddIntegration(IntegrationDefinition integrationDefinition)
        {
            try
            {
                configurator.AddIntegration(integrationDefinition);

                IntegrationWindowsService.RefreshIntegrationInLogger(integrationDefinition.Id);

                scheduledJobManager.QueueScheduledJobs(clearExistingScheduledJobInstancesFromWaitingQueue: true);
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        public void UpdateIntegration(IntegrationDefinition integrationDefinition)
        {
            try
            {
                configurator.UpdateIntegration(integrationDefinition);

                IntegrationWindowsService.RefreshIntegrationInLogger(integrationDefinition.Id);

                scheduledJobManager.QueueScheduledJobs(clearExistingScheduledJobInstancesFromWaitingQueue: true);
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        public void DeleteIntegration(Guid integrationId, bool deleteHistory)
        {
            try
            {
                configurator.DeleteIntegration(integrationId, false);

                IntegrationWindowsService.RefreshIntegrationInLogger(integrationId);

                scheduledJobManager.QueueScheduledJobs(clearExistingScheduledJobInstancesFromWaitingQueue: true);

                SyncEngineLogger.DeleteAllIntegrationHistory(integrationId);
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        public bool IntegrationExistsById(Guid integrationId)
        {
            try
            {
                return configurator.IntegrationExistsById(integrationId);
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        public bool IntegrationExistsByName(string integrationName)
        {
            try
            {
                return configurator.IntegrationExistsByName(integrationName);
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        public Integration GetIntegrationById(Guid integrationId)
        {
            try
            {
                return configurator.GetIntegrationById(integrationId);
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        public Integration GetIntegrationByName(string integrationName)
        {
            try
            {
                return configurator.GetIntegrationByName(integrationName);
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        public Integration GetIntegrationByJobId(Guid jobId)
        {
            try
            {
                return configurator.GetIntegrationByJobId(jobId);
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        public List<Integration> GetIntegrations()
        {
            try
            {
                return configurator.GetIntegrations();
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        public void AddJob(Guid integrationId, JobDefinition jobDefinition)
        {
            try
            {
                configurator.AddJob(integrationId, jobDefinition);

                IntegrationWindowsService.RefreshJobInLogger(jobDefinition.Id);

                scheduledJobManager.QueueScheduledJobs(clearExistingScheduledJobInstancesFromWaitingQueue: true);
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        public void UpdateJob(JobDefinition jobDefinition)
        {
            try
            {
                configurator.UpdateJob(jobDefinition);

                IntegrationWindowsService.RefreshJobInLogger(jobDefinition.Id);

                scheduledJobManager.QueueScheduledJobs(clearExistingScheduledJobInstancesFromWaitingQueue: true);
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        public void DeleteJob(Guid jobId)
        {
            try
            {
                configurator.DeleteJob(jobId);

                IntegrationWindowsService.RefreshJobInLogger(jobId);

                scheduledJobManager.QueueScheduledJobs(clearExistingScheduledJobInstancesFromWaitingQueue: true);
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        public bool JobExistsById(Guid jobId)
        {
            try
            {
                return configurator.JobExistsById(jobId);
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        public bool JobExistsByName(string integrationName, string jobName)
        {
            try
            {
                return configurator.JobExistsByName(integrationName, jobName);
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        public Job GetJobById(Guid jobId)
        {
            try
            {
                return configurator.GetJobById(jobId);
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        public Job GetJobByName(string integrationName, string jobName)
        {
            try
            {
                return configurator.GetJobByName(integrationName, jobName);
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        public List<Job> GetJobsByIntegrationId(Guid integrationId)
        {
            try
            {
                return configurator.GetJobsByIntegrationId(integrationId);
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        public List<Job> GetJobsByIntegrationName(string integrationName)
        {
            try
            {
                return configurator.GetJobsByIntegrationName(integrationName);
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        public List<Job> GetJobsByDataSourceId(Guid dataSourceId)
        {
            try
            {
                return configurator.GetJobsByDataSourceId(dataSourceId);
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        public List<Job> GetJobsByDataSourceName(string dataSourceName)
        {
            try
            {
                return configurator.GetJobsByDataSourceName(dataSourceName);
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        public void AddJobDataSource(Guid jobId, Guid dataSourceId, SyncSide syncSide)
        {
            try
            {
                configurator.AddJobDataSource(jobId, dataSourceId, syncSide);

                scheduledJobManager.QueueScheduledJobs(clearExistingScheduledJobInstancesFromWaitingQueue: true);
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        public void DeleteJobDataSource(Guid jobId, Guid dataSourceId, SyncSide syncSide)
        {
            try
            {
                configurator.DeleteJobDataSource(jobId, dataSourceId, syncSide);

                scheduledJobManager.QueueScheduledJobs(clearExistingScheduledJobInstancesFromWaitingQueue: true);
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        public void DeleteJobDataSources(Guid jobId, SyncSide syncSide)
        {
            try
            {
                configurator.DeleteJobDataSources(jobId, syncSide);

                scheduledJobManager.QueueScheduledJobs(clearExistingScheduledJobInstancesFromWaitingQueue: true);
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        public void DeleteAllJobDataSources(Guid jobId)
        {
            try
            {
                configurator.DeleteAllJobDataSources(jobId);

                scheduledJobManager.QueueScheduledJobs(clearExistingScheduledJobInstancesFromWaitingQueue: true);
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        public bool JobDataSourceExists(Guid jobId, Guid dataSourceId, SyncSide syncSide)
        {
            try
            {
                return configurator.JobDataSourceExists(jobId, dataSourceId, syncSide);
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        public DataSource GetJobDataSource(Guid jobId, Guid dataSourceId, SyncSide syncSide)
        {
            try
            {
                return configurator.GetJobDataSource(jobId, dataSourceId, syncSide);
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        public RunHistory GetJobDataSourceRunHistory(Guid jobId, Guid sourceDataSourceId, Guid targetDataSourceId)
        {
            try
            {
                return configurator.GetJobDataSourceRunHistory(jobId, sourceDataSourceId, targetDataSourceId);
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        public List<DataSource> GetJobDataSourcesByJobId(Guid jobId, SyncSide syncSide)
        {
            try
            {
                return configurator.GetJobDataSourcesByJobId(jobId, syncSide);
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        public List<DataSource> GetJobDataSourcesByJobName(string integrationName, string jobName, SyncSide syncSide)
        {
            try
            {
                return configurator.GetJobDataSourcesByJobName(integrationName, jobName, syncSide);
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        public void AddJobSchedule(Guid jobId, JobSchedule jobSchedule)
        {
            try
            {
                configurator.AddJobSchedule(jobId, jobSchedule);

                scheduledJobManager.QueueScheduledJobs(clearExistingScheduledJobInstancesFromWaitingQueue: true);
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        public void UpdateJobSchedule(JobSchedule jobSchedule)
        {
            try
            {
                configurator.UpdateJobSchedule(jobSchedule);

                scheduledJobManager.QueueScheduledJobs(clearExistingScheduledJobInstancesFromWaitingQueue: true);
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        public void DeleteJobSchedule(Guid jobScheduleId)
        {
            try
            {
                configurator.DeleteJobSchedule(jobScheduleId);

                scheduledJobManager.QueueScheduledJobs(clearExistingScheduledJobInstancesFromWaitingQueue: true);
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        public void DeleteAllJobSchedules(Guid jobId)
        {
            try
            {
                configurator.DeleteAllJobSchedules(jobId);

                scheduledJobManager.QueueScheduledJobs(clearExistingScheduledJobInstancesFromWaitingQueue: true);
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        public bool JobScheduleExistsById(Guid jobScheduleId)
        {
            try
            {
                return configurator.JobScheduleExistsById(jobScheduleId);
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        public JobSchedule GetJobScheduleById(Guid jobScheduleId)
        {
            try
            {
                return configurator.GetJobScheduleById(jobScheduleId);
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        public List<JobSchedule> GetJobSchedulesByJobId(Guid jobId)
        {
            try
            {
                return configurator.GetJobSchedulesByJobId(jobId);
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        public List<JobSchedule> GetJobSchedulesByJobName(string integrationName, string jobName)
        {
            try
            {
                return configurator.GetJobSchedulesByJobName(integrationName, jobName);
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        public void AddJobStep(Guid jobId, JobStepDefinition jobStepDefinition)
        {
            try
            {
                configurator.AddJobStep(jobId, jobStepDefinition);

                scheduledJobManager.QueueScheduledJobs(clearExistingScheduledJobInstancesFromWaitingQueue: true);
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        public void UpdateJobStep(JobStepDefinition jobStepDefinition)
        {
            try
            {
                configurator.UpdateJobStep(jobStepDefinition);

                scheduledJobManager.QueueScheduledJobs(clearExistingScheduledJobInstancesFromWaitingQueue: true);
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        public void DeleteJobStep(Guid jobStepId)
        {
            try
            {
                configurator.DeleteJobStep(jobStepId);

                scheduledJobManager.QueueScheduledJobs(clearExistingScheduledJobInstancesFromWaitingQueue: true);
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        public void DeleteAllJobSteps(Guid jobId)
        {
            try
            {
                configurator.DeleteAllJobSteps(jobId);

                scheduledJobManager.QueueScheduledJobs(clearExistingScheduledJobInstancesFromWaitingQueue: true);
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        public bool JobStepExistsById(Guid jobStepId)
        {
            try
            {
                return configurator.JobStepExistsById(jobStepId);
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        public bool JobStepExistsByName(string integrationName, string jobName, string jobStepName)
        {
            try
            {
                return configurator.JobStepExistsByName(integrationName, jobName, jobStepName);
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        public JobStep GetJobStepById(Guid jobStepId)
        {
            try
            {
                return configurator.GetJobStepById(jobStepId);
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        public JobStep GetJobStepByName(string integrationName, string jobName, string jobStepName)
        {
            try
            {
                return configurator.GetJobStepByName(integrationName, jobName, jobStepName);
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        public List<JobStep> GetJobStepsByJobId(Guid jobId)
        {
            try
            {
                return configurator.GetJobStepsByJobId(jobId);
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        public List<JobStep> GetJobStepsByJobName(string integrationName, string jobName)
        {
            try
            {
                return configurator.GetJobStepsByJobName(integrationName, jobName);
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        public void MoveJobStepUp(Guid jobStepId)
        {
            try
            {
                configurator.MoveJobStepUp(jobStepId);

                scheduledJobManager.QueueScheduledJobs(clearExistingScheduledJobInstancesFromWaitingQueue: true);
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }

        public void MoveJobStepDown(Guid jobStepId)
        {
            try
            {
                configurator.MoveJobStepDown(jobStepId);

                scheduledJobManager.QueueScheduledJobs(clearExistingScheduledJobInstancesFromWaitingQueue: true);
            }
            catch (Exception ex)
            {
                SyncEngineLogger.WriteExceptionToLog(ex, WEB_SERVICE_EXCEPTION_MESSAGE);

                throw;
            }
        }
    }
}
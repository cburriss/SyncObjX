using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Data.SqlClient;
using System.Linq;
using SyncObjX.Core;
using SyncObjX.Exceptions;
using SyncObjX.Logging;
using SyncObjX.Management;
using SyncObjX.Service;
using ls = SyncObjX.Configuration.LinqToSql;
using so = SyncObjX.SyncObjects;

namespace SyncObjX.Configuration
{
    public class SyncEngineDatabaseConfigurator : ISyncEngineConfigurator
    {
        private string _connectionString;

        public SyncEngineDatabaseConfigurator(string connectionString)
        {
            if (String.IsNullOrWhiteSpace(connectionString))
                throw new Exception("Connection string is null or empty.");

            // test connection to configuration database; if a failure occurs, an exception will be thrown
            SqlConnection conn = new SqlConnection(connectionString);
            conn.Open();
            conn.Close();

            _connectionString = connectionString;
        }

        #region Global

        public void UpdateServiceConfig(ServiceConfig serviceConfig)
        {
            var dbContext = new ls.ConfigurationDataContext(_connectionString);

            var now = DateTime.Now;

            var serviceConfigFromLinq = dbContext.ServiceConfigs.ToDictionary(k => k.PropertyKey, v => v, StringComparer.OrdinalIgnoreCase);

            // Interval in Seconds
            if (serviceConfigFromLinq.ContainsKey("IntervalInSeconds"))
            {
                serviceConfigFromLinq["IntervalInSeconds"].PropertyValue = serviceConfig.IntervalInSeconds.ToString();
                serviceConfigFromLinq["IntervalInSeconds"].UpdatedDate = now;
            }
            else
            {
                var propertyToAdd = new ls.ServiceConfig();
                propertyToAdd.PropertyKey = "IntervalInSeconds";
                propertyToAdd.PropertyValue = serviceConfig.IntervalInSeconds.ToString();
                propertyToAdd.CreatedDate = now;
                propertyToAdd.UpdatedDate = now;

                dbContext.ServiceConfigs.InsertOnSubmit(propertyToAdd);
            }

            // Logging Level
            if (serviceConfigFromLinq.ContainsKey("LoggingLevel"))
            {
                serviceConfigFromLinq["LoggingLevel"].PropertyValue = ((int)serviceConfig.LoggingLevel).ToString();
                serviceConfigFromLinq["LoggingLevel"].UpdatedDate = now;
            }
            else
            {
                var propertyToAdd = new ls.ServiceConfig();
                propertyToAdd.PropertyKey = "LoggingLevel";
                propertyToAdd.PropertyValue = ((int)serviceConfig.LoggingLevel).ToString();
                propertyToAdd.CreatedDate = now;
                propertyToAdd.UpdatedDate = now;

                dbContext.ServiceConfigs.InsertOnSubmit(propertyToAdd);
            }

            // Log to database
            if (serviceConfigFromLinq.ContainsKey("LogToDatabase"))
            {
                serviceConfigFromLinq["LogToDatabase"].PropertyValue = serviceConfig.LogToDatabase.ToString();
                serviceConfigFromLinq["LogToDatabase"].UpdatedDate = now;
            }
            else
            {
                var propertyToAdd = new ls.ServiceConfig();
                propertyToAdd.PropertyKey = "LogToDatabase";
                propertyToAdd.PropertyValue = serviceConfig.LogToDatabase.ToString();
                propertyToAdd.CreatedDate = now;
                propertyToAdd.UpdatedDate = now;

                dbContext.ServiceConfigs.InsertOnSubmit(propertyToAdd);
            }

            // Days of database logging history
            if (serviceConfigFromLinq.ContainsKey("DaysOfDatabaseLoggingHistory"))
            {
                serviceConfigFromLinq["DaysOfDatabaseLoggingHistory"].PropertyValue = serviceConfig.DaysOfDatabaseLoggingHistory.ToString();
                serviceConfigFromLinq["DaysOfDatabaseLoggingHistory"].UpdatedDate = now;
            }
            else
            {
                var propertyToAdd = new ls.ServiceConfig();
                propertyToAdd.PropertyKey = "DaysOfDatabaseLoggingHistory";
                propertyToAdd.PropertyValue = serviceConfig.DaysOfDatabaseLoggingHistory.ToString();
                propertyToAdd.CreatedDate = now;
                propertyToAdd.UpdatedDate = now;

                dbContext.ServiceConfigs.InsertOnSubmit(propertyToAdd);
            }

            // Log to file
            if (serviceConfigFromLinq.ContainsKey("LogToFile"))
            {
                serviceConfigFromLinq["LogToFile"].PropertyValue = serviceConfig.LogToFile.ToString();
                serviceConfigFromLinq["LogToFile"].UpdatedDate = now;
            }
            else
            {
                var propertyToAdd = new ls.ServiceConfig();
                propertyToAdd.PropertyKey = "LogToFile";
                propertyToAdd.PropertyValue = serviceConfig.LogToFile.ToString();
                propertyToAdd.CreatedDate = now;
                propertyToAdd.UpdatedDate = now;

                dbContext.ServiceConfigs.InsertOnSubmit(propertyToAdd);
            }

            // Days of file logging history
            if (serviceConfigFromLinq.ContainsKey("DaysOfFileLoggingHistory"))
            {
                serviceConfigFromLinq["DaysOfFileLoggingHistory"].PropertyValue = serviceConfig.DaysOfFileLoggingHistory.ToString();
                serviceConfigFromLinq["DaysOfFileLoggingHistory"].UpdatedDate = now;
            }
            else
            {
                var propertyToAdd = new ls.ServiceConfig();
                propertyToAdd.PropertyKey = "DaysOfFileLoggingHistory";
                propertyToAdd.PropertyValue = serviceConfig.DaysOfFileLoggingHistory.ToString();
                propertyToAdd.CreatedDate = now;
                propertyToAdd.UpdatedDate = now;

                dbContext.ServiceConfigs.InsertOnSubmit(propertyToAdd);
            }

            // Max log file size in MB
            if (serviceConfigFromLinq.ContainsKey("MaxLogFileSizeInMB"))
            {
                serviceConfigFromLinq["MaxLogFileSizeInMB"].PropertyValue = serviceConfig.MaxLogFileSizeInMB.ToString();
                serviceConfigFromLinq["MaxLogFileSizeInMB"].UpdatedDate = now;
            }
            else
            {
                var propertyToAdd = new ls.ServiceConfig();
                propertyToAdd.PropertyKey = "MaxLogFileSizeInMB";
                propertyToAdd.PropertyValue = serviceConfig.MaxLogFileSizeInMB.ToString();
                propertyToAdd.CreatedDate = now;
                propertyToAdd.UpdatedDate = now;

                dbContext.ServiceConfigs.InsertOnSubmit(propertyToAdd);
            }

            // Enable Mail Notifications
            if (serviceConfigFromLinq.ContainsKey("EnableMailNotifications"))
            {
                serviceConfigFromLinq["EnableMailNotifications"].PropertyValue = serviceConfig.EnableMailNotifications.ToString();
                serviceConfigFromLinq["EnableMailNotifications"].UpdatedDate = now;
            }
            else
            {
                var propertyToAdd = new ls.ServiceConfig();
                propertyToAdd.PropertyKey = "EnableMailNotifications";
                propertyToAdd.PropertyValue = serviceConfig.EnableMailNotifications.ToString();
                propertyToAdd.CreatedDate = now;
                propertyToAdd.UpdatedDate = now;

                dbContext.ServiceConfigs.InsertOnSubmit(propertyToAdd);
            }

            // 'From' Email Address
            if (serviceConfigFromLinq.ContainsKey("FromEmailAddress"))
            {
                serviceConfigFromLinq["FromEmailAddress"].PropertyValue = serviceConfig.FromEmailAddress.ToString();
                serviceConfigFromLinq["FromEmailAddress"].UpdatedDate = now;
            }
            else
            {
                var propertyToAdd = new ls.ServiceConfig();
                propertyToAdd.PropertyKey = "FromEmailAddress";
                propertyToAdd.PropertyValue = serviceConfig.FromEmailAddress.ToString();
                propertyToAdd.CreatedDate = now;
                propertyToAdd.UpdatedDate = now;

                dbContext.ServiceConfigs.InsertOnSubmit(propertyToAdd);
            }

            // 'To' Email Addresses
            if (serviceConfigFromLinq.ContainsKey("ToEmailAddresses"))
            {
                serviceConfigFromLinq["ToEmailAddresses"].PropertyValue = string.Join(";", serviceConfig.ToEmailAddresses.ToArray());
                serviceConfigFromLinq["ToEmailAddresses"].UpdatedDate = now;
            }
            else
            {
                var propertyToAdd = new ls.ServiceConfig();
                propertyToAdd.PropertyKey = "ToEmailAddresses";
                propertyToAdd.PropertyValue = string.Join(";", serviceConfig.ToEmailAddresses.ToArray());
                propertyToAdd.CreatedDate = now;
                propertyToAdd.UpdatedDate = now;

                dbContext.ServiceConfigs.InsertOnSubmit(propertyToAdd);
            }

            // SMTP Host
            if (serviceConfigFromLinq.ContainsKey("SmtpHost"))
            {
                serviceConfigFromLinq["SmtpHost"].PropertyValue = serviceConfig.SmtpHost.ToString();
                serviceConfigFromLinq["SmtpHost"].UpdatedDate = now;
            }
            else
            {
                var propertyToAdd = new ls.ServiceConfig();
                propertyToAdd.PropertyKey = "SmtpHost";
                propertyToAdd.PropertyValue = serviceConfig.SmtpHost.ToString();
                propertyToAdd.CreatedDate = now;
                propertyToAdd.UpdatedDate = now;

                dbContext.ServiceConfigs.InsertOnSubmit(propertyToAdd);
            }

            // SMTP Port
            if (serviceConfigFromLinq.ContainsKey("SmtpPort"))
            {
                serviceConfigFromLinq["SmtpPort"].PropertyValue = serviceConfig.SmtpPort.ToString();
                serviceConfigFromLinq["SmtpPort"].UpdatedDate = now;
            }
            else
            {
                var propertyToAdd = new ls.ServiceConfig();
                propertyToAdd.PropertyKey = "SmtpPort";
                propertyToAdd.PropertyValue = serviceConfig.SmtpPort.ToString();
                propertyToAdd.CreatedDate = now;
                propertyToAdd.UpdatedDate = now;

                dbContext.ServiceConfigs.InsertOnSubmit(propertyToAdd);
            }

            // SMTP Requires SSL
            if (serviceConfigFromLinq.ContainsKey("SmtpRequiresSsl"))
            {
                serviceConfigFromLinq["SmtpRequiresSsl"].PropertyValue = serviceConfig.SmtpRequiresSsl.ToString();
                serviceConfigFromLinq["SmtpRequiresSsl"].UpdatedDate = now;
            }
            else
            {
                var propertyToAdd = new ls.ServiceConfig();
                propertyToAdd.PropertyKey = "SmtpRequiresSsl";
                propertyToAdd.PropertyValue = serviceConfig.SmtpRequiresSsl.ToString();
                propertyToAdd.CreatedDate = now;
                propertyToAdd.UpdatedDate = now;

                dbContext.ServiceConfigs.InsertOnSubmit(propertyToAdd);
            }

            // SMTP Username
            if (serviceConfigFromLinq.ContainsKey("SmtpUsername"))
            {
                serviceConfigFromLinq["SmtpUsername"].PropertyValue = serviceConfig.SmtpUsername.ToString();
                serviceConfigFromLinq["SmtpUsername"].UpdatedDate = now;
            }
            else
            {
                var propertyToAdd = new ls.ServiceConfig();
                propertyToAdd.PropertyKey = "SmtpUsername";
                propertyToAdd.PropertyValue = serviceConfig.SmtpUsername.ToString();
                propertyToAdd.CreatedDate = now;
                propertyToAdd.UpdatedDate = now;

                dbContext.ServiceConfigs.InsertOnSubmit(propertyToAdd);
            }

            // SMTP Password
            if (serviceConfigFromLinq.ContainsKey("SmtpPassword"))
            {
                serviceConfigFromLinq["SmtpPassword"].PropertyValue = serviceConfig.SmtpPassword.ToString();
                serviceConfigFromLinq["SmtpPassword"].UpdatedDate = now;
            }
            else
            {
                var propertyToAdd = new ls.ServiceConfig();
                propertyToAdd.PropertyKey = "SmtpPassword";
                propertyToAdd.PropertyValue = serviceConfig.SmtpPassword.ToString();
                propertyToAdd.CreatedDate = now;
                propertyToAdd.UpdatedDate = now;

                dbContext.ServiceConfigs.InsertOnSubmit(propertyToAdd);
            }

            dbContext.SubmitChanges();
        }

        public ServiceConfig GetServiceConfig()
        {
            var dbContext = new ls.ConfigurationDataContext(_connectionString);

            // get config with defaults
            var serviceConfig = new ServiceConfig();

            var supportedProperties = ServiceConfigProperty.GetSupportedProperties();

            var serviceConfigFromLinq = dbContext.ServiceConfigs;
            dbContext.Refresh(RefreshMode.KeepChanges, serviceConfigFromLinq);

            var serviceConfigProperties = serviceConfigFromLinq.ToDictionary(k => k.PropertyKey, v => v.PropertyValue, StringComparer.OrdinalIgnoreCase);

            if (serviceConfigProperties.ContainsKey("IntervalInSeconds"))
                serviceConfig.IntervalInSeconds = int.Parse(serviceConfigProperties["IntervalInSeconds"]);

            if (serviceConfigProperties.ContainsKey("LoggingLevel"))
                serviceConfig.LoggingLevel = SupportedPropertyOption.GetEnumValueForOption<LoggingLevel>(supportedProperties["LoggingLevel"], serviceConfigProperties["LoggingLevel"]);

            if (serviceConfigProperties.ContainsKey("LogToDatabase"))
                serviceConfig.LogToDatabase = bool.Parse(serviceConfigProperties["LogToDatabase"]);

            if (serviceConfigProperties.ContainsKey("DaysOfDatabaseLoggingHistory"))
                serviceConfig.DaysOfDatabaseLoggingHistory = int.Parse(serviceConfigProperties["DaysOfDatabaseLoggingHistory"]);

            if (serviceConfigProperties.ContainsKey("LogToFile"))
                serviceConfig.LogToFile = bool.Parse(serviceConfigProperties["LogToFile"]);

            if (serviceConfigProperties.ContainsKey("DaysOfFileLoggingHistory"))
                serviceConfig.DaysOfFileLoggingHistory = int.Parse(serviceConfigProperties["DaysOfFileLoggingHistory"]);

            if (serviceConfigProperties.ContainsKey("MaxLogFileSizeInMB"))
                serviceConfig.MaxLogFileSizeInMB = int.Parse(serviceConfigProperties["MaxLogFileSizeInMB"]);

            if (serviceConfigProperties.ContainsKey("EnableMailNotifications"))
                serviceConfig.EnableMailNotifications = bool.Parse(serviceConfigProperties["EnableMailNotifications"]);
                        
            if (serviceConfigProperties.ContainsKey("FromEmailAddress"))
                serviceConfig.FromEmailAddress = serviceConfigProperties["FromEmailAddress"];

            if (serviceConfigProperties.ContainsKey("ToEmailAddresses"))
                serviceConfig.ToEmailAddresses = (serviceConfigProperties["ToEmailAddresses"] ?? "").Split(';');

            if (serviceConfigProperties.ContainsKey("SmtpHost"))
                serviceConfig.SmtpHost = serviceConfigProperties["SmtpHost"];

            if (serviceConfigProperties.ContainsKey("SmtpPort"))
                serviceConfig.SmtpPort = int.Parse(serviceConfigProperties["SmtpPort"]);

            if (serviceConfigProperties.ContainsKey("SmtpRequiresSsl"))
                serviceConfig.SmtpRequiresSsl = bool.Parse(serviceConfigProperties["SmtpRequiresSsl"]);
            
            if (serviceConfigProperties.ContainsKey("SmtpUsername"))
                serviceConfig.SmtpUsername = serviceConfigProperties["SmtpUsername"];

            if (serviceConfigProperties.ContainsKey("SmtpPassword"))
                serviceConfig.SmtpPassword = serviceConfigProperties["SmtpPassword"];

            return serviceConfig;
        }

        public void SyncJobPriorityConfigChanges(Dictionary<JobPriority, TimeSpan> jobPriorityConfigChangesToApply)
        {
            var dbContext = new ls.ConfigurationDataContext(_connectionString);

            if (jobPriorityConfigChangesToApply == null || jobPriorityConfigChangesToApply.Count == 0)
                return;

            Dictionary<JobPriority, ls.JobPriority> jobPriorityConfigFromLinq = dbContext.JobPriorities
                                                                                            .Where(d => d.JobPriorityId > 0)
                                                                                            .ToDictionary(d => (JobPriority)d.JobPriorityId, d => d);

            foreach (var jobPriority in jobPriorityConfigChangesToApply)
            {
                if (jobPriority.Key == JobPriority.AlwaysRunOtherJobsFirst)
                    continue;

                if (jobPriority.Value.TotalSeconds < 0)
                    throw new Exception(string.Format("Job priority '{0}' must be greater than or equal to 0.", Enum.GetName(typeof(JobPriority), jobPriority.Key)));

                jobPriorityConfigFromLinq[jobPriority.Key].MaxDelayedStartInSeconds = (int)jobPriority.Value.TotalSeconds;
            }

            dbContext.SubmitChanges();
        }

        public Dictionary<JobPriority, TimeSpan> GetJobPriorityConfig()
        {
            var dbContext = new ls.ConfigurationDataContext(_connectionString);

            return dbContext.JobPriorities
                                .Where(d => d.JobPriorityId > 0)
                                .ToDictionary(d => (JobPriority)d.JobPriorityId, d => d.MaxDelayedStartInSeconds == null ? new TimeSpan(0, 0, 0) : new TimeSpan(0, 0, d.MaxDelayedStartInSeconds.Value));
        }

        #endregion

        #region Extended Properties

        public void SyncExtendedPropertyChanges(so.SyncObjectType objectType, Guid objectId, Dictionary<string, string> extendedPropertiesToApply)
        {
            var dbContext = new ls.ConfigurationDataContext(_connectionString);

            if (extendedPropertiesToApply == null)
                extendedPropertiesToApply = new Dictionary<string, string>();

            var existingExtendedPropertyKeys = GetExtendedPropertiesFromLinq(objectType, objectId).Select(d => d.Key.ToLower()).ToList();

            /* Insert */

            var extendedPropertyKeysToInsert = extendedPropertiesToApply.Keys.Where(d => !existingExtendedPropertyKeys.Contains(d.ToLower()));

            foreach (var key in extendedPropertyKeysToInsert)
            {
                AddExtendedProperty(objectType, objectId, key, extendedPropertiesToApply[key]);
            }

            /* Update */

            var extendedPropertyKeysToUpdate = extendedPropertiesToApply.Where(d => !extendedPropertyKeysToInsert.Contains(d.Key.ToLower())).Select(d => d.Key);

            foreach (var key in extendedPropertyKeysToUpdate)
            {
                UpdateExtendedProperty(objectType, objectId, key, extendedPropertiesToApply[key]);
            }

            /* Delete */
            
            var extendedPropertyKeysToDelete = existingExtendedPropertyKeys.Where(d => !extendedPropertiesToApply.Keys.Select(e => e.ToLower()).Contains(d.ToLower())).ToList();

            foreach (var key in extendedPropertyKeysToDelete)
            {
                DeleteExtendedProperty(objectType, objectId, key);
            }
        }

        public int AddExtendedProperty(so.SyncObjectType objectType, Guid objectId, string key, string value)
        {
            var dbContext = new ls.ConfigurationDataContext(_connectionString);

            var extendedPropertyWithSameKey = GetExtendedPropertyFromLinq(objectType, objectId, key);

            if (extendedPropertyWithSameKey != null)
            {
                throw new Exception(string.Format("Extended property with key '{0}' already exists for sync object '{1}' with ID '{2}'.",
                                                    key, Enum.GetName(typeof(so.SyncObjectType), objectType), objectId));
            }

            var now = DateTime.Now;

            var lsExtendedProperty = new ls.ExtendedProperty();

            lsExtendedProperty.CreatedDate = now;
            lsExtendedProperty.UpdatedDate = now;
            lsExtendedProperty.SyncObjectTypeId = (byte)objectType;
            lsExtendedProperty.SyncObjectId = objectId;
            lsExtendedProperty.Key = key;
            lsExtendedProperty.Value = value;

            dbContext.ExtendedProperties.InsertOnSubmit(lsExtendedProperty);

            dbContext.SubmitChanges();

            return lsExtendedProperty.ExtendedPropertyId;
        }

        public void UpdateExtendedProperty(so.SyncObjectType objectType, Guid objectId, string key, string value)
        {
            var dbContext = new ls.ConfigurationDataContext(_connectionString);

            var extendedPropertyFromLinq = dbContext.ExtendedProperties
                                                        .Where(d => !d.IsDeleted
                                                                    && d.SyncObjectTypeId == (byte)objectType
                                                                    && d.SyncObjectId == objectId
                                                                    && d.Key.ToLower() == key.ToLower())
                                                        .FirstOrDefault();

            if (extendedPropertyFromLinq == null)
            {
                throw new RecordNotFoundException(string.Format("Record could not be updated. Extended property with key '{0}' was not found for sync object '{1}' with ID '{2}'.",
                                                    key, Enum.GetName(typeof(so.SyncObjectType), objectType), objectId));
            }
            else
            {
                extendedPropertyFromLinq.UpdatedDate = DateTime.Now;
                extendedPropertyFromLinq.Value = value;

                var changes = dbContext.GetChangeSet();
                if (changes.Updates.Contains(extendedPropertyFromLinq))
                {
                    dbContext.SubmitChanges();
                }
            }
        }

        public void DeleteExtendedProperty(so.SyncObjectType objectType, Guid objectId, string key)
        {
            var dbContext = new ls.ConfigurationDataContext(_connectionString);

            var extendedPropertyFromLinq = GetExtendedPropertyFromLinq(objectType, objectId, key);

            if (extendedPropertyFromLinq == null)
            {
                throw new RecordNotFoundException(string.Format("Record could not be deleted. Extended property with key '{0}' was not found for sync object '{1}' with ID '{2}'.",
                                                    key, Enum.GetName(typeof(so.SyncObjectType), objectType), objectId));
            }
            else
            {
                extendedPropertyFromLinq.UpdatedDate = DateTime.Now;
                extendedPropertyFromLinq.IsDeleted = true;

                dbContext.SubmitChanges();
            }
        }

        public void DeleteAllExtendedProperties(so.SyncObjectType objectType, Guid objectId)
        {
            var extendedPropertiesFromLinq = GetExtendedPropertiesFromLinq(objectType, objectId);

            foreach (var extendedPropertyFromLinq in extendedPropertiesFromLinq)
            {
                DeleteExtendedProperty(objectType, objectId, extendedPropertyFromLinq.Key);
            }
        }

        public string GetExtendedProperty(so.SyncObjectType objectType, Guid objectId, string key)
        {
            if (String.IsNullOrWhiteSpace(key))
                throw new Exception("Key is missing or empty.");

            var extendedPropertyFromLinq = GetExtendedPropertyFromLinq(objectType, objectId, key);

            if (extendedPropertyFromLinq == null)
            {
                throw new RecordNotFoundException(string.Format("Extended property with key '{0}' does not exist for sync object '{1}' with ID '{2}'.",
                                                    key, Enum.GetName(typeof(so.SyncObjectType), objectType), objectId));
            }
            else
            {
                return extendedPropertyFromLinq.Value;
            }
        }

        public Dictionary<string, string> GetExtendedProperties(so.SyncObjectType objectType, Guid objectId)
        {
            Dictionary<string, string> extendedProperties = new Dictionary<string, string>();

            var extendedPropertiesFromLinq = GetExtendedPropertiesFromLinq(objectType, objectId);

            foreach (var extendedPropertyFromLinq in extendedPropertiesFromLinq)
            {
                extendedProperties.Add(extendedPropertyFromLinq.Key, extendedPropertyFromLinq.Value);
            }

            return extendedProperties;
        }

        private ls.ExtendedProperty GetExtendedPropertyFromLinq(so.SyncObjectType objectType, Guid objectId, string key)
        {
            if (String.IsNullOrWhiteSpace(key))
                throw new Exception("Key is missing or empty.");

            var extendedProperties = GetExtendedPropertiesFromLinq(objectType, objectId);

            return extendedProperties.Where(d => d.Key.ToLower() == key.ToLower()).FirstOrDefault();
        }

        private IQueryable<ls.ExtendedProperty> GetExtendedPropertiesFromLinq(so.SyncObjectType objectType, Guid objectId)
        {
            var dbContext = new ls.ConfigurationDataContext(_connectionString);

            return dbContext.ExtendedProperties
                        .Where(d => !d.IsDeleted
                                    && d.SyncObjectTypeId == (byte)objectType
                                    && d.SyncObjectId == objectId)
                        .Select(d => d);
        }

        private void ThrowExceptionIfObjectDoesntExist(so.SyncObjectType objectType, Guid objectId)
        {
            switch (objectType)
            {
                case so.SyncObjectType.Adapter:
                    if (!AdapterExistsById(objectId))
                        throw new RecordNotFoundException(string.Format("Adapter with ID '{0}' does not exist.", objectId));
                    break;

                case so.SyncObjectType.DataSource:
                    if (!DataSourceExistsById(objectId))
                        throw new RecordNotFoundException(string.Format("Data Source with ID '{0}' does not exist.", objectId));
                    break;

                case so.SyncObjectType.Integration:
                    if (!IntegrationExistsById(objectId))
                        throw new RecordNotFoundException(string.Format("Integration with ID '{0}' does not exist.", objectId));
                    break;

                case so.SyncObjectType.Job:
                    if (!JobExistsById(objectId))
                        throw new RecordNotFoundException(string.Format("Job with ID '{0}' does not exist.", objectId));
                    break;

                case so.SyncObjectType.Step:
                    if (!JobStepExistsById(objectId))
                        throw new RecordNotFoundException(string.Format("Job step with ID '{0}' does not exist.", objectId));
                    break;

                default:
                    throw new EnumValueNotImplementedException<so.SyncObjectType>(objectType);
            }
        }

        #endregion

        #region Adapter

        public void AddAdapter(so.AdapterDefinition adapterDefinition)
        {
            if (adapterDefinition == null)
                throw new Exception("Adapter definition can not be null.");

            if (AdapterExistsByName(adapterDefinition.Name))
                throw new Exception(string.Format("Adapter with name '{0}' already exists.", adapterDefinition.Name));

            var dbContext = new ls.ConfigurationDataContext(_connectionString);

            var now = DateTime.Now;

            var lsAdapter = new ls.Adapter();

            lsAdapter.AdapterId = adapterDefinition.Id;
            lsAdapter.CreatedDate = now;
            lsAdapter.UpdatedDate = now;

            lsAdapter.Name = adapterDefinition.Name;
            lsAdapter.Description = adapterDefinition.Description;
            lsAdapter.DllDirectory = adapterDefinition.DllDirectory;
            lsAdapter.DllFilename = adapterDefinition.DllFilename;
            lsAdapter.FullyQualifiedName = adapterDefinition.FullyQualifiedName;

            dbContext.Adapters.InsertOnSubmit(lsAdapter);

            dbContext.SubmitChanges();
        }

        public void UpdateAdapter(so.AdapterDefinition adapterDefinition)
        {
            if (adapterDefinition == null)
                throw new Exception("Adapter definition can not be null.");

            var dbContext = new ls.ConfigurationDataContext(_connectionString);

            var adapterFromLinq = dbContext.Adapters
                                        .Where(d => !d.IsDeleted && d.AdapterId == adapterDefinition.Id)
                                        .FirstOrDefault();

            if (adapterFromLinq == null)
                throw new RecordNotFoundException(string.Format("Adapter could not be updated because ID '{0}' does not exist.", adapterDefinition.Id));

            if (dbContext.Adapters
                        .Where(d => !d.IsDeleted
                                    && d.Name.ToLower() == adapterDefinition.Name.ToLower()
                                    && d.AdapterId != adapterDefinition.Id)
                        .Count() > 0)
            {
                throw new Exception(string.Format("Adapter with name '{0}' already exists.", adapterDefinition.Name));
            }

            adapterFromLinq.UpdatedDate = DateTime.Now;
            adapterFromLinq.Name = adapterDefinition.Name;
            adapterFromLinq.Description = adapterDefinition.Description;
            adapterFromLinq.DllDirectory = adapterDefinition.DllDirectory;
            adapterFromLinq.DllFilename = adapterDefinition.DllFilename;
            adapterFromLinq.FullyQualifiedName = adapterDefinition.FullyQualifiedName;

            dbContext.SubmitChanges();
        }

        public void DeleteAdapter(Guid adapterId)
        {
            var dbContext = new ls.ConfigurationDataContext(_connectionString);

            var adapterFromLinq = dbContext.Adapters
                                        .Where(d => !d.IsDeleted && d.AdapterId == adapterId)
                                        .FirstOrDefault();

            if (adapterFromLinq == null)
                throw new RecordNotFoundException(string.Format("Adapter with ID '{0}' does not exist.", adapterId));

            var dataSourcesUsingAdapter = GetDataSourcesByAdapterId(adapterId);

            if (dataSourcesUsingAdapter != null && dataSourcesUsingAdapter.Count > 0)
                throw new AssociatedRecordsExistException(string.Format("Adapter with name '{0}' can not be deleted because one or more data sources are associated.", adapterFromLinq.Name));

            adapterFromLinq.UpdatedDate = DateTime.Now;
            adapterFromLinq.IsDeleted = true;

            dbContext.SubmitChanges();

            DeleteAllExtendedProperties(so.SyncObjectType.Adapter, adapterId);
        }

        public bool AdapterExistsById(Guid adapterId)
        {
            var dbContext = new ls.ConfigurationDataContext(_connectionString);

            return dbContext.Adapters
                            .Where(d => !d.IsDeleted && d.AdapterId == adapterId)
                            .Count() > 0;
        }

        public bool AdapterExistsByName(string adapterName)
        {
            if (String.IsNullOrWhiteSpace(adapterName))
                throw new Exception("Adapter name is missing or empty.");

            var dbContext = new ls.ConfigurationDataContext(_connectionString);

            return dbContext.Adapters
                            .Where(d => !d.IsDeleted && d.Name.ToLower() == adapterName.ToLower())
                            .Count() > 0;
        }

        public so.Adapter GetAdapterById(Guid adapterId)
        {
            var dbContext = new ls.ConfigurationDataContext(_connectionString);

            return GetAdapterFromLinqObject(dbContext.Adapters
                                                    .Where(d => !d.IsDeleted && d.AdapterId == adapterId)
                                                    .FirstOrDefault());
        }

        public so.Adapter GetAdapterByName(string adapterName)
        {
            if (String.IsNullOrWhiteSpace(adapterName))
                throw new Exception("Adapter name is missing or empty.");

            var dbContext = new ls.ConfigurationDataContext(_connectionString);

            return GetAdapterFromLinqObject(dbContext.Adapters
                                                    .Where(d => !d.IsDeleted && d.Name.ToLower() == adapterName.ToLower())
                                                    .FirstOrDefault());
        }

        public List<so.Adapter> GetAdapters()
        {
            var dbContext = new ls.ConfigurationDataContext(_connectionString);

            return dbContext.Adapters
                .Where(d => !d.IsDeleted)
                .Select(d => GetAdapterFromLinqObject(d))
                .ToList();
        }

        private so.Adapter GetAdapterFromLinqObject(ls.Adapter adapterFromLinq)
        {
            if (adapterFromLinq == null)
                return null;

            var adapter = new so.Adapter(adapterFromLinq.AdapterId, adapterFromLinq.Name, adapterFromLinq.DllDirectory + "\\" + adapterFromLinq.DllFilename, adapterFromLinq.FullyQualifiedName)
            {
                CreatedDate = adapterFromLinq.CreatedDate,
                UpdatedDate = adapterFromLinq.UpdatedDate,
                Description = adapterFromLinq.Description
            };

            adapter.ExtendedProperties = GetExtendedProperties(so.SyncObjectType.Adapter, adapterFromLinq.AdapterId);

            return adapter;
        }

        #endregion

        #region Data Source

        public void AddDataSource(so.DataSourceDefinition dataSourceDefinition)
        {
            if (dataSourceDefinition == null)
                throw new Exception("Data source definition can not be null.");

            if (DataSourceExistsByName(dataSourceDefinition.Name))
                throw new Exception(string.Format("Data Source with name '{0}' already exists.", dataSourceDefinition.Name));

            var dbContext = new ls.ConfigurationDataContext(_connectionString);

            var now = DateTime.Now;

            var lsDataSource = new ls.DataSource();

            lsDataSource.DataSourceId = dataSourceDefinition.Id;
            lsDataSource.CreatedDate = now;
            lsDataSource.UpdatedDate = now;

            if (!AdapterExistsById(dataSourceDefinition.Adapter.Id))
                throw new RecordNotFoundException(string.Format("Adapter with ID '{0}' does not exist.", dataSourceDefinition.Adapter.Id));

            lsDataSource.AdapterId = dataSourceDefinition.Adapter.Id;
            lsDataSource.Name = dataSourceDefinition.Name;
            lsDataSource.Description = dataSourceDefinition.Description;

            dbContext.DataSources.InsertOnSubmit(lsDataSource);

            dbContext.SubmitChanges();
        }

        public void UpdateDataSource(so.DataSourceDefinition dataSourceDefinition)
        {
            if (dataSourceDefinition == null)
                throw new Exception("Data source definition can not be null.");

            var dbContext = new ls.ConfigurationDataContext(_connectionString);

            var now = DateTime.Now;

            var dataSourceFromLinq = dbContext.DataSources
                                            .Where(d => !d.IsDeleted && d.DataSourceId == dataSourceDefinition.Id)
                                            .FirstOrDefault();

            if (dataSourceFromLinq == null)
                throw new RecordNotFoundException(string.Format("Data source could not be updated because ID '{0}' does not exist.", dataSourceDefinition.Id));

            if (dbContext.DataSources
                    .Where(d => !d.IsDeleted
                                && d.Name.ToLower() == dataSourceDefinition.Name.ToLower()
                                && d.DataSourceId != dataSourceDefinition.Id)
                    .Count() > 0)
            {
                throw new Exception(string.Format("Data source with name '{0}' already exists.", dataSourceDefinition.Name));
            }

            if (!AdapterExistsById(dataSourceDefinition.Adapter.Id))
                throw new RecordNotFoundException(string.Format("Data source could not be updated. Adapter with ID '{0}' does not exist.", dataSourceDefinition.Adapter.Id));

            dataSourceFromLinq.UpdatedDate = now;
            dataSourceFromLinq.Name = dataSourceDefinition.Name;
            dataSourceFromLinq.Description = dataSourceDefinition.Description;
            dataSourceFromLinq.AdapterId = dataSourceDefinition.Adapter.Id;

            dbContext.SubmitChanges();
        }

        public void DeleteDataSource(Guid dataSourceId)
        {
            var dbContext = new ls.ConfigurationDataContext(_connectionString);

            var dataSourceFromLinq = dbContext.DataSources
                                            .Where(d => !d.IsDeleted && d.DataSourceId == dataSourceId)
                                            .FirstOrDefault();

            if (dataSourceFromLinq == null)
                throw new RecordNotFoundException(string.Format("Data source with ID '{0}' does not exist.", dataSourceId));

            var jobsUsingDataSource = GetJobsByDataSourceId(dataSourceId);

            if (jobsUsingDataSource != null && jobsUsingDataSource.Count > 0)
                throw new AssociatedRecordsExistException(string.Format("Data source with name '{0}' can not be deleted because one or more jobs are associated.", dataSourceFromLinq.Name));

            dataSourceFromLinq.UpdatedDate = DateTime.Now;
            dataSourceFromLinq.IsDeleted = true;
            
            dbContext.SubmitChanges();

            DeleteAllExtendedProperties(so.SyncObjectType.DataSource, dataSourceId);            
        }

        public bool DataSourceExistsById(Guid dataSourceId)
        {
            var dbContext = new ls.ConfigurationDataContext(_connectionString);

            return dbContext.DataSources
                            .Where(d => !d.IsDeleted && d.DataSourceId == dataSourceId)
                            .Count() > 0;
        }

        public bool DataSourceExistsByName(string dataSourceName)
        {
            if (String.IsNullOrWhiteSpace(dataSourceName))
                throw new Exception("Data source name is missing or empty.");

            var dbContext = new ls.ConfigurationDataContext(_connectionString);

            return dbContext.DataSources
                            .Where(d => !d.IsDeleted && d.Name.ToLower() == dataSourceName.ToLower())
                            .Count() > 0;
        }

        public so.DataSource GetDataSourceById(Guid dataSourceId)
        {
            var dbContext = new ls.ConfigurationDataContext(_connectionString);

            return GetDataSourceFromLinqObject(dbContext.DataSources
                                                    .Where(d => !d.IsDeleted && d.DataSourceId == dataSourceId)
                                                    .FirstOrDefault());
        }

        public so.DataSource GetDataSourceByName(string dataSourceName)
        {
            if (String.IsNullOrWhiteSpace(dataSourceName))
                throw new Exception("Data source name is missing or empty.");

            var dbContext = new ls.ConfigurationDataContext(_connectionString);

            return GetDataSourceFromLinqObject(dbContext.DataSources
                                                    .Where(d => !d.IsDeleted && d.Name.ToLower() == dataSourceName.ToLower())
                                                    .FirstOrDefault());
        }

        public List<so.DataSource> GetDataSources()
        {
            var dbContext = new ls.ConfigurationDataContext(_connectionString);

            return dbContext.DataSources
                            .Where(d => !d.IsDeleted)
                            .Select(d => GetDataSourceFromLinqObject(d))
                            .ToList();
        }

        public List<so.DataSource> GetDataSourcesByAdapterId(Guid adapterId)
        {
            return GetDataSources().Where(d => d.Adapter.Id == adapterId).ToList();
        }

        public List<so.DataSource> GetDataSourcesByAdapterName(string adapterName)
        {
            if (String.IsNullOrWhiteSpace(adapterName))
                throw new Exception("Adapter name is missing or empty.");

            return GetDataSources().Where(d => d.Adapter.Name.ToLower() == adapterName.ToLower()).ToList();
        }

        private so.DataSource GetDataSourceFromLinqObject(ls.DataSource dataSourceFromLinq)
        {
            if (dataSourceFromLinq == null)
                return null;

            var dataSource = new so.DataSource(dataSourceFromLinq.DataSourceId, dataSourceFromLinq.Name, GetAdapterById(dataSourceFromLinq.AdapterId))
            {
                CreatedDate = dataSourceFromLinq.CreatedDate,
                UpdatedDate = dataSourceFromLinq.UpdatedDate,
                Description = dataSourceFromLinq.Description
            };

            dataSource.ExtendedProperties = GetExtendedProperties(so.SyncObjectType.DataSource, dataSourceFromLinq.DataSourceId);

            return dataSource;
        }

        #endregion

        #region Integration

        public void AddIntegration(so.IntegrationDefinition integrationDefinition)
        {
            if (integrationDefinition == null)
                throw new Exception("Integration definition can not be null.");

            if (IntegrationExistsByName(integrationDefinition.Name))
                throw new Exception(string.Format("Integration with name '{0}' already exists.", integrationDefinition.Name));

            var dbContext = new ls.ConfigurationDataContext(_connectionString);

            var now = DateTime.Now;

            var integrationFromLinq = new ls.Integration();
            integrationFromLinq.IntegrationId = integrationDefinition.Id;
            integrationFromLinq.CreatedDate = now;
            integrationFromLinq.UpdatedDate = now;
            integrationFromLinq.Name = integrationDefinition.Name;
            integrationFromLinq.Description = integrationDefinition.Description;
            integrationFromLinq.PackageDllDirectory = integrationDefinition.PackageDllDirectory;
            integrationFromLinq.PackageDllFilename = integrationDefinition.PackageDllFilename;
            integrationFromLinq.SourceName = integrationDefinition.SourceName;
            integrationFromLinq.TargetName = integrationDefinition.TargetName;
            integrationFromLinq.IsEnabled = integrationDefinition.IsEnabled;
            integrationFromLinq.MaxConcurrentThreads = integrationDefinition.MaxConcurrentThreads;
            integrationFromLinq.LoggingLevelId = (byte)integrationDefinition.LoggingLevel;
            integrationFromLinq.LogToDatabase = integrationDefinition.LogToDatabase;
            integrationFromLinq.DaysOfDatabaseLoggingHistory = integrationDefinition.DaysOfDatabaseLoggingHistory;
            integrationFromLinq.LogToFile = integrationDefinition.LogToFile;
            integrationFromLinq.DaysOfFileLoggingHistory = integrationDefinition.DaysOfFileLoggingHistory;

            dbContext.Integrations.InsertOnSubmit(integrationFromLinq);

            dbContext.SubmitChanges();
        }

        public void UpdateIntegration(so.IntegrationDefinition integrationDefinition)
        {
            if (integrationDefinition == null)
                throw new Exception("Integration definition can not be null.");

            var dbContext = new ls.ConfigurationDataContext(_connectionString);

            var integrationFromLinq = dbContext.Integrations
                                            .Where(d => !d.IsDeleted && d.IntegrationId == integrationDefinition.Id)
                                            .FirstOrDefault();

            if (integrationFromLinq == null)
                throw new RecordNotFoundException(string.Format("Integration could not be updated because ID '{0}' does not exist.", integrationDefinition.Id));

            if (dbContext.Integrations
                    .Where(d => !d.IsDeleted
                                && d.Name.ToLower() == integrationDefinition.Name.ToLower()
                                && d.IntegrationId != integrationDefinition.Id)
                .Count() > 0)
            {
                throw new Exception(string.Format("Integration with name '{0}' already exists.", integrationDefinition.Name));
            }

            integrationFromLinq.UpdatedDate = DateTime.Now;
            integrationFromLinq.Name = integrationDefinition.Name;
            integrationFromLinq.Description = integrationDefinition.Description;
            integrationFromLinq.PackageDllDirectory = integrationDefinition.PackageDllDirectory;
            integrationFromLinq.PackageDllFilename = integrationDefinition.PackageDllFilename;
            integrationFromLinq.SourceName = integrationDefinition.SourceName;
            integrationFromLinq.TargetName = integrationDefinition.TargetName;
            integrationFromLinq.IsEnabled = integrationDefinition.IsEnabled;
            integrationFromLinq.MaxConcurrentThreads = integrationDefinition.MaxConcurrentThreads;
            integrationFromLinq.LoggingLevelId = (byte)integrationDefinition.LoggingLevel;
            integrationFromLinq.LogToDatabase = integrationDefinition.LogToDatabase;
            integrationFromLinq.DaysOfDatabaseLoggingHistory = integrationDefinition.DaysOfDatabaseLoggingHistory;
            integrationFromLinq.LogToFile = integrationDefinition.LogToFile;
            integrationFromLinq.DaysOfFileLoggingHistory = integrationDefinition.DaysOfFileLoggingHistory;

            dbContext.SubmitChanges();
        }

        public void DeleteIntegration(Guid integrationId, bool deleteHistory)
        {
            if (deleteHistory)
                SyncEngineLogger.DeleteAllIntegrationHistory(integrationId);

            var dbContext = new ls.ConfigurationDataContext(_connectionString);

            var integrationFromLinq = dbContext.Integrations
                                            .Where(d => !d.IsDeleted && d.IntegrationId == integrationId)
                                            .FirstOrDefault();

            if (integrationFromLinq == null)
                throw new RecordNotFoundException(string.Format("Integration with ID '{0}' does not exist.", integrationId));

            var integrationJobs = GetJobsByIntegrationId(integrationId);

            if (integrationJobs != null && integrationJobs.Count > 0)
                throw new AssociatedRecordsExistException(string.Format("Integration with name '{0}' can not be deleted because one or more jobs are associated.", integrationFromLinq.Name));

            integrationFromLinq.UpdatedDate = DateTime.Now;
            integrationFromLinq.IsDeleted = true;

            dbContext.SubmitChanges();

            DeleteAllExtendedProperties(so.SyncObjectType.Integration, integrationId);
        }

        public bool IntegrationExistsById(Guid integrationId)
        {
            var dbContext = new ls.ConfigurationDataContext(_connectionString);

            return dbContext.Integrations
                            .Where(d => !d.IsDeleted && d.IntegrationId == integrationId)
                            .Count() > 0;
        }

        public bool IntegrationExistsByName(string integrationName)
        {
            if (String.IsNullOrWhiteSpace(integrationName))
                throw new Exception("Integration name is missing or empty.");

            var dbContext = new ls.ConfigurationDataContext(_connectionString);

            return dbContext.Integrations
                            .Where(d => !d.IsDeleted && d.Name.ToLower() == integrationName.ToLower())
                            .Count() > 0;
        }

        public so.Integration GetIntegrationById(Guid integrationId)
        {
            var dbContext = new ls.ConfigurationDataContext(_connectionString);

            return GetIntegrationFromLinqObject(dbContext.Integrations
                                                    .Where(d => !d.IsDeleted && d.IntegrationId == integrationId)
                                                    .FirstOrDefault());
        }

        public so.Integration GetIntegrationByName(string integrationName)
        {
            if (String.IsNullOrWhiteSpace(integrationName))
                throw new Exception("Integration name is missing or empty.");

            var dbContext = new ls.ConfigurationDataContext(_connectionString);

            return GetIntegrationFromLinqObject(dbContext.Integrations
                                                        .Where(d => !d.IsDeleted && d.Name.ToLower() == integrationName.ToLower())
                                                        .FirstOrDefault());
        }

        public so.Integration GetIntegrationByJobId(Guid jobId)
        {
            var dbContext = new ls.ConfigurationDataContext(_connectionString);

            var job = dbContext.Jobs.Where(d => d.JobId == jobId).FirstOrDefault();

            if (job == null)
                return null;

            return GetIntegrationFromLinqObject(dbContext.Integrations
                                                        .Where(d => !d.IsDeleted && d.IntegrationId == job.IntegrationId)
                                                        .FirstOrDefault());
        }

        public List<so.Integration> GetIntegrations()
        {
            var dbContext = new ls.ConfigurationDataContext(_connectionString);

            return dbContext.Integrations
                            .Where(d => !d.IsDeleted)
                            .Select(d => GetIntegrationFromLinqObject(d))
                            .ToList();
        }

        private so.Integration GetIntegrationFromLinqObject(ls.Integration integrationFromLinq)
        {
            if (integrationFromLinq == null)
                return null;

            var integration = new so.Integration(integrationFromLinq.IntegrationId, integrationFromLinq.Name, integrationFromLinq.PackageDllDirectory + "\\" + integrationFromLinq.PackageDllFilename,
                                              integrationFromLinq.SourceName, integrationFromLinq.TargetName, integrationFromLinq.IsEnabled)
            {
                CreatedDate = integrationFromLinq.CreatedDate,
                UpdatedDate = integrationFromLinq.UpdatedDate,
                Description = integrationFromLinq.Description,
                MaxConcurrentThreads = integrationFromLinq.MaxConcurrentThreads,
                LoggingLevel = (LoggingLevel)integrationFromLinq.LoggingLevelId,
                LogToDatabase = integrationFromLinq.LogToDatabase,
                DaysOfDatabaseLoggingHistory = integrationFromLinq.DaysOfDatabaseLoggingHistory,
                LogToFile = integrationFromLinq.LogToFile,
                DaysOfFileLoggingHistory = integrationFromLinq.DaysOfFileLoggingHistory
            };

            integration.ExtendedProperties = GetExtendedProperties(so.SyncObjectType.Integration, integrationFromLinq.IntegrationId);

            var jobs = GetJobsByIntegrationId(integrationFromLinq.IntegrationId);

            if (jobs != null && jobs.Count > 0)
                integration.AddJobs(jobs);

            return integration;
        }

        #endregion

        #region Job

        public void AddJob(Guid integrationId, so.JobDefinition jobDefinition)
        {
            var integration = GetIntegrationById(integrationId);

            if (integration == null)
                throw new RecordNotFoundException(string.Format("Integration with ID '{0}' does not exist.", integrationId));

            if (jobDefinition == null)
                throw new Exception("Job definition can not be null.");

            if (JobExistsById(jobDefinition.Id))
                throw new Exception(string.Format("Job with ID '{0}' already exists.", jobDefinition.Id));

            if (JobExistsByName(integration.Name, jobDefinition.Name))
                throw new Exception(string.Format("Job with name '{0}' already exists in integration with name '{1}'.", jobDefinition.Name, integration.Name));

            var dbContext = new ls.ConfigurationDataContext(_connectionString);

            var now = DateTime.Now;

            var jobFromLinq = new ls.Job();
            jobFromLinq.JobId = jobDefinition.Id;
            jobFromLinq.CreatedDate = now;
            jobFromLinq.UpdatedDate = now;
            jobFromLinq.IntegrationId = integrationId;
            jobFromLinq.Name = jobDefinition.Name;
            jobFromLinq.Description = jobDefinition.Description;
            jobFromLinq.IsEnabled = jobDefinition.IsEnabled;
            jobFromLinq.JobPriorityId = (byte)jobDefinition.Priority;
            jobFromLinq.TerminateOnErrorTypeId = (byte)jobDefinition.TerminateOnErrorType;
            jobFromLinq.LoggingLevelId = (byte)jobDefinition.LoggingLevel;

            dbContext.Jobs.InsertOnSubmit(jobFromLinq);

            dbContext.SubmitChanges();
        }

        public void UpdateJob(so.JobDefinition jobDefinition)
        {
            if (jobDefinition == null)
                throw new Exception("Job definition can not be null.");

            var dbContext = new ls.ConfigurationDataContext(_connectionString);

            var jobFromLinq = dbContext.Jobs
                                    .Where(d => !d.IsDeleted && d.JobId == jobDefinition.Id)
                                    .FirstOrDefault();

            if (jobFromLinq == null)
                throw new RecordNotFoundException(string.Format("Job could not be updated because ID '{0}' does not exist.", jobDefinition.Id));

            if (dbContext.Jobs
                    .Where(d => !d.IsDeleted
                                && d.Name.ToLower() == jobDefinition.Name.ToLower()
                                && d.IntegrationId == jobFromLinq.IntegrationId
                                && d.JobId != jobDefinition.Id)
                    .Count() > 0)
            {
                throw new Exception(string.Format("Job with name '{0}' already exists.", jobDefinition.Name));
            }

            jobFromLinq.UpdatedDate = DateTime.Now;
            jobFromLinq.Name = jobDefinition.Name;
            jobFromLinq.Description = jobDefinition.Description;
            jobFromLinq.IsEnabled = jobDefinition.IsEnabled;
            jobFromLinq.JobPriorityId = (byte)jobDefinition.Priority;
            jobFromLinq.TerminateOnErrorTypeId = (byte)jobDefinition.TerminateOnErrorType;
            jobFromLinq.LoggingLevelId = (byte)jobDefinition.LoggingLevel;

            dbContext.SubmitChanges();
        }

        public void DeleteJob(Guid jobId)
        {
            var dbContext = new ls.ConfigurationDataContext(_connectionString);

            var jobFromLinq = dbContext.Jobs
                                .Where(d => !d.IsDeleted && d.JobId == jobId)
                                .FirstOrDefault();

            if (jobFromLinq == null)
                throw new RecordNotFoundException(string.Format("Job with ID '{0}' does not exist.", jobId));

            DeleteAllExtendedProperties(so.SyncObjectType.Job, jobId);

            DeleteAllJobDataSources(jobId);

            DeleteAllJobSteps(jobId);

            DeleteAllJobSchedules(jobId);

            jobFromLinq.UpdatedDate = DateTime.Now;
            jobFromLinq.IsDeleted = true;

            dbContext.SubmitChanges();

            //SyncEngineLogger.DeleteJobDataSourceHistory(jobId);
        }

        public bool JobExistsById(Guid jobId)
        {
            var dbContext = new ls.ConfigurationDataContext(_connectionString);

            return dbContext.Jobs
                        .Where(d => !d.IsDeleted && d.JobId == jobId)
                        .Count() > 0;
        }

        public bool JobExistsByName(string integrationName, string jobName)
        {
            if (String.IsNullOrWhiteSpace(integrationName))
                throw new Exception("Integration name is missing or empty.");

            if (String.IsNullOrWhiteSpace(jobName))
                throw new Exception("Job name is missing or empty.");

            var dbContext = new ls.ConfigurationDataContext(_connectionString);

            var integrationId = dbContext.Integrations
                                        .Where(d => !d.IsDeleted && d.Name.ToLower() == integrationName.ToLower())
                                        .Select(d => d.IntegrationId)
                                        .FirstOrDefault();

            if (integrationId == Guid.Empty)
                return false;
            else
                return dbContext.Jobs
                                .Where(d => !d.IsDeleted
                                            && d.IntegrationId == integrationId
                                            && d.Name.ToLower() == jobName.ToLower())
                                .Select(d => d.JobId)
                                .Count() > 0;
        }

        public so.Job GetJobById(Guid jobId)
        {
            var dbContext = new ls.ConfigurationDataContext(_connectionString);

            return GetJobFromLinqObject(dbContext.Jobs
                                            .Where(d => !d.IsDeleted && d.JobId == jobId)
                                            .FirstOrDefault());
        }

        public so.Job GetJobByName(string integrationName, string jobName)
        {
            var integration = GetIntegrationByName(integrationName);

            if (integration == null)
                throw new RecordNotFoundException(string.Format("Integration with name '{0}' does not exist.", integrationName));

            if (String.IsNullOrWhiteSpace(jobName))
                throw new Exception("Job name is missing or empty.");

            var dbContext = new ls.ConfigurationDataContext(_connectionString);

            return GetJobFromLinqObject(dbContext.Jobs
                                            .Where(d => !d.IsDeleted 
                                                        && d.Name.ToLower() == jobName.ToLower()
                                                        && d.IntegrationId == integration.Id)
                                            .FirstOrDefault());
        }

        public List<so.Job> GetJobsByIntegrationId(Guid integrationId)
        {
            if (!IntegrationExistsById(integrationId))
                throw new RecordNotFoundException(string.Format("Integration with ID '{0}' does not exist.", integrationId));

            var dbContext = new ls.ConfigurationDataContext(_connectionString);

            return dbContext.Jobs
                        .Where(d => !d.IsDeleted
                                    && d.IntegrationId == integrationId)
                        .Select(d => GetJobById(d.JobId))
                        .ToList();
        }

        public List<so.Job> GetJobsByIntegrationName(string integrationName)
        {
            var integration = GetIntegrationByName(integrationName);

            if (integration == null)
                throw new RecordNotFoundException(string.Format("Integration with name '{0}' does not exist.", integrationName));
            
            return integration.Jobs.ToList();
        }

        public List<so.Job> GetJobsByDataSourceId(Guid dataSourceId)
        {
            var dbContext = new ls.ConfigurationDataContext(_connectionString);

            return dbContext.JobDataSources
                            .Where(d => !d.IsDeleted && d.DataSourceId == dataSourceId)
                            .Select(d => GetJobById(d.JobId))
                            .ToList();
        }

        public List<so.Job> GetJobsByDataSourceName(string dataSourceName)
        {
            var dataSource = GetDataSourceByName(dataSourceName);

            if (dataSource == null)
                throw new RecordNotFoundException(string.Format("Data source with name '{0}' does not exist.", dataSourceName));

            var dbContext = new ls.ConfigurationDataContext(_connectionString);

            return dbContext.JobDataSources
                            .Where(d => !d.IsDeleted && d.DataSourceId == dataSource.Id)
                            .Select(d => GetJobById(d.JobId))
                            .ToList();
        }

        private so.Job GetJobFromLinqObject(ls.Job jobFromLinq)
        {
            if (jobFromLinq == null)
                return null;

            var job = new so.Job(jobFromLinq.JobId, jobFromLinq.Name, jobFromLinq.IsEnabled)
            {
                CreatedDate = jobFromLinq.CreatedDate,
                UpdatedDate = jobFromLinq.UpdatedDate,
                Description = jobFromLinq.Description,
                Priority = (JobPriority)jobFromLinq.JobPriorityId,
                TerminateOnErrorType = (JobTerminateOnErrorType)jobFromLinq.TerminateOnErrorTypeId,
                LoggingLevel = (LoggingLevel)jobFromLinq.LoggingLevelId
            };

            job.ExtendedProperties = GetExtendedProperties(so.SyncObjectType.Job, jobFromLinq.JobId);

            var sourceSidejobDataSources = GetJobDataSourcesByJobId(job.Id, SyncSide.Source);

            if (sourceSidejobDataSources != null & sourceSidejobDataSources.Count > 0)
                job.AddDataSources(sourceSidejobDataSources, SyncSide.Source);

            var targetSidejobDataSources = GetJobDataSourcesByJobId(job.Id, SyncSide.Target);

            if (targetSidejobDataSources != null & targetSidejobDataSources.Count > 0)
                job.AddDataSources(targetSidejobDataSources, SyncSide.Target);

            var jobSteps = GetJobStepsByJobId(job.Id);

            if (jobSteps != null && jobSteps.Count > 0)
                job.AddSteps(jobSteps);

            return job;
        }

        #endregion

        #region Job Data Source

        public void AddJobDataSource(Guid jobId, Guid dataSourceId, SyncSide syncSide)
        {
            if (!JobExistsById(jobId))
                throw new RecordNotFoundException(string.Format("Job with ID '{0}' does not exist.", jobId));

            if (!DataSourceExistsById(dataSourceId))
                throw new RecordNotFoundException(string.Format("Data source with ID '{0}' does not exist.", dataSourceId));

            if (!JobDataSourceExists(jobId, dataSourceId, syncSide))
            {
                var dbContext = new ls.ConfigurationDataContext(_connectionString);

                var now = DateTime.Now;

                var lsJobDataSource = new ls.JobDataSource();
                lsJobDataSource.JobDataSourceId = Guid.NewGuid();
                lsJobDataSource.CreatedDate = now;
                lsJobDataSource.UpdatedDate = now;
                lsJobDataSource.JobId = jobId;
                lsJobDataSource.DataSourceId = dataSourceId;
                lsJobDataSource.SyncSide = Enum.GetName(typeof(SyncSide), syncSide);

                dbContext.JobDataSources.InsertOnSubmit(lsJobDataSource);

                dbContext.SubmitChanges();
            }
        }

        public void DeleteJobDataSource(Guid jobId, Guid dataSourceId, SyncSide syncSide)
        {
            var dbContext = new ls.ConfigurationDataContext(_connectionString);

            var jobDataSourceFromLinq = dbContext.JobDataSources
                                                .Where(d => !d.IsDeleted && d.JobId == jobId 
                                                            && d.DataSourceId == dataSourceId 
                                                            && d.SyncSide == Enum.GetName(typeof(SyncSide), syncSide))
                                                .FirstOrDefault();

            if (jobDataSourceFromLinq == null)
                throw new RecordNotFoundException(string.Format("{0}-side data source with job ID '{1}' and data source ID '{2}' does not exist.",
                                                                 Enum.GetName(typeof(SyncSide), syncSide), jobId, dataSourceId));

            jobDataSourceFromLinq.UpdatedDate = DateTime.Now;
            jobDataSourceFromLinq.IsDeleted = true;

            dbContext.SubmitChanges();
        }

        public void DeleteJobDataSources(Guid jobId, SyncSide syncSide)
        {
            var dbContext = new ls.ConfigurationDataContext(_connectionString);

            var jobDataSourcesFromLinq = dbContext.JobDataSources
                                    .Where(d => !d.IsDeleted && d.JobId == jobId && d.SyncSide == Enum.GetName(typeof(SyncSide), syncSide))
                                    .Select(d => d);

            foreach (var jobDataSource in jobDataSourcesFromLinq)
            {
                jobDataSource.UpdatedDate = DateTime.Now;
                jobDataSource.IsDeleted = true;
            }

            dbContext.SubmitChanges();
        }

        public void DeleteAllJobDataSources(Guid jobId)
        {
            var dbContext = new ls.ConfigurationDataContext(_connectionString);

            var jobDataSourcesFromLinq = dbContext.JobDataSources
                                                .Where(d => !d.IsDeleted && d.JobId == jobId)
                                                .Select(d => d);

            foreach (var jobDataSource in jobDataSourcesFromLinq)
            {
                jobDataSource.UpdatedDate = DateTime.Now;
                jobDataSource.IsDeleted = true;
            }

            dbContext.SubmitChanges();
        }

        public so.DataSource GetJobDataSource(Guid jobId, Guid dataSourceId, SyncSide syncSide)
        {
            return GetDataSourceById(dataSourceId);
        }

        public bool JobDataSourceExists(Guid jobId, Guid dataSourceId, SyncSide syncSide)
        {
            var dbContext = new ls.ConfigurationDataContext(_connectionString);

            return dbContext.JobDataSources
                        .Where(d => !d.IsDeleted 
                                    && d.JobId == jobId 
                                    && d.DataSourceId == dataSourceId
                                    && d.SyncSide == Enum.GetName(typeof(SyncSide), syncSide))
                        .Count() > 0;
        }

        public RunHistory GetJobDataSourceRunHistory(Guid jobId, Guid sourceDataSourceId, Guid targetDataSourceId)
        {
            var dbContext = new ls.ConfigurationDataContext(_connectionString);

            var jobDataSourceHistoryFromLinq = dbContext.JobDataSourceHistories
                                                    .Where(d => d.JobId == jobId &&
                                                                d.SourceDataSourceId == sourceDataSourceId &&
                                                                d.TargetDataSourceId == targetDataSourceId)
                                                    .FirstOrDefault();
            if (jobDataSourceHistoryFromLinq == null)
            {
                return new RunHistory(new DateTime?(), new DateTime?(), new DateTime?(), new DateTime?(), new DateTime?(), new DateTime?());
            }
            else
            {
                return new RunHistory(jobDataSourceHistoryFromLinq.LastStartTime, jobDataSourceHistoryFromLinq.LastEndTime,
                                      jobDataSourceHistoryFromLinq.LastStartTimeWithoutRecordErrors, jobDataSourceHistoryFromLinq.LastEndTimeWithoutRecordErrors,
                                      jobDataSourceHistoryFromLinq.LastStartTimeWithoutRuntimeErrors, jobDataSourceHistoryFromLinq.LastEndTimeWithoutRuntimeErrors);
            }
        }

        public List<so.DataSource> GetJobDataSourcesByJobId(Guid jobId, SyncSide syncSide)
        {
            var dbContext = new ls.ConfigurationDataContext(_connectionString);

            return dbContext.JobDataSources
                            .Where(d => !d.IsDeleted 
                                        && d.JobId == jobId
                                        && d.SyncSide == Enum.GetName(typeof(SyncSide), syncSide))
                            .Select(d => GetDataSourceById(d.DataSourceId))
                            .ToList();
        }

        public List<so.DataSource> GetJobDataSourcesByJobName(string integrationName, string jobName, SyncSide syncSide)
        {
            var job = GetJobByName(integrationName, jobName);

            if (job == null)
                throw new RecordNotFoundException(string.Format("Job with name '{0}' in integration with name '{1}' does not exist.", jobName, integrationName));

            return GetJobDataSourcesByJobId(job.Id, syncSide);
        }

        #endregion

        #region Job Schedule

        public void AddJobSchedule(Guid jobId, so.JobSchedule jobSchedule)
        {
            if (!JobExistsById(jobId))
                throw new RecordNotFoundException(string.Format("Job with ID '{0}' does not exist.", jobId));

            if (jobSchedule == null)
                throw new Exception("Job schedule can not be null.");

            var dbContext = new ls.ConfigurationDataContext(_connectionString);

            var now = DateTime.Now;

            var lsJobSchedule = new ls.JobSchedule();
            lsJobSchedule.JobScheduleId = jobSchedule.Id;
            lsJobSchedule.CreatedDate = now;
            lsJobSchedule.UpdatedDate = now;
            lsJobSchedule.JobId = jobId;
            lsJobSchedule.IsEnabled = jobSchedule.IsEnabled;
            lsJobSchedule.StartDate = jobSchedule.StartDate;
            lsJobSchedule.EndDate = jobSchedule.EndDate;
            lsJobSchedule.DaysOfWeek = so.JobSchedule.GetDaysOfWeekAsString(jobSchedule.DaysOfWeek);
            lsJobSchedule.StartTime = jobSchedule.StartTime;
            lsJobSchedule.EndTime = jobSchedule.EndTime;
            lsJobSchedule.FrequencyInSeconds = jobSchedule.FrequencyInSeconds;

            dbContext.JobSchedules.InsertOnSubmit(lsJobSchedule);

            dbContext.SubmitChanges();
        }

        public void UpdateJobSchedule(so.JobSchedule jobSchedule)
        {
            if (jobSchedule == null)
                throw new Exception("Job schedule can not be null.");

            var dbContext = new ls.ConfigurationDataContext(_connectionString);

            var jobScheduleFromLinq = dbContext.JobSchedules
                                            .Where(d => d.JobScheduleId == jobSchedule.Id)
                                            .FirstOrDefault();

            if (jobScheduleFromLinq == null)
                throw new RecordNotFoundException(string.Format("Job schedule could not be updated because ID '{0}' does not exist.", jobSchedule.Id));

            jobScheduleFromLinq.UpdatedDate = DateTime.Now;
            jobScheduleFromLinq.IsEnabled = jobSchedule.IsEnabled;
            jobScheduleFromLinq.StartDate = jobSchedule.StartDate;
            jobScheduleFromLinq.EndDate = jobSchedule.EndDate;
            jobScheduleFromLinq.DaysOfWeek = so.JobSchedule.GetDaysOfWeekAsString(jobSchedule.DaysOfWeek);
            jobScheduleFromLinq.StartTime = jobSchedule.StartTime;
            jobScheduleFromLinq.EndTime = jobSchedule.EndTime;
            jobScheduleFromLinq.FrequencyInSeconds = jobSchedule.FrequencyInSeconds;

            dbContext.SubmitChanges();
        }

        public void DeleteJobSchedule(Guid jobScheduleId)
        {
            var dbContext = new ls.ConfigurationDataContext(_connectionString);

            var jobScheduleFromLinq = dbContext.JobSchedules
                                .Where(d => !d.IsDeleted && d.JobScheduleId == jobScheduleId)
                                .FirstOrDefault();

            if (jobScheduleFromLinq == null)
                throw new RecordNotFoundException(string.Format("Job schedule with ID '{0}' does not exist.", jobScheduleId));

            jobScheduleFromLinq.UpdatedDate = DateTime.Now;
            jobScheduleFromLinq.IsDeleted = true;

            dbContext.SubmitChanges();
        }

        public void DeleteAllJobSchedules(Guid jobId)
        {
            var jobSchedules = GetJobSchedulesByJobId(jobId);

            if (jobSchedules != null)
            {
                foreach (var jobSchedule in jobSchedules)
                {
                    DeleteJobSchedule(jobSchedule.Id);
                }
            }
        }

        public bool JobScheduleExistsById(Guid jobScheduleId)
        {
            var dbContext = new ls.ConfigurationDataContext(_connectionString);

            return dbContext.JobSchedules
                        .Where(d => !d.IsDeleted && d.JobScheduleId == jobScheduleId)
                        .Count() > 0;
        }

        public so.JobSchedule GetJobScheduleById(Guid jobScheduleId)
        {
            var dbContext = new ls.ConfigurationDataContext(_connectionString);

            return GetJobScheduleFromLinqObject(dbContext.JobSchedules
                                                        .Where(d => !d.IsDeleted && d.JobScheduleId == jobScheduleId)
                                                        .FirstOrDefault());
        }

        public List<so.JobSchedule> GetJobSchedulesByJobId(Guid jobId)
        {
            // was getting database connection is already open exceptions, so added a separate instance of dbContext
            var dbContext = new ls.ConfigurationDataContext(_connectionString);

            return dbContext.JobSchedules
                            .Where(d => !d.IsDeleted
                                        && d.JobId == jobId)
                            .Select(d => GetJobScheduleById(d.JobScheduleId))
                            .ToList();
        }

        public List<so.JobSchedule> GetJobSchedulesByJobName(string integrationName, string jobName)
        {
            var job = GetJobByName(integrationName, jobName);

            if (job == null)
                throw new RecordNotFoundException(string.Format("Job with name '{0}' in integration with name '{1}' does not exist.", jobName, integrationName));

            var dbContext = new ls.ConfigurationDataContext(_connectionString);

            return dbContext.JobSchedules
                            .Where(d => !d.IsDeleted
                                        && d.JobId == job.Id)
                            .Select(d => GetJobScheduleById(d.JobScheduleId))
                            .ToList();
        }

        private so.JobSchedule GetJobScheduleFromLinqObject(ls.JobSchedule jobScheduleFromLinq)
        {
            if (jobScheduleFromLinq == null)
                return null;

            return new so.JobSchedule(jobScheduleFromLinq.JobScheduleId, jobScheduleFromLinq.StartDate, jobScheduleFromLinq.EndDate, jobScheduleFromLinq.FrequencyInSeconds)
            {
                CreatedDate = jobScheduleFromLinq.CreatedDate,
                UpdatedDate = jobScheduleFromLinq.UpdatedDate,
                IsEnabled = jobScheduleFromLinq.IsEnabled,
                DaysOfWeek = so.JobSchedule.GetDaysOfWeekFromString(jobScheduleFromLinq.DaysOfWeek),
                StartTime = jobScheduleFromLinq.StartTime,
                EndTime = jobScheduleFromLinq.EndTime,
            };
        }

        #endregion

        #region Job Step

        public void AddJobStep(Guid jobId, so.JobStepDefinition jobStepDefinition)
        {
            var job = GetJobById(jobId);

            if (job == null)
                throw new RecordNotFoundException(string.Format("Job with ID '{0}' does not exist.", jobId));

            if (jobStepDefinition == null)
                throw new Exception("Job step definition can not be null.");

            var jobStepWithSameName = GetJobStepsByJobId(jobId).Where(d => d.Name.ToLower() == jobStepDefinition.Name.ToLower()).Select(d => d);

            if (jobStepWithSameName != null && jobStepWithSameName.Count() > 0)
                throw new Exception(string.Format("Job step with name '{0}' already exists in job with name '{1}'.", jobStepDefinition.Name, job.Name));

            var dbContext = new ls.ConfigurationDataContext(_connectionString);

            byte maxOrderIdx = 0;

            var orderIdxs = dbContext.JobSteps
                                    .Where(d => !d.IsDeleted && d.JobId == jobId)
                                    .Select(d => d.OrderIndex);

            if (orderIdxs.Count() > 0)
                maxOrderIdx = orderIdxs.Max();

            var now = DateTime.Now;

            var jobStepFromLinq = new ls.JobStep();
            jobStepFromLinq.JobStepId = jobStepDefinition.Id;
            jobStepFromLinq.CreatedDate = now;
            jobStepFromLinq.UpdatedDate = now;
            jobStepFromLinq.JobId = jobId;
            jobStepFromLinq.Name = jobStepDefinition.Name;
            jobStepFromLinq.Description = jobStepDefinition.Description;
            jobStepFromLinq.FullyQualifiedName = jobStepDefinition.FullyQualifiedName;
            jobStepFromLinq.IsEnabled = jobStepDefinition.IsEnabled;
            jobStepFromLinq.OrderIndex = ++maxOrderIdx;

            dbContext.JobSteps.InsertOnSubmit(jobStepFromLinq);

            dbContext.SubmitChanges();
        }

        public void UpdateJobStep(so.JobStepDefinition jobStepDefinition)
        {
            if (jobStepDefinition == null)
                throw new Exception("Job step definition can not be null.");

            var dbContext = new ls.ConfigurationDataContext(_connectionString);

            var jobStepFromLinq = dbContext.JobSteps
                                    .Where(d => !d.IsDeleted && d.JobStepId == jobStepDefinition.Id)
                                    .FirstOrDefault();

            if (jobStepFromLinq == null)
                throw new RecordNotFoundException(string.Format("Job step could not be updated because ID '{0}' does not exist.", jobStepDefinition.Id));

            if (dbContext.JobSteps
                    .Where(d => !d.IsDeleted
                                && d.Name.ToLower() == jobStepDefinition.Name.ToLower()
                                && d.JobId == jobStepFromLinq.JobId
                                && d.JobStepId != jobStepDefinition.Id)
                    .Count() > 0)
            {
                throw new Exception(string.Format("Job step with name '{0}' already exists.", jobStepDefinition.Name));
            }

            jobStepFromLinq.UpdatedDate = DateTime.Now;
            jobStepFromLinq.Name = jobStepDefinition.Name;
            jobStepFromLinq.Description = jobStepDefinition.Description;
            jobStepFromLinq.FullyQualifiedName = jobStepDefinition.FullyQualifiedName;
            jobStepFromLinq.IsEnabled = jobStepDefinition.IsEnabled;

            dbContext.SubmitChanges();
        }

        public void DeleteJobStep(Guid jobStepId)
        {
            var dbContext = new ls.ConfigurationDataContext(_connectionString);

            var jobStepFromLinq = dbContext.JobSteps
                                        .Where(d => !d.IsDeleted && d.JobStepId == jobStepId)
                                        .FirstOrDefault();
            
            if (jobStepFromLinq == null)
                throw new RecordNotFoundException(string.Format("Job step with ID '{0}' does not exist.", jobStepId));

            jobStepFromLinq.UpdatedDate = DateTime.Now;
            jobStepFromLinq.IsDeleted = true;

            // was getting "row not found or changed" exception on last job step and not sure why
            // UPDATE: likely due to the job step order index being updated from the prior job step; Linq to SQL caches data and checks for changes when updating
            // found a workaround here: http://stackoverflow.com/questions/45045/what-can-i-do-to-resolve-a-row-not-found-or-changed-exception-in-linq-to-sql-o
            dbContext.Refresh(RefreshMode.KeepChanges, jobStepFromLinq);

            dbContext.SubmitChanges();

            var sql = string.Format(@"UPDATE JobStep 
                                      SET OrderIndex = OrderIndex - 1 
                                      WHERE JobId = '{0}' AND OrderIndex > {1}", jobStepFromLinq.JobId, jobStepFromLinq.OrderIndex);

            dbContext.ExecuteCommand(sql);
        }

        public void DeleteAllJobSteps(Guid jobId)
        {
            var jobSteps = GetJobStepsByJobId(jobId);

            if (jobSteps != null)
            {
                foreach (var jobStep in jobSteps)
                {
                    DeleteJobStep(jobStep.Id);
                }
            }
        }

        public bool JobStepExistsById(Guid jobStepId)
        {
            var dbContext = new ls.ConfigurationDataContext(_connectionString);

            return dbContext.JobSteps
                        .Where(d => !d.IsDeleted && d.JobStepId == jobStepId)
                        .Count() > 0;
        }

        public bool JobStepExistsByName(string integrationName, string jobName, string jobStepName)
        {
            if (String.IsNullOrWhiteSpace(integrationName))
                throw new Exception("Integration name is missing or empty.");

            if (String.IsNullOrWhiteSpace(jobName))
                throw new Exception("Job name is missing or empty.");

            if (String.IsNullOrWhiteSpace(jobStepName))
                throw new Exception("Job step name is missing or empty.");

            var dbContext = new ls.ConfigurationDataContext(_connectionString);

            var integrationId = dbContext.Integrations
                                        .Where(d => !d.IsDeleted && d.Name.ToLower() == integrationName.ToLower())
                                        .Select(d => d.IntegrationId)
                                        .FirstOrDefault();

            if (integrationId == Guid.Empty)
                return false;

            var jobId = dbContext.Jobs
                            .Where(d => !d.IsDeleted 
                                        && d.IntegrationId == integrationId
                                        && d.Name.ToLower() == jobName.ToLower())
                            .Select(d => d.JobId)
                            .FirstOrDefault();
            if (jobId == Guid.Empty)
                return false;
           
            return dbContext.JobSteps
                        .Where(d => !d.IsDeleted
                                    && d.JobId == jobId
                                    && d.Name.ToLower() == jobStepName.ToLower())
                        .Count() > 0;
        }

        public so.JobStep GetJobStepById(Guid jobStepId)
        {
            var dbContext = new ls.ConfigurationDataContext(_connectionString);

            return GetJobStepFromLinqObject(dbContext.JobSteps
                                                    .Where(d => !d.IsDeleted && d.JobStepId == jobStepId)
                                                    .FirstOrDefault());
        }

        public so.JobStep GetJobStepByName(string integrationName, string jobName, string jobStepName)
        {
            var job = GetJobByName(integrationName, jobName);

            if (job == null)
                throw new RecordNotFoundException(string.Format("Job with name '{0}' does not exist in integration with name '{1}'.", jobName, integrationName));

            if (String.IsNullOrWhiteSpace(jobStepName))
                throw new Exception("Job step name is missing or empty.");

            var dbContext = new ls.ConfigurationDataContext(_connectionString);

            return GetJobStepFromLinqObject(dbContext.JobSteps
                                                    .Where(d => !d.IsDeleted 
                                                                && d.JobId == job.Id
                                                                && d.Name.ToLower() == jobStepName.ToLower())
                                                    .FirstOrDefault());
        }

        public List<so.JobStep> GetJobStepsByJobId(Guid jobId)
        {
            if (!JobExistsById(jobId))
                throw new RecordNotFoundException(string.Format("Job with ID '{0}' does not exist.", jobId));

            var dbContext = new ls.ConfigurationDataContext(_connectionString);

            return dbContext.JobSteps
                            .Where(d => !d.IsDeleted && d.JobId == jobId)
                            .OrderBy(d => d.OrderIndex)
                            .Select(d => GetJobStepFromLinqObject(d))
                            .ToList();
        }

        public List<so.JobStep> GetJobStepsByJobName(string integrationName, string jobName)
        {
            var job = GetJobByName(integrationName, jobName);

            if (job == null)
                throw new RecordNotFoundException(string.Format("Job with name '{0}' does not exist in integration with name '{1}'.", jobName, integrationName));

            var dbContext = new ls.ConfigurationDataContext(_connectionString);

            return dbContext.JobSteps
                            .Where(d => !d.IsDeleted
                                        && d.JobId == job.Id)
                            .OrderBy(d => d.OrderIndex)
                            .Select(d => GetJobStepFromLinqObject(d))
                            .ToList();
        }

        public void MoveJobStepUp(Guid jobStepId)
        {
            var dbContext = new ls.ConfigurationDataContext(_connectionString);

            var jobStepFromLinq = dbContext.JobSteps
                                        .Where(d => !d.IsDeleted && d.JobStepId == jobStepId)
                                        .FirstOrDefault();

            if (jobStepFromLinq == null)
                throw new RecordNotFoundException(string.Format("Job step with ID '{0}' does not exist.", jobStepId));

            // if the 1st step, ignore; otherwise, swap with the next one up
            if (jobStepFromLinq.OrderIndex > 1)
            {
                var oldOrderIdx = jobStepFromLinq.OrderIndex;

                jobStepFromLinq.UpdatedDate = DateTime.Now;
                jobStepFromLinq.OrderIndex = (byte)(oldOrderIdx - 1);

                var sql = string.Format(@"UPDATE JobStep
                                          SET OrderIndex = OrderIndex + 1 
                                          WHERE JobId = '{0}' AND OrderIndex = {1}", jobStepFromLinq.JobId, oldOrderIdx - 1);

                // first update the step to swap with
                dbContext.ExecuteCommand(sql);

                dbContext.SubmitChanges();
            }
        }

        public void MoveJobStepDown(Guid jobStepId)
        {
            var dbContext = new ls.ConfigurationDataContext(_connectionString);

            var jobStepFromLinq = dbContext.JobSteps
                                        .Where(d => !d.IsDeleted && d.JobStepId == jobStepId)
                                        .FirstOrDefault();

            if (jobStepFromLinq == null)
                throw new RecordNotFoundException(string.Format("Job step with ID '{0}' does not exist.", jobStepId));

            var maxOrderIdx = dbContext.JobSteps
                                    .Where(d => !d.IsDeleted && d.JobId == jobStepFromLinq.JobId)
                                    .Select(d => d.OrderIndex)
                                    .Max();

            // if order index is max order index, ignore; otherwise, swap with the next one down
            if (jobStepFromLinq.OrderIndex != maxOrderIdx)
            {
                var oldOrderIdx = jobStepFromLinq.OrderIndex;

                jobStepFromLinq.UpdatedDate = DateTime.Now;
                jobStepFromLinq.OrderIndex = (byte)(oldOrderIdx + 1);

                var sql = string.Format(@"UPDATE JobStep
                                          SET OrderIndex = OrderIndex - 1 
                                          WHERE JobId = '{0}' AND OrderIndex = {1}", jobStepFromLinq.JobId, oldOrderIdx + 1);

                // first update the step to swap with
                dbContext.ExecuteCommand(sql);

                dbContext.SubmitChanges();
            }
        }

        private so.JobStep GetJobStepFromLinqObject(ls.JobStep jobStepFromLinq)
        {
            if (jobStepFromLinq == null)
                return null;

            return new so.JobStep(jobStepFromLinq.JobStepId, jobStepFromLinq.Name, jobStepFromLinq.FullyQualifiedName, jobStepFromLinq.IsEnabled)
            {
                CreatedDate = jobStepFromLinq.CreatedDate,
                UpdatedDate = jobStepFromLinq.UpdatedDate,
                Description =  jobStepFromLinq.Description
            };
        }

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Runtime.InteropServices;
using SyncObjX.Util;
using SyncObjX.Core;
using SyncObjX.SyncObjects;
using SyncObjX.Configuration;
using SyncObjX.Data;
using SyncObjX.Logging;
using SyncObjX.Exceptions;

namespace SyncObjX.Adapters
{
    [AdapterInfo(Name = "SQL Server")]
    [Guid("524ad4aa-b18a-476c-ad80-431d5352a764")]
    public class SqlServerAdapter : AdapterInstance, IAdapterConfig
    {
        private SqlConnection _activeSqlConnection;

        public SqlServerAdapter(Dictionary<string, object> properties, DataSource associatedDataSource) : base(properties, associatedDataSource) { }

        public override Dictionary<string, SupportedProperty> GetSupportedProperties()
        {
            Dictionary<string, SupportedProperty> supportedProperties = new Dictionary<string, SupportedProperty>();

            supportedProperties.Add("ConnectionString", new SupportedProperty("ConnectionString", SupportedPropertyType.Text, true)
            {
                Description = "Provide the SQL Server connection string to the database."
            });

            supportedProperties.Add("DatabaseName", new SupportedProperty("DatabaseName", SupportedPropertyType.Text, true)
            {
                Description = "Provide the database name."
            });

            return supportedProperties;
        }

        public override DataSet GetData(object commandObj)
        {
            if (commandObj == null)
                throw new Exception("Command object for SQL Server adapter can not be null.");
            else if (commandObj.GetType() != typeof(string))
                throw new Exception("Command object for SQL Server adapter must be of type System.String.");

            var commandText = (string)commandObj;

            SyncEngineLogger.WriteByParallelTaskContext(LogEntryType.Debug, this.AssociatedDataSource, "Command text to get data: {0}", commandText);

            DataSet ds;

            ds = GetDataSet(commandText);

            SyncEngineLogger.WriteByParallelTaskContext(LogEntryType.Debug, this.AssociatedDataSource, () =>
            {
                StringBuilder logMessage = new StringBuilder();

                if (ds.Tables.Count == 0)
                    logMessage.AppendLine("No data tables were returned from GetData.");
                else
                {
                    logMessage.AppendLine("Data returned:");

                    foreach (var tbl in ds.Tables.Cast<DataTable>())
                        logMessage.AppendLine(string.Format("{0}: {1} row(s) returned.", tbl.TableName, tbl.Rows.Count));
                }

                return logMessage.ToString();
            });

            return ds;
        }

        public override void ProcessBatch(JobBatch jobBatch)
        {
            SyncEngineLogger.WriteByParallelTaskContext(LogEntryType.Info, this.AssociatedDataSource,
                                                            "Processing {0} entity batch(es) for sync side '{1}'.",
                                                            jobBatch.EntityBatches.Count, Enum.GetName(typeof(SyncSide), jobBatch.SyncSide));

            foreach (EntityBatch entityBatch in jobBatch.EntityBatches)
            {
                ProcessBatch(entityBatch);
            }
        }

        public void ProcessBatch(EntityBatch entityBatch)
        {
            SyncEngineLogger.WriteByParallelTaskContext(LogEntryType.Info, this.AssociatedDataSource, () =>
            {
                if (entityBatch.EntityDefinition.TechnicalEntityName == entityBatch.EntityDefinition.UserFriendlyEntityName)
                {
                    return string.Format("Processing {0} insert(s), {1} update(s), and {2} deletion(s) for entity batch for entity '{3}'.",
                                                        entityBatch.RecordsToAdd.Count, entityBatch.RecordsToUpdate.Count, entityBatch.RecordsToDelete.Count,
                                                        entityBatch.EntityDefinition.TechnicalEntityName);
                }
                else
                {
                    return string.Format("Processing {0} insert(s), {1} update(s), and {2} deletion(s) for entity batch for entity '{3}' ({4}).",
                                                        entityBatch.RecordsToAdd.Count, entityBatch.RecordsToUpdate.Count, entityBatch.RecordsToDelete.Count,
                                                        entityBatch.EntityDefinition.TechnicalEntityName, entityBatch.EntityDefinition.UserFriendlyEntityName);
                }

            });

            if (!(entityBatch.EntityDefinition.PrimaryKeyGenerationType == Data.PrimaryKeyGenerationType.AutoGenerate ||
                entityBatch.EntityDefinition.PrimaryKeyGenerationType == Data.PrimaryKeyGenerationType.Manual ||
                entityBatch.EntityDefinition.PrimaryKeyGenerationType == Data.PrimaryKeyGenerationType.Custom))
                throw new EnumValueNotImplementedException<PrimaryKeyGenerationType>(entityBatch.EntityDefinition.PrimaryKeyGenerationType);

            var tableName = entityBatch.EntityDefinition.TechnicalEntityName;

            ProcessInserts(entityBatch.RecordsToAdd, entityBatch, tableName);

            ProcessUpdates(entityBatch.RecordsToUpdate, entityBatch, tableName);

            ProcessDeletions(entityBatch.RecordsToDelete, entityBatch, tableName);

            entityBatch.HasBeenProcessed = true;
        }

        private void ProcessInserts(BindingList<RecordToAdd> recordsToAdd, EntityBatch entityBatch, string tableName)
        {
            for (int i = 0; i < recordsToAdd.Count; i++)
            {
                var recordToAdd = recordsToAdd[i];

                SqlInsertFactory insertFactory;
                if (entityBatch.EntityDefinition.PrimaryKeyGenerationType == PrimaryKeyGenerationType.Custom)
                    insertFactory = new SqlInsertFactory(tableName, entityBatch.EntityDefinition.PrimaryKeyColumnNames[0], entityBatch.EntityDefinition.CustomCommand, recordToAdd.FieldValuePairs);
                else
                    insertFactory = new SqlInsertFactory(tableName, entityBatch.EntityDefinition.PrimaryKeyColumnNames[0], recordToAdd.FieldValuePairs);

                if (entityBatch.EntityDefinition.AutoBindingForeignKeys != null)
                {
                    foreach (var autoBindingForeignKey in entityBatch.EntityDefinition.AutoBindingForeignKeys)
                    {

                        EntityRecord parentRecord = null;
                        string primaryKeyValue = null;

                        try
                        {
                            parentRecord = recordToAdd.Parents[autoBindingForeignKey.Relationship.Name];
                        }
                        catch (Exception ex)
                        {
                            throw new Exception(string.Format("Relationship with name '{0}' was not found within parents for entity record '{1}' with primary key field '{2}' to bind to parent field(s) '{3}'.  Data only values for the failed record are: {4}.",
                                                                autoBindingForeignKey.Relationship.Name, entityBatch.EntityDefinition.TechnicalEntityName,
                                                                autoBindingForeignKey.FieldNameToUpdate, StringHelper.GetDelimitedString(autoBindingForeignKey.Relationship.ChildFieldNamesToMatchOn),
                                                                recordToAdd.GetDataOnlyValuesAsText()), ex);

                        }

                        if (parentRecord is EntityRecordWithDataChange)
                        {
                            var parentRecordWithChange = (EntityRecordWithDataChange)parentRecord;

                            if (parentRecordWithChange.HasError)
                            {
                                recordsToAdd[i].HasError = true;
                                recordsToAdd[i].ErrorMessage = "See parent record for error details.";
                                continue;
                            }
                        }

                        try
                        {
                            primaryKeyValue = parentRecord.PrimaryKeyValues[autoBindingForeignKey.ParentPrimaryKeyColumnIdx];
                        }
                        catch (Exception ex)
                        {
                            throw new Exception(string.Format("Primary key index '{0}' was not found for parent entity record '{1}' with primary key value(s) '{2}' and data only field(s): {3}.",
                                                                autoBindingForeignKey.Relationship.Name, entityBatch.EntityDefinition.TechnicalEntityName,
                                                                StringHelper.GetDelimitedString(parentRecord.PrimaryKeyValues),
                                                                parentRecord.GetDataOnlyValuesAsText()), ex);
                        }

                        insertFactory.AddField(autoBindingForeignKey.FieldNameToUpdate, primaryKeyValue);
                    }
                }

                string recordId = "";

                try
                {
                    recordsToAdd[i].CommandText = insertFactory.GetSQL();

                    recordId = ExecuteCommand(recordsToAdd[i].CommandText).ToString();

                    recordsToAdd[i].PrimaryKeyValues.Add(recordId);
                }
                catch (Exception ex)
                {
                    recordsToAdd[i].HasError = true;

                    recordsToAdd[i].ErrorMessage = ExceptionFormatter.Format(ex);

                    recordsToAdd[i].Exception = ex;
                }
                finally
                {
                    recordsToAdd[i].HasBeenProcessed = true;
                }
            }
        }

        private void ProcessUpdates(BindingList<RecordToUpdate> recordsToUpdate, EntityBatch entityBatch, string tableName)
        {
            for (int i = 0; i < recordsToUpdate.Count; i++)
            {
                var recordToUpdate = recordsToUpdate[i];

                var updateFactory = new SqlUpdateFactory(tableName, entityBatch.EntityDefinition.PrimaryKeyColumnNames[0],
                                                         recordToUpdate.PrimaryKeyValues[0].ToString());

                updateFactory.FieldValuePairs = recordsToUpdate[i].FieldValuePairs;

                var sql = updateFactory.GetSQL();

                recordToUpdate.CommandText = sql;

                try
                {
                    ExecuteCommand(sql);
                }
                catch (Exception ex)
                {
                    recordToUpdate.HasError = true;
                    recordToUpdate.ErrorMessage = ExceptionFormatter.Format(ex);
                    recordToUpdate.Exception = ex;
                }

                recordToUpdate.HasBeenProcessed = true;
            }
        }

        private void ProcessDeletions(BindingList<RecordToDelete> recordsToDelete, EntityBatch entityBatch, string tableName)
        {
            if (recordsToDelete.Count > 0 && entityBatch.EntityDefinition.PrimaryKeyColumnNames.Count > 1)
                throw new NotImplementedException("Support for multiple primary keys is not implemented.");

            var deleteSql = string.Format(@"DELETE FROM {0} WHERE {1}", tableName, entityBatch.EntityDefinition.PrimaryKeyColumnNames[0]) + " = '{0}'";

            for (int i = 0; i < recordsToDelete.Count; i++)
            {
                var sql = string.Format(deleteSql, recordsToDelete[i].PrimaryKeyValues[0]);

                try
                {
                    if (recordsToDelete[i].FieldValuePairs.Count > 0)
                        throw new Exception("RecordToDelete with field/value pairs is not implemented.");

                    recordsToDelete[i].CommandText = sql;
                    ExecuteCommand(sql);
                }
                catch (Exception ex)
                {
                    recordsToDelete[i].HasError = true;
                    recordsToDelete[i].ErrorMessage = ExceptionFormatter.Format(ex);
                    recordsToDelete[i].Exception = ex;
                }

                recordsToDelete[i].HasBeenProcessed = true;
            }
        }

        private DataSet GetDataSet(string sql)
        {
            SqlCommand comm = new SqlCommand(sql, GetActiveSqlConnection());
            comm.CommandTimeout = 300; // 5 minutes

            SqlDataAdapter sqlAdapter = new SqlDataAdapter(comm);
            sqlAdapter.SelectCommand.CommandTimeout = 900; // 15 minutes

            DataSet ds = new DataSet();
            sqlAdapter.Fill(ds);

            return ds;
        }

        public override object ExecuteCommand(object commandObj)
        {
            if (commandObj == null)
                throw new Exception("Command object for SQL Server adapter can not be null.");
            else if (commandObj.GetType() != typeof(string))
                throw new Exception("Command object for SQL Server adapter must be of type System.String.");

            var commandText = (string)commandObj;

            SyncEngineLogger.WriteByParallelTaskContext(LogEntryType.Debug, this.AssociatedDataSource, "Command text to execute: {0}", commandText);

            SqlCommand comm = new SqlCommand(commandText, GetActiveSqlConnection());
            comm.CommandTimeout = 900; // 15 minutes

            var returnedValue = comm.ExecuteScalar();

            return returnedValue;
        }

        public string GetVersion()
        {
            SqlCommand comm = new SqlCommand(@"SELECT @@VERSION", GetActiveSqlConnection());

            var version = comm.ExecuteScalar();

            return version.ToString();
        }

        private SqlConnection GetActiveSqlConnection()
        {
            if (_activeSqlConnection == null || _activeSqlConnection.State != ConnectionState.Open)
            {
                _activeSqlConnection = new SqlConnection(Properties["ConnectionString"].ToString());
                _activeSqlConnection.Open();
            }

            return _activeSqlConnection;
        }

        private void CloseActiveSqlConnection()
        {
            if (_activeSqlConnection != null && _activeSqlConnection.State == ConnectionState.Open)
                _activeSqlConnection.Close();

            _activeSqlConnection = null;
        }

        public override void Dispose()
        {
            CloseActiveSqlConnection();
        }
    }
}

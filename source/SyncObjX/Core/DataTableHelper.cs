using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using SyncObjX.Data;
using SyncObjX.Exceptions;
using SyncObjX.Logging;
using SyncObjX.Util;

namespace SyncObjX.Core
{
    public class DataTableHelper
    {
        public const string SOURCE_PREFIX = "source_";

        public const string TARGET_PREFIX = "target_";

        /// <summary>
        /// Returns a combined DataTable using the specified join.
        /// </summary>
        /// <returns></returns>
        public static DataTable JoinTables(DataTable sourceTable, DataTable targetTable, JoinFieldCollection joinKeysCollection,
                                           JoinType joinType = JoinType.OuterJoin, string sourceSidePrefix = SOURCE_PREFIX, string targetSidePrefix = TARGET_PREFIX)
        {
            if (joinType == JoinType.OuterJoin)
                return ApplyFullOuterJoin(sourceTable, targetTable, joinKeysCollection, sourceSidePrefix, targetSidePrefix);
            else if (joinType == JoinType.LeftJoin)
                return ApplyLeftJoin(sourceTable, targetTable, joinKeysCollection, sourceSidePrefix, targetSidePrefix);
            else
                throw new EnumValueNotImplementedException<JoinType>(joinType);
        }

        public static DataTable ApplyLeftJoin(DataTable sourceTable, DataTable targetTable, JoinFieldCollection joinKeysCollection,
                                              string leftSidePrefix = SOURCE_PREFIX, string rightSidePrefix = TARGET_PREFIX)
        {
            if (leftSidePrefix == null)
                leftSidePrefix = "";

            if (rightSidePrefix == null)
                rightSidePrefix = "";

            if (leftSidePrefix.Trim() == rightSidePrefix.Trim())
                throw new Exception("Left and right side prefixes can not be the same.");

            if (SourceKeyColumnsExist(sourceTable, joinKeysCollection) && TargetKeyColumnsExist(targetTable, joinKeysCollection))
            {
                // modify source and target columns to include prefixes
                AddPrefixToColumnNames(leftSidePrefix, sourceTable);
                AddPrefixToColumnNames(rightSidePrefix, targetTable);

                // create a dataset and combined data table
                DataSet ds = new DataSet();
                ds.Tables.Add("Combined");
                DataTable combinedTable = ds.Tables[0];

                // add source table columns
                foreach (DataColumn dc in sourceTable.Columns)
                    combinedTable.Columns.Add(dc.ToString(), dc.DataType);

                // add target table columms
                foreach (DataColumn dc in targetTable.Columns)
                    combinedTable.Columns.Add(dc.ToString(), dc.DataType);

                // clone the schema
                var unmatchingTargetRowsTable = combinedTable.Clone();

                // add source data to combined table
                foreach (DataRow sourceRow in sourceTable.Rows)
                    combinedTable.ImportRow(sourceRow);

                // set the table's primary key; a key is required for Rows.Find()
                SetTargetPrimaryKeys(targetTable, joinKeysCollection, rightSidePrefix);
                var searchTable = targetTable;

                // apply full outer join between source and target tables
                foreach (DataRow row in combinedTable.Rows)
                {
                    var dr = searchTable.Rows.Find(GetKeyValues(row, joinKeysCollection, SyncSide.Source, leftSidePrefix));

                    // if a row matched, merge the row
                    if (dr != null)
                    {
                        foreach (DataColumn dc in searchTable.Columns)
                            row[dc.ColumnName] = dr[dc.ColumnName];
                    }
                }

                // remove the key from the combined table
                combinedTable.PrimaryKey = null;

                // remove source and target column prefixes
                RemovePrefixFromColumnNames(leftSidePrefix, sourceTable);
                RemovePrefixFromColumnNames(rightSidePrefix, targetTable);

                return combinedTable;
            }
            else
                throw new Exception("One or more join keys are missing from the source and target DataTable objects.");
        }

        private static DataTable ApplyFullOuterJoin(DataTable sourceTable, DataTable targetTable, JoinFieldCollection joinKeys,
                                                    string sourceSidePrefix = SOURCE_PREFIX, string targetSidePrefix = TARGET_PREFIX)
        {
            if (sourceSidePrefix == null)
                sourceSidePrefix = "";

            if (targetSidePrefix == null)
                targetSidePrefix = "";

            if (sourceSidePrefix.Trim() == targetSidePrefix.Trim())
                throw new Exception("Left and right side prefixes can not be the same.");

            if (SourceKeyColumnsExist(sourceTable, joinKeys) && TargetKeyColumnsExist(targetTable, joinKeys))
            {
                // modify source and target columns to include prefixes
                AddPrefixToColumnNames(sourceSidePrefix, sourceTable);
                AddPrefixToColumnNames(targetSidePrefix, targetTable);

                // create a dataset and combined data table
                DataSet ds = new DataSet();
                ds.Tables.Add("Combined");
                DataTable combinedTable = ds.Tables[0];

                // add source table columns
                foreach (DataColumn dc in sourceTable.Columns)
                    combinedTable.Columns.Add(dc.ToString(), dc.DataType);

                // add target table columms
                foreach (DataColumn dc in targetTable.Columns)
                    combinedTable.Columns.Add(dc.ToString(), dc.DataType);

                // add source data to combined table
                foreach (DataRow sourceRow in sourceTable.Rows)
                    combinedTable.ImportRow(sourceRow);

                // clone the schema
                var unmatchingTargetRowsTable = combinedTable.Clone();

                // set the source table's primary key as the key for the combined table; a key is required for Rows.Find()
                SetCombinedPrimaryKeys(combinedTable, joinKeys, SyncSide.Source, sourceSidePrefix);

                // apply full outer join between source and target tables
                foreach (DataRow targetRow in targetTable.Rows)
                {
                    var dr = combinedTable.Rows.Find(GetKeyValues(targetRow, joinKeys, SyncSide.Target, targetSidePrefix));

                    // if a row from the source table matched the target row's key, join the target data
                    if (dr != null)
                    {
                        foreach (DataColumn dc in targetTable.Columns)
                            dr[dc.ColumnName] = targetRow[dc.ColumnName];
                    }
                    else
                    {
                        //if a matching record wasn't found, add the target row to as a new combined row
                        var newRow = unmatchingTargetRowsTable.NewRow();

                        foreach (DataColumn dc in targetTable.Columns)
                            newRow[dc.ColumnName] = targetRow[dc.ColumnName];

                        unmatchingTargetRowsTable.Rows.Add(newRow);
                    }
                }

                // remove the key from the combined table
                combinedTable.PrimaryKey = null;

                foreach (DataRow targetOnlyRow in unmatchingTargetRowsTable.Rows)
                {
                    combinedTable.Rows.Add(targetOnlyRow.ItemArray);
                }

                // remove source and target column prefixes
                RemovePrefixFromColumnNames(sourceSidePrefix, sourceTable);
                RemovePrefixFromColumnNames(targetSidePrefix, targetTable);

                return combinedTable;
            }
            else
                throw new Exception("One or more join keys are missing from the source and target DataTable objects.");
        }


        private static bool SourceKeyColumnsExist(DataTable sourceTable, JoinFieldCollection joinKeyRelationships)
        {
            foreach (JoinFieldPair jkp in joinKeyRelationships.JoinFields)
            {
                if (!sourceTable.Columns.Contains(jkp.SourceJoinField))
                    throw new Exception(string.Format("Join column '{0}' not found in source DataTable.", jkp.SourceJoinField));
            }

            return true;
        }

        private static bool TargetKeyColumnsExist(DataTable targetTable, JoinFieldCollection joinKeyRelationships)
        {
            foreach (JoinFieldPair jkp in joinKeyRelationships.JoinFields)
            {
                if (!targetTable.Columns.Contains(jkp.TargetJoinField))
                    throw new Exception(string.Format("Join column '{0}' not found in target DataTable.", jkp.TargetJoinField));
            }

            return true;
        }

        private static void SetSourcePrimaryKeys(DataTable sourceTable, JoinFieldCollection joinKeysCollection, string sourceSidePrefix)
        {
            var sourceJoinKeys = new DataColumn[joinKeysCollection.Count];

            for (int i = 0; i < joinKeysCollection.Count; i++)
            {
                sourceJoinKeys[i] = sourceTable.Columns[sourceSidePrefix + joinKeysCollection.JoinFields[i].SourceJoinField];
            }

            try
            {
                sourceTable.PrimaryKey = sourceJoinKeys;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("The source-side data has one or more duplicate primary key value(s) for column(s) '{0}'.",
                                                  StringHelper.GetDelimitedString(joinKeysCollection.JoinFields.Select(d => d.SourceJoinField))), ex);
            }
        }

        private static void SetTargetPrimaryKeys(DataTable targetTable, JoinFieldCollection joinKeysCollection, string targetSidePrefix)
        {
            var targetJoinKeys = new DataColumn[joinKeysCollection.Count];

            for (int i = 0; i < joinKeysCollection.Count; i++)
            {
                targetJoinKeys[i] = targetTable.Columns[targetSidePrefix + joinKeysCollection.JoinFields[i].TargetJoinField];
            }

            try
            {
                targetTable.PrimaryKey = targetJoinKeys;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("The target-side data has one or more duplicate primary key value(s) for column(s) '{0}'.",
                                                  StringHelper.GetDelimitedString(joinKeysCollection.JoinFields.Select(d => d.TargetJoinField))), ex);
            }
        }

        private static void SetCombinedPrimaryKeys(DataTable combinedTable, JoinFieldCollection joinKeysCollection, SyncSide syncSide, string joinSidePrefix)
        {
            var combinedJoinKeys = new DataColumn[joinKeysCollection.Count];

            for (int i = 0; i < joinKeysCollection.Count; i++)
            {
                if (syncSide == SyncSide.Source)
                    combinedJoinKeys[i] = combinedTable.Columns[joinSidePrefix + joinKeysCollection.JoinFields[i].SourceJoinField];
                else if (syncSide == SyncSide.Target)
                    combinedJoinKeys[i] = combinedTable.Columns[joinSidePrefix + joinKeysCollection.JoinFields[i].TargetJoinField];
                else
                    throw new EnumValueNotImplementedException<SyncSide>(syncSide);
            }

            try
            {
                combinedTable.PrimaryKey = combinedJoinKeys;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("The {0}-side data has one or more duplicate primary key value(s) for column(s) '{1}'.",
                                                  Enum.GetName(typeof(SyncSide), syncSide).ToLower(),
                                                  StringHelper.GetDelimitedString(joinKeysCollection.JoinFields.Select(d => d.SourceJoinField))), ex);
            }

            // set the key to allow Nulls (otherwise, inserting target data w/o a matching source record will error)
            foreach (DataColumn col in combinedTable.PrimaryKey)
            {
                col.AllowDBNull = true;
            }
        }

        private static object[] GetKeyValues(DataRow row, JoinFieldCollection joinKeysCollection, SyncSide syncSide, string joinSidePrefix)
        {
            var keyValues = new object[joinKeysCollection.Count];

            for (int i = 0; i < joinKeysCollection.Count; i++)
            {
                if (syncSide == SyncSide.Source)
                    keyValues[i] = row[joinSidePrefix + joinKeysCollection.JoinFields[i].SourceJoinField];
                else if (syncSide == SyncSide.Target)
                    keyValues[i] = row[joinSidePrefix + joinKeysCollection.JoinFields[i].TargetJoinField];
                else
                    throw new EnumValueNotImplementedException<SyncSide>(syncSide);
            }

            return keyValues;
        }

        /// <summary>
        /// Cleans the table of duplicates and returns a collection of keys for removed rows.
        /// </summary>
        /// <returns></returns>
        public static HashSet<RecordKeyCombo> RemoveDuplicates(DataTable tbl, string uniqueKeyColumnName, DuplicateRowBehavior duplicateRowBehavior = DuplicateRowBehavior.ThrowException)
        {
            return RemoveDuplicates(null, tbl, new List<string>() { uniqueKeyColumnName }, duplicateRowBehavior);
        }

        /// <summary>
        /// Cleans the table of duplicates and returns a collection of keys for removed rows.
        /// </summary>
        /// <returns></returns>
        public static HashSet<RecordKeyCombo> RemoveDuplicates(DataTable tbl, List<string> uniqueKeyColumnNames, DuplicateRowBehavior duplicateRowBehavior = DuplicateRowBehavior.ThrowException)
        {
            return RemoveDuplicates(null, tbl, uniqueKeyColumnNames, duplicateRowBehavior);
        }

        /// <summary>
        /// Cleans the table of duplicates and returns a collection of keys for removed rows.
        /// </summary>
        /// <returns></returns>
        public static HashSet<RecordKeyCombo> RemoveDuplicates(SyncSide? syncSide, DataTable tbl, string uniqueKeyColumnName, DuplicateRowBehavior duplicateRowBehavior = DuplicateRowBehavior.ThrowException)
        {
            return RemoveDuplicates(syncSide, tbl, new List<string>() { uniqueKeyColumnName }, duplicateRowBehavior);
        }

        /// <summary>
        /// Cleans the table of duplicates and returns a collection of keys for removed rows.
        /// </summary>
        /// <returns></returns>
        public static HashSet<RecordKeyCombo> RemoveDuplicates(SyncSide? syncSide, DataTable tbl, List<string> uniqueKeyColumnNames, DuplicateRowBehavior duplicateRowBehavior = DuplicateRowBehavior.ThrowException)
        {
            if (tbl == null)
                throw new Exception("Table can not be null.");

            if (uniqueKeyColumnNames == null || uniqueKeyColumnNames.Count == 0)
                throw new Exception("At least one unique key column name is required.");

            if (uniqueKeyColumnNames.Distinct(StringComparer.OrdinalIgnoreCase).Count() < uniqueKeyColumnNames.Count)
                throw new Exception("All key column names must be unique.");

            var rowsByKey = new Dictionary<RecordKeyCombo, DataRow>(new RecordKeyComboComparer());

            var duplicateRows = new HashSet<DataRow>();

            var duplicateKeys = new HashSet<RecordKeyCombo>(new RecordKeyComboComparer());

            foreach (DataRow row in tbl.Rows)
            {
                var rowKeys = GetKeys(row, uniqueKeyColumnNames);

                if (rowsByKey.ContainsKey(rowKeys))
                {
                    duplicateKeys.Add(rowKeys);

                    duplicateRows.Add(row);

                    if (duplicateRowBehavior == DuplicateRowBehavior.RemoveAllDuplicateRows)
                        duplicateRows.Add(rowsByKey[rowKeys]);
                }
                else
                    rowsByKey[rowKeys] = row;
            }

            if (duplicateRows.Count > 0)
            {
                if (duplicateRowBehavior == DuplicateRowBehavior.ThrowException)
                {
                    if (syncSide.HasValue)
                        throw new Exception(GetDuplicateRowsExceptionMessage(syncSide.Value, tbl.TableName, uniqueKeyColumnNames, duplicateKeys));
                    else
                        throw new Exception(GetDuplicateRowsExceptionMessage(tbl.TableName, uniqueKeyColumnNames, duplicateKeys));
                }
                else
                {
                    SyncEngineLogger.WriteByParallelTaskContext(LogEntryType.Warning, () =>
                        {
                            var msg = new StringBuilder();

                            if (syncSide.HasValue)
                                msg.Append(GetDuplicateRowsExceptionMessage(syncSide.Value, tbl.TableName, uniqueKeyColumnNames, duplicateKeys));
                            else
                                msg.Append(GetDuplicateRowsExceptionMessage(tbl.TableName, uniqueKeyColumnNames, duplicateKeys));

                            if (duplicateRowBehavior == DuplicateRowBehavior.RemoveAllDuplicateRows)
                                msg.Append(" All duplicate rows were removed.");
                            else if (duplicateRowBehavior == DuplicateRowBehavior.RemoveDuplicateRowsExceptFirst)
                                msg.Append(" All duplicate rows were removed except for the first row for each set of unique keys.");

                            return msg.ToString();
                        });
                }
            }

            foreach (var duplicateRow in duplicateRows)
            {
                tbl.Rows.Remove(duplicateRow);
            }

            return duplicateKeys;
        }

        public static string GetDuplicateRowsExceptionMessage(SyncSide syncSide, string tableName, List<string> uniqueKeyColumnNames, HashSet<RecordKeyCombo> duplicateKeys)
        {
            if (uniqueKeyColumnNames.Count == 1)
            {
                return string.Format("The {0}-side data with table name '{1}' has one or more duplicate key value(s) for column '{2}' and value(s): '{3}'.",
                                     Enum.GetName(typeof(SyncSide), syncSide).ToLower(), tableName, uniqueKeyColumnNames[0],
                                     StringHelper.GetDelimitedString(duplicateKeys.Select(d => d.Key1), "', '"));
            }
            else
            {
                return string.Format("The {0}-side data with table name '{1}' has one or more duplicate key value(s) for columns '{2}' and value(s): ({3}).",
                                     Enum.GetName(typeof(SyncSide), syncSide).ToLower(), tableName,
                                     StringHelper.GetDelimitedString(uniqueKeyColumnNames),
                                     StringHelper.GetDelimitedString(duplicateKeys, "), ("));
            }
        }

        public static string GetDuplicateRowsExceptionMessage(string tableName, List<string> uniqueKeyColumnNames, HashSet<RecordKeyCombo> duplicateKeys)
        {
            if (uniqueKeyColumnNames.Count == 1)
            {
                return string.Format("The data table with name '{0}' has one or more duplicate key value(s) for column '{1}' and value(s): '{2}'.",
                                     tableName, uniqueKeyColumnNames[0],
                                     StringHelper.GetDelimitedString(duplicateKeys.Select(d => d.Key1), "', '"));
            }
            else
            {
                return string.Format("The data table with name '{0}' has one or more duplicate key value(s) for columns '{1}' and value(s): ({2}).",
                                     tableName,
                                     StringHelper.GetDelimitedString(uniqueKeyColumnNames),
                                     StringHelper.GetDelimitedString(duplicateKeys, "), ("));
            }
        }

        public static RecordKeyCombo GetKeys(DataRow row, List<string> uniqueKeyColumnNames)
        {
            var keyValues = new List<string>();

            foreach (var uniqueKeyColumnName in uniqueKeyColumnNames)
            {
                var keyValue = row[uniqueKeyColumnName];

                if (keyValue == null)
                    keyValues.Add(null);
                else
                    keyValues.Add(keyValue.ToString());
            }

            return new RecordKeyCombo(keyValues);
        }

        /// <summary>
        /// Updates DataTable column names to include a prefix.
        /// </summary>
        /// <param name="prefix">The column prefix to add to the existing column name.</param>
        /// <param name="tbl">The table to update.</param>
        public static void AddPrefixToColumnNames(string prefix, DataTable tbl)
        {
            foreach (DataColumn dc in tbl.Columns)
                dc.ColumnName = prefix + dc.ColumnName;
        }

        public static void RemovePrefixFromColumnNames(string prefix, DataTable tbl)
        {
            if (prefix == "")
                return;

            foreach (DataColumn dc in tbl.Columns)
                dc.ColumnName = dc.ColumnName.Replace(prefix, "");
        }

        public static void AddMissingDataColumns(DataTable tbl, IEnumerable<string> columns)
        {
            List<string> addedColumns = new List<string>();

            foreach (var column in columns)
            {
                if (!tbl.Columns.Contains(column))
                {
                    tbl.Columns.Add(column);

                    addedColumns.Add(column);
                }
            }

            SyncEngineLogger.WriteByParallelTaskContext(LogEntryType.Debug, () =>
            {
                StringBuilder logMessage = new StringBuilder();

                logMessage.AppendLine("Missing data columns added to data:");

                foreach (var column in addedColumns)
                    logMessage.AppendLine(column);

                return logMessage.ToString();
            });
        }

        public static DataTable ApplyFilter(DataTable sourceTbl, Func<DataRow, bool> rowsToInclude)
        {
            var rows = sourceTbl.Rows.Cast<DataRow>().Where(d => rowsToInclude(d));

            DataTable tbl;

            if (rows.Count() > 0)
                tbl = rows.CopyToDataTable();
            else
                tbl = sourceTbl.Clone(); // clone schema

            return tbl;
        }

        public static DataTable GetDataMapSafeTable(DataTable schemaSafeTable, IEnumerable<DataRow> rows)
        {
            DataTable tbl;

            if (rows.Count() > 0)
                tbl = rows.CopyToDataTable();
            else
                tbl = schemaSafeTable.Clone(); // clone schema

            return tbl;
        }
    }
}

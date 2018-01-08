using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SyncObjX.Adapters.PostgreSQL.Data
{
    public class PostgresSqlInsertFactory
    {
        string _tableName;
        List<string> _primaryKeyColumnNames = new List<string>();
        List<string> _primaryKeyValues = new List<string>();
        bool _isAutoPrimaryKey;

        public Dictionary<string, string> FieldsAndValues = new Dictionary<string, string>();

        public string TableName
        {
            set { _tableName = value; }
            get { return _tableName; }
        }

        public IEnumerable<string> PrimaryKeyColumnNames
        {
            set
            {
                if (value == null)
                    _primaryKeyColumnNames = new List<string>();
                else
                    _primaryKeyColumnNames = value.ToList();
            }
            get { return _primaryKeyColumnNames; }
        }

        public IEnumerable<string> PrimaryKeyValues
        {
            set
            {
                if (value == null)
                    _primaryKeyValues = new List<string>();
                else
                    _primaryKeyValues = value.ToList();
            }
            get { return _primaryKeyValues; }
        }

        public bool IsAutoPrimaryKey
        {
            set { _isAutoPrimaryKey = value; }
            get { return _isAutoPrimaryKey; }
        }

        public object CustomCommand;

        /// <summary>
        /// Auto-generates primary key.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="primaryKeyColumnName"></param>
        /// <param name="fieldsAndValues"></param>
        public PostgresSqlInsertFactory(string tableName, string primaryKeyColumnName, Dictionary<string, string> fieldsAndValues)
        {
            IsAutoPrimaryKey = true;

            TableName = tableName;

            PrimaryKeyColumnNames = new List<string>() { primaryKeyColumnName };

            FieldsAndValues = fieldsAndValues;
        }

        public PostgresSqlInsertFactory(string tableName, string primaryKeyColumnName, string primaryKeyValue, Dictionary<string, string> fieldsAndValues)
        {
            IsAutoPrimaryKey = false;

            TableName = tableName;

            PrimaryKeyColumnNames = new List<string>() { primaryKeyColumnName };

            PrimaryKeyValues = new List<string>() { primaryKeyValue };

            FieldsAndValues = fieldsAndValues;
        }

        public PostgresSqlInsertFactory(string tableName, List<string> primaryKeyColumnNames, List<string> primaryKeyValues, Dictionary<string, string> fieldsAndValues)
        {
            IsAutoPrimaryKey = false;

            TableName = tableName;

            PrimaryKeyColumnNames = primaryKeyColumnNames;

            PrimaryKeyValues = primaryKeyValues;

            FieldsAndValues = fieldsAndValues;
        }

        public PostgresSqlInsertFactory(string tableName, string primaryKeyColumn, object customCommand, Dictionary<string, string> fieldsAndValues)
        {
            IsAutoPrimaryKey = false;

            TableName = tableName;

            PrimaryKeyColumnNames = new List<string> { primaryKeyColumn };

            PrimaryKeyValues = new List<string>();

            FieldsAndValues = fieldsAndValues;

            CustomCommand = customCommand;
        }

        public void AddField(string Field, string NewValue)
        {
            FieldsAndValues.Add(Field, NewValue);
        }

        public void RemoveField(string Field)
        {
            try
            {
                FieldsAndValues.Remove(Field);
            }
            catch { throw; }
        }

        public string GetSQL()
        {
            try
            {
                return GetSQL(true);
            }
            catch { throw; }
        }

        public string GetSQL(bool addPrimaryKeyOutput)
        {
            try
            {
                if (CustomCommand == null && !IsAutoPrimaryKey && _primaryKeyColumnNames.Count != _primaryKeyValues.Count)
                    throw new Exception("Count of primary key column names and values must match.");

                if (_primaryKeyColumnNames.Count > 1 && IsAutoPrimaryKey)
                    throw new NotImplementedException("Support for multiple primary keys is not implemented.");

                //-- multiple Primary Keys returned
                //DECLARE  @result table(TestTableId1 varchar(100), TestTableId2 varchar(100))

                //INSERT INTO TestTable
                //OUTPUT INSERTED.TestTableId1, INSERTED.TestTableId2 INTO @result
                //VALUES (6, 2, 'Test')

                //SELECT * FROM @result

                //-- one primary key returned
                //INSERT INTO TestTable
                //OUTPUT INSERTED.TestTableId1
                //VALUES (5, 2, 'Test')

                StringBuilder sql = new StringBuilder();

                if (CustomCommand != null)
                    throw new NotImplementedException("Support for custom primary keys is not implemented.");

                //if (addPrimaryKeyOutput && IsAutoPrimaryKey)
                //    sql.AppendLine("DECLARE @T TABLE (TableId int)");
                //else 
                //if (CustomCommand != null && CustomCommand is string)
                //    sql.AppendLine(CustomCommand.ToString());

                string primaryKeyColumnKeyNameInFieldsAndValues = null;
                bool primaryKeyColumnIsInFieldsAndValues = false;

                // if the primary key column already exists in the field collection, overwrite with the custom command's SQL variable
                foreach (var key in FieldsAndValues.Keys)
                {
                    if (key.Equals(_primaryKeyColumnNames[0], StringComparison.OrdinalIgnoreCase))
                    {
                        primaryKeyColumnKeyNameInFieldsAndValues = key;
                        primaryKeyColumnIsInFieldsAndValues = true;
                        break;
                    }
                }

                sql.Append("INSERT INTO ");
                sql.Append(_tableName);

                sql.Append(" (");

                if (_isAutoPrimaryKey == false && (CustomCommand == null || (CustomCommand != null && !primaryKeyColumnIsInFieldsAndValues)))
                {
                    sql.Append(_primaryKeyColumnNames[0]);
                    sql.Append(", ");
                }

                int fieldCount = 0;

                foreach (KeyValuePair<string, string> kvp in FieldsAndValues)
                {
                    if (fieldCount == 0)
                        sql.Append(kvp.Key);
                    else
                    {
                        sql.Append(", ");
                        sql.Append(kvp.Key);
                    }

                    fieldCount++;
                }

                sql.Append(") ");

                //if ((addPrimaryKeyOutput == true && IsAutoPrimaryKey) && CustomCommand == null)
                //{
                //    sql.Append(" OUTPUT INSERTED.");
                //    sql.Append(_primaryKeyColumnNames[0]);
                //    sql.Append(" INTO @T ");
                //}

                sql.Append(" VALUES (");

                if (CustomCommand != null && CustomCommand is string)
                {
                    if (primaryKeyColumnIsInFieldsAndValues)
                        FieldsAndValues.Remove(primaryKeyColumnKeyNameInFieldsAndValues);

                    //sql.Append("@TableId, ");
                }
                else if (_isAutoPrimaryKey == false && CustomCommand == null)
                {
                    sql.Append("'");
                    sql.Append(_primaryKeyValues[0].Replace("'", "''"));
                    sql.Append("', ");
                }

                fieldCount = 0;

                foreach (KeyValuePair<string, string> kvp in FieldsAndValues)
                {
                    if (fieldCount != 0)
                        sql.Append(", ");

                    if (kvp.Value != null && kvp.Value.StartsWith("SQL:"))
                        sql.AppendFormat("{0}", kvp.Value.Substring(4, kvp.Value.Length - 4));
                    else
                        sql.AppendFormat("{0}", kvp.Value == null ? "NULL" : "'" + kvp.Value.Replace("'", "''") + "'");

                    fieldCount++;
                }

                sql.Append(")");

                if (_isAutoPrimaryKey)
                    sql.AppendFormat(" RETURNING {0}", _primaryKeyColumnNames[0]);

                //if (CustomCommand != null && CustomCommand is string)
                //    sql.AppendLine().AppendLine("SELECT @TableId");
                //else if (addPrimaryKeyOutput && IsAutoPrimaryKey)
                //{
                //    sql.AppendLine();
                //    sql.AppendLine("SELECT * FROM @T");
                //}

                return sql.ToString();
            }
            catch { throw; }

        }

        public string GetDataChanges()
        {
            string DataChanges = "";

            foreach (KeyValuePair<string, string> kvp in FieldsAndValues)
            {
                if (DataChanges == "")
                    DataChanges += kvp.Key + " -- Old: ; New: " + kvp.Value;
                else
                    DataChanges += " | " + kvp.Key + " -- Old: ; New: " + kvp.Value;
            }

            return DataChanges;
        }

        public void Clear()
        {
            _tableName = "";
            _primaryKeyColumnNames = new List<string>();
            _primaryKeyValues = new List<string>();
            FieldsAndValues.Clear();
        }
    }
}

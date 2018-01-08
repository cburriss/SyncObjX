using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SyncObjX.Data
{
    public class SqlUpdateFactory
    {
        string _tableName;
        List<string> _primaryKeyColumnNames = new List<string>();
        List<string> _primaryKeyValues = new List<string>();

        public Dictionary<string, UpdateValueSet> FieldValuePairs = new Dictionary<string,UpdateValueSet>();

        public string TableName
        {
            set { _tableName = value; }
            get { return _tableName; }
        }

        public List<string> PrimaryKeyColumnNames
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

        public List<string> PrimaryKeyValues
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

        public SqlUpdateFactory(string tableName, string primaryKeyColumnName, string primaryKeyValue)
            : this(tableName, new List<string>() { primaryKeyColumnName }, new List<string>() { primaryKeyValue }) { }

        public SqlUpdateFactory(string tableName, List<string> primaryKeyColumnNames, List<string> primaryKeyValues)
        {
            TableName = tableName;

            PrimaryKeyColumnNames = primaryKeyColumnNames;

            PrimaryKeyValues = primaryKeyValues;
        }

        public void AddFields(IEnumerable<KeyValuePair<string, string>> fieldAndValuePairs)
        {
            foreach (var fieldValuePair in fieldAndValuePairs)
            {
                AddField(fieldValuePair.Key, fieldValuePair.Value);
            }
        }

        public void AddFields(IEnumerable<KeyValuePair<string, UpdateValueSet>> fieldAndValuePairs)
        {
            foreach (var fieldValuePair in fieldAndValuePairs)
            {
                FieldValuePairs.Add(fieldValuePair.Key, fieldValuePair.Value);
            }
        }

        public void AddField(string Field, string NewValue)
        {
            AddField(Field, "", NewValue);
        }

        public void AddField(string Field, string OldValue, string NewValue)
        {
            if (FieldValuePairs.ContainsKey(Field))
                throw new Exception("Field already exists.");
            
            UpdateValueSet valueSet = new UpdateValueSet(OldValue, NewValue);

            FieldValuePairs.Add(Field, valueSet);
        }

        public void RemoveField(string Field)
        {
            try
            {
                FieldValuePairs.Remove(Field);
            }
            catch { throw; }
        }

        public string GetSQL()
        {
            if (_primaryKeyColumnNames.Count != _primaryKeyValues.Count)
                throw new Exception("Count of primary key column names and values must match.");

            StringBuilder sql = new StringBuilder();

            if (FieldValuePairs.Count != 0)
            {
                sql.Append("UPDATE ");
                sql.Append(_tableName);
                sql.Append(" SET ");

                int fieldCount = 0;

                foreach (KeyValuePair<string, UpdateValueSet> kvp in FieldValuePairs)
                {
                    if (fieldCount != 0)
                        sql.Append(", ");

                    if (kvp.Value.NewValue != null && kvp.Value.NewValue.StartsWith("SQL:"))
                        sql.AppendFormat("{0} = {1}", kvp.Key, kvp.Value.NewValue.Substring(4, kvp.Value.NewValue.Length - 4));
                    else
                        sql.AppendFormat("{0} = {1}", kvp.Key, kvp.Value.NewValue == null ? "NULL" : "'" + kvp.Value.NewValue.Replace("'", "''") + "'");

                    fieldCount++;
                }

                if (_primaryKeyColumnNames.Count > 0)
                {
                    sql.Append(" WHERE ");
                    sql.Append(_primaryKeyColumnNames[0]);
                    sql.Append(" = '");
                    sql.Append(_primaryKeyValues[0]);
                    sql.Append("'");

                    for (int i = 1; i < _primaryKeyColumnNames.Count; i++)
                    {
                        sql.Append(" AND ");
                        sql.Append(_primaryKeyColumnNames[i]);
                        sql.Append(" = '");
                        sql.Append(_primaryKeyValues[i]);
                        sql.Append("'");
                    }
                }
            }

            return sql.ToString();
        }

        public void Clear()
        {
            _tableName = "";
            _primaryKeyColumnNames = new List<string>();
            _primaryKeyValues = new List<string>();
            FieldValuePairs.Clear();
        }
    }
}

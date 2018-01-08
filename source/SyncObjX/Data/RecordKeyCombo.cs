using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.Serialization;
using SyncObjX.Util;

namespace SyncObjX.Data
{
    [DataContract]
    public class RecordKeyCombo
    {
        string _key1;

        string _key2;

        string _key3;

        string _key4;

        string _key5;

        string _key6;

        string _key7;

        [DataMember]
        public string Key1
        {
            get { return _key1; }

            set
            {
                if (value == null)
                    _key1 = "";
                else
                    _key1 = value;
            }
        }

        [DataMember]
        public string Key2
        {
            get { return _key2; }

            set
            {
                if (value == null)
                    _key2 = "";
                else
                    _key2 = value;
            }
        }

        [DataMember]
        public string Key3
        {
            get { return _key3; }

            set
            {
                if (value == null)
                    _key3 = "";
                else
                    _key3 = value;
            }
        }

        [DataMember]
        public string Key4
        {
            get { return _key4; }

            set
            {
                if (value == null)
                    _key4 = "";
                else
                    _key4 = value;
            }
        }

        [DataMember]
        public string Key5
        {
            get { return _key5; }

            set
            {
                if (value == null)
                    _key5 = "";
                else
                    _key5 = value;
            }
        }

        [DataMember]
        public string Key6
        {
            get { return _key6; }

            set
            {
                if (value == null)
                    _key6 = "";
                else
                    _key6 = value;
            }
        }

        [DataMember]
        public string Key7
        {
            get { return _key7; }

            set
            {
                if (value == null)
                    _key7 = "";
                else
                    _key7 = value;
            }
        }

        [DataMember]
        public List<string> Keys
        {
            get
            {
                var keys = new List<string>();

                keys.Add(Key1);

                var key2 = Key2;

                if (key2 != null)
                    keys.Add(Key2);

                var key3 = Key3;

                if (key3 != null)
                    keys.Add(Key3);

                var key4 = Key4;

                if (key4 != null)
                    keys.Add(Key4);

                var key5 = Key5;

                if (key5 != null)
                    keys.Add(Key5);

                var key6 = Key6;

                if (key6 != null)
                    keys.Add(Key6);

                var key7 = Key7;

                if (key7 != null)
                    keys.Add(Key7);

                return keys;
            }

            private set { }
        }

        [DataMember]
        public List<string> AllKeys
        {
            get
            {
                var keys = new List<string>();

                keys.Add(Key1);
                keys.Add(Key2);
                keys.Add(Key3);
                keys.Add(Key4);
                keys.Add(Key5);
                keys.Add(Key6);
                keys.Add(Key7);

                return keys;
            }

            private set { }
        }

        public RecordKeyCombo(List<string> keys)
        {
            if (keys == null || keys.Count == 0)
                throw new Exception("At least one key is required.");

            if (keys.Count >= 1)
                Key1 = keys[0];

            if (keys.Count >= 2)
                Key2 = keys[1];

            if (keys.Count >= 3)
                Key3 = keys[2];

            if (keys.Count >= 4)
                Key4 = keys[3];

            if (keys.Count >= 5)
                Key5 = keys[4];

            if (keys.Count >= 6)
                Key6 = keys[5];

            if (keys.Count >= 7)
                Key7 = keys[6];

            if (keys.Count > 7)
                throw new Exception("Support for more than 7 keys is not implemented.");
        }

        public RecordKeyCombo(string key1)
            : this(key1, "", "", "", "", "", "") { }

        public RecordKeyCombo(string key1, string key2)
            : this(key1, key2, "", "", "", "", "") { }

        public RecordKeyCombo(string key1, string key2, string key3)
            : this(key1, key2, key3, "", "", "", "") { }

        public RecordKeyCombo(string key1, string key2, string key3, string key4)
            : this(key1, key2, key3, key4, "", "", "") { }

        public RecordKeyCombo(string key1, string key2, string key3, string key4, string key5)
            : this(key1, key2, key3, key4, key5, "", "") { }

        public RecordKeyCombo(string key1, string key2, string key3, string key4, string key5, string key6)
            : this(key1, key2, key3, key4, key5, key6, "") { }

        public RecordKeyCombo(string key1, string key2, string key3, string key4, string key5, string key6, string key7)
        {
            Key1 = key1;

            Key2 = key2;

            Key3 = key3;

            Key4 = key4;

            Key5 = key5;

            Key6 = key6;

            Key7 = key7;
        }

        public static RecordKeyCombo GetRecordKeyComboFromDataRow(DataRow row, List<string> keyColumnNames)
        {
            if (keyColumnNames == null || keyColumnNames.Count == 0)
                throw new Exception("At least one key column name is required.");

            var recordKeyCombo = new RecordKeyCombo((row.Field<object>(keyColumnNames[0]) ?? "").ToString());

            if (keyColumnNames.Count == 1)
                return recordKeyCombo;

            if (keyColumnNames.Count >= 2)
                recordKeyCombo.Key2 = (row.Field<object>(keyColumnNames[1]) ?? "").ToString();
            else
                return recordKeyCombo;

            if (keyColumnNames.Count >= 3)
                recordKeyCombo.Key3 = (row.Field<object>(keyColumnNames[2]) ?? "").ToString();
            else
                return recordKeyCombo;

            if (keyColumnNames.Count >= 4)
                recordKeyCombo.Key4 = (row.Field<object>(keyColumnNames[3]) ?? "").ToString();
            else
                return recordKeyCombo;

            if (keyColumnNames.Count >= 5)
                recordKeyCombo.Key5 = (row.Field<object>(keyColumnNames[4]) ?? "").ToString();
            else
                return recordKeyCombo;

            if (keyColumnNames.Count >= 6)
                recordKeyCombo.Key6 = (row.Field<object>(keyColumnNames[5]) ?? "").ToString();
            else
                return recordKeyCombo;

            if (keyColumnNames.Count >= 7)
                recordKeyCombo.Key7 = (row.Field<object>(keyColumnNames[6]) ?? "").ToString();
            else
                return recordKeyCombo;

            if (keyColumnNames.Count > 7)
                throw new Exception("Support for more than 7 keys is not implemented.");

            return recordKeyCombo;
        }

        public override string ToString()
        {
            return string.Format("'{0}'", StringHelper.GetDelimitedString(Keys, "','"));
        }
    }
}
using System;
using System.Runtime.Serialization;
using SyncObjX.Core;
using SyncObjX.Management;

namespace SyncObjX.SyncObjects
{
    [DataContract]
    public class JobDataSource
    {
        private RunHistory _history;

        [DataMember]
        public readonly DataSource DataSource;

        [DataMember]
        public readonly SyncSide SyncSide;

        [DataMember]
        public RunHistory History
        {
            get { return _history; }

            set
            {
                if (value == null)
                    throw new Exception("Update history can not be null.");
                else
                    _history = value;
            }
        }

        public JobDataSource(SyncSide syncSide, DataSource dataSource)
            : this(syncSide, dataSource, new RunHistory(new DateTime?(), new DateTime?(), new DateTime?(), new DateTime?(), new DateTime?(), new DateTime?())) { }

        public JobDataSource(SyncSide syncSide, DataSource dataSource, RunHistory history)
        {
            if (dataSource == null)
                throw new Exception("Data source can not be null.");

            DataSource = dataSource;

            SyncSide = syncSide;

            History = history;
        }
    }
}

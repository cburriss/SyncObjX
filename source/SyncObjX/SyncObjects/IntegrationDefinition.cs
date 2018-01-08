using System;
using System.IO;
using System.Runtime.Serialization;
using SyncObjX.Logging;

namespace SyncObjX.SyncObjects
{
    [DataContract]
    public class IntegrationDefinition : SyncObject
    {
        public static readonly int MaxConcurrentThreadsDefault = 1;

        public static readonly LoggingLevel LoggingLevelDefault = LoggingLevel.ErrorsWarningsAndInfo;

        public static readonly bool LogToDatabaseDefault = true;

        public static readonly int DaysOfDatabaseLoggingHistoryDefault = 30;

        public static readonly bool LogToFileDefault = false;

        public static readonly int DaysOfFileLoggingHistoryDefault = 30;

        public static readonly SyncObjectType SyncObjectType = SyncObjectType.Integration;

        private string _name;

        private string _description;

        private bool _isEnabled;

        private string _packageDllDirectory;

        private string _packageDllFilename;

        private string _sourceName;

        private string _targetName;

        private int _maxConcurrentThreads = MaxConcurrentThreadsDefault;

        private LoggingLevel _loggingLevel = LoggingLevelDefault;

        private bool _logToDatabase = LogToDatabaseDefault;

        private int _daysOfDatabaseLoggingHistory = DaysOfDatabaseLoggingHistoryDefault;

        private bool _logToFile = LogToFileDefault;

        private int _daysOfFileLoggingHistory = DaysOfFileLoggingHistoryDefault;

        [DataMember]
        public string Name
        {
            get { return _name; }
            
            set 
            {
                if (String.IsNullOrWhiteSpace(value))
                    throw new Exception("Integration name is missing or empty.");
                else
                    _name = value.Trim();
            }
        }

        [DataMember]
        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }

        [DataMember]
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { _isEnabled = value; }
        }

        [DataMember]
        public string PackageDllDirectory
        {
            get { return _packageDllDirectory; }
            
            set
            {
                if (String.IsNullOrWhiteSpace(value))
                    throw new Exception("Integration package directory is missing or empty. Provide the directory path to a valid DLL.");

                _packageDllDirectory = value.Trim();
            }
        }

        [DataMember]
        public string PackageDllFilename
        {
            get { return _packageDllFilename; }

            set
            {
                if (String.IsNullOrWhiteSpace(value) || !(Path.GetExtension(value).ToLower().Equals(".dll") || Path.GetExtension(value).ToLower().Equals(".exe")))
                    throw new Exception("Integration package filename is missing, empty, or invalid. Provide the filename for a valid DLL.");
                else
                    _packageDllFilename = value.Trim();
            }
        }

        [DataMember]
        public string PackageDllPathAndFilename
        {
            get { return PackageDllDirectory + "\\" + PackageDllFilename; }

            private set { }
        }

        [DataMember]
        public string SourceName
        {
            get { return _sourceName; }

            set
            {
                if (String.IsNullOrWhiteSpace(value))
                    throw new Exception("Source name is missing or empty.");
                else if (value == TargetName)
                    throw new Exception("Source and target names must be different.");
                else
                    _sourceName = value.Trim();
            }
        }

        [DataMember]
        public string TargetName
        {
            get { return _targetName; }

            set
            {
                if (String.IsNullOrWhiteSpace(value))
                    throw new Exception("Target name is missing or empty.");
                else if (value == SourceName)
                    throw new Exception("Source and target names must be different.");
                else
                    _targetName = value.Trim();
            }
        }

        [DataMember]
        public int MaxConcurrentThreads
        {
            get { return _maxConcurrentThreads; }

            set
            {
                if (value < 1)
                    throw new Exception("Max concurrent threads must be 1 or greater");
                else
                    _maxConcurrentThreads = value;
            }
        }

        [DataMember]
        public LoggingLevel LoggingLevel
        {
            get { return _loggingLevel; }
            set { _loggingLevel = value; }
        }

        [DataMember]
        public bool LogToDatabase
        {
            get { return _logToDatabase; }
            set { _logToDatabase = value; }
        }

        [DataMember]
        public int DaysOfDatabaseLoggingHistory
        {
            get { return _daysOfDatabaseLoggingHistory; }

            set
            {
                if (value < 0)
                    throw new Exception("Days of database logging history must be 0 or greater");
                else
                    _daysOfDatabaseLoggingHistory = value;
            }
        }

        [DataMember]
        public bool LogToFile
        {
            get { return _logToFile; }
            set { _logToFile = value; }
        }

        [DataMember]
        public int DaysOfFileLoggingHistory
        {
            get { return _daysOfFileLoggingHistory; }

            set
            {
                if (value < 0)
                    throw new Exception("Days of file logging history must be 0 or greater");
                else
                    _daysOfFileLoggingHistory = value;
            }
        }

        public IntegrationDefinition(Guid integrationId, string name, string packageDllDirectoryAndFilename, string sourceName, string targetName, bool isEnabled = true)
        {
            if (integrationId == Guid.Empty)
                throw new Exception("Integration ID must be a valid GUID.");

            Id = integrationId;

            Name = name;

            PackageDllDirectory = Path.GetDirectoryName(packageDllDirectoryAndFilename);

            PackageDllFilename = Path.GetFileName(packageDllDirectoryAndFilename);

            SourceName = sourceName;

            TargetName = targetName;

            IsEnabled = isEnabled;
        }
    }
}

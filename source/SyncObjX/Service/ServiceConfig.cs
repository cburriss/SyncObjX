using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Runtime.Serialization;
using SyncObjX.Configuration;
using SyncObjX.Logging;

namespace SyncObjX.Service
{
    [DataContract]
    public class ServiceConfig
    {
        private Dictionary<string, SupportedProperty> _supportedProperties;

        private int _intervalInSeconds;

        private LoggingLevel _loggingLevel;

        private bool _logToDatabase;

        private int _daysOfDatabaseLoggingHistory;

        private bool _logToFile;

        private int _daysOfFileLoggingHistory;

        private int _maxLogFileSizeInMB;

        private bool _enableMailNotifications;

        private string _fromEmailAddress;

        private string _toEmailAddresses;

        private string _smtpHost;

        private int _smtpPort;

        private bool _smtpRequiresSsl;

        private string _smtpUsername;

        private string _smtpPassword;

        [DataMember]
        public int IntervalInSeconds
        {
            get { return _intervalInSeconds; }

            set
            {
                SupportedProperty.ThrowExceptionIfInvalid(_supportedProperties["IntervalInSeconds"], value);

                _intervalInSeconds = (int)value;
            }
        }

        [DataMember]
        public LoggingLevel LoggingLevel
        {
            get { return _loggingLevel; }

            set
            {
                if (value == Logging.LoggingLevel.InheritFromParent)
                    throw new Exception("'Inherit from Parent' is not a valid option for ServiceConfig.");

                _loggingLevel = value;
            }
        }

        [DataMember]
        public bool LogToDatabase
        {
            get { return _logToDatabase; }

            set
            {
                SupportedProperty.ThrowExceptionIfInvalid(_supportedProperties["LogToDatabase"], value);

                _logToDatabase = (bool)value;
            }
        }

        [DataMember]
        public int DaysOfDatabaseLoggingHistory
        {
            get { return _daysOfDatabaseLoggingHistory; }

            set
            {
                SupportedProperty.ThrowExceptionIfInvalid(_supportedProperties["DaysOfDatabaseLoggingHistory"], value);

                _daysOfDatabaseLoggingHistory = (int)value;
            }
        }

        [DataMember]
        public bool LogToFile
        {
            get { return _logToFile; }

            set 
            {
                SupportedProperty.ThrowExceptionIfInvalid(_supportedProperties["LogToFile"], value);

                _logToFile = (bool)value;
            }
        }

        [DataMember]
        public int DaysOfFileLoggingHistory
        {
            get { return _daysOfFileLoggingHistory; }

            set
            {
                SupportedProperty.ThrowExceptionIfInvalid(_supportedProperties["DaysOfFileLoggingHistory"], value);

                _daysOfFileLoggingHistory = (int)value;
            }
        }

        [DataMember]
        public int MaxLogFileSizeInMB
        {
            get { return _maxLogFileSizeInMB; }

            set
            {
                SupportedProperty.ThrowExceptionIfInvalid(_supportedProperties["MaxLogFileSizeInMB"], value);

                _maxLogFileSizeInMB = (int)value;
            }
        }

        [DataMember]
        public bool EnableMailNotifications
        {
            get { return _enableMailNotifications; }

            set
            {
                SupportedProperty.ThrowExceptionIfInvalid(_supportedProperties["EnableMailNotifications"], value);

                _enableMailNotifications = (bool)value;
            }
        }

        [DataMember]
        public string FromEmailAddress
        {
            get { return _fromEmailAddress; }

            set
            {
                SupportedProperty.ThrowExceptionIfInvalid(_supportedProperties["FromEmailAddress"], value);

                try
                {
                    var address = new MailAddress(value);
                }
                catch (Exception)
                {
                    throw new Exception("'From' email address is invalid.");
                }

                _fromEmailAddress = value;
            }
        }

        [DataMember]
        public IEnumerable<string> ToEmailAddresses
        {
            get 
            {
                List<string> toEmailAddresses = new List<string>();

                if (_toEmailAddresses == null || _toEmailAddresses.Trim() == string.Empty)
                    return toEmailAddresses;

                foreach (var toEmailAddress in _toEmailAddresses.Split(';'))
                    toEmailAddresses.Add(toEmailAddress);

                return toEmailAddresses;
            }

            set
            {
                SupportedProperty.ThrowExceptionIfInvalid(_supportedProperties["ToEmailAddresses"], value);

                if (value == null || value.Count() == 0)
                    _toEmailAddresses = "";
                else
                {
                    foreach (var toEmailAddress in value)
                    {
                        try
                        {
                            var address = new MailAddress(toEmailAddress);
                        }
                        catch (Exception)
                        {
                            throw new Exception("One or more 'To' email addresses are invalid.");
                        }
                    }

                    _toEmailAddresses = string.Join(";", value.ToArray());
                }
            }
        }

        [DataMember]
        public string SmtpHost
        {
            get { return _smtpHost; }

            set
            {
                SupportedProperty.ThrowExceptionIfInvalid(_supportedProperties["SmtpHost"], value);

                _smtpHost = value;
            }
        }

        [DataMember]
        public int SmtpPort
        {
            get { return _smtpPort; }

            set
            {
                SupportedProperty.ThrowExceptionIfInvalid(_supportedProperties["SmtpPort"], value);

                _smtpPort = (int)value;
            }
        }

        [DataMember]
        public bool SmtpRequiresSsl
        {
            get { return _smtpRequiresSsl; }

            set
            {
                SupportedProperty.ThrowExceptionIfInvalid(_supportedProperties["SmtpRequiresSsl"], value);

                _smtpRequiresSsl = (bool)value;
            }
        }

        [DataMember]
        public string SmtpUsername
        {
            get { return _smtpUsername; }

            set
            {
                SupportedProperty.ThrowExceptionIfInvalid(_supportedProperties["SmtpUsername"], value);

                _smtpUsername = value;
            }
        }

        [DataMember]
        public string SmtpPassword
        {
            get { return _smtpPassword; }

            set
            {
                SupportedProperty.ThrowExceptionIfInvalid(_supportedProperties["SmtpPassword"], value);

                _smtpPassword = value;
            }
        }

        public ServiceConfig()
        {
            _supportedProperties = ServiceConfigProperty.GetSupportedProperties();

            _intervalInSeconds = (int)_supportedProperties["IntervalInSeconds"].DefaultValue;

            _loggingLevel = SupportedPropertyOption.GetEnumValueForOption<LoggingLevel>(_supportedProperties["LoggingLevel"], _supportedProperties["LoggingLevel"].DefaultValue.ToString());

            _logToDatabase = (bool)_supportedProperties["LogToDatabase"].DefaultValue;

            _daysOfDatabaseLoggingHistory = (int)_supportedProperties["DaysOfDatabaseLoggingHistory"].DefaultValue;

            _logToFile = (bool)_supportedProperties["LogToFile"].DefaultValue;

            _daysOfFileLoggingHistory = (int)_supportedProperties["DaysOfFileLoggingHistory"].DefaultValue;

            _maxLogFileSizeInMB = (int)_supportedProperties["MaxLogFileSizeInMB"].DefaultValue;

            _enableMailNotifications = (bool)_supportedProperties["EnableMailNotifications"].DefaultValue;

            _fromEmailAddress = (string)_supportedProperties["FromEmailAddress"].DefaultValue;

            _toEmailAddresses = (string)_supportedProperties["ToEmailAddresses"].DefaultValue;

            _smtpHost = (string)_supportedProperties["SmtpHost"].DefaultValue;

            _smtpPort = (int)_supportedProperties["SmtpPort"].DefaultValue;

            _smtpRequiresSsl = (bool)_supportedProperties["SmtpRequiresSsl"].DefaultValue;

            _smtpUsername = (string)_supportedProperties["SmtpUsername"].DefaultValue;

            _smtpPassword = (string)_supportedProperties["SmtpPassword"].DefaultValue;
        }

        public ServiceConfig(int intervalInSeconds, LoggingLevel loggingLevel, int daysOfDatabaseLoggingHistory, int daysOfFileLoggingHistory)
            : this(intervalInSeconds, loggingLevel, true, daysOfDatabaseLoggingHistory, true, daysOfFileLoggingHistory) { }

        public ServiceConfig(int intervalInSeconds, LoggingLevel loggingLevel, bool logToDatabase, int daysOfDatabaseLoggingHistory, bool logToFile, int daysOfFileLoggingHistory)
        {
            _supportedProperties = ServiceConfigProperty.GetSupportedProperties();

            IntervalInSeconds = intervalInSeconds;

            LoggingLevel = loggingLevel;

            LogToDatabase = true;

            DaysOfDatabaseLoggingHistory = daysOfDatabaseLoggingHistory;

            LogToFile = true;

            DaysOfFileLoggingHistory = daysOfFileLoggingHistory;
        }

        // http://social.msdn.microsoft.com/Forums/vstudio/en-US/a0cad0af-7db8-46f6-8240-364cddc8221b/datacontractjsonserializer-does-not-call-the-default-constructor?forum=wcf
        // http://blogs.msdn.com/b/carlosfigueira/archive/2011/09/06/wcf-extensibility-serialization-callbacks.aspx
        [OnDeserializing]
        private void OnDeserializing(StreamingContext context)
        {
            _supportedProperties = ServiceConfigProperty.GetSupportedProperties();
        }
    }
}

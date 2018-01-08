using System;
using System.Collections.Generic;
using SyncObjX.Configuration;
using SyncObjX.Logging;

namespace SyncObjX.Service
{
    public static class ServiceConfigProperty
    {
        public static Dictionary<string, SupportedProperty> GetSupportedProperties()
        {
            Dictionary<string, SupportedProperty> supportedProperties = new Dictionary<string, SupportedProperty>(StringComparer.OrdinalIgnoreCase);

            supportedProperties.Add("IntervalInSeconds", new SupportedProperty("IntervalInSeconds", SupportedPropertyType.Integer, true)
            {
                Description = "Specifies how often the Windows service checks for new job instances to execute.",
                MinValue = 1,
                MaxValue = int.MaxValue,
                DefaultValue = 1
            });

            supportedProperties.Add("LoggingLevel", new SupportedProperty("LoggingLevel", SupportedPropertyType.Text, true)
            {
                Description = "Specifies the logging level.",
                Options = new List<SupportedPropertyOption> { new SupportedPropertyOption("1", "1 - Errors Only"), 
										                      new SupportedPropertyOption("2", "2 - Errors and Warnings"), 
										                      new SupportedPropertyOption("3", "3 - Errors, Warnings, and Info"), 
										                      new SupportedPropertyOption("4", "4 - Errors, Warnings, Info, and Debug"),
										                      new SupportedPropertyOption("5", "5 - Errors, Warnings, Info, Debug, and Trace") },
                OptionsEnumType = typeof(LoggingLevel),
                DefaultValue = "3"
            });

            supportedProperties.Add("LogToDatabase", new SupportedProperty("LogToDatabase", SupportedPropertyType.Boolean, true)
            {
                Description = "Specifies if general logging should be saved to the database.",
                DefaultValue = true
            });

            supportedProperties.Add("DaysOfDatabaseLoggingHistory", new SupportedProperty("DaysOfDatabaseLoggingHistory", SupportedPropertyType.Integer, true)
            {
                Description = "Specifies how many days of logging history should be kept in the database. A value of '-1' indicates that the log history should not be purged.",
                MinValue = -1,
                MaxValue = int.MaxValue,
                DefaultValue = -1
            });

            supportedProperties.Add("LogToFile", new SupportedProperty("LogToFile", SupportedPropertyType.Boolean, true)
            {
                Description = "Specifies if general logging should be saved to text files.",
                DefaultValue = true
            });

            supportedProperties.Add("DaysOfFileLoggingHistory", new SupportedProperty("DaysOfDatabaseLoggingHistory", SupportedPropertyType.Integer, true)
            {
                Description = "Specifies how many days of logging history should be kept as text files. A value of '-1' indicates that the log history should not be purged.",
                MinValue = -1,
                MaxValue = int.MaxValue,
                DefaultValue = 30
            });

            supportedProperties.Add("MaxLogFileSizeInMB", new SupportedProperty("MaxLogFileSizeInMB", SupportedPropertyType.Integer, false)
            {
                Description = "Specifies the max file size for log files. A new log file is created with the next index # if the max file size is exceeded.",
                MinValue = 1,
                MaxValue = int.MaxValue,
                DefaultValue = 100
            });

            supportedProperties.Add("EnableMailNotifications", new SupportedProperty("EnableMailNotifications", SupportedPropertyType.Boolean, false)
            {
                Description = "Specifies if error notifications should be emailed.",
                DefaultValue = false
            });

            supportedProperties.Add("FromEmailAddress", new SupportedProperty("FromEmailAddress", SupportedPropertyType.Text, false)
            {
                Description = "Specifies the From email address to use when emailing error notifications.",
                DefaultValue = ""
            });

            supportedProperties.Add("ToEmailAddresses", new SupportedProperty("ToEmailAddresses", SupportedPropertyType.Text, false)
            {
                Description = "Specifies the To email addresses to use when emailing error notifications. Separate multiple email addresses with a semi-colon.",
                DefaultValue = ""
            });

            supportedProperties.Add("SmtpHost", new SupportedProperty("SmtpHost", SupportedPropertyType.Text, false)
            {
                Description = "Specifies the SMTP host server to use when emailing error notifications.",
                DefaultValue = ""
            });

            supportedProperties.Add("SmtpPort", new SupportedProperty("SmtpPort", SupportedPropertyType.Integer, false)
            {
                Description = "Specifies the SMTP port to use when emailing error notifications.",
                DefaultValue = 25,
                MinValue = 1,
                MaxValue = int.MaxValue
            });

            supportedProperties.Add("SmtpRequiresSsl", new SupportedProperty("SmtpRequiresSsl", SupportedPropertyType.Boolean, false)
            {
                Description = "Specifies whether the SMTP server requires SSL when emailing error notifications.",
                DefaultValue = false
            });

            supportedProperties.Add("SmtpUsername", new SupportedProperty("SmtpUsername", SupportedPropertyType.Text, false)
            {
                Description = "Specifies the username to authenticate with when emailing error notifications.",
                DefaultValue = ""
            });

            supportedProperties.Add("SmtpPassword", new SupportedProperty("SmtpPassword", SupportedPropertyType.Text, false)
            {
                Description = "Specifies the password to authenticate with when emailing error notifications.",
                DefaultValue = ""
            });

            return supportedProperties;
        }

        private static Object ConvertOptionToLoggingLevel(SupportedProperty property, Object value)
        {	
	        string valueAsString = value == null ? null : value.ToString();

	        switch(valueAsString)
	        {
		        case "1 - Errors Only":
			        return LoggingLevel.ErrorsOnly;

                case "2 - Errors and Warnings":
                    return LoggingLevel.ErrorsAndWarnings;

                case "3 - Errors, Warnings, and Info":
                    return LoggingLevel.ErrorsWarningsAndInfo;

                case "4 - Errors, Warnings, Info, and Debug":
                    return LoggingLevel.ErrorsWarningsInfoAndDebug;

                case "5 - Errors, Warnings, Info, Debug, and Trace":
                    return LoggingLevel.ErrorsWarningsInfoDebugAndTrace;
			
		        default:
			        throw new Exception(string.Format("Supported property '{0}' could not convert option '{1}' to enum type '{2}'.", property.Name, value == null ? "null" : value, property.OptionsEnumType.Name));
	        }
        }
    }
}

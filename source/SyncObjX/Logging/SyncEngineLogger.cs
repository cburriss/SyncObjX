using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using SyncObjX.Data;
using SyncObjX.Exceptions;
using SyncObjX.Logging.Mail;
using SyncObjX.Management;
using SyncObjX.SyncObjects;
using SyncObjX.Web;

namespace SyncObjX.Logging
{
    public static class SyncEngineLogger
    {
        private static LoggingLevel serviceLoggingLevel = LoggingLevel.ErrorsWarningsAndInfo;

        private static MailNotifier mailNotifier;

        // NOTE: if changed, account for the possibility of an infinite loop during mail failure
        public static readonly LoggingLevel SendEmailsFor = LoggingLevel.ErrorsOnly;

        public static MailConfig MailConfig
        {
            get
            {
                if (mailNotifier == null)
                    return null;

                return mailNotifier.Config;
            }

            set
            {
                mailNotifier = new MailNotifier(value);
            }
        }

        public static bool EnableMailNotifications = false;

        public static LoggingLevel ServiceLoggingLevel
        {
            get { return serviceLoggingLevel; }

            set
            {
                if (value == LoggingLevel.InheritFromParent)
                    throw new Exception("'Inherit from parent' is not a valid service logging level.");

                serviceLoggingLevel = value;
            }
        }

        private static List<RegisteredLogger> registeredLoggers = new List<RegisteredLogger>();

        // TODO: nothing is being done with registerIntegrations
        private static Dictionary<Guid, RegisteredIntegration> registeredIntegrations = new Dictionary<Guid, RegisteredIntegration>();

        // TODO: nothing is being done with registeredStandaloneDataSources
        private static Dictionary<Guid, RegisteredDataSource> registeredStandaloneDataSources = new Dictionary<Guid, RegisteredDataSource>();

        static SyncEngineLogger()
        {
            MidnightNotifier.DayChanged += new EventHandler<EventArgs>(HandleDayChange);
        }

        public static void RegisterLogger(ISyncEngineLogger logger, LoggingLevel maxLoggingLevel, int daysOfServiceHistoryToMaintain)
        {
            if (logger == null)
                throw new Exception("Logger can not be null.");

            var loggingLevel = maxLoggingLevel == LoggingLevel.InheritFromParent ? ServiceLoggingLevel : maxLoggingLevel;

            var loggerToRegister = new RegisteredLogger(logger, loggingLevel, daysOfServiceHistoryToMaintain);

            if (!registeredLoggers.Contains(loggerToRegister))
                registeredLoggers.Add(loggerToRegister);

            DeleteExpiredLoggerHistory(loggerToRegister);
        }

        public static bool DeregisterLogger(ISyncEngineLogger logger)
        {
            return registeredLoggers.RemoveAll(d => d.Logger == logger) > 0 ? true : false;
        }

        public static void RegisterIntegration(Integration integration)
        {
            // register integration
            var integrationLoggingLevel = integration.LoggingLevel == LoggingLevel.InheritFromParent ? ServiceLoggingLevel : integration.LoggingLevel;

            var integrationToRegister = new RegisteredIntegration(integration, integrationLoggingLevel);

            if (registeredIntegrations.ContainsKey(integration.Id))
                registeredIntegrations[integration.Id] = integrationToRegister;
            else
                registeredIntegrations.Add(integration.Id, integrationToRegister);

            // register jobs
            foreach (var job in integration.Jobs)
            {
                var jobLoggingLevel = job.LoggingLevel == LoggingLevel.InheritFromParent ? integrationLoggingLevel : job.LoggingLevel;

                var jobToRegister = new RegisteredJob(job, jobLoggingLevel);

                if (registeredIntegrations[integration.Id].RegisteredJobs.ContainsKey(job.Id))
                    registeredIntegrations[integration.Id].RegisteredJobs[job.Id] = jobToRegister;
                else
                    registeredIntegrations[integration.Id].RegisteredJobs.Add(job.Id, jobToRegister);
            }

            //DeleteExpiredIntegrationHistoryForAllLoggers(integrationToRegister);
        }

        public static bool DeregisterIntegration(Guid integrationId)
        {
            return registeredIntegrations.Remove(integrationId);        
        }

        public static void RegisterStandaloneDataSource(DataSource dataSource, LoggingLevel loggingLevel)
        {
            if (dataSource == null)
                throw new Exception("Data source can not be null.");

            if (loggingLevel == LoggingLevel.InheritFromParent)
                throw new Exception("'Inherit from parent' is not a valid standalone data source logging level.");

            var registeredDataSource = new RegisteredDataSource(dataSource, loggingLevel);

            if (registeredStandaloneDataSources.ContainsKey(dataSource.Id))
                registeredStandaloneDataSources[dataSource.Id] = registeredDataSource;
            else
                registeredStandaloneDataSources.Add(dataSource.Id, registeredDataSource);
        }

        public static bool DeregisterStandaloneDataSource(Guid dataSourceId)
        {
            return registeredStandaloneDataSources.Remove(dataSourceId);
        }

        public static void Clear()
        {
            ServiceLoggingLevel = LoggingLevel.ErrorsWarningsAndInfo;

            registeredLoggers.Clear();

            registeredIntegrations.Clear();

            registeredStandaloneDataSources.Clear();
        }

        private static void HandleDayChange(object sender, EventArgs e)
        {
            DeleteAllExpiredHistory();
        }

        public static void DeleteAllHistory()
        {
            foreach (var registeredLogger in registeredLoggers)
            {
                try
                {
                    registeredLogger.Logger.DeleteAllHistory();
                }
                catch (Exception ex)
                {
                    WriteExceptionToLog(ex);
                }
            }
        }

        public static void DeleteAllExpiredHistory()
        {
            // TODO: Add separate Service History config property for when LogEntry.IntegrationId IS NULL
            if (registeredLoggers.Count > 0)
            {
                var serviceHistoryDaysToKeep = registeredLoggers.Select(d => d.DaysOfServiceHistoryToMaintain).Max();

                DeleteServiceHistory(serviceHistoryDaysToKeep);
            }

            foreach (var registeredLogger in registeredLoggers)
            {
                try
                {
                    DeleteExpiredLoggerHistory(registeredLogger);
                }
                catch (Exception ex)
                {
                    WriteExceptionToLog(ex);
                }
            }
        }

        private static void DeleteExpiredLoggerHistory(RegisteredLogger registeredLogger)
        {
            try
            {
                if (registeredLogger.DaysOfServiceHistoryToMaintain >= 0)
                    registeredLogger.Logger.DeleteServiceHistory(registeredLogger.DaysOfServiceHistoryToMaintain);
            }
            catch (Exception ex)
            {
                WriteExceptionToLog(ex);
            }

            foreach (var registeredIntegration in registeredIntegrations.Values)
            {
                try
                {
                    DeleteExpiredIntegrationHistory(registeredLogger, registeredIntegration);
                }
                catch (Exception ex)
                {
                    WriteExceptionToLog(ex);
                }
            }
        }

        private static void DeleteExpiredIntegrationHistoryForAllLoggers(RegisteredIntegration registeredIntegration)
        {
            foreach (var registeredLogger in registeredLoggers)
            {
                try
                {
                    DeleteExpiredIntegrationHistory(registeredLogger, registeredIntegration);
                }
                catch (Exception ex)
                {
                    WriteExceptionToLog(ex);
                }
            }
        }

        private static void DeleteExpiredIntegrationHistory(RegisteredLogger registeredLogger, RegisteredIntegration registeredIntegration)
        {
            try
            {
                if (registeredLogger.Logger is DatabaseLogger && registeredIntegration.Integration.DaysOfDatabaseLoggingHistory >= 0)
                    registeredLogger.Logger.DeleteIntegrationHistory(registeredIntegration.Integration.Id, registeredIntegration.Integration.DaysOfDatabaseLoggingHistory);
                else if (registeredLogger.Logger is TextFileLogger && registeredIntegration.Integration.DaysOfFileLoggingHistory >= 0)
                    registeredLogger.Logger.DeleteIntegrationHistory(registeredIntegration.Integration.Id, registeredIntegration.Integration.DaysOfFileLoggingHistory);
                else
                    throw new NotImplementedException(string.Format("Logger of type '{0}' is not implemented.", registeredLogger.Logger.GetType().Name));
            }
            catch (Exception ex)
            {
                WriteExceptionToLog(ex);
            }
        }

        public static void DeleteAllServiceHistory()
        {
            DeleteServiceHistory(0);
        }

        public static void DeleteServiceHistory(int daysToKeep)
        {
            foreach (var registeredLogger in registeredLoggers)
            {
                try
                {
                    registeredLogger.Logger.DeleteServiceHistory(daysToKeep);
                }
                catch (Exception ex)
                {
                    WriteExceptionToLog(ex);
                }
            }
        }

        public static void DeleteJobDataSourceHistory(Guid jobId)
        {
            foreach (var registeredLogger in registeredLoggers)
            {
                try
                {
                    registeredLogger.Logger.DeleteJobDataSourceHistory(jobId);
                }
                catch (Exception ex)
                {
                    WriteExceptionToLog(ex);
                }
            }
        }

        public static void DeleteAllIntegrationHistory(Guid integrationId)
        {
            DeleteIntegrationHistory(integrationId, 0);
        }

        public static void DeleteIntegrationHistory(Guid integrationId, int daysToKeep)
        {
            foreach (var registeredLogger in registeredLoggers)
            {
                try
                {
                    registeredLogger.Logger.DeleteIntegrationHistory(integrationId, daysToKeep);
                }
                catch (Exception ex)
                {
                    WriteExceptionToLog(ex);
                }
            }
        }

        public static void WriteByParallelTaskContext(Exception ex, string customMessageFormat, params object[] args)
        {
            try
            {
                WriteByParallelTaskContext(ex, string.Format(customMessageFormat, args));
            }
            catch (Exception thrownException)
            {
                WriteExceptionToLog(thrownException);
            }
        }

        public static void WriteByParallelTaskContext(Exception ex, string customMessage = null)
        {
            try
            {
                if (!String.IsNullOrWhiteSpace(customMessage))
                    ex = new Exception(customMessage, ex);

                NotifyViaMail(ex);

                WriteByParallelTaskContext(LogEntryType.Error, ExceptionFormatter.Format(ex));
            }
            catch (Exception thrownException)
            {
                WriteExceptionToLog(thrownException);
            }
        }

        public static void WriteByParallelTaskContext(Exception ex, DataSource dataSource, string customMessageFormat, params object[] args)
        {
            try
            {
                WriteByParallelTaskContext(ex, dataSource, string.Format(customMessageFormat, args));
            }
            catch (Exception thrownException)
            {
                WriteExceptionToLog(thrownException);
            }
        }

        public static void WriteByParallelTaskContext(Exception ex, DataSource dataSource, string customMessage = null)
        {
            try
            {
                if (!String.IsNullOrWhiteSpace(customMessage))
                    ex = new Exception(customMessage, ex);

                NotifyViaMail(ex);

                WriteByParallelTaskContext(LogEntryType.Error, dataSource, ExceptionFormatter.Format(ex));
            }
            catch (Exception thrownException)
            {
                WriteExceptionToLog(thrownException);
            }
        }

        public static void WriteByParallelTaskContext(LogEntryType logEntryType, Func<string> messageDelegate)
        {
            try
            {
                WriteByParallelTaskContext(logEntryType, null, messageDelegate);
            }
            catch (Exception thrownException)
            {
                WriteExceptionToLog(thrownException);
            }
        }

        public static void WriteByParallelTaskContext(LogEntryType logEntryType, string format, params object[] args)
        {
            try
            {
                WriteByParallelTaskContext(logEntryType, null, () => { return string.Format(format, args); });
            }
            catch (Exception ex)
            {
                WriteExceptionToLog(ex);
            }
        }

        public static void WriteByParallelTaskContext(LogEntryType logEntryType, string message)
        {
            try
            {
                WriteByParallelTaskContext(logEntryType, null, message);
            }
            catch (Exception ex)
            {
                WriteExceptionToLog(ex);
            }
        }

        public static void WriteByParallelTaskContext(LogEntryType logEntryType, DataSource dataSource, string messageFormat, params object[] args)
        {
            try
            {
                WriteByParallelTaskContext(logEntryType, dataSource, () => { return string.Format(messageFormat, args); });
            }
            catch (Exception ex)
            {
                WriteExceptionToLog(dataSource, ex);
            }
        }

        public static void WriteByParallelTaskContext(LogEntryType logEntryType, DataSource dataSource, string message)
        {
            WriteByParallelTaskContext(logEntryType, dataSource, () => { return message; });
        }

        public static void WriteByParallelTaskContext(LogEntryType logEntryType, DataSource dataSource, Func<string> messageDelegate)
        {
            Integration integration = null;
            JobInstance jobInstance = null;
            JobStepInstance jobStepInstance = null;

            try
            {
                // returns null if not found
                jobInstance = JobQueueManager.GetJobInstanceByParallelTaskId(Thread.CurrentThread.ManagedThreadId);

                if (jobInstance != null)
                {
                    integration = jobInstance.Integration;
                    jobStepInstance = jobInstance.RunningJobStepInstance;
                }

                WriteToLog(logEntryType, integration, dataSource, jobInstance, jobStepInstance, messageDelegate);
            }
            catch (Exception ex)
            {
                WriteExceptionToLog(integration, dataSource, jobInstance, jobStepInstance, ex);
            }
        }

        public static void WriteToLog(JobInstance jobInstance, JobStepInstance jobStepInstance, JobBatch jobBatch)
        {
            foreach (var logger in registeredLoggers)
            {
                try
                {
                    logger.Logger.WriteToLog(jobInstance, jobStepInstance, jobBatch);
                }
                catch (Exception ex)
                {
                    WriteExceptionToLog(jobInstance.Integration, jobBatch.AssociatedDataSource.DataSource, jobInstance, jobStepInstance, ex);
                }
            }
        }

        public static void WriteToLog(LogEntryType logEntryType, Func<string> messageDelegate)
        {
            try
            {
                WriteToLog(logEntryType, messageDelegate());
            }
            catch (Exception ex)
            {
                WriteExceptionToLog(ex);
            }
        }

        public static void WriteToLog(LogEntryType logEntryType, string message)
        {
            try
            {
                WriteToLog(logEntryType, null, null, null, null, message);
            }
            catch (Exception ex)
            {
                WriteExceptionToLog(ex, "An exception occurred for message: {0}", message ?? "");
            }
        }

        public static void WriteToLog(LogEntryType logEntryType, string format, params object[] args)
        {
            try
            {
                WriteToLog(logEntryType, null, null, null, null, () => { return string.Format(format, args); });
            }
            catch (Exception ex)
            {
                WriteExceptionToLog(ex);
            }
        }

        public static void WriteToLog(LogEntryType logEntryType, Integration integration, string message)
        {
            try
            {
                WriteToLog(logEntryType, integration, null, null, null, message);
            }
            catch (Exception ex)
            {
                WriteExceptionToLog(integration, ex, "An exception occurred for message: {0}", message ?? "");
            }
        }

        public static void WriteToLog(LogEntryType logEntryType, Integration integration, string format, params object[] args)
        {
            try
            {
                WriteToLog(logEntryType, integration, null, null, null, () => { return string.Format(format, args); });
            }
            catch (Exception ex)
            {
                WriteExceptionToLog(integration, ex);
            }
        }

        public static void WriteToLog(LogEntryType logEntryType, DataSource dataSource, string message)
        {
            try
            {
                WriteToLog(logEntryType, null, dataSource, null, null, () => { return message; });
            }
            catch (Exception ex)
            {
                WriteExceptionToLog(dataSource, ex, "An exception occurred for message: {0}", message ?? "");
            }
        }

        public static void WriteToLog(LogEntryType logEntryType, DataSource dataSource, string format, params object[] args)
        {
            try
            {
                WriteToLog(logEntryType, null, dataSource, null, null, () => { return string.Format(format, args); });
            }
            catch (Exception ex)
            {
                WriteExceptionToLog(dataSource, ex);
            }
        }

        public static void WriteToLog(LogEntryType logEntryType, JobInstance jobInstance, string message)
        {
            try
            {
                WriteToLog(logEntryType, jobInstance.Integration, null, jobInstance, jobInstance.RunningJobStepInstance, message);
            }
            catch (Exception ex)
            {
                WriteExceptionToLog(jobInstance, ex, "An exception occurred for message: {0}", message ?? "");
            }
        }

        public static void WriteToLog(LogEntryType logEntryType, JobInstance jobInstance, string format, params object[] args)
        {
            Integration integration = null;
            JobStepInstance jobStepInstance = null;

            if (jobInstance != null)
            {
                integration = jobInstance.Integration;
                jobStepInstance = jobInstance.RunningJobStepInstance;
            }

            try
            {
                WriteToLog(logEntryType, integration, null, jobInstance, jobStepInstance, () => { return string.Format(format, args); });
            }
            catch (Exception ex)
            {
                WriteExceptionToLog(jobInstance, ex);
            }
        }

        public static void WriteToLog(LogEntryType logEntryType, JobInstance jobInstance, JobStepInstance jobStepInstance, string message)
        {
            try
            {
                WriteToLog(logEntryType, jobInstance.Integration, null, jobInstance, jobStepInstance, message);
            }
            catch (Exception ex)
            {
                WriteExceptionToLog(jobInstance, ex, "An exception occurred for message: {0}", message ?? "");
            }
        }

        public static void WriteToLog(LogEntryType logEntryType, JobInstance jobInstance, JobStepInstance jobStepInstance, string format, params object[] args)
        {
            try
            {
                WriteToLog(logEntryType, jobInstance.Integration, null, jobInstance, jobStepInstance, () => { return string.Format(format, args); });
            }
            catch (Exception ex)
            {
                WriteExceptionToLog(jobInstance, ex);
            }
        }

        public static void WriteToLog(LogEntryType logEntryType, JobInstance jobInstance, JobStepInstance jobStepInstance, DataSource dataSource, string message)
        {
            try
            {
                WriteToLog(logEntryType, jobInstance.Integration, dataSource, jobInstance, jobStepInstance, message);
            }
            catch (Exception ex)
            {
                WriteExceptionToLog(jobInstance, ex, "An exception occurred for message: {0}", message ?? "");
            }
        }

        public static void WriteToLog(LogEntryType logEntryType, JobInstance jobInstance, JobStepInstance jobStepInstance, DataSource dataSource, string format, params object[] args)
        {
            try
            {
                WriteToLog(logEntryType, jobInstance.Integration, dataSource, jobInstance, jobStepInstance, () => { return string.Format(format, args); });
            }
            catch (Exception ex)
            {
                WriteExceptionToLog(jobInstance, ex);
            }
        }

        public static void WriteToLog(LogEntryType logEntryType, Integration integration, DataSource dataSource, JobInstance jobInstance, JobStepInstance jobStepInstance, string message)
        {
            WriteToLog(logEntryType, integration, dataSource, jobInstance, jobStepInstance, () => { return message; });
        }

        public static void WriteToLog(LogEntryType logEntryType, Integration integration, DataSource dataSource, JobInstance jobInstance, JobStepInstance jobStepInstance, Func<string> messageDelegate)
        {
            if (integration == null && jobInstance != null)
                integration = jobInstance.Integration;

            if (jobStepInstance == null && jobInstance != null)
                jobStepInstance = jobInstance.RunningJobStepInstance;

            string message = null;

            foreach (var logger in registeredLoggers)
            {
                if ((int)logEntryType <= (int)logger.MaxLoggingLevel)
                {
                    if (message == null)
                        message = messageDelegate();

                    logger.Logger.WriteToLog(logEntryType, integration, dataSource, jobInstance, jobStepInstance, message);
                }
            }
        }

        public static void WriteExceptionToLog(Exception ex, Func<string> messageDelegate)
        {
            try
            {
                WriteExceptionToLog(null, null, null, null, ex, messageDelegate());
            }
            catch (Exception thrownException)
            {
                WriteExceptionToLog(thrownException);
            }
        }

        public static void WriteExceptionToLog(Exception ex, string customMessageFormat, params object[] args)
        {
            WriteExceptionToLog(null, null, null, null, ex, string.Format(customMessageFormat, args));
        }

        public static void WriteExceptionToLog(Exception ex, string customMessage = null)
        {
            WriteExceptionToLog(null, null, null, null, ex, customMessage);
        }

        public static void WriteExceptionToLog(Integration integration, Exception ex, string customMessageFormat, params object[] args)
        {
            WriteExceptionToLog(integration, null, null, null, ex, string.Format(customMessageFormat, args));
        }

        public static void WriteExceptionToLog(Integration integration, Exception ex, string customMessage = null)
        {
            WriteExceptionToLog(integration, null, null, null, ex, customMessage);
        }

        public static void WriteExceptionToLog(DataSource dataSource, Exception ex, string customMessageFormat, params object[] args)
        {
            WriteExceptionToLog(null, dataSource, null, null, ex, string.Format(customMessageFormat, args));
        }

        public static void WriteExceptionToLog(DataSource dataSource, Exception ex, string customMessage = null)
        {
            WriteExceptionToLog(null, dataSource, null, null, ex, customMessage);
        }

        public static void WriteExceptionToLog(JobInstance jobInstance, Exception ex, string customMessageFormat, params object[] args)
        {
            WriteExceptionToLog(null, null, jobInstance, null, ex, string.Format(customMessageFormat, args));
        }

        public static void WriteExceptionToLog(JobInstance jobInstance, Exception ex, string customMessage = null)
        {
            WriteExceptionToLog(null, null, jobInstance, null, ex, customMessage);
        }

        public static void WriteExceptionToLog(Integration integration, DataSource dataSource, JobInstance jobInstance, JobStepInstance jobStepInstance, Exception ex, string customMessageFormat, params object[] args)
        {
            WriteExceptionToLog(integration, dataSource, jobInstance, jobStepInstance, ex, string.Format(customMessageFormat, args));
        }

        public static void WriteExceptionToLog(Integration integration, DataSource dataSource, JobInstance jobInstance, JobStepInstance jobStepInstance, Exception ex, string customMessage = null)
        {
            if(!String.IsNullOrWhiteSpace(customMessage))
                ex = new Exception(customMessage, ex);

            NotifyViaMail(ex);

            WriteToLog(LogEntryType.Error, integration, dataSource, jobInstance, jobStepInstance, () => { return ExceptionFormatter.Format(ex); });
        }

        private static void NotifyViaMail(Exception ex)
        {
            // if a MailException itself, exit to avoid an infinite loop
            if (ex is MailException || !EnableMailNotifications)
                return;

            if (mailNotifier == null)
            {
                WriteToLog(LogEntryType.Warning, "Email notifications are enabled but not configured. The email could not be sent.");
                return;
            }

            string msg = "";

            Integration integration = null;
            JobInstance jobInstance = null;
            JobStepInstance jobStepInstance = null;

            try
            {
                // returns null if not found
                jobInstance = JobQueueManager.GetJobInstanceByParallelTaskId(Thread.CurrentThread.ManagedThreadId);

                if (jobInstance != null)
                {
                    integration = jobInstance.Integration;
                    jobStepInstance = jobInstance.RunningJobStepInstance;
                }

                var jobInfo = MailFormatter.GetFormattedJobInfoAsHtml(integration, jobInstance, jobStepInstance);

                msg = MailFormatter.GetTextAsFormattedDiv(jobInfo) + ExceptionFormatter.FormatExceptionForWeb(ex);
            }
            catch (Exception innerEx) 
            {
                StringBuilder failureInfo = new StringBuilder();

                failureInfo.AppendFormat("<b>Job info could not be obtained because of error:</b><br /> {0} {1}", innerEx.Message, innerEx.StackTrace);
                failureInfo.Append(HtmlBuilder.GetLineBreak(2));

                msg = MailFormatter.GetTextAsFormattedDiv(failureInfo.ToString()) + ExceptionFormatter.FormatExceptionForWeb(ex);
            }

            // Exceptions are caught within MailNotifier class and logged via the SyncEngineLogger
            if (integration == null)
                mailNotifier.SendMessage("SyncObjX Integration Services - An Error Has Occurred", msg);
            else
                mailNotifier.SendMessage(string.Format("SyncObjX Integration Services - Error Occurred in \"{0}\"", integration.Name), msg);
        }

        private class RegisteredLogger
        {
            public LoggingLevel MaxLoggingLevel;

            public ISyncEngineLogger Logger;

            public int DaysOfServiceHistoryToMaintain;

            public RegisteredLogger(ISyncEngineLogger logger, LoggingLevel maxLoggingLevel, int daysOfServiceHistoryToMaintain)
            {
                if (logger == null)
                    throw new Exception("Logger can not be null.");

                switch (maxLoggingLevel)
                {
                    case LoggingLevel.ErrorsOnly:
                    case LoggingLevel.ErrorsAndWarnings:
                    case LoggingLevel.ErrorsWarningsAndInfo:
                    case LoggingLevel.ErrorsWarningsInfoAndDebug:
                    case LoggingLevel.ErrorsWarningsInfoDebugAndTrace:

                        MaxLoggingLevel = maxLoggingLevel;
                        
                        break;
                    
                    default:
                        throw new EnumValueNotImplementedException<LoggingLevel>(maxLoggingLevel);
                }

                if (daysOfServiceHistoryToMaintain < -1)
                    throw new Exception("Days of history to keep must be -1 or greater. Use -1 to indicate that logging should never be deleted.");

                Logger = logger;

                MaxLoggingLevel = maxLoggingLevel;

                DaysOfServiceHistoryToMaintain = daysOfServiceHistoryToMaintain;
            }
        }

        private abstract class RegisteredSyncObject
        {
            public LoggingLevel DerivedLoggingLevel;

            public RegisteredSyncObject(LoggingLevel derivedLoggingLevel)
            {
                if (derivedLoggingLevel == LoggingLevel.InheritFromParent)
                    throw new Exception("'Inherit from parent' is not a valid derived logging level.");

                DerivedLoggingLevel = derivedLoggingLevel;
            }
        }

        private class RegisteredIntegration : RegisteredSyncObject
        {
            public Integration Integration;

            public Dictionary<Guid, RegisteredJob> RegisteredJobs = new Dictionary<Guid,RegisteredJob>();

            public RegisteredIntegration(Integration integration, LoggingLevel derivedLoggingLevel)
                : base(derivedLoggingLevel)
            {
                if (integration == null)
                    throw new Exception("Integration can not be null.");

                Integration = integration;
            }
        }

        private class RegisteredJob : RegisteredSyncObject
        {
            public Job Job;

            public RegisteredJob(Job job, LoggingLevel derivedLoggingLevel)
                : base(derivedLoggingLevel)
            {
                if (job == null)
                    throw new Exception("Job can not be null.");

                Job = job;
            }
        }

        private class RegisteredDataSource : RegisteredSyncObject
        {
            DataSource DataSource;

            public RegisteredDataSource(DataSource dataSource, LoggingLevel derivedLoggingLevel)
                : base(derivedLoggingLevel)
            {
                if (dataSource == null)
                    throw new Exception("Data source can not be null.");

                DataSource = dataSource;
            }
        }
    }
}

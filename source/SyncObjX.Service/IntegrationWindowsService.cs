using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Net;
using System.Net.Mail;
using System.ServiceModel;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using SyncObjX.Configuration;
using SyncObjX.Logging;
using SyncObjX.Logging.Mail;
using SyncObjX.Management;

namespace SyncObjX.Service
{
    public class IntegrationWindowsService : ServiceBase
    {
        //TODO: Future consideration - http://stackoverflow.com/questions/10697216/in-which-case-onstop-of-net-window-service-will-not-fire
        //                             http://stackoverflow.com/questions/6799955/how-to-detect-windows-shutdown-or-logoff
        //                             http://msdn.microsoft.com/en-us/library/microsoft.win32.systemevents.aspx                            

        private ServiceHost _configurationServiceHost = null;

        private ServiceHost _queueManagementServiceHost = null;

        private ServiceHost _logManagementServiceHost = null;

        private ServiceHost _dataServiceHost = null;

        private static string _integrationDbConnectionString;

        private static ISyncEngineLogger _databaseLogger;

        private static ISyncEngineLogger _textFileLogger;

        private static JobQueueDatabaseLogger _dbQueueLogger;

        private static SyncEngineDatabaseConfigurator _configurator;

        private static ServiceConfig _serviceConfig;

        private ScheduledJobManager _scheduledJobManager;

        public static string IntegrationDbConnectionString
        {
            get { return _integrationDbConnectionString; }
            private set { _integrationDbConnectionString = value; }
        }

        public IntegrationWindowsService()
        {
            // Name the Windows Service
            ServiceName = "SyncObjX Integration Service";
        }

        public static void Main()
        {
            ServiceBase.Run(new IntegrationWindowsService());
        }

        // Start the Windows service.
        protected override void OnStart(string[] onStartArgs)
        {
            try
            {
                // get connection string from connectionStrings in App.config
                GetIntegrationDbConnectionString();

                // test connection to the queue database
                TestIntegrationDbConnection(IntegrationDbConnectionString);

                _dbQueueLogger = new JobQueueDatabaseLogger(IntegrationDbConnectionString);
                _configurator = new SyncEngineDatabaseConfigurator(IntegrationDbConnectionString);

                JobQueueManager.Configurator = _configurator;

                _serviceConfig = _configurator.GetServiceConfig();

                // reset the logger as a precaution
                SyncEngineLogger.Clear();

                // configure mail notifications, if enabled
                if (_serviceConfig.EnableMailNotifications)
                    ConfigureMailNotifications();

                // add database and text file loggers
                RegisterLoggers();

                // register integrations for the database and text file loggers
                RegisterIntegrations();

                // associate the database logger with the queue manager
                JobQueueLogManager.AddLogger(_dbQueueLogger);

                // recover any job instances from a server or service restart; this will only apply to on-demand job instances
                _dbQueueLogger.RecoverJobInstancesFromQueueLog();

                // add the next run times for each scheduled job; clear any existing scheduled jobs from the queue
                _scheduledJobManager = new ScheduledJobManager(_configurator);
                _scheduledJobManager.QueueScheduledJobs(clearExistingScheduledJobInstancesFromWaitingQueue: true);

                // once a scheduled job is complete (i.e. queue request), queue for the next run time
                JobQueueManager.JobQueueRequestStatusChanged += new EventHandler<JobQueueRequestStatusChangedArgs>((s, e) =>
                {
                    if (e.QueueRequest.Status == JobQueueRequestStatus.Completed &&
                        e.QueueRequest.InvocationSourceType == JobInvocationSourceType.Scheduled)
                    {
                        _scheduledJobManager.QueueJobForNextScheduledRunTime(e.QueueRequest.Job.Id);
                    }
                });

                JobQueueManager.MaxDelayedStartByJobPriority = _configurator.GetJobPriorityConfig();

                JobQueueManager.Start(_serviceConfig.IntervalInSeconds);

                InitializeWebServiceHosts();
            }
            catch (Exception ex)
            {
                EventViewerLogger.WriteToLog(ex);

                SyncEngineLogger.WriteExceptionToLog(ex);

                // stop the service
                this.Stop();
            }
        }

        public static void RefreshServiceConfig()
        {
            try
            {
                _serviceConfig = _configurator.GetServiceConfig();

                JobQueueManager.Stop();

                ConfigureMailNotifications();

                if (!_serviceConfig.LogToDatabase)
                    SyncEngineLogger.DeregisterLogger(_databaseLogger);

                if (!_serviceConfig.LogToFile)
                    SyncEngineLogger.DeregisterLogger(_textFileLogger);

                JobQueueManager.Start(_serviceConfig.IntervalInSeconds);
            }
            catch (Exception ex)
            {
                EventViewerLogger.WriteToLog(ex);

                SyncEngineLogger.WriteExceptionToLog(ex);

                SyncEngineLogger.WriteToLog(LogEntryType.Warning, "The service config could not be refreshed and will occur the next time the service starts.");
            }
        }

        private static void ConfigureMailNotifications()
        {
            SyncEngineLogger.EnableMailNotifications = _serviceConfig.EnableMailNotifications;

            if (_serviceConfig.EnableMailNotifications)
            {
                try
                {
                    var fromAddress = new MailAddress(_serviceConfig.FromEmailAddress);

                    var toAddresses = new List<MailAddress>();

                    foreach (var toAddress in _serviceConfig.ToEmailAddresses)
                    {
                        toAddresses.Add(new MailAddress(toAddress));
                    }

                    SyncEngineLogger.MailConfig = new MailConfig(fromAddress, toAddresses,
                                                                 _serviceConfig.SmtpHost, _serviceConfig.SmtpPort);

                    SyncEngineLogger.MailConfig.EnableSsl = _serviceConfig.SmtpRequiresSsl;

                    if (!(_serviceConfig.SmtpUsername == null || _serviceConfig.SmtpUsername.Trim() == string.Empty))
                        SyncEngineLogger.MailConfig.Credentials = new NetworkCredential(_serviceConfig.SmtpUsername, _serviceConfig.SmtpPassword);
                }
                catch (Exception ex)
                {
                    EventViewerLogger.WriteToLog(ex);

                    SyncEngineLogger.WriteExceptionToLog(ex);
                }
            }
        }

        private static void RegisterLoggers()
        {
            SyncEngineLogger.ServiceLoggingLevel = _serviceConfig.LoggingLevel;

            // TODO: Add different configurable logging levels for database and text file
            if (_serviceConfig.LogToDatabase)
            {
                _databaseLogger = new DatabaseLogger(IntegrationDbConnectionString);
                SyncEngineLogger.RegisterLogger(_databaseLogger, LoggingLevel.ErrorsAndWarnings, _serviceConfig.DaysOfDatabaseLoggingHistory);
            }

            if (_serviceConfig.LogToFile)
            {
                _textFileLogger = new TextFileLogger(AppDomain.CurrentDomain.BaseDirectory + "\\Logs", _serviceConfig.MaxLogFileSizeInMB);
                SyncEngineLogger.RegisterLogger(_textFileLogger, _serviceConfig.LoggingLevel, _serviceConfig.DaysOfFileLoggingHistory);
            }
        }

        private static void RegisterIntegrations()
        {
            var integrations = _configurator.GetIntegrations();

            foreach (var integration in integrations)
            {
                SyncEngineLogger.RegisterIntegration(integration);
            }
        }

        public static void RefreshIntegrationInLogger(Guid integrationId)
        {
            try
            {
                SyncEngineLogger.DeregisterIntegration(integrationId);

                var integrationToRegister = _configurator.GetIntegrationById(integrationId);

                SyncEngineLogger.RegisterIntegration(integrationToRegister);
            }
            catch (Exception ex)
            {
                EventViewerLogger.WriteToLog(ex);

                SyncEngineLogger.WriteExceptionToLog(ex);

                SyncEngineLogger.WriteToLog(LogEntryType.Warning, "The integration '{0}' could not be refreshed and will occur the next time the service starts.", integrationId);
            }
        }

        public static void RefreshJobInLogger(Guid jobId)
        {
            try
            {
                var integrationToRegister = _configurator.GetIntegrationByJobId(jobId);

                if (integrationToRegister == null)
                    throw new Exception(string.Format("No integration exists for job '{0}'.", jobId));

                SyncEngineLogger.DeregisterIntegration(integrationToRegister.Id);

                SyncEngineLogger.RegisterIntegration(integrationToRegister);
            }
            catch (Exception ex)
            {
                EventViewerLogger.WriteToLog(ex);

                SyncEngineLogger.WriteExceptionToLog(ex);

                SyncEngineLogger.WriteToLog(LogEntryType.Warning, "The job '{0}' could not be refreshed and will occur the next time the service starts.", jobId);
            }
        }

        private void InitializeWebServiceHosts()
        {
            InitializeWebServiceHost<ConfigurationService>(_configurationServiceHost);
            InitializeWebServiceHost<QueueManagementService>(_queueManagementServiceHost);
            InitializeWebServiceHost<LogManagementService>(_logManagementServiceHost);
            InitializeWebServiceHost<DataService>(_dataServiceHost);
        }

        private void InitializeWebServiceHost<TService>(ServiceHost serviceHost)
        {
            if (serviceHost != null)
            {
                serviceHost.Close();
            }

            // Create a ServiceHost for the SyncEngineService type and provide the base address.
            serviceHost = new ServiceHost(typeof(TService));

            // Open the ServiceHostBase to create listeners and start listening for messages.
            serviceHost.Open();
        }

        protected override void OnStop()
        {
            CloseWebServiceHosts();

            // stop the queue manager so that no new job instances are started
            JobQueueManager.Stop();

            if (JobQueueManager.HasRunningJobs)
            {
                StringBuilder msg = new StringBuilder();

                var runningJobs = JobQueueManager.RunningJobsByIntegrationId;

                msg.AppendLine("The Windows service is stopping.  The following jobs will be completed first:");

                foreach (var integrationName in runningJobs.Keys)
                {
                    foreach (var jobInstance in runningJobs[integrationName])
                    {
                        msg.AppendFormat("{0} : {1} : {2} ({3})",
                                        jobInstance.Integration.Name, jobInstance.Job.Name, jobInstance.SourceDataSource.DataSource.Name, jobInstance.Id);
                    }
                }

                SyncEngineLogger.WriteToLog(LogEntryType.Info, msg.ToString());
            }

            // continue to pause the service thread so long as jobs are running
            while (JobQueueManager.HasRunningJobs)
            {
                Thread.Sleep(1000);
            }
        }

        private void CloseWebServiceHosts()
        {
            CloseWebServiceHost(_configurationServiceHost);
            CloseWebServiceHost(_queueManagementServiceHost);
            CloseWebServiceHost(_logManagementServiceHost);
            CloseWebServiceHost(_dataServiceHost);
        }
        
        private void CloseWebServiceHost(ServiceHost host)
        {
            if (host != null)
            {
                host.Close();
                host = null;
            }
        }

        private ConnectionStringSettings GetIntegrationDbConnectionString()
        {
            var integrationDbConnStringElem = ConfigurationManager.ConnectionStrings["SyncObjX"];

            if (integrationDbConnStringElem == null)
                throw new Exception("A connection string with name 'SyncObjX' is missing from App.config.");
            else
                IntegrationDbConnectionString = integrationDbConnStringElem.ConnectionString;
            return integrationDbConnStringElem;
        }

        private static void TestIntegrationDbConnection(string connectionString)
        {
            SqlConnection conn = new SqlConnection(connectionString);
            conn.Open();
            conn.Close();
        }
    }
}

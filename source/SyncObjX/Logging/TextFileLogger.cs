using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using SyncObjX.Data;
using SyncObjX.Management;
using SyncObjX.SyncObjects;

namespace SyncObjX.Logging
{
    public class TextFileLogger : ISyncEngineLogger
    {
        private Object _lock = new Object();

        private const string PRE_MESSAGES_TABS = "\t\t\t\t\t\t\t\t\t";

        private const int NUMBER_OF_FILEACCESS_RETRIES = 10;
        private const int DELAY_ON_FILEACCES_RETRY = 1000;

        public readonly string LogDirectory;

        private Dictionary<Guid, TextFileLogInfo> logFileInfoByIntegrationId = new Dictionary<Guid, TextFileLogInfo>();

        private TextFileLogInfo serviceLogFileInfo = new TextFileLogInfo();

        /// <summary>
        /// Max log file size in bytes. A new log file with an incremented index number is created if the max size is exceeded.
        /// </summary>
        public readonly int MaxFileSize;

        /// <summary>
        /// Defaults to 100MB for max log file size.
        /// </summary>
        /// <param name="logDirectory"></param>
        public TextFileLogger(string logDirectory)
            : this(logDirectory, 100) { }

        public TextFileLogger(string logDirectory, int maxFileSizeInMB)
        {
            if (String.IsNullOrWhiteSpace(logDirectory))
                throw new Exception("Log directory is missing or empty.");

            if (!Directory.Exists(logDirectory))
                Directory.CreateDirectory(logDirectory);

            if (maxFileSizeInMB < 1)
                throw new Exception("Max file size must be at least 1MB.");

            LogDirectory = logDirectory;

            MaxFileSize = maxFileSizeInMB * 1024 * 1024; // convert to MB to bytes

            MidnightNotifier.DayChanged += new EventHandler<EventArgs>(HandleDayChange);

            InitiateTextFileLogInfo();
        }

        private void HandleDayChange(object sender, EventArgs e)
        {
            logFileInfoByIntegrationId.Clear();

            serviceLogFileInfo.CurrentFileIndex = 1;
            serviceLogFileInfo.CurrentFileSizeInBytes = 0;
        }

        private void InitiateTextFileLogInfo()
        {
            if (Directory.Exists(LogDirectory))
            {
                var fileNameFragmentForToday = string.Format(" - {0:yyyyMMdd}", DateTime.Now.Date);

                var logFilesCreatedToday = new DirectoryInfo(LogDirectory).GetFiles().Where(d => d.Name.EndsWith(".log") && d.Name.Contains(fileNameFragmentForToday));

                foreach (var logFile in logFilesCreatedToday)
                {
                    var fileIdx = GetFileNumberFromName(logFile.Name);

                    if (logFile.Name.StartsWith("Integration Service"))
                    {
                        if (fileIdx > serviceLogFileInfo.CurrentFileIndex)
                        {
                            serviceLogFileInfo.CurrentFileIndex = fileIdx;
                            serviceLogFileInfo.CurrentFileSizeInBytes = logFile.Length;
                            serviceLogFileInfo.FileName = logFile.Name;
                            serviceLogFileInfo.PathAndFileName = LogDirectory + "\\" + logFile.Name;
                        }
                    }
                    else
                    {
                        var fragmentForTodayIdx = logFile.Name.IndexOf(fileNameFragmentForToday);

                        var preGuidHyphen = logFile.Name.Substring(0, fragmentForTodayIdx).LastIndexOf(" - ");

                        var integrationIdText = logFile.Name.Substring(preGuidHyphen + 2, fragmentForTodayIdx - preGuidHyphen - 2).Trim();

                        Guid integrationId;

                        if (GuidTryParse(integrationIdText, out integrationId))
                        {
                            if (!logFileInfoByIntegrationId.ContainsKey(integrationId))
                                logFileInfoByIntegrationId.Add(integrationId, new TextFileLogInfo());

                            logFileInfoByIntegrationId[integrationId].CurrentFileIndex = fileIdx;
                            logFileInfoByIntegrationId[integrationId].CurrentFileSizeInBytes = logFile.Length;
                            logFileInfoByIntegrationId[integrationId].FileName = logFile.Name;
                            logFileInfoByIntegrationId[integrationId].PathAndFileName = LogDirectory + "\\" + logFile.Name;
                        }
                    }
                }
            }
        }

        private static bool GuidTryParse(string s, out Guid result)
        {
            if (s == null)
                throw new ArgumentNullException("s");
            Regex format = new Regex(
                "^[A-Fa-f0-9]{32}$|" +
                "^({|\\()?[A-Fa-f0-9]{8}-([A-Fa-f0-9]{4}-){3}[A-Fa-f0-9]{12}(}|\\))?$|" +
                "^({)?[0xA-Fa-f0-9]{3,10}(, {0,1}[0xA-Fa-f0-9]{3,6}){2}, {0,1}({)([0xA-Fa-f0-9]{3,4}, {0,1}){7}[0xA-Fa-f0-9]{3,4}(}})$");
            Match match = format.Match(s);
            if (match.Success)
            {
                result = new Guid(s);
                return true;
            }
            else
            {
                result = Guid.Empty;
                return false;
            }
        }

        private static int GetFileNumberFromName(string fileName)
        {
            var currentFileNumber = 1;
            string currentFileNumberText;

            // split at the file index #
            var splitFileName = fileName.Split('_');

            if (splitFileName.Length > 1)
            {
                // strip of file extension
                splitFileName = splitFileName[1].Split('.');

                currentFileNumberText = splitFileName[0];

                int parsedFileNumber;

                // if is a valid integer, break out of the loop
                if (int.TryParse(currentFileNumberText, out parsedFileNumber))
                {
                    currentFileNumber = parsedFileNumber;
                }
            }

            return currentFileNumber;
        }

        public void DeleteJobDataSourceHistory(Guid jobId)
        {
            // do nothing
        }

        public void DeleteIntegrationHistory(Guid integrationId, int daysToKeep)
        {
            var filesWithIntegrationId = Directory.GetFiles(LogDirectory).Where(d => d.Contains(integrationId.ToString()));

            foreach (var filename in filesWithIntegrationId)
            {
                FileInfo file = new FileInfo(filename);

                if ((DateTime.Now - file.LastWriteTime).Days > daysToKeep)
                    File.Delete(filename);
            }
        }

        public void DeleteServiceHistory(int daysToKeep)
        {
            var filesForServiceHistory = Directory.GetFiles(LogDirectory).Where(f => Path.GetFileName(f).StartsWith("Integration Service"));

            foreach (var filename in filesForServiceHistory)
            {
                FileInfo file = new FileInfo(filename);

                if ((DateTime.Now - file.LastWriteTime).Days > daysToKeep)
                    File.Delete(filename);
            }
        }

        public void DeleteAllHistory()
        {
            Array.ForEach(Directory.GetFiles(LogDirectory), (path) => File.Delete(path));
        }

        public void WriteToLog(LogEntryType logEntryType, Integration integration, DataSource dataSource, JobInstance jobInstance, JobStepInstance jobStepInstance, string message)
        {
            string path = null;

            for (int i = 0; i <= NUMBER_OF_FILEACCESS_RETRIES; i++) {

                try {

                    if (integration == null)
                        path = GetServiceLogPathAndFileName();
                    else
                        path = GetIntegrationLogPathAndFileName(integration);

                    break;
                }
                catch (IOException) {

                    if (i == NUMBER_OF_FILEACCESS_RETRIES)
                        throw;
                    else
                        Thread.Sleep(DELAY_ON_FILEACCES_RETRY);
                }
            }

            lock (_lock)
            {
                for (int i = 0; i <= NUMBER_OF_FILEACCESS_RETRIES; i++)
                {
                    try
                    {
                        using (TextWriter log = File.AppendText(path))
                        {
                            StringBuilder logLine = new StringBuilder();

                            logLine.Append(DateTime.Now);
                            logLine.Append("\t");

                            logLine.Append(logEntryType);
                            logLine.Append("\t");

                            if (integration == null)
                                logLine.Append("N/A");
                            else
                                logLine.Append(string.Format("{0} ({1})", integration.Name, integration.Id));
                            logLine.Append("\t");

                            if (dataSource == null)
                                logLine.Append("N/A");
                            else
                                logLine.Append(string.Format("{0} ({1})", dataSource.Name, dataSource.Id));
                            logLine.Append("\t");

                            if (jobInstance == null)
                                logLine.Append("N/A\tN/A\tN/A\tN/A\t");
                            else
                            {
                                logLine.Append(string.Format("{0} ({1})", jobInstance.Job.Name, jobInstance.Job.Id));
                                logLine.Append("\t");
                                logLine.Append(string.Format("{0} ({1})", jobInstance.QueueRequest.InvocationSourceType, jobInstance.QueueRequest.Id));
                                logLine.Append("\t");
                                logLine.Append(jobInstance.Id);
                                logLine.Append("\t");
                                logLine.Append(JobFilterHelper.GetTextForDatabase(jobInstance.Filters));
                                logLine.Append("\t");
                            }

                            if (jobStepInstance == null)
                                logLine.Append("N/A\tN/A\t");
                            else
                            {
                                logLine.Append(string.Format("{0} ({1})", jobStepInstance.JobStep.Name, jobStepInstance.JobStep.Id));
                                logLine.Append("\t");
                                logLine.Append(jobStepInstance.Id);
                                logLine.Append("\t");
                            }

                            logLine.Append(message);

                            log.WriteLine(logLine);

                            log.Flush();
                            log.Close();

                            if (integration == null)
                                serviceLogFileInfo.CurrentFileSizeInBytes += logLine.Length;
                            else
                                logFileInfoByIntegrationId[integration.Id].CurrentFileSizeInBytes += logLine.Length;
                        }

                        break;
                    }
                    catch (IOException)
                    {
                        if (i == NUMBER_OF_FILEACCESS_RETRIES)
                            throw;
                        else
                            Thread.Sleep(DELAY_ON_FILEACCES_RETRY);
                    }
                }
            }
        }

        public void WriteToLog(JobInstance jobInstance, JobStepInstance jobStepInstance, JobBatch jobBatch)
        {
            string path = null;

            for (int i = 0; i <= NUMBER_OF_FILEACCESS_RETRIES; i++)
            {
                try
                {
                    path = GetIntegrationLogPathAndFileName(jobInstance.Integration);

                    break;
                }
                catch (IOException)
                {
                    if (i == NUMBER_OF_FILEACCESS_RETRIES)
                        throw;
                    else
                        Thread.Sleep(DELAY_ON_FILEACCES_RETRY);
                }
            }

            lock (_lock)
            {
                for (int i = 0; i <= NUMBER_OF_FILEACCESS_RETRIES; i++)
                {
                    try
                    {
                        using (TextWriter log = File.AppendText(path))
                        {
                            StringBuilder logLine = new StringBuilder();

                            var now = DateTime.Now;

                            var errorLogLineMinusMessage = GetLogLine(now, LogEntryType.Error, jobInstance, jobStepInstance, jobBatch, null);
                            var warningLogLineMinusMessage = GetLogLine(now, LogEntryType.Warning, jobInstance, jobStepInstance, jobBatch, null);
                            var infoLogLineMinusMessage = GetLogLine(now, LogEntryType.Info, jobInstance, jobStepInstance, jobBatch, null);

                            foreach (var entityBatch in jobBatch.EntityBatches)
                            {
                                int countOfInsertsWithoutError = 0;
                                int countOfUpdatesWithoutError = 0;
                                int countOfDeletionsWithoutError = 0;

                                if (entityBatch.RecordsToAdd.Count == 0 && entityBatch.RecordsToUpdate.Count == 0 && entityBatch.RecordsToDelete.Count == 0)
                                    continue;

                                if (!entityBatch.HasBeenProcessed)
                                {
                                    logLine.Append(warningLogLineMinusMessage);
                                    logLine.AppendLine("The entity batch has not been processed by a data source. No history can be logged.");
                                    continue;
                                }

                                logLine.Append(infoLogLineMinusMessage);

                                if (entityBatch.EntityDefinition.TechnicalEntityName == entityBatch.EntityDefinition.UserFriendlyEntityName)
                                {
                                    logLine.AppendLine(string.Format("Entity batch applied on sync side '{0}' for entity '{1}':",
                                        jobBatch.SyncSide, entityBatch.EntityDefinition.TechnicalEntityName));
                                }
                                else
                                {
                                    logLine.AppendLine(string.Format("Entity batch applied on sync side '{0}' for entity '{1}' ({2}):",
                                        jobBatch.SyncSide, entityBatch.EntityDefinition.TechnicalEntityName, entityBatch.EntityDefinition.UserFriendlyEntityName));
                                }

                                foreach (var insertedRecord in entityBatch.RecordsToAdd)
                                {
                                    if (entityBatch.LoggingBehavior.MaxNumberOfInsertsWithoutErrorToLog.HasValue && !insertedRecord.HasError)
                                    {
                                        countOfInsertsWithoutError++;

                                        if (countOfInsertsWithoutError > entityBatch.LoggingBehavior.MaxNumberOfInsertsWithoutErrorToLog.Value)
                                            continue;
                                    }

                                    if (insertedRecord.HasError)
                                    {
                                        logLine.Append(errorLogLineMinusMessage);
                                        logLine.AppendLine(string.Format("INSERT failed with error: {0}. Command text is: {1}. Failed values are:",
                                            insertedRecord.ErrorMessage, insertedRecord.CommandText));
                                    }
                                    else if (jobInstance.Integration.LoggingLevel >= LoggingLevel.ErrorsWarningsAndInfo)
                                    {
                                        if (jobInstance.Integration.LoggingLevel >= LoggingLevel.ErrorsWarningsInfoAndDebug)
                                            logLine.AppendLine(string.Format("Command text successfully executed: {0}", insertedRecord.CommandText));

                                        logLine.Append(infoLogLineMinusMessage);
                                        logLine.AppendLine(string.Format("INSERT applied for keys \"{0}\" with values:",
                                            LogHelper.GetKeysAndValuesAsText(entityBatch.EntityDefinition.PrimaryKeyColumnNames, insertedRecord.PrimaryKeyValues.ToList())));
                                    }

                                    StringBuilder insertedMessage = new StringBuilder();

                                    foreach (var fieldValuePair in insertedRecord.FieldValuePairs)
                                    {
                                        insertedMessage.AppendLine(string.Format("{0}{1}: {2}",
                                            PRE_MESSAGES_TABS, fieldValuePair.Key, fieldValuePair.Value == null ? "NULL" : "'" + fieldValuePair.Value + "'"));
                                    }

                                    logLine.AppendLine(insertedMessage.ToString());

                                    WriteAndClearLogLineIfExceedsSize(jobInstance.Integration.Id, log, logLine);
                                }

                                foreach (var updatedRecord in entityBatch.RecordsToUpdate)
                                {
                                    if (entityBatch.LoggingBehavior.MaxNumberOfUpdatesWithoutErrorToLog.HasValue && !updatedRecord.HasError)
                                    {
                                        countOfUpdatesWithoutError++;

                                        if (countOfUpdatesWithoutError > entityBatch.LoggingBehavior.MaxNumberOfUpdatesWithoutErrorToLog.Value)
                                            continue;
                                    }

                                    if (updatedRecord.HasError)
                                    {
                                        logLine.Append(errorLogLineMinusMessage);
                                        logLine.AppendLine(string.Format("UPDATE with keys '{0}' failed with error: {1}.  Command text is: {2}. Failed values are:",
                                            LogHelper.GetKeysAndValuesAsText(entityBatch.EntityDefinition.PrimaryKeyColumnNames, updatedRecord.PrimaryKeyValues.ToList()), updatedRecord.ErrorMessage,
                                            updatedRecord.CommandText));
                                    }
                                    else if (jobInstance.Integration.LoggingLevel >= LoggingLevel.ErrorsWarningsAndInfo)
                                    {
                                        if (jobInstance.Integration.LoggingLevel >= LoggingLevel.ErrorsWarningsInfoAndDebug)
                                            logLine.AppendLine(string.Format("Command text successfully executed: {0}", updatedRecord.CommandText));

                                        logLine.Append(infoLogLineMinusMessage);
                                        logLine.AppendLine(string.Format("UPDATE applied for keys \"{0}\" with values:",
                                            LogHelper.GetKeysAndValuesAsText(entityBatch.EntityDefinition.PrimaryKeyColumnNames, updatedRecord.PrimaryKeyValues.ToList())));
                                    }

                                    StringBuilder updatedMessage = new StringBuilder();

                                    foreach (var fieldValuePair in updatedRecord.FieldValuePairs)
                                    {
                                        updatedMessage.AppendLine(string.Format("{0}{1}: {2} -> {3}",
                                            PRE_MESSAGES_TABS, fieldValuePair.Key,
                                            fieldValuePair.Value.OldValue == null ? "NULL" : "'" + fieldValuePair.Value.OldValue + "'",
                                            fieldValuePair.Value.NewValue == null ? "NULL" : "'" + fieldValuePair.Value.NewValue + "'"));
                                    }

                                    logLine.Append(updatedMessage.ToString());

                                    WriteAndClearLogLineIfExceedsSize(jobInstance.Integration.Id, log, logLine);
                                }

                                foreach (var deletedRecord in entityBatch.RecordsToDelete)
                                {
                                    if (entityBatch.LoggingBehavior.MaxNumberOfDeletionsWithoutErrorToLog.HasValue && !deletedRecord.HasError)
                                    {
                                        countOfDeletionsWithoutError++;

                                        if (countOfDeletionsWithoutError > entityBatch.LoggingBehavior.MaxNumberOfDeletionsWithoutErrorToLog.Value)
                                            continue;
                                    }

                                    if (deletedRecord.HasError)
                                    {
                                        logLine.Append(errorLogLineMinusMessage);
                                        logLine.AppendLine(string.Format("DELETE with keys \"{0}\" failed with error: {1}.",
                                            LogHelper.GetKeysAndValuesAsText(entityBatch.EntityDefinition.PrimaryKeyColumnNames, deletedRecord.PrimaryKeyValues.ToList()), deletedRecord.ErrorMessage));
                                    }
                                    else if (jobInstance.Integration.LoggingLevel >= LoggingLevel.ErrorsWarningsAndInfo)
                                    {
                                        if (jobInstance.Integration.LoggingLevel >= LoggingLevel.ErrorsWarningsInfoAndDebug)
                                            logLine.AppendLine(string.Format("Command text successfully executed: {0}", deletedRecord.CommandText));

                                        logLine.Append(infoLogLineMinusMessage);
                                        logLine.AppendLine(string.Format("DELETE applied for keys '{0}'.",
                                            LogHelper.GetKeysAndValuesAsText(entityBatch.EntityDefinition.PrimaryKeyColumnNames, deletedRecord.PrimaryKeyValues.ToList())));
                                    }

                                    WriteAndClearLogLineIfExceedsSize(jobInstance.Integration.Id, log, logLine);
                                }
                            }

                            log.Write(logLine);

                            log.Flush();
                            log.Close();
                        }

                        break;
                    }
                    catch (IOException)
                    {
                        if (i == NUMBER_OF_FILEACCESS_RETRIES)
                            throw;
                        else
                            Thread.Sleep(DELAY_ON_FILEACCES_RETRY);
                    }
                }
            }
        }

        private void WriteAndClearLogLineIfExceedsSize(Guid integrationId, TextWriter log, StringBuilder logLine)
        {
            if (logLine.Length > 500000)
            {
                logFileInfoByIntegrationId[integrationId].CurrentFileSizeInBytes += logLine.Length;

                log.Write(logLine);

                // clear StringBuilder (Clear method wasn't added until .NET 4.0)
                logLine.Length = 0;
                logLine.Capacity = 0;
            }
        }

        private static string GetLogLine(DateTime logDate, LogEntryType logEntryType, JobInstance jobInstance, JobStepInstance jobStepInstance, JobBatch jobBatch, string message)
        {
            StringBuilder fullLine = new StringBuilder();

            fullLine.Append(logDate);
            fullLine.Append("\t");
            fullLine.Append(logEntryType);
            fullLine.Append("\t");
            fullLine.Append(string.Format("{0} ({1})", jobInstance.Integration.Name, jobInstance.Integration.Id));
            fullLine.Append("\t");
            fullLine.Append(string.Format("{0} ({1})", jobBatch.AssociatedDataSource.DataSource.Name, jobBatch.AssociatedDataSource.DataSource.Id));
            fullLine.Append("\t");
            fullLine.Append(string.Format("{0} ({1})", jobInstance.Job.Name, jobInstance.Job.Id));
            fullLine.Append("\t");
            fullLine.Append(string.Format("{0} ({1})", jobInstance.QueueRequest.InvocationSourceType, jobInstance.QueueRequest.Id));
            fullLine.Append("\t");
            fullLine.Append(jobInstance.Id);
            fullLine.Append("\t");
            fullLine.Append(JobFilterHelper.GetTextForDatabase(jobInstance.Filters));
            fullLine.Append("\t");
            fullLine.Append(string.Format("{0} ({1})", jobStepInstance.JobStep.Name, jobStepInstance.JobStep.Id));
            fullLine.Append("\t");
            fullLine.Append(jobStepInstance.Id);
            fullLine.Append("\t");
            fullLine.Append(message);

            return fullLine.ToString();
        }

        private string GetLogFileHeader()
        {
            StringBuilder headerRow = new StringBuilder();

            headerRow.Append("Date/Time\t");
            headerRow.Append("Level\t");
            headerRow.Append("Integration\t");
            headerRow.Append("Data Source\t");
            headerRow.Append("Job\t");
            headerRow.Append("Queue Request\t");
            headerRow.Append("Job Instance ID\t");
            headerRow.Append("Filters\t");
            headerRow.Append("Job Step\t");
            headerRow.Append("Job Step Instance\t");
            headerRow.Append("Message");

            return headerRow.ToString();
        }

        private string GetServiceLogPathAndFileName()
        {
            if (serviceLogFileInfo.CurrentFileSizeInBytes >= MaxFileSize || serviceLogFileInfo.CurrentFileIndex == 0)
            {
                serviceLogFileInfo.CurrentFileIndex++;
                serviceLogFileInfo.CurrentFileSizeInBytes = 0;
                serviceLogFileInfo.FileName = null;
                serviceLogFileInfo.PathAndFileName = null;
            }

            if (serviceLogFileInfo.FileName == null)
            {
                var startOfFileName = string.Format("Integration Service - {0:yyyyMMdd}", DateTime.Now.Date);

                if (serviceLogFileInfo.CurrentFileIndex == 1)
                    serviceLogFileInfo.FileName = startOfFileName + ".log";
                else
                    serviceLogFileInfo.FileName = string.Format("{0}_{1:D2}.log", startOfFileName, serviceLogFileInfo.CurrentFileIndex);

                serviceLogFileInfo.PathAndFileName = LogDirectory + "\\" + serviceLogFileInfo.FileName;

                if (!File.Exists(serviceLogFileInfo.PathAndFileName))
                {
                    using (TextWriter log = File.CreateText(serviceLogFileInfo.PathAndFileName))
                    {
                        log.WriteLine(GetLogFileHeader());
                        log.Flush();
                        log.Close();
                    }
                }
            }

            return serviceLogFileInfo.PathAndFileName;
        }

        private string GetIntegrationLogPathAndFileName(Integration integration)
        {
            if (!logFileInfoByIntegrationId.ContainsKey(integration.Id))
            {
                logFileInfoByIntegrationId.Add(integration.Id, new TextFileLogInfo());
            }

            var logFileInfo = logFileInfoByIntegrationId[integration.Id];

            if (logFileInfo.CurrentFileSizeInBytes >= MaxFileSize || logFileInfo.CurrentFileIndex == 0)
            {
                logFileInfo.CurrentFileIndex++;
                logFileInfo.CurrentFileSizeInBytes = 0;
                logFileInfo.FileName = null;
                logFileInfo.PathAndFileName = null;
            }

            if (logFileInfo.FileName == null)
            {
                char[] invalidFileNameChars = Path.GetInvalidFileNameChars();

                // Builds a string out of valid chars from the integration name
                var validIntegrationName = new string(integration.Name.Where(ch => !invalidFileNameChars.Contains(ch)).ToArray());

                var startOfFileName = string.Format("{0} - {1} - {2:yyyyMMdd}", validIntegrationName, integration.Id, DateTime.Now.Date);

                if (logFileInfo.CurrentFileIndex == 1)
                    logFileInfo.FileName = startOfFileName + ".log";
                else
                    logFileInfo.FileName = string.Format("{0}_{1:D2}.log", startOfFileName, logFileInfo.CurrentFileIndex);

                logFileInfo.PathAndFileName = LogDirectory + "\\" + logFileInfo.FileName;

                if (!File.Exists(logFileInfo.PathAndFileName))
                {
                    using (TextWriter log = File.CreateText(logFileInfo.PathAndFileName))
                    {
                        log.WriteLine(GetLogFileHeader());
                        log.Flush();
                        log.Close();
                    }
                }
            }

            return logFileInfo.PathAndFileName;
        }

        private class TextFileLogInfo
        {
            public int CurrentFileIndex;

            public long CurrentFileSizeInBytes;

            public string FileName;

            public string PathAndFileName;
        }
    }
}

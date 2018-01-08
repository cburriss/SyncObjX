using System;
using System.Diagnostics;

namespace SyncObjX.Logging
{
    public static class EventViewerLogger
    {
        private static string source = "SyncObjX Integration Service";

        private static string logName = "Application";

        public static void WriteToLog(string message)
        {
            WriteToEventLog(message);
        }

        public static void WriteToLog(string format, params object[] args)
        {
            try
            {
                WriteToEventLog(string.Format(format, args));
            }
            catch (Exception ex)
            {
                WriteToLog(ex);
            }
        }

        public static void WriteToLog(Exception ex)
        {
            WriteToEventLog(ExceptionFormatter.Format(ex));
        }

        private static void WriteToEventLog(string message)
        {
            CreateEventSource();

            // Windows Event Viewer entries have a max length of 32,766 characters (but that # will cause an exception, so go lower)
            if (message != null && message.Length > 30000)
                EventLog.WriteEntry(source, message.Substring(0, 30000), EventLogEntryType.Error);
            else
                EventLog.WriteEntry(source, message, EventLogEntryType.Error);
        }

        private static void CreateEventSource()
        {
            if (!EventLog.SourceExists(source))
                EventLog.CreateEventSource(source, logName);
        }
    }
}

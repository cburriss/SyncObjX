using SyncObjX.Data;
using SyncObjX.Management;
using SyncObjX.SyncObjects;

namespace SyncObjX.Logging
{
    public interface ISyncEngineLogWriter
    {
        void WriteToLog(LogEntryType logEntryType, Integration integration, DataSource dataSource, JobInstance jobInstance, JobStepInstance jobStepInstance, string message);

        void WriteToLog(JobInstance jobInstance, JobStepInstance jobStepInstance, JobBatch jobBatch);
    }
}

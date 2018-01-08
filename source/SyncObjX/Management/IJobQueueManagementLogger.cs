using System;
using System.Collections.Generic;

namespace SyncObjX.Management
{
    public interface IJobQueueManagementLogger
    {
        void AddToQueueLog(JobInstance jobInstance);

        void UpdateJobStatusInQueueLog(JobInstance jobInstance);

        void UpdateJobStepStatusInQueueLog(JobStepInstance jobStepInstance);

        void DeleteFromQueueLog(IEnumerable<Guid> jobInstanceGuids);

        void DeleteFromQueueLog(Guid jobInstanceGuid);

        void UpdateJobDataSourceHistory(JobInstance jobInstance);

        void MoveToHistoryLog(JobInstance jobInstance);

        List<JobInstance> GetJobInstancesFromQueueLog(bool throwExceptionIfJobInstanceSyncObjectsHaveChanged = false);

        JobInstance GetJobInstanceFromQueueLog(Guid jobInstanceGuid, bool throwExceptionIfJobInstanceSyncObjectsHaveChanged);

        List<JobInstance> RecoverJobInstancesFromQueueLog();
    }
}

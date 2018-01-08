using System;

namespace SyncObjX.Management
{
    public class JobQueueLogManager
    {
        public static void AddLogger(IJobQueueManagementLogger logger)
        {
            var jobQueuedEventHandler = new EventHandler<JobQueuedArgs>((s, e) =>
            {
                logger.AddToQueueLog(e.QueuedJobInstance);
            });

            JobQueueManager.JobQueued += jobQueuedEventHandler;

            JobQueueManager.JobDequeued += new EventHandler<JobDequeuedArgs>((s, e) =>
            {
                logger.DeleteFromQueueLog(e.DequeuedJobInstance.Id);
            });

            JobQueueManager.JobInstanceStatusChanged += new EventHandler<JobQueueStatusChangedArgs>((s, e) =>
            {
                if (e.UpdatedJobInstance.Status == JobQueueStatus.Completed ||
                    e.UpdatedJobInstance.Status == JobQueueStatus.Completed_WithError ||
                    e.UpdatedJobInstance.Status == JobQueueStatus.Terminated_WithError ||
                    e.UpdatedJobInstance.Status == JobQueueStatus.DidNotRun_ErrorWithinQueueRequest)
                    //TODO: || args.UpdatedJobInstance.Status == JobQueueStatus.DidNotRun_JobCancellation
                {
                    logger.UpdateJobDataSourceHistory(e.UpdatedJobInstance);

                    logger.MoveToHistoryLog(e.UpdatedJobInstance);
                }
                else
                {
                    logger.UpdateJobStatusInQueueLog(e.UpdatedJobInstance);
                }
            });

            JobQueueManager.JobStepInstanceStatusChanged += new EventHandler<JobStepQueueStatusChangedArgs>((s, e) =>
            {
                logger.UpdateJobStepStatusInQueueLog(e.UpdatedJobStepInstance);
            });
        }

        public static void RefreshQueueFromLog(IJobQueueManagementLogger logger)
        {
            logger.RecoverJobInstancesFromQueueLog();
        }
    }
}

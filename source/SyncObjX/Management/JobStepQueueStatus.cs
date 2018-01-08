
namespace SyncObjX.Management
{
    public enum JobStepQueueStatus
    {
        Waiting = 1,
        InProgress = 2,
        Completed = 3,
        Completed_WithError = 4,
        DidNotRun_JobError = 5
    }
}

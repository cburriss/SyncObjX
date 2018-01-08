
namespace SyncObjX.Management
{
    public enum JobQueueStatus
    {
        Scheduled = 1,
        Delayed_NoAvailableThread = 2,
        InProgress = 3,
        InProgress_MaxDelayExceeded = 4,
        Completed = 5,
        Completed_WithError = 6,
        Terminated_WithError = 7,
        DidNotRun_ErrorWithinQueueRequest = 8
    }
}

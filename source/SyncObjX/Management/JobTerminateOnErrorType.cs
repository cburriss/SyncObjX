
namespace SyncObjX.Management
{
    public enum JobTerminateOnErrorType
    {
        NeverTerminateOnError = 0,
        TerminateExecutingStepOnly = 1,
        TerminateExecutingDataSourceOnly = 2,
        TerminateJob = 3
    }
}

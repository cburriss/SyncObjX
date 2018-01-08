
namespace SyncObjX.Management
{
    public abstract class JobBatchStep : JobStepInvocation
    {
        public abstract JobBatchStepOutput Process();
    }
}


namespace SyncObjX.Management
{
    public abstract class EntityBatchStep : JobStepInvocation
    {
        public abstract EntityBatchStepOutput Process();
    }
}
using SyncObjX.Data;

namespace SyncObjX.Management
{
    public class JobBatchStepOutput
    {
        public JobBatch SourceSideJobBatch;

        public JobBatch TargetSideJobBatch;

        public JobBatchStepOutput(JobBatch sourceSideJobBatch, JobBatch targetSideJobBatch)
        {
            SourceSideJobBatch = sourceSideJobBatch;

            TargetSideJobBatch = targetSideJobBatch;
        }
    }
}

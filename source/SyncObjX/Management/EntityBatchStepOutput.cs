using System.Collections.Generic;
using SyncObjX.Data;

namespace SyncObjX.Management
{
    public class EntityBatchStepOutput
    {
        public List<EntityBatch> SourceSideEntityBatches;

        public List<EntityBatch> TargetSideEntityBatches;

        public bool DeferExecutionUntilNextStep;

        public EntityBatchStepOutput(EntityBatch sourceSideEntityBatch, EntityBatch targetSideEntityBatch, bool deferExecutionUntilNextStep = false)
            : this(sourceSideEntityBatch == null ? null : new List<EntityBatch>() { sourceSideEntityBatch }, 
                   targetSideEntityBatch == null ? null : new List<EntityBatch>() { targetSideEntityBatch }, 
                   deferExecutionUntilNextStep) { }

        public EntityBatchStepOutput(List<EntityBatch> sourceSideEntityBatches, List<EntityBatch> targetSideEntityBatches, bool deferExecutionUntilNextStep = false)
        {
            SourceSideEntityBatches = sourceSideEntityBatches;

            TargetSideEntityBatches = targetSideEntityBatches;

            DeferExecutionUntilNextStep = deferExecutionUntilNextStep;
        }
    }
}

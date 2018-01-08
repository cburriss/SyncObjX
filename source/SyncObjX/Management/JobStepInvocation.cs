using SyncObjX.Configuration;
using SyncObjX.SyncObjects;

namespace SyncObjX.Management
{
    public abstract class JobStepInvocation
    {
        public Integration Integration { get; private set; }

        public JobInstance JobInstance { get; private set; }

        public JobStepInstance JobStepInstance { get; private set; }

        public JobStepInstance PreviousJobStepInstance { get; private set; }

        public ISyncEngineConfigurator Configurator { get; private set; }

        public void Initialize(Integration integration, JobInstance jobInstance,
                               JobStepInstance previousJobStepInstance, JobStepInstance jobStepInstance, ISyncEngineConfigurator configurator)
        {
            Integration = integration;

            JobInstance = jobInstance;

            PreviousJobStepInstance = previousJobStepInstance;

            JobStepInstance = jobStepInstance;

            Configurator = configurator;
        }
    }
}

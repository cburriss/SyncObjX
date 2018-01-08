using System;
using SyncObjX.Configuration;
using SyncObjX.Core;
using SyncObjX.Data;
using SyncObjX.Exceptions;
using SyncObjX.Logging;

namespace SyncObjX.Management
{
    public static class JobStepInvoker
    {
        public static void ExecuteStep(JobInstance jobInstance, JobStepInstance previousJobStepInstance, 
                                       JobStepInstance currentJobStepInstance, ISyncEngineConfigurator configurator)
        {
            Type jobStepType = jobInstance.Integration.PackageAssembly.GetType(currentJobStepInstance.JobStep.FullyQualifiedName);

            if (jobStepType == null)
                throw new Exception(string.Format("Job step with fully qualified name '{0}' was not found in assembly '{1}'.", currentJobStepInstance.JobStep.FullyQualifiedName, jobInstance.Integration.PackageAssembly.Location));

            // ensure the step class inherits from the proper base class to ensure the Initialize method is available
            bool hasCorrectBaseType = false;

            var baseType = jobStepType.BaseType;

            while (baseType != typeof(Object))
            {
                if (baseType == typeof(JobStepInvocation))
                {
                    hasCorrectBaseType = true;
                    break;
                }

                baseType = baseType.BaseType;
            }

            if (!hasCorrectBaseType)
                throw new Exception(string.Format("Job step class '{0}' must derive from '{1}'.", jobStepType.Name, typeof(JobStepInvocation).FullName));

            var jobStepInvocation = Activator.CreateInstance(jobStepType);

            if (jobStepInvocation is CustomActionStep)
            {
                var jobStepObj = (CustomActionStep)jobStepInvocation;

                jobStepObj.Initialize(jobInstance.Integration, jobInstance, previousJobStepInstance, currentJobStepInstance, configurator);

                jobStepObj.Process();

                if (previousJobStepInstance != null 
                    && ((previousJobStepInstance.HasDeferredExecutionUntilNextStep.HasValue 
                    && previousJobStepInstance.HasDeferredExecutionUntilNextStep.Value == true) 
                    || (previousJobStepInstance.GetType() is CustomActionStep)))
                {
                    currentJobStepInstance.SourceJobBatch = previousJobStepInstance.SourceJobBatch;
                    currentJobStepInstance.TargetJobBatch = previousJobStepInstance.TargetJobBatch;
                    currentJobStepInstance.HasDeferredExecutionUntilNextStep = true;
                }
                else
                    currentJobStepInstance.HasDeferredExecutionUntilNextStep = false;
            }
            else if (jobStepInvocation is DataMapStep)
            {
                var jobStepObj = (DataMapStep)jobStepInvocation;

                jobStepObj.Initialize(jobInstance.Integration, jobInstance, previousJobStepInstance, currentJobStepInstance, configurator);

                var dataMapOutput = jobStepObj.Process();

                if (dataMapOutput == null)
                    throw new Exception("Job step must return a value.");

                currentJobStepInstance.HasDeferredExecutionUntilNextStep = dataMapOutput.DeferExecutionUntilNextStep;

                JobBatch sourceJobBatch = null;
                JobBatch targetJobBatch = null;
                EntityBatch sourceEntityBatch = null;
                EntityBatch targetEntityBatch = null;
                EntityBatch oneWayEntityBatch = null;
                IOneWayDataMap oneWayDataMap = null;
                TwoWayDataMap twoWayDataMap = null;

                if (dataMapOutput.DataMap is IOneWayDataMap)
                {
                    oneWayDataMap = (IOneWayDataMap)dataMapOutput.DataMap;

                    if (dataMapOutput.DataMap is OneWayDataMap)
                    {
                        oneWayEntityBatch = OneToOneDataMapProcessor.Compare((OneWayDataMap)dataMapOutput.DataMap,
                                                                             dataMapOutput.SourceData, dataMapOutput.SourceDataDuplicateRowBehavior,
                                                                             dataMapOutput.TargetData, dataMapOutput.TargetDataDuplicateRowBehavior,
                                                                             dataMapOutput.RowsToProcess);
                        
                    }
                    else if (dataMapOutput.DataMap is OneToMany_OneWayDataMap)
                    {
                        if (dataMapOutput.RowsToProcess != null)
                            throw new Exception("Rows to process Func is not supported for one-to-many data maps.");

                        oneWayEntityBatch = OneToManyDataMapProcessor.Compare((OneToMany_OneWayDataMap)dataMapOutput.DataMap,
                                                                              dataMapOutput.SourceData, dataMapOutput.SourceDataDuplicateRowBehavior,
                                                                              dataMapOutput.TargetData, dataMapOutput.TargetDataDuplicateRowBehavior);
                    }

                    if (oneWayEntityBatch.EntityDefinition.SyncSide == SyncSide.Source)
                    {
                        oneWayEntityBatch.LoggingBehavior = dataMapOutput.SourceSideLoggingBehavior;
                    }
                    else if (oneWayEntityBatch.EntityDefinition.SyncSide == SyncSide.Target)
                    {
                        oneWayEntityBatch.LoggingBehavior = dataMapOutput.TargetSideLoggingBehavior;
                    }
                }
                else if (dataMapOutput.DataMap is TwoWayDataMap)
                {
                    twoWayDataMap = (TwoWayDataMap)dataMapOutput.DataMap;

                    OneToOneDataMapProcessor.Compare(twoWayDataMap, dataMapOutput.SourceData, dataMapOutput.SourceDataDuplicateRowBehavior, 
                                                    dataMapOutput.TargetData, dataMapOutput.TargetDataDuplicateRowBehavior,
                                                    dataMapOutput.RowsToProcess, out sourceEntityBatch, out targetEntityBatch);

                    sourceEntityBatch.LoggingBehavior = dataMapOutput.SourceSideLoggingBehavior;
                    targetEntityBatch.LoggingBehavior = dataMapOutput.TargetSideLoggingBehavior;
                }
                else
                    throw new DerivedClassNotImplementedException<OneToOneDataMap>(dataMapOutput.DataMap);

                if (previousJobStepInstance != null 
                    && previousJobStepInstance.HasDeferredExecutionUntilNextStep.HasValue
                    && previousJobStepInstance.HasDeferredExecutionUntilNextStep.Value == true)
                {
                    sourceJobBatch = previousJobStepInstance.SourceJobBatch;
                    targetJobBatch = previousJobStepInstance.TargetJobBatch;
                }
                else
                {
                    sourceJobBatch = new JobBatch(SyncSide.Source, jobInstance.SourceDataSource);
                    targetJobBatch = new JobBatch(SyncSide.Target, jobInstance.TargetDataSource);
                }

                if (dataMapOutput.DataMap is IOneWayDataMap)
                {
                    if (oneWayDataMap.SyncDirection == SyncDirection.SourceToTarget)
                        targetJobBatch.EntityBatches.Add(oneWayEntityBatch);
                    else if (oneWayDataMap.SyncDirection == SyncDirection.TargetToSource)
                        sourceJobBatch.EntityBatches.Add(oneWayEntityBatch);
                    else
                        throw new EnumValueNotImplementedException<SyncDirection>(oneWayDataMap.SyncDirection);
                }
                else if (dataMapOutput.DataMap is TwoWayDataMap)
                {
                    sourceJobBatch.EntityBatches.Add(sourceEntityBatch);
                    targetJobBatch.EntityBatches.Add(targetEntityBatch);
                }
                else
                    throw new DerivedClassNotImplementedException<OneToOneDataMap>(dataMapOutput.DataMap);

                currentJobStepInstance.SourceJobBatch = sourceJobBatch;
                currentJobStepInstance.TargetJobBatch = targetJobBatch;

                if (!currentJobStepInstance.HasDeferredExecutionUntilNextStep.Value)
                {
                    sourceJobBatch.SubmitToDataSource();

                    if (sourceJobBatch.HasRecordErrors)
                    {
                        currentJobStepInstance.HasRecordErrors = true;
                        currentJobStepInstance.Exceptions = sourceJobBatch.GetExceptions();
                    }

                    SyncEngineLogger.WriteToLog(jobInstance, currentJobStepInstance, sourceJobBatch);

                    targetJobBatch.SubmitToDataSource();

                    if (targetJobBatch.HasRecordErrors)
                    {
                        currentJobStepInstance.HasRecordErrors = true;
                        currentJobStepInstance.Exceptions = targetJobBatch.GetExceptions();
                    }

                    SyncEngineLogger.WriteToLog(jobInstance, currentJobStepInstance, targetJobBatch);
                }
            }
            else if (jobStepInvocation is EntityBatchStep)
            {
                var jobStepObj = (EntityBatchStep)jobStepInvocation;

                jobStepObj.Initialize(jobInstance.Integration, jobInstance, previousJobStepInstance, currentJobStepInstance, configurator);

                var entityBatchOutput = jobStepObj.Process();

                if (entityBatchOutput == null)
                    throw new Exception("Job step must return a value.");

                currentJobStepInstance.HasDeferredExecutionUntilNextStep = entityBatchOutput.DeferExecutionUntilNextStep;

                JobBatch sourceJobBatch;
                JobBatch targetJobBatch;

                if (previousJobStepInstance != null && previousJobStepInstance.HasDeferredExecutionUntilNextStep.Value == true)
                {
                    sourceJobBatch = previousJobStepInstance.SourceJobBatch;
                    targetJobBatch = previousJobStepInstance.TargetJobBatch;
                }
                else
                {
                    sourceJobBatch = new JobBatch(SyncSide.Source, jobInstance.SourceDataSource);
                    targetJobBatch = new JobBatch(SyncSide.Target, jobInstance.TargetDataSource);
                }

                if (entityBatchOutput.SourceSideEntityBatches != null)
                {
                    foreach (var sourceSideEntityBatch in entityBatchOutput.SourceSideEntityBatches)
                    {
                        sourceJobBatch.EntityBatches.Add(sourceSideEntityBatch);
                    }
                }

                if (entityBatchOutput.TargetSideEntityBatches != null)
                {
                    foreach (var targetSideEntityBatch in entityBatchOutput.TargetSideEntityBatches)
                    {
                        targetJobBatch.EntityBatches.Add(targetSideEntityBatch);
                    }
                }

                currentJobStepInstance.SourceJobBatch = sourceJobBatch;
                currentJobStepInstance.TargetJobBatch = targetJobBatch;

                if (!currentJobStepInstance.HasDeferredExecutionUntilNextStep.Value)
                {
                    sourceJobBatch.SubmitToDataSource();

                    if (sourceJobBatch.HasRecordErrors)
                    {
                        currentJobStepInstance.HasRecordErrors = true;
                        currentJobStepInstance.Exceptions = sourceJobBatch.GetExceptions();
                    }

                    SyncEngineLogger.WriteToLog(jobInstance, currentJobStepInstance, sourceJobBatch);

                    targetJobBatch.SubmitToDataSource();

                    if (targetJobBatch.HasRecordErrors)
                    {
                        currentJobStepInstance.HasRecordErrors = true;
                        currentJobStepInstance.Exceptions = targetJobBatch.GetExceptions();
                    }

                    SyncEngineLogger.WriteToLog(jobInstance, currentJobStepInstance, targetJobBatch);
                }  
            }
            else if (jobStepInvocation is JobBatchStep)
            {
                var jobStepObj = (JobBatchStep)jobStepInvocation;

                jobStepObj.Initialize(jobInstance.Integration, jobInstance, previousJobStepInstance, currentJobStepInstance, configurator);

                var jobBatchOutput = jobStepObj.Process();

                if (jobBatchOutput == null)
                    throw new Exception("Job step must return a value.");

                if (jobBatchOutput.SourceSideJobBatch != null)
                {
                    jobBatchOutput.SourceSideJobBatch.SubmitToDataSource();

                    if (jobBatchOutput.SourceSideJobBatch.HasRecordErrors)
                    {
                        currentJobStepInstance.HasRecordErrors = true;
                        currentJobStepInstance.Exceptions = jobBatchOutput.SourceSideJobBatch.GetExceptions();
                    }

                    SyncEngineLogger.WriteToLog(jobInstance, currentJobStepInstance, jobBatchOutput.SourceSideJobBatch);
                }
                else
                    SyncEngineLogger.WriteToLog(LogEntryType.Info, jobInstance, currentJobStepInstance, jobInstance.SourceDataSource.DataSource,
                        "Job step '{0}' for job '{1}' returned a null job batch for source-side data source '{2}'.",
                        currentJobStepInstance.JobStep.Name, jobInstance.Job.Name, jobInstance.SourceDataSource.DataSource.Name);

                if (jobBatchOutput.TargetSideJobBatch != null)
                {
                    jobBatchOutput.TargetSideJobBatch.SubmitToDataSource();

                    if (jobBatchOutput.TargetSideJobBatch.HasRecordErrors)
                    {
                        currentJobStepInstance.HasRecordErrors = true;
                        currentJobStepInstance.Exceptions = jobBatchOutput.TargetSideJobBatch.GetExceptions();
                    }

                    SyncEngineLogger.WriteToLog(jobInstance, currentJobStepInstance, jobBatchOutput.TargetSideJobBatch);
                }
                else
                    SyncEngineLogger.WriteToLog(LogEntryType.Info, jobInstance, currentJobStepInstance, jobInstance.TargetDataSource.DataSource,
                        "Job step '{0}' for job '{1}' returned a null job batch for target-side data source '{2}'.",
                        currentJobStepInstance.JobStep.Name, jobInstance.Job.Name, jobInstance.TargetDataSource.DataSource.Name);
            }
            else
                throw new DerivedClassNotImplementedException<JobStepInvocation>(jobStepInvocation);
        }
    }
}

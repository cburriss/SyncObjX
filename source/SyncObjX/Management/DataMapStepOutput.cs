using System;
using System.Data;
using SyncObjX.Core;
using SyncObjX.Logging;

namespace SyncObjX.Management
{
    public class DataMapStepOutput
    {
        private EntityBatchLoggingBehavior _sourceSideLoggingBehavior = new EntityBatchLoggingBehavior();

        private EntityBatchLoggingBehavior _targetSideLoggingBehavior = new EntityBatchLoggingBehavior();

        public DataMap DataMap;

        public DataTable SourceData;

        public DuplicateRowBehavior SourceDataDuplicateRowBehavior;

        public DataTable TargetData;

        public DuplicateRowBehavior TargetDataDuplicateRowBehavior;

        public Func<DataRow, bool> RowsToProcess;

        public bool DeferExecutionUntilNextStep;

        public EntityBatchLoggingBehavior SourceSideLoggingBehavior
        {
            get { return _sourceSideLoggingBehavior; }

            set
            {
                if (DataMap is OneWayDataMap)
                {
                    var oneWayDataMap = (OneWayDataMap)DataMap;

                    if (oneWayDataMap.SyncDirection == SyncDirection.SourceToTarget)
                        throw new Exception(string.Format("{0}-side logging behavior can not be updated for sync direction '{1}'.",
                                                       Enum.GetName(typeof(SyncSide), oneWayDataMap.EntityToUpdateDefinition.SyncSide),
                                                       Enum.GetName(typeof(SyncDirection), oneWayDataMap.SyncDirection)));
                }

                _sourceSideLoggingBehavior = value;
            }
        }

        public EntityBatchLoggingBehavior TargetSideLoggingBehavior
        {
            get { return _targetSideLoggingBehavior; }

            set
            {
                if (DataMap is OneWayDataMap)
                {
                    var oneWayDataMap = (OneWayDataMap)DataMap;

                    if (oneWayDataMap.SyncDirection == SyncDirection.TargetToSource)
                        throw new Exception(string.Format("{0}-side logging behavior can not be updated for sync direction '{1}'.",
                                                       Enum.GetName(typeof(SyncSide), oneWayDataMap.EntityToUpdateDefinition.SyncSide),
                                                       Enum.GetName(typeof(SyncDirection), oneWayDataMap.SyncDirection)));
                }

                _targetSideLoggingBehavior = value;
            }
        }

        public DataMapStepOutput(DataMap dataMap, DataTable sourceData, DataTable targetData, bool deferExecutionUntilNextStep = false)
            : this(dataMap, sourceData, DuplicateRowBehavior.ThrowException, 
                   targetData, DuplicateRowBehavior.ThrowException, null, deferExecutionUntilNextStep) { }

        public DataMapStepOutput(DataMap dataMap, DataTable sourceData, DataTable targetData, Func<DataRow, bool> rowsToProcess, 
                                 bool deferExecutionUntilNextStep = false)
            : this(dataMap, sourceData, DuplicateRowBehavior.ThrowException, 
                   targetData, DuplicateRowBehavior.ThrowException, rowsToProcess, deferExecutionUntilNextStep) { }

        public DataMapStepOutput(DataMap dataMap, 
                                 DataTable sourceData, DuplicateRowBehavior sourceDataDuplicateRowBehavior,
                                 DataTable targetData, DuplicateRowBehavior targetDataDuplicateRowBehavior,
                                 bool deferExecutionUntilNextStep = false)
            : this(dataMap, sourceData, sourceDataDuplicateRowBehavior,
                   targetData, targetDataDuplicateRowBehavior, null, deferExecutionUntilNextStep) { }

        public DataMapStepOutput(DataMap dataMap, 
                                 DataTable sourceData, DuplicateRowBehavior sourceDataDuplicateRowBehavior,
                                 DataTable targetData, DuplicateRowBehavior targetDataDuplicateRowBehavior, 
                                 Func<DataRow, bool> rowsToProcess, bool deferExecutionUntilNextStep = false)
        {
            if (dataMap == null)
                throw new Exception("Data map can not be null.");

            if (sourceData == null)
                throw new Exception("Source data can not be null.");

            if (targetData == null)
                throw new Exception("Target data can not be null.");

            DataMap = dataMap;

            SourceData = sourceData;

            SourceDataDuplicateRowBehavior = sourceDataDuplicateRowBehavior;

            TargetData = targetData;

            TargetDataDuplicateRowBehavior = targetDataDuplicateRowBehavior;

            RowsToProcess = rowsToProcess;

            DeferExecutionUntilNextStep = deferExecutionUntilNextStep;
        }
    }
}

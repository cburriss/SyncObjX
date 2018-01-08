using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using SyncObjX.Exceptions;

namespace SyncObjX.Core
{
    public abstract class DataMap
    {
        public abstract DataTransferType TransferType { get; }

        public JoinFieldCollection JoinKeysCollection;

        protected List<string> _addedSourceFields = new List<string>();

        protected List<string> _addedTargetFields = new List<string>();

        protected List<CustomSetField> _customSetFields = new List<CustomSetField>();

        public IEnumerable<CustomSetField> CustomSetFields
        {
            get { return _customSetFields; }
        }

        public DataMap(JoinFieldCollection joinKeys)
        {

            if (joinKeys == null)
                throw new Exception("Join keys can not be null.");

            if (joinKeys.Count == 0)
                throw new Exception("At least one join key pair is required.");

            JoinKeysCollection = joinKeys;
        }

        protected void AddCustomSetField(SyncSide syncSide, string fieldToCompare, string fieldToUpdate, Func<DataRow, object> customSetMethod, SyncOperation appliesTo, bool onlyApplyWithOtherChanges)
        {
            if (_customSetFields.Exists(d => (appliesTo.HasFlag(SyncOperation.Inserts) || appliesTo.HasFlag(SyncOperation.All))
                                                      && (d.AppliesTo.HasFlag(SyncOperation.Inserts) || d.AppliesTo.HasFlag(SyncOperation.All))
                                                      && d.SyncSide == syncSide
                                                      && string.Equals(d.FieldNameToUpdate, fieldToUpdate, StringComparison.OrdinalIgnoreCase)))
            {
                throw new Exception(string.Format("Field '{0}' already exists for operation '{1}' and sync side '{2}'.",
                                                fieldToUpdate, Enum.GetName(typeof(SyncOperation), SyncOperation.Inserts), Enum.GetName(typeof(SyncSide), syncSide)));
            }

            if (_customSetFields.Exists(d => (appliesTo.HasFlag(SyncOperation.Updates) || appliesTo.HasFlag(SyncOperation.All))
                                                          && (d.AppliesTo.HasFlag(SyncOperation.Updates) || d.AppliesTo.HasFlag(SyncOperation.All))
                                                          && d.SyncSide == syncSide
                                                          && string.Equals(d.FieldNameToUpdate, fieldToUpdate, StringComparison.OrdinalIgnoreCase)))
            {
                throw new Exception(string.Format("Field '{0}' already exists for operation '{1}' and sync side '{2}'.",
                                                fieldToUpdate, Enum.GetName(typeof(SyncOperation), SyncOperation.Updates), Enum.GetName(typeof(SyncSide), syncSide)));
            }

            if (_customSetFields.Exists(d => (appliesTo.HasFlag(SyncOperation.Deletes) || appliesTo.HasFlag(SyncOperation.All))
                                                          && (d.AppliesTo.HasFlag(SyncOperation.Deletes) || d.AppliesTo.HasFlag(SyncOperation.All))
                                                          && d.SyncSide == syncSide
                                                          && string.Equals(d.FieldNameToUpdate, fieldToUpdate, StringComparison.OrdinalIgnoreCase)))
            {
                throw new Exception(string.Format("Field '{0}' already exists for operation '{1}' and sync side '{2}'.",
                                                fieldToUpdate, Enum.GetName(typeof(SyncOperation), SyncOperation.Deletes), Enum.GetName(typeof(SyncSide), syncSide)));
            }

            switch (syncSide)
            {
                case SyncSide.Source:

                    if (!_addedSourceFields.Contains(fieldToUpdate, StringComparer.OrdinalIgnoreCase))
                        AddToSyncSideFieldsList(syncSide, fieldToUpdate);

                    break;

                case SyncSide.Target:

                    if (!_addedTargetFields.Contains(fieldToUpdate, StringComparer.OrdinalIgnoreCase))
                        AddToSyncSideFieldsList(syncSide, fieldToUpdate);

                    break;

                default:
                    throw new EnumValueNotImplementedException<SyncSide>(syncSide);
            }

            _customSetFields.Add(new CustomSetField(syncSide, fieldToCompare, fieldToUpdate, customSetMethod, appliesTo, onlyApplyWithOtherChanges));
        }

        protected void AddToSyncSideFieldsList(SyncSide syncSide, string fieldName)
        {
            if (String.IsNullOrWhiteSpace(fieldName))
                throw new Exception("Field name is missing or empty.");

            fieldName = fieldName.Trim();

            switch (syncSide)
            {
                case SyncSide.Source:

                    if (_addedSourceFields.Exists(d => d.Equals(fieldName, StringComparison.OrdinalIgnoreCase)))
                        throw new Exception(string.Format("Source-side field already exists with name '{0}'.", fieldName));
                    else
                        _addedSourceFields.Add(fieldName.Trim());

                    break;

                case SyncSide.Target:

                    if (_addedTargetFields.Exists(d => d.Equals(fieldName, StringComparison.OrdinalIgnoreCase)))
                        throw new Exception(string.Format("Target-side field already exists with name '{0}'.", fieldName));
                    else
                        _addedTargetFields.Add(fieldName);

                    break;

                default:
                    throw new EnumValueNotImplementedException<SyncSide>(syncSide);
            }
        }

        public abstract HashSet<string> GetMappedDataFieldNames(SyncSide syncSide, DataMapFieldType includedFieldTypes, string prefix = "");
    }
}

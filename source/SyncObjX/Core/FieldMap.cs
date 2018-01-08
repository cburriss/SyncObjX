using System;
using System.Collections.Generic;
using SyncObjX.Exceptions;

namespace SyncObjX.Core
{
    public abstract class FieldMap
    {
        private List<FieldMapPair> _maps = new List<FieldMapPair>();

        public readonly StringComparison ComparisonType;

        protected string SourceSideDefaultValue;

        protected string TargetSideDefaultValue;

        protected UnmappedValueBehavior SourceSideUnmappedValueBehavior;

        protected UnmappedValueBehavior TargetSideUnmappedValueBehavior;

        public IEnumerable<FieldMapPair> Maps
        {
            get { return _maps; }
        }

        public FieldMap(UnmappedValueBehavior sourceSideUnmappedValueBehavior, 
            UnmappedValueBehavior targetSideUnmappedValueBehavior)
            : this(sourceSideUnmappedValueBehavior, targetSideUnmappedValueBehavior, StringComparison.OrdinalIgnoreCase)
        {
        }

        public FieldMap(UnmappedValueBehavior sourceSideUnmappedValueBehavior, 
            UnmappedValueBehavior targetSideUnmappedValueBehavior, StringComparison comparisonType)
        {
            SourceSideUnmappedValueBehavior = sourceSideUnmappedValueBehavior;

            TargetSideUnmappedValueBehavior = targetSideUnmappedValueBehavior;

            ComparisonType = comparisonType;
        }

        /// <summary>
        /// Sets UnmappedBehavior to 'UseDefaultValue'.
        /// </summary>
        public FieldMap(string sourceSideDefaultValue, string targetSideDefaultValue)
            : this(sourceSideDefaultValue, targetSideDefaultValue, StringComparison.OrdinalIgnoreCase) { }

        /// <summary>
        /// Sets UnmappedBehavior to 'UseDefaultValue'.
        /// </summary>
        /// <param name="defaultValue"></param>
        /// <param name="comparisonType"></param>
        public FieldMap(string sourceSideDefaultValue, string targetSideDefaultValue, StringComparison comparisonType)
        {
            SourceSideDefaultValue = sourceSideDefaultValue;

            TargetSideDefaultValue = targetSideDefaultValue;

            SourceSideUnmappedValueBehavior = UnmappedValueBehavior.UseDefaultValue;

            TargetSideUnmappedValueBehavior = UnmappedValueBehavior.UseDefaultValue;

            ComparisonType = comparisonType;
        }

        protected void AddMap(string sourceValue, string targetValue)
        {
            _maps.Add(new FieldMapPair(sourceValue, targetValue));
        }

        protected void AddMap(FieldMapPair fieldMapPair)
        {
            _maps.Add(fieldMapPair);
        }

        public bool SourceValueIsDuplicate(FieldMapPair fieldMapPair)
        {
            return SourceValueIsDuplicate(fieldMapPair.SourceValue);
        }

        public bool SourceValueIsDuplicate(string sourceValue)
        {
            if (sourceValue == null)
            {
                foreach (FieldMapPair fmp in _maps)
                {
                    if (fmp.SourceValue == null)
                        return true;
                }
            }
            else
            {
                foreach (FieldMapPair fmp in _maps)
                {
                    if (fmp.SourceValue != null && fmp.SourceValue.Trim().Equals(sourceValue.Trim(), ComparisonType))
                        return true;
                }
            }

            return false;
        }

        public bool TargetValueIsDuplicate(FieldMapPair fieldMapPair)
        {
            return TargetValueIsDuplicate(fieldMapPair.TargetValue);
        }

        public bool TargetValueIsDuplicate(string targetValue)
        {
            if (targetValue == null)
            {
                foreach (FieldMapPair fmp in _maps)
                {
                    if (fmp.TargetValue == null)
                        return true;
                }
            }
            else
            {
                foreach (FieldMapPair fmp in _maps)
                {
                    if (fmp.TargetValue != null && fmp.TargetValue.Trim().Equals(targetValue.Trim(), 
                        ComparisonType))
                        return true;
                }
            }

            return false;
        }

        public string GetMappedValueWithExceptionThrower(string fieldToUpdateDescription, 
            SyncSide syncSideToUpdate, string nonUpdateSideValue, string updateSideValue)
        {
            try
            {
                if (syncSideToUpdate == SyncSide.Source)
                    return GetMappedValue(updateSideValue, nonUpdateSideValue, syncSideToUpdate);
                else if (syncSideToUpdate == SyncSide.Target)
                    return GetMappedValue(nonUpdateSideValue, updateSideValue, syncSideToUpdate);
                else
                    throw new EnumValueNotImplementedException<SyncSide>(syncSideToUpdate);
            }
            catch (UnmappedValueException ex)
            {
                UnmappedValueExceptionHandler.ThrowException(fieldToUpdateDescription, ex);
            }

            return null;
        }

        public string GetMappedValueWithExceptionThrower(string fieldToUpdateDescription, 
            SyncSide syncSideToUpdate, string nonUpdateSideValue)
        {
            try
            {
                if (syncSideToUpdate == SyncSide.Source)
                    return GetMappedValue(null, nonUpdateSideValue, syncSideToUpdate);
                else if (syncSideToUpdate == SyncSide.Target)
                    return GetMappedValue(nonUpdateSideValue, null, syncSideToUpdate);
                else
                    throw new EnumValueNotImplementedException<SyncSide>(syncSideToUpdate);
            }
            catch (UnmappedValueException ex)
            {
                UnmappedValueExceptionHandler.ThrowException(fieldToUpdateDescription, ex);
            }

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceValue"></param>
        /// <param name="targetValue"></param>
        /// <param name="syncDirection"></param>
        /// <returns>Returns the new value.  The return value will be equal to the old value if no changes were applied.</returns>
        public string GetMappedValue(string sourceValue, string targetValue, SyncSide syncSideToUpdate)
        {
            if (sourceValue != null)
                sourceValue = sourceValue.Trim();

            if (targetValue != null)
                targetValue = targetValue.Trim();

            foreach (FieldMapPair fmp in Maps)
            {
                if (syncSideToUpdate == SyncSide.Target)
                {
                    if (string.Equals(sourceValue, fmp.SourceValue, ComparisonType))
                        return GetAppliedFieldMapValue(targetValue, fmp.TargetValue);
                }
                else if (syncSideToUpdate == SyncSide.Source)
                {
                    if (string.Equals(targetValue, fmp.TargetValue, ComparisonType))
                        return GetAppliedFieldMapValue(sourceValue, fmp.SourceValue);
                }
                else
                    throw new EnumValueNotImplementedException<SyncSide>(syncSideToUpdate);
            }

            if (syncSideToUpdate == SyncSide.Target)
            {
                // the source value was not found in the fied map, so apply unmapped behavior
                switch (TargetSideUnmappedValueBehavior)
                {
                    case UnmappedValueBehavior.DoNotUpdate:
                        // As DoNotUpdate is configured, return the old target value if the source key wasn't found
                        return targetValue;

                    case UnmappedValueBehavior.PassThrough:
                        // if the source value wasn't found in the map, "pass through" the source value to target as is
                        return sourceValue;

                    case UnmappedValueBehavior.UseDefaultValue:
                        // if the source value wasn't found in the map, return the configured default value
                        return TargetSideDefaultValue;

                    case UnmappedValueBehavior.ThrowException:
                        throw new UnmappedValueException(SyncDirection.SourceToTarget, sourceValue);

                    default:
                        throw new EnumValueNotImplementedException<UnmappedValueBehavior>(TargetSideUnmappedValueBehavior);
                }
            }
            else if (syncSideToUpdate == SyncSide.Source)
            {
                // the source value was not found in the fied map, so apply unmapped behavior
                switch (SourceSideUnmappedValueBehavior)
                {
                    case UnmappedValueBehavior.DoNotUpdate:
                        // As DoNotUpdate is configured, return the old target value if the source key wasn't found
                        return sourceValue;

                    case UnmappedValueBehavior.PassThrough:
                        // if the source value wasn't found in the map, "pass through" the source value to target as is
                        return targetValue;

                    case UnmappedValueBehavior.UseDefaultValue:
                        // if the source value wasn't found in the map, return the configured default value
                        return SourceSideDefaultValue;

                    case UnmappedValueBehavior.ThrowException:
                        throw new UnmappedValueException(SyncDirection.TargetToSource, targetValue);

                    default:
                        throw new EnumValueNotImplementedException<UnmappedValueBehavior>(TargetSideUnmappedValueBehavior);
                }
            }
            else
                throw new EnumValueNotImplementedException<SyncSide>(syncSideToUpdate);
        }

        private string GetAppliedFieldMapValue(string oldValue, string newValue)
        {
            // use case-sensitive comparison when determing if values are different
            if (oldValue.Equals(newValue))
                return oldValue;
            else
                return newValue;
        }
    }
}

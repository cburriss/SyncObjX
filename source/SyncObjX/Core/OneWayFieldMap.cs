using System;
using System.Linq;
using SyncObjX.Exceptions;

namespace SyncObjX.Core
{
    /// <summary>
    /// This field map allows duplicate source or target values within the map definition, depending upon the sync direction.
    /// 
    /// For example, if Source has values 'A' and 'B', both may be mapped to the same Target value (A -> C and B -> C).
    /// On the contrary, it is not permissible for 'A' to map to more than one Target value.
    /// </summary>
    public class OneWayFieldMap : FieldMap
    {
        private SyncDirection? _syncDirection;

        public SyncDirection? SyncDirection
        {
            get { return _syncDirection; }

            set
            {
                _syncDirection = value;

                ThrowExceptionIfInvalidDuplicateFieldMapValues();
            }
        }

        public string DefaultValue
        {
            // return Source-side always; should always match Target-side
            get { return base.SourceSideDefaultValue; }

            set
            {
                base.SourceSideDefaultValue = value;
                base.TargetSideDefaultValue = value;
            }
        }

        public UnmappedValueBehavior UnmappedValueBehavior
        {
            // return Source-side always; should always match Target-side
            get { return base.SourceSideUnmappedValueBehavior;  }

            set
            {
                base.SourceSideUnmappedValueBehavior = value;
                base.TargetSideUnmappedValueBehavior = value;
            }
        }

        public OneWayFieldMap(UnmappedValueBehavior unmappedValueBehavior)
            : base(unmappedValueBehavior, unmappedValueBehavior)
        {
            UnmappedValueBehavior = unmappedValueBehavior;
        }

        public OneWayFieldMap(UnmappedValueBehavior unmappedValueBehavior, StringComparison comparisonType)
            : base(unmappedValueBehavior, unmappedValueBehavior, comparisonType) 
        {
            UnmappedValueBehavior = unmappedValueBehavior;
        }

        public OneWayFieldMap(string defaultValue)
            : base(defaultValue, defaultValue)
        {
            DefaultValue = DefaultValue;
        }

        public OneWayFieldMap(string defaultValue, StringComparison comparisonType)
            : base(defaultValue, defaultValue, comparisonType)
        {
            DefaultValue = defaultValue;
        }

        public new void AddMap(string sourceValue, string targetValue)
        {
            AddMap(new FieldMapPair(sourceValue, targetValue));
        }

        public new void AddMap(FieldMapPair fieldMapPair)
        {
            ThrowExceptionIfInvalidDuplicateFieldMapValue(fieldMapPair);

            base.AddMap(fieldMapPair);
        }

        private void ThrowExceptionIfInvalidDuplicateFieldMapValues()
        {
            if (SyncDirection.HasValue)
            {
                if (SyncDirection.Value == Core.SyncDirection.SourceToTarget)
                {
                    foreach (var fieldMapPair in base.Maps)
                    {
                        if (base.Maps.Where(d => string.Equals(d.SourceValue, fieldMapPair.SourceValue, base.ComparisonType)).Count() > 1)
                            throw new Exception(string.Format("Source value '{0}' can not be duplicated within a one-way field map with a sync direction of source -> target.",
                                                          fieldMapPair.SourceValue == null ? "null" : fieldMapPair.SourceValue));
                    }
                }
                else if (SyncDirection.Value == Core.SyncDirection.TargetToSource)
                {
                    foreach (var fieldMapPair in base.Maps)
                    {
                        if (base.Maps.Where(d => string.Equals(d.TargetValue, fieldMapPair.TargetValue, base.ComparisonType)).Count() > 1)
                            throw new Exception(string.Format("Target value '{0}' can not be duplicated within a one-way field map with a sync direction of target -> source.",
                                                          fieldMapPair.TargetValue == null ? "null" : fieldMapPair.TargetValue));
                    }
                }
                else
                    throw new EnumValueNotImplementedException<SyncDirection>(SyncDirection.Value);
            }
        }

        private void ThrowExceptionIfInvalidDuplicateFieldMapValue(FieldMapPair fieldMapPair)
        {
            if (SyncDirection.HasValue)
            {
                if (SyncDirection.Value == Core.SyncDirection.SourceToTarget)
                {
                    if (SourceValueIsDuplicate(fieldMapPair))
                        throw new Exception(string.Format("Source value '{0}' can not be duplicated within a one-way field map.",
                                                          fieldMapPair.SourceValue == null ? "null" : fieldMapPair.SourceValue));
                }
                else if (SyncDirection.Value == Core.SyncDirection.TargetToSource)
                {
                    if (TargetValueIsDuplicate(fieldMapPair))
                        throw new Exception(string.Format("Target value '{0}' can not be duplicated within a one-way field map.",
                                                          fieldMapPair.TargetValue == null ? "null" : fieldMapPair.TargetValue));
                }
                else
                    throw new EnumValueNotImplementedException<SyncDirection>(SyncDirection.Value);
            }
        }
    }
}
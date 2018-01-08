using System;

namespace SyncObjX.Core
{
    public class TwoWayFieldMap : FieldMap
    {
        public TwoWayFieldMap(UnmappedValueBehavior sourceSideUnmappedValueBehavior, 
            UnmappedValueBehavior targetSideUnmappedValueBehavior)
            : base(sourceSideUnmappedValueBehavior, targetSideUnmappedValueBehavior) { }

        public TwoWayFieldMap(UnmappedValueBehavior sourceSideUnmappedValueBehavior, 
            UnmappedValueBehavior targetSideUnmappedValueBehavior, StringComparison comparisonType)
            : base(sourceSideUnmappedValueBehavior, targetSideUnmappedValueBehavior, comparisonType) { }

        /// <summary>
        /// Sets UnmappedBehavior to 'UseDefaultValue'.
        /// </summary>
        public TwoWayFieldMap(string sourceSideDefaultValue, string targetSideDefaultValue)
            : base(sourceSideDefaultValue, targetSideDefaultValue) { }

        /// <summary>
        /// Sets UnmappedBehavior to 'UseDefaultValue'.
        /// </summary>
        /// <param name="defaultValue"></param>
        /// <param name="comparisonType"></param>
        public TwoWayFieldMap(string sourceSideDefaultValue, string targetSideDefaultValue, 
            StringComparison comparisonType)
            : base(sourceSideDefaultValue, targetSideDefaultValue, comparisonType) { }

        public new string SourceSideDefaultValue
        {
            get { return base.SourceSideDefaultValue; }
            set { base.SourceSideDefaultValue = value; }
        }

        public new string TargetSideDefaultValue
        {
            get { return base.TargetSideDefaultValue; }
            set { base.TargetSideDefaultValue = value; }
        }

        public new UnmappedValueBehavior SourceSideUnmappedValueBehavior
        {
            get { return base.SourceSideUnmappedValueBehavior; }
            set { base.SourceSideUnmappedValueBehavior = value; }
        }

        public new UnmappedValueBehavior TargetSideUnmappedValueBehavior
        {
            get { return base.TargetSideUnmappedValueBehavior; }
            set { base.TargetSideUnmappedValueBehavior = value; }
        }
        
        public new void AddMap(string sourceValue, string targetValue)
        {
            AddMap(new FieldMapPair(sourceValue, targetValue));
        }

        public new void AddMap(FieldMapPair fieldMapPair)
        {
            if (SourceValueIsDuplicate(fieldMapPair))
                throw new Exception(string.Format("Source value '{0}' can not be duplicated within a two-way field map.",
                                                  fieldMapPair.SourceValue == null ? "null" : fieldMapPair.SourceValue));

            if (TargetValueIsDuplicate(fieldMapPair))
                throw new Exception(string.Format("Target value '{0}' can not be duplicated within a two-way field map.",
                                                  fieldMapPair.TargetValue == null ? "null" : fieldMapPair.TargetValue));

            base.AddMap(fieldMapPair);
        }
    }
}

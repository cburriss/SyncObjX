
namespace SyncObjX.Core
{
    public class FieldMapPair
    {
        public readonly string SourceValue;

        public readonly string TargetValue;

        public FieldMapPair(string sourceValue, string targetValue)
        {
            if (sourceValue == null)
                SourceValue = null;
            else
                SourceValue = sourceValue.Trim();

            if (targetValue == null)
                TargetValue = null;
            else
                TargetValue = targetValue.Trim();
        }
    }
}

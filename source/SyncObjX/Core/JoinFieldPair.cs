using System;

namespace SyncObjX.Core
{
    public class JoinFieldPair
    {
        public readonly string SourceJoinField;

        public readonly string SourceJoinFieldWithPrefix;

        public readonly string TargetJoinField;

        public readonly string TargetJoinFieldWithPrefix;

        public JoinFieldPair(string sourceJoinField, string targetJoinField)
        {
            if (String.IsNullOrWhiteSpace(sourceJoinField))
                throw new Exception("Source join field is missing or empty.");

            if (String.IsNullOrWhiteSpace(targetJoinField))
                throw new Exception("Target join field is missing or empty.");

            SourceJoinField = sourceJoinField.Trim();
            TargetJoinField = targetJoinField.Trim();

            SourceJoinFieldWithPrefix = DataTableHelper.SOURCE_PREFIX + SourceJoinField;
            TargetJoinFieldWithPrefix = DataTableHelper.TARGET_PREFIX + TargetJoinField;
        }
    }
}

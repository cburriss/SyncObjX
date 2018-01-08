using System;
using System.Collections.Generic;

namespace SyncObjX.Core
{
    public class JoinFieldCollection
    {
        List<JoinFieldPair> _joinFields = new List<JoinFieldPair>();

        int _count = 0;

        public int Count
        {
            get { return _count; }
        }

        public JoinFieldPair[] JoinFields
        {
            get { return _joinFields.ToArray(); }
        }

        public JoinFieldCollection(string sourceJoinField, string targetJoinField)
            : this(new JoinFieldPair(sourceJoinField, targetJoinField)) { }

        public JoinFieldCollection(JoinFieldPair joinFields)
        {
            _joinFields.Add(joinFields);

            _count++;
        }

        public JoinFieldCollection(string[] sourceJoinFields, string[] targetJoinFields)
        {
            if (sourceJoinFields == null || sourceJoinFields.Length == 0)
                throw new Exception("Source join fields are missing or empty.");

            if (targetJoinFields == null || targetJoinFields.Length == 0)
                throw new Exception("Target join fields are missing or empty.");

            if (sourceJoinFields.Length != targetJoinFields.Length)
                throw new Exception("Source and target must have the same number of join fields.");

            for (int i = 0; i < sourceJoinFields.Length; i++)
            {
                AddJoinField(new JoinFieldPair(sourceJoinFields[i], targetJoinFields[i]));
            }
        }

        public JoinFieldCollection(params JoinFieldPair[] joinFields)
        {
            if (joinFields == null || joinFields.Length == 0)
                throw new Exception("At least one join field is required.");

            for (int i = 0; i < joinFields.Length; i++)
            {
                AddJoinField(joinFields[i]);
            }
        }

        public void AddJoinField(string sourceJoinField, string targetJoinField)
        {
            if (FieldIsDuplicated(sourceJoinField, targetJoinField))
                throw new Exception("One or more join fields are duplicated.");

            _joinFields.Add(new JoinFieldPair(sourceJoinField, targetJoinField));

            _count++;
        }

        public void AddJoinField(JoinFieldPair joinFields)
        {
            if (FieldIsDuplicated(joinFields))
                throw new Exception("One or more join fields are duplicated.");

            _joinFields.Add(joinFields);

            _count++;
        }

        private bool FieldIsDuplicated(string sourceJoinField, string targetJoinField)
        {
            return FieldIsDuplicated(new JoinFieldPair(sourceJoinField, targetJoinField));
        }

        private bool FieldIsDuplicated(JoinFieldPair joinField)
        {
            foreach (JoinFieldPair jkp in _joinFields)
            {
                if (jkp.SourceJoinField.Trim().Equals(joinField.SourceJoinField, StringComparison.OrdinalIgnoreCase) ||
                    jkp.TargetJoinField.Trim().Equals(joinField.TargetJoinField, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}

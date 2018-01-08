using System;
using System.Collections.Generic;
using System.Data;

namespace SyncObjX.Core
{
    public class TransposeResult_DeleteRecord : TransposeResult
    {
        public readonly List<string> PrimaryKeyValues = new List<string>();

        public readonly DataRow AssociatedManySideDataRow;

        public TransposeResult_DeleteRecord(DataRow associatedManySideDataRow, string primaryKeyValue)
            : this(associatedManySideDataRow, new List<string>() { primaryKeyValue }) { }

        public TransposeResult_DeleteRecord(DataRow associatedManySideDataRow, IList<string> primaryKeyValues)
        {
            if (associatedManySideDataRow == null)
                throw new Exception("Associated many-side data row is required.");

            if (primaryKeyValues == null || primaryKeyValues.Count == 0)
                throw new Exception("One or more primary key values are required.");

            foreach (var primaryKeyValue in primaryKeyValues)
            {
                PrimaryKeyValues.Add(primaryKeyValue);
            }

            AssociatedManySideDataRow = associatedManySideDataRow;
        }
    }
}
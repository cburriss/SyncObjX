using System.Collections.Generic;

namespace SyncObjX.Data
{
    public class RecordKeyComboComparer : IEqualityComparer<RecordKeyCombo>
    {
        public bool Equals(RecordKeyCombo keyComboA, RecordKeyCombo keyComboB)
        {
            return (keyComboA.Key1 ?? "").Equals(keyComboB.Key1 ?? "") &&
                   (keyComboA.Key2 ?? "").Equals(keyComboB.Key2 ?? "") &&
                   (keyComboA.Key3 ?? "").Equals(keyComboB.Key3 ?? "") &&
                   (keyComboA.Key4 ?? "").Equals(keyComboB.Key4 ?? "") &&
                   (keyComboA.Key5 ?? "").Equals(keyComboB.Key5 ?? "") &&
                   (keyComboA.Key6 ?? "").Equals(keyComboB.Key6 ?? "") &&
                   (keyComboA.Key7 ?? "").Equals(keyComboB.Key7 ?? "");
        }

        // see http://stackoverflow.com/questions/5059994/custom-type-gethashcode
        public int GetHashCode(RecordKeyCombo keyCombo)
        {
            unchecked
            {
                int result = 37; // prime

                result *= 397; // also prime
                if (keyCombo.Key1 != null)
                    result += keyCombo.Key1.GetHashCode();

                result *= 397;
                if (keyCombo.Key2 != null)
                    result += keyCombo.Key2.GetHashCode();

                result *= 397;
                if (keyCombo.Key3 != null)
                    result += keyCombo.Key3.GetHashCode();

                result *= 397;
                if (keyCombo.Key4 != null)
                    result += keyCombo.Key4.GetHashCode();

                result *= 397;
                if (keyCombo.Key5 != null)
                    result += keyCombo.Key5.GetHashCode();

                result *= 397;
                if (keyCombo.Key6 != null)
                    result += keyCombo.Key6.GetHashCode();

                result *= 397;
                if (keyCombo.Key7 != null)
                    result += keyCombo.Key7.GetHashCode();

                return result;
            }
        }
    }
}

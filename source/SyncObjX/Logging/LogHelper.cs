using System.Collections.Generic;
using System.Text;

namespace SyncObjX.Logging
{
    public class LogHelper
    {
        public static string GetKeysAndValuesAsText(IList<string> keys, IList<string> values)
        {
            StringBuilder combined = new StringBuilder();

            if (values[0] == null)
                combined.AppendFormat("{0}=NULL", keys[0]);
            else
                combined.AppendFormat("{0}='{1}'", keys[0], values[0]);

            for (int i = 1; i < keys.Count; i++)
            {
                if (values[i] == null)
                    combined.AppendFormat(", {0}=NULL", keys[i]);
                else
                    combined.AppendFormat(", {0}='{1}'", keys[i], values[i]);
            }

            return combined.ToString();
        }
    }
}

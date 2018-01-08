using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SyncObjX.Util
{
    public class StringHelper
    {
        public static string AbridgeText(string text, int maxLength, string trailingText)
        {
            if (text.Length <= maxLength)
                return text;
            else
                return text.Remove(maxLength) + trailingText;
        }

        public static string GetInExpression(IEnumerable<string> collection)
        {
            if (collection.Count() == 0)
                return "('')";
            else
            {
                var inExpression = "('";

                foreach (string item in collection)
                    inExpression += item + "','";

                return inExpression = inExpression.Remove(inExpression.Length - 2, 2) + ")";
            }
        }

        public static string GetDelimitedString(IEnumerable<double> collection, string delimiter = ",")
        {
            return GetDelimitedString<double>(collection, delimiter);
        }

        public static string GetDelimitedString(IEnumerable<decimal> collection, string delimiter = ",")
        {
            return GetDelimitedString<decimal>(collection, delimiter);
        }

        public static string GetDelimitedString(IEnumerable<float> collection, string delimiter = ",")
        {
            return GetDelimitedString<float>(collection, delimiter);
        }

        public static string GetDelimitedString(IEnumerable<byte> collection, string delimiter = ",")
        {
            return GetDelimitedString<byte>(collection, delimiter);
        }

        public static string GetDelimitedString(IEnumerable<short> collection, string delimiter = ",")
        {
            return GetDelimitedString<short>(collection, delimiter);
        }

        public static string GetDelimitedString(IEnumerable<int> collection, string delimiter = ",")
        {
            return GetDelimitedString<int>(collection, delimiter);
        }

        public static string GetDelimitedString(IEnumerable<long> collection, string delimiter = ",")
        {
            return GetDelimitedString<long>(collection, delimiter);
        }

        public static string GetDelimitedString(IEnumerable<string> collection, string delimiter = ",")
        {
            return GetDelimitedString<string>(collection, delimiter);
        }

        public static string GetDelimitedString<TSource>(IEnumerable collection, string delimiter = ",")
        {
            return GetDelimitedString<TSource>(collection.Cast<TSource>(), delimiter);
        }

        public static string GetDelimitedString<TSource>(IEnumerable<TSource> collection, string delimiter = ",")
        {
            string delimitedString = "";

            if (collection != null && collection.Count() > 0)
            {
                foreach (var item in collection)
                    delimitedString += item + delimiter;

                delimitedString = delimitedString.Remove(delimitedString.Length - delimiter.Length, delimiter.Length);
            }

            return delimitedString;
        }

        public static bool IsNullOrWhiteSpace(string text)
        {
            if (text == null)
                return true;

            return text.Trim() == string.Empty;
        }

        public static string ToSafeString(object obj)
        {
            if (obj == null)
                return null;
            else
                return obj.ToString();
        }
    }
}

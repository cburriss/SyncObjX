using System;
using System.Data;

namespace SyncObjX.Core
{
    public static class FieldComparer
    {
        /// <summary>
        /// Compares two objects if their string values are equal.
        /// </summary>
        /// <param name="sourceValue"></param>
        /// <param name="targetValue"></param>
        /// <param name="ignoreCase"></param>
        /// <returns> Returns true if equal or both null.</returns>
        public static bool AreEqualStrings(object sourceValue, object targetValue, bool ignoreCase = false)
        {
            if (sourceValue == null && targetValue == null)
                return true;
            else if (sourceValue == null || targetValue == null)
                return false;

            if (ignoreCase)
                return sourceValue.ToString().Equals(targetValue.ToString(), StringComparison.OrdinalIgnoreCase);
            else
                return sourceValue.ToString().Equals(targetValue.ToString(), StringComparison.Ordinal);
        }

        /// <summary>
        /// Compares two objects if their boolean values are equal.
        /// </summary>
        /// <param name="sourceValue"></param>
        /// <param name="targetValue"></param>
        /// <returns>Returns true if equal or both null.</returns>
        public static bool AreEqualBooleans(object sourceValue, object targetValue)
        {
            if ((sourceValue == null || sourceValue == DBNull.Value) && (targetValue == null || targetValue == DBNull.Value))
                return true;
            else if ((sourceValue == null || sourceValue == DBNull.Value) || (targetValue == null || targetValue == DBNull.Value))
                return false;

            bool source;
            bool target;

            try
            {
                source = Convert.ToBoolean(sourceValue);
            }
            catch (Exception)
            {
                throw new Exception(string.Format("'{0}' failed conversion to System.Boolean.", sourceValue.ToString()));
            }

            try
            {
                target = Convert.ToBoolean(targetValue);
            }
            catch (Exception)
            {
                throw new Exception(string.Format("'{0}' failed conversion to System.Boolean.", targetValue.ToString()));
            }

            return source == target;
        }

        public static bool AreEqualDateTimes(object sourceValue, object targetValue, bool dateOnly = false)
        {
            if ((sourceValue == null || sourceValue == DBNull.Value) && (targetValue == null || targetValue == DBNull.Value))
                return true;
            else if ((sourceValue == null || sourceValue == DBNull.Value) || (targetValue == null || targetValue == DBNull.Value))
                return false;

            DateTime source;
            DateTime target;

            try
            {
                source = Convert.ToDateTime(sourceValue);
            }
            catch (Exception)
            {
                throw new Exception(string.Format("'{0}' failed conversion to System.DateTime.", sourceValue.ToString()));
            }

            try
            {
                target = Convert.ToDateTime(targetValue);
            }
            catch (Exception)
            {
                throw new Exception(string.Format("'{0}' failed conversion to System.DateTime.", targetValue.ToString()));
            }

            // remove milliseconds
            source = source.AddMilliseconds(-source.Millisecond);
            target = target.AddMilliseconds(-target.Millisecond);

            if (dateOnly)
                return source.Date == target.Date;
            else
                return source == target;
        }

        public static bool AreEqualIntegers(object sourceValue, object targetValue)
        {
            if ((sourceValue == null || sourceValue == DBNull.Value) && (targetValue == null || targetValue == DBNull.Value))
                return true;
            else if ((sourceValue == null || sourceValue == DBNull.Value) || (targetValue == null || targetValue == DBNull.Value))
                return false;

            int source;
            int target;

            try
            {
                source = Convert.ToInt32(sourceValue);
            }
            catch (Exception)
            {
                throw new Exception(string.Format("'{0}' failed conversion to System.Int32.", sourceValue.ToString()));
            }

            try
            {
                target = Convert.ToInt32(targetValue);
            }
            catch (Exception)
            {
                throw new Exception(string.Format("'{0}' failed conversion to System.Int32.", targetValue.ToString()));
            }

            return source == target;
        }

        public static bool AreEqualNumerics(object sourceValue, object targetValue)
        {
            if ((sourceValue == null || sourceValue == DBNull.Value) && (targetValue == null || targetValue == DBNull.Value))
                return true;
            else if ((sourceValue == null || sourceValue == DBNull.Value) || (targetValue == null || targetValue == DBNull.Value))
                return false;

            double source;
            double target;

            try
            {
                source = Convert.ToDouble(sourceValue);
            }
            catch (Exception)
            {
                throw new Exception(string.Format("'{0}' failed conversion to System.Double.", sourceValue.ToString()));
            }

            try
            {
                target = Convert.ToDouble(targetValue);
            }
            catch (Exception)
            {
                throw new Exception(string.Format("'{0}' failed conversion to System.Double.", targetValue.ToString()));
            }

            return source == target;
        }

        public static bool AreEqualOfUnknownType(object sourceValue, object targetValue)
        {
            if (sourceValue is bool && targetValue is bool)
                return AreEqualBooleans(sourceValue, targetValue);
            else if (sourceValue is DateTime && targetValue is DateTime)
                return AreEqualDateTimes(sourceValue, targetValue);
            else if ((sourceValue is byte || sourceValue is int) && (targetValue is byte || targetValue is int))
                return AreEqualIntegers(sourceValue, targetValue);
            else if ((sourceValue is decimal || sourceValue is double || sourceValue is float) && (targetValue is decimal || targetValue is double || targetValue is float))
                return AreEqualNumerics(sourceValue, targetValue);
            else
                return FieldComparer.AreEqualStrings(sourceValue, targetValue);
        }

        public static bool AreEqual(string fieldToUpdate, string sourceValue, string targetValue, FieldMap fieldMap, SyncSide syncSideToUpdate, ref string newValue)
        {
            string oldValue;

            if (syncSideToUpdate == SyncSide.Target)
            {
                newValue = fieldMap.GetMappedValueWithExceptionThrower(fieldToUpdate, syncSideToUpdate, sourceValue, targetValue);
                oldValue = targetValue;
            }
            else
            {
                newValue = fieldMap.GetMappedValueWithExceptionThrower(fieldToUpdate, syncSideToUpdate, targetValue, sourceValue);
                oldValue = sourceValue;
            }

            if (oldValue == newValue)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Uses a Custom comparison method to compare the values.
        /// TODO: Not unit tested because it is not used at present. We can revisit 
        /// in the future.
        /// </summary>
        /// <param name="row"></param>
        /// <param name="sourceValue"></param>
        /// <param name="targetValue"></param>
        /// <param name="customCompareMethod"></param>
        /// <returns></returns>
        public static bool AreEqual(DataRow row, object sourceValue, object targetValue, Func<DataRow, object, object, ComparisonResult> customCompareMethod)
        {
            var result = customCompareMethod(row, sourceValue, targetValue);

            if (result == ComparisonResult.AreDifferent)
                return false;
            else
                return true;
        }
    }
}

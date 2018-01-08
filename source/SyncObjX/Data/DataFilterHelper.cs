using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SyncObjX.Exceptions;

namespace SyncObjX.Data
{
    public class DataFilterHelper
    {
        public static string GetSqlClause(DataFilter filter)
        {
            if (filter.Value == null && !(filter.Operator == DataFilterOperator.Equals || filter.Operator == DataFilterOperator.NotEquals))
            {
                throw new Exception(string.Format("NULL filter value is invalid for {0} '{1}'.",
                    typeof(DataFilterOperator).Name, Enum.GetName(typeof(DataFilterOperator), filter.Operator)));
            }

            switch (filter.Operator)
            {
                case DataFilterOperator.Equals:
                    if (filter.Value == null)
                        return string.Format("{0} IS NULL", filter.FieldName);
                    else
                        if (filter.EncloseValueInSingleQuotes)
                            return string.Format("{0} = '{1}'", filter.FieldName, filter.Value.ToString());
                        else
                            return string.Format("{0} = {1}", filter.FieldName, filter.Value.ToString());

                case DataFilterOperator.NotEquals:
                    if (filter.Value == null)
                        return string.Format("{0} IS NOT NULL", filter.FieldName);
                    else
                        if (filter.EncloseValueInSingleQuotes)
                            return string.Format("{0} <> '{1}'", filter.FieldName, filter.Value.ToString());
                        else
                            return string.Format("{0} <> {1}", filter.FieldName, filter.Value.ToString());


                case DataFilterOperator.GreaterThanOrEqualTo:
                    if (filter.EncloseValueInSingleQuotes)
                        return string.Format("{0} >= '{1}'", filter.FieldName, filter.Value.ToString());
                    else
                        return string.Format("{0} >= {1}", filter.FieldName, filter.Value.ToString());


                case DataFilterOperator.GreaterThan:
                    if (filter.EncloseValueInSingleQuotes)
                        return string.Format("{0} > '{1}'", filter.FieldName, filter.Value.ToString());
                    else
                        return string.Format("{0} > {1}", filter.FieldName, filter.Value.ToString());

                case DataFilterOperator.LessThanOrEqualTo:
                    if (filter.EncloseValueInSingleQuotes)
                        return string.Format("{0} <= '{1}'", filter.FieldName, filter.Value.ToString());
                    else
                        return string.Format("{0} <= {1}", filter.FieldName, filter.Value.ToString());

                case DataFilterOperator.LessThan:
                    if (filter.EncloseValueInSingleQuotes)
                        return string.Format("{0} < '{1}'", filter.FieldName, filter.Value.ToString());
                    else
                        return string.Format("{0} < {1}", filter.FieldName, filter.Value.ToString());

                case DataFilterOperator.StartsWith:
                    return string.Format("{0} LIKE '{1}%'", filter.FieldName, filter.Value.ToString());

                case DataFilterOperator.Contains:
                    return string.Format("{0} LIKE '%{1}%'", filter.FieldName, filter.Value.ToString());

                case DataFilterOperator.EndsWith:
                    return string.Format("{0} LIKE '%{1}'", filter.FieldName, filter.Value.ToString());

                default:
                    throw new EnumValueNotImplementedException<DataFilterOperator>(filter.Operator);
            }
        }

        public static string GetSqlClause(IEnumerable<DataFilter> dataFilters)
        {
            if (dataFilters == null || dataFilters.Count() == 0)
                return "";

            var filters = dataFilters.ToArray();

            var text = new StringBuilder();

            if (filters.Length > 0)
            {
                for (int i = 0; i < filters.Length; i++)
                {
                    if (i > 0)
                        text.Append(" AND ");

                    text.Append(GetSqlClause(filters[i]));
                }
            }

            return text.ToString();
        }
    }
}

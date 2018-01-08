using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SyncObjX.Core;
using SyncObjX.Exceptions;

namespace SyncObjX.Management
{
    public class JobFilterHelper
    {
        public static string GetOperatorToken(JobFilterOperator @operator)
        {
            switch (@operator)
            {
                case JobFilterOperator.Equals:
                    return "=";

                case JobFilterOperator.NotEquals:
                    return "<>";

                case JobFilterOperator.GreaterThanOrEqualTo:
                    return ">=";

                case JobFilterOperator.GreaterThan:
                    return ">";

                case JobFilterOperator.LessThanOrEqualTo:
                    return "<=";

                case JobFilterOperator.LessThan:
                    return "<";

                case JobFilterOperator.StartsWith:
                    return "^=";

                case JobFilterOperator.Contains:
                    return "*=";

                case JobFilterOperator.EndsWith:
                    return "$=";

                default:
                    throw new EnumValueNotImplementedException<JobFilterOperator>(@operator);
            }
        }

        public static JobFilterOperator GetEnumFromToken(string token)
        {
            if (token == null)
                throw new Exception("Operator token can not be null.");

            token = token.Trim();

            switch (token)
            {
                case "=":
                    return JobFilterOperator.Equals;

                case "<>":
                    return JobFilterOperator.NotEquals;

                case ">=":
                    return JobFilterOperator.GreaterThanOrEqualTo;

                case ">":
                    return JobFilterOperator.GreaterThan;

                case "<=":
                    return JobFilterOperator.LessThanOrEqualTo;

                case "<":
                    return JobFilterOperator.LessThan;

                case "^=":
                    return JobFilterOperator.StartsWith;

                case "*=":
                    return JobFilterOperator.Contains;

                case "$=":
                    return JobFilterOperator.EndsWith;

                default:
                    throw new Exception(string.Format("No operator match was found for '{0}'.", token));
            }
        }

        public static string GetTextForDatabase(IEnumerable<JobFilter> jobFilters)
        {
            if (jobFilters == null || jobFilters.Count() == 0)
                return "";

            var sourceSideFilters = jobFilters.Where(d => d.SyncSide == SyncSide.Source).ToArray();
            var targetSideFilters = jobFilters.Where(d => d.SyncSide == SyncSide.Target).ToArray();

            var text = new StringBuilder();

            if (sourceSideFilters.Length > 0)
            {
                text.Append("Source: ");

                for (int i = 0; i < sourceSideFilters.Length; i++)
                {
                    if (i > 0)
                        text.Append(" AND ");

                    if (sourceSideFilters[i].Value == null)
                    {
                        text.AppendFormat("{0} {1} NULL",
                            sourceSideFilters[i].FieldName, GetOperatorToken(sourceSideFilters[i].Operator));
                    }
                    else
                    {
                        text.AppendFormat("{0} {1} '{2}'",
                            sourceSideFilters[i].FieldName, GetOperatorToken(sourceSideFilters[i].Operator), sourceSideFilters[i].Value.ToString());
                    }
                }
            }

            if (targetSideFilters.Length > 0)
            {
                if (text.Length > 0)
                    text.Append(" | ");

                text.Append("Target: ");

                for (int i = 0; i < targetSideFilters.Length; i++)
                {
                    if (i > 0)
                        text.Append(" AND ");

                    if (targetSideFilters[i].Value == null)
                    {
                        text.AppendFormat("{0} {1} NULL",
                            targetSideFilters[i].FieldName, GetOperatorToken(targetSideFilters[i].Operator));
                    }
                    else
                    {
                        text.AppendFormat("{0} {1} '{2}'",
                            targetSideFilters[i].FieldName, GetOperatorToken(targetSideFilters[i].Operator), targetSideFilters[i].Value.ToString());
                    }
                }
            }

            return text.ToString();
        }

        public static IEnumerable<JobFilter> ParseFromDatabaseText(string text)
        {
            if (String.IsNullOrWhiteSpace(text))
                return new List<JobFilter>();

            var jobFilters = new List<JobFilter>();

            var targetIdx = text.IndexOf("Target:");

            string sourceSide = "";
            string targetSide = "";

            if (targetIdx != -1)
            {
                targetSide = text.Substring(targetIdx).Trim();
                text = text.Substring(0, targetIdx).Trim();
            }

            if (targetIdx > 0)
                sourceSide = text.Substring(0, targetIdx - 1).Trim();
            else
                sourceSide = text.Substring(0).Trim();

            jobFilters.AddRange(ParseJobFiltersFromDatabaseTextFragment(SyncSide.Source, sourceSide));

            jobFilters.AddRange(ParseJobFiltersFromDatabaseTextFragment(SyncSide.Target, targetSide));

            return jobFilters;
        }

        private static List<JobFilter> ParseJobFiltersFromDatabaseTextFragment(SyncSide syncSide, string fragment)
        {
            if (String.IsNullOrWhiteSpace(fragment))
                return new List<JobFilter>();

            var jobFilters = new List<JobFilter>();

            if (fragment.LastIndexOf("|") == fragment.Length - 1)
                fragment = fragment.Remove(fragment.Length - 1);

            fragment = fragment.Replace("Source:", "").Replace("Target:", "");

            while (fragment != string.Empty)
            {
                fragment = fragment.Trim();

                if (fragment.IndexOf("AND") == 0)
                    fragment = fragment.Substring(4).Trim();

                var nextSpaceIdx = fragment.IndexOf(" ");

                var fieldName = fragment.Substring(0, nextSpaceIdx).TrimStart();

                fragment = fragment.Substring(nextSpaceIdx).TrimStart();

                nextSpaceIdx = fragment.IndexOf(" ");

                var operatorToken = fragment.Substring(0, nextSpaceIdx);

                var @operator = GetEnumFromToken(operatorToken);

                nextSpaceIdx = fragment.IndexOf(" ");

                fragment = fragment.Substring(nextSpaceIdx).Trim();

                var leftSideQuoteIdx = fragment.IndexOf("'");
                var rightSideQuoteIdx = fragment.IndexOf("'", leftSideQuoteIdx + 1);
                var nullValueIdx = fragment.IndexOf("NULL");

                nextSpaceIdx = (nullValueIdx != -1 && nullValueIdx < rightSideQuoteIdx) ? nullValueIdx + 4 : rightSideQuoteIdx + 1;

                if (nextSpaceIdx == 0)
                    nextSpaceIdx = fragment.Length;

                var value = fragment.Substring(0, nextSpaceIdx).Trim();

                if (value == "NULL")
                    jobFilters.Add(new JobFilter(syncSide, fieldName, @operator, null));
                else
                    jobFilters.Add(new JobFilter(syncSide, fieldName, @operator, value.Substring(1, value.Length - 2)));

                fragment = fragment.Substring(nextSpaceIdx).Trim();
            }

            return jobFilters;
        }

        public static string GetSqlClause(JobFilter jobFilter)
        {
            if (jobFilter.Value == null && !(jobFilter.Operator == JobFilterOperator.Equals || jobFilter.Operator == JobFilterOperator.NotEquals))
            {
                throw new Exception(string.Format("NULL filter value is invalid for {0} '{1}'.",
                    typeof(JobFilterOperator).Name, Enum.GetName(typeof(JobFilterOperator), jobFilter.Operator)));
            }

            switch (jobFilter.Operator)
            {
                case JobFilterOperator.Equals:
                    if (jobFilter.Value == null)
                        return string.Format("{0} IS NULL", jobFilter.FieldName);
                    else
                        if (jobFilter.EncloseValueInSingleQuotes)
                            return string.Format("{0} = '{1}'", jobFilter.FieldName, jobFilter.Value.ToString());
                        else
                            return string.Format("{0} = {1}", jobFilter.FieldName, jobFilter.Value.ToString());

                case JobFilterOperator.NotEquals:
                    if (jobFilter.Value == null)
                        return string.Format("{0} IS NOT NULL", jobFilter.FieldName);
                    else
                        if (jobFilter.EncloseValueInSingleQuotes)
                            return string.Format("{0} <> '{1}'", jobFilter.FieldName, jobFilter.Value.ToString());
                        else
                            return string.Format("{0} <> {1}", jobFilter.FieldName, jobFilter.Value.ToString());


                case JobFilterOperator.GreaterThanOrEqualTo:
                    if (jobFilter.EncloseValueInSingleQuotes)
                        return string.Format("{0} >= '{1}'", jobFilter.FieldName, jobFilter.Value.ToString());
                    else
                        return string.Format("{0} >= {1}", jobFilter.FieldName, jobFilter.Value.ToString());


                case JobFilterOperator.GreaterThan:
                    if (jobFilter.EncloseValueInSingleQuotes)
                        return string.Format("{0} > '{1}'", jobFilter.FieldName, jobFilter.Value.ToString());
                    else
                        return string.Format("{0} > {1}", jobFilter.FieldName, jobFilter.Value.ToString());

                case JobFilterOperator.LessThanOrEqualTo:
                    if (jobFilter.EncloseValueInSingleQuotes)
                        return string.Format("{0} <= '{1}'", jobFilter.FieldName, jobFilter.Value.ToString());
                    else
                        return string.Format("{0} <= {1}", jobFilter.FieldName, jobFilter.Value.ToString());

                case JobFilterOperator.LessThan:
                    if (jobFilter.EncloseValueInSingleQuotes)
                        return string.Format("{0} < '{1}'", jobFilter.FieldName, jobFilter.Value.ToString());
                    else
                        return string.Format("{0} < {1}", jobFilter.FieldName, jobFilter.Value.ToString());

                case JobFilterOperator.StartsWith:
                    return string.Format("{0} LIKE '{1}%'", jobFilter.FieldName, jobFilter.Value.ToString());

                case JobFilterOperator.Contains:
                    return string.Format("{0} LIKE '%{1}%'", jobFilter.FieldName, jobFilter.Value.ToString());

                case JobFilterOperator.EndsWith:
                    return string.Format("{0} LIKE '%{1}'", jobFilter.FieldName, jobFilter.Value.ToString());

                default:
                    throw new EnumValueNotImplementedException<JobFilterOperator>(jobFilter.Operator);
            }
        }

        public static string GetSqlClause(IEnumerable<JobFilter> jobFilters)
        {
            if (jobFilters == null || jobFilters.Count() == 0)
                return "";

            var filters = jobFilters.ToArray();

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
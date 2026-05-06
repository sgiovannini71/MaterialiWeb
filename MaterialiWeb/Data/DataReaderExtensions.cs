using System;
using System.Data;

namespace MaterialiGestioneWeb.Data
{
    public static class DataReaderExtensions
    {
        public static string GetStringOrEmpty(this IDataRecord record, string columnName)
        {
            var value = record[columnName];
            return value == DBNull.Value ? string.Empty : Convert.ToString(value);
        }

        public static int GetInt32Value(this IDataRecord record, string columnName)
        {
            return Convert.ToInt32(record[columnName]);
        }

        public static int? GetNullableInt32(this IDataRecord record, string columnName)
        {
            var value = record[columnName];
            return value == DBNull.Value ? (int?)null : Convert.ToInt32(value);
        }

        public static decimal? GetNullableDecimal(this IDataRecord record, string columnName)
        {
            var value = record[columnName];
            return value == DBNull.Value ? (decimal?)null : Convert.ToDecimal(value);
        }

        public static bool GetBooleanValue(this IDataRecord record, string columnName)
        {
            return Convert.ToBoolean(record[columnName]);
        }

        public static bool? GetNullableBoolean(this IDataRecord record, string columnName)
        {
            var value = record[columnName];
            return value == DBNull.Value ? (bool?)null : Convert.ToBoolean(value);
        }
    }
}

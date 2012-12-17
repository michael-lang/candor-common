using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Candor.Data
{
    public static class DataRecordExtensions
    {
        /// <summary>
        /// Gets the index of a field of a specific name in a record.
        /// </summary>
        /// <param name="record">The source record.</param>
        /// <param name="name">The field name to find.</param>
        /// <returns>The index location, or -1 if not found.</returns>
        public static int FieldIndex(this IDataRecord record, string name)
        {
            for (int i = 0; i < record.FieldCount; i++)
            {
                if (record.GetName(i) == name)
                    return i;
            }
            return -1;
        }
        /// <summary>
        /// Gets a field value from a record.
        /// </summary>
        /// <param name="record">The source record.</param>
        /// <param name="name">The field name to find.</param>
        /// <returns>Returns null if the field does not exist or the value is 
        /// null(DbNull).  Otherwise the field value is returned.  </returns>
        public static object GetValue(this IDataRecord record, string name)
        {
            return GetValue(record, name, false);
        }
        /// <summary>
        /// Gets a field value from a record.
        /// </summary>
        /// <param name="record">The source record.</param>
        /// <param name="name">The field name to find.</param>
        /// <param name="ignoreErrors">Specifies if a missing field should return the default value.</param>
        /// <returns>Returns null if the field does not exist or the value is 
        /// null(DbNull).  Otherwise the field value is returned.  </returns>
        public static object GetValue(this IDataRecord record, string name, bool ignoreErrors)
        {
            int index = FieldIndex(record, name);
            if (index < 0)
            {
                if (ignoreErrors)
                    return null;
                else
                    throw new ArgumentOutOfRangeException("name", name, "Field name does not exist.");
            }
            if (record[index] == DBNull.Value)
                return null;

            //value exists, and is not DBNull
            return record[index];
        }
        /// <summary>
        /// Gets a value from a database field value or output parameter.
        /// </summary>
        /// <param name="fieldValue">The value to convert.</param>
        /// <returns>The converted value.</returns>
        public static object GetValue(object fieldValue)
        {
            return GetValue(fieldValue, null);
        }
        /// <summary>
        /// Gets a value from a database field value or output parameter.
        /// </summary>
        /// <param name="fieldValue">The value to convert.</param>
        /// <param name="defaultValue">The default value if the original is DbNull.</param>
        /// <returns>The converted value.</returns>
        public static object GetValue(object fieldValue, object defaultValue)
        {
            if (fieldValue == DBNull.Value)
                return defaultValue;
            else
                return fieldValue;
        }
        /// <summary>
        /// Gets a field value from a record.
        /// </summary>
        /// <param name="record">The source record.</param>
        /// <param name="name">The field name to find.</param>
        /// <returns></returns>
        public static string GetString(this IDataRecord record, string name)
        {
            return GetString(record, name, "", false);
        }
        /// <summary>
        /// Gets a field value from a record.
        /// </summary>
        /// <param name="record">The source record.</param>
        /// <param name="name">The field name to find.</param>
        /// <param name="defaultValue">The default in case the value is not assigned.</param>
        /// <returns></returns>
        public static string GetString(this IDataRecord record, string name, string defaultValue)
        {
            return GetString(record, name, defaultValue, false);
        }
        /// <summary>
        /// Gets a field value from a record.
        /// </summary>
        /// <param name="record">The source record.</param>
        /// <param name="name">The field name to find.</param>
        /// <param name="defaultValue">The default in case the value is not assigned.</param>
        /// <param name="ignoreErrors">Specifies if a missing field should return the default value.</param>
        /// <returns></returns>
        public static string GetString(this IDataRecord record, string name, string defaultValue, bool ignoreErrors)
        {
            int index = FieldIndex(record, name);
            if (index < 0)
            {
                if (ignoreErrors)
                    return defaultValue;
                else
                    throw new ArgumentOutOfRangeException("name", name, "Field name does not exist.");
            }
            if (record[index] == DBNull.Value)
                return defaultValue;

            //value exists, and is not DBNull
            return Convert.ToString(record[index]);
        }
        /// <summary>
        /// Gets a value from a database field value or output parameter.
        /// </summary>
        /// <param name="fieldValue">The value to convert.</param>
        /// <returns>The converted value.</returns>
        public static string GetString(object fieldValue)
        {
            return GetString(fieldValue, null);
        }
        /// <summary>
        /// Gets a value from a database field value or output parameter.
        /// </summary>
        /// <param name="fieldValue">The value to convert.</param>
        /// <param name="defaultValue">The default value if the original is DbNull.</param>
        /// <returns>The converted value.</returns>
        public static string GetString(object fieldValue, string defaultValue)
        {
            if (fieldValue == DBNull.Value || fieldValue == null)
                return defaultValue;
            else
                return Convert.ToString(fieldValue);
        }
        /// <summary>
        /// Gets a field value from a record.
        /// </summary>
        /// <param name="record">The source record.</param>
        /// <param name="name">The field name to find.</param>
        /// <returns></returns>
        public static short GetInt16(this IDataRecord record, string name)
        {
            return GetInt16(record, name, -1, false);
        }
        /// <summary>
        /// Gets a field value from a record.
        /// </summary>
        /// <param name="record">The source record.</param>
        /// <param name="name">The field name to find.</param>
        /// <param name="defaultValue">The default in case the value is not assigned.</param>
        /// <returns></returns>
        public static short GetInt16(this IDataRecord record, string name, short defaultValue)
        {
            return GetInt16(record, name, defaultValue, false);
        }
        /// <summary>
        /// Gets a field value from a record.
        /// </summary>
        /// <param name="record">The source record.</param>
        /// <param name="name">The field name to find.</param>
        /// <param name="defaultValue">The default in case the value is not assigned.</param>
        /// <param name="ignoreErrors">Specifies if a missing field should return the default value.</param>
        /// <returns></returns>
        public static short GetInt16(this IDataRecord record, string name, short defaultValue, bool ignoreErrors)
        {
            int index = FieldIndex(record, name);
            if (index < 0)
            {
                if (ignoreErrors)
                    return defaultValue;
                else
                    throw new ArgumentOutOfRangeException("name", name, "Field name does not exist.");
            }
            return GetInt16(record[index], defaultValue);
        }
        /// <summary>
        /// Gets a value from a database field value or output parameter.
        /// </summary>
        /// <param name="fieldValue">The value to convert.</param>
        /// <returns>The converted value.</returns>
        public static short GetInt16(object fieldValue)
        {
            return GetInt16(fieldValue, -1);
        }
        /// <summary>
        /// Gets a value from a database field value or output parameter.
        /// </summary>
        /// <param name="fieldValue">The value to convert.</param>
        /// <param name="defaultValue">The default value if the original is DbNull.</param>
        /// <returns>The converted value.</returns>
        public static short GetInt16(object fieldValue, short defaultValue)
        {
            if (fieldValue == DBNull.Value || fieldValue == null)
                return defaultValue;

            Type valType = fieldValue.GetType();
            if (valType == typeof(short))
                return (short)fieldValue;
            else if (valType == typeof(decimal))
                return (short)Math.Floor((decimal)fieldValue);
            else if (valType == typeof(double))
                return (short)Math.Floor((double)fieldValue);
            else
                return Convert.ToInt16(fieldValue);
        }
        /// <summary>
        /// Gets a field value from a record.
        /// </summary>
        /// <param name="record">The source record.</param>
        /// <param name="name">The field name to find.</param>
        /// <returns></returns>
        public static int GetInt32(this IDataRecord record, string name)
        {
            return GetInt32(record, name, -1, false);
        }
        /// <summary>
        /// Gets a field value from a record.
        /// </summary>
        /// <param name="record">The source record.</param>
        /// <param name="name">The field name to find.</param>
        /// <param name="defaultValue">The default in case the value is not assigned.</param>
        /// <returns></returns>
        public static int GetInt32(this IDataRecord record, string name, int defaultValue)
        {
            return GetInt32(record, name, defaultValue, false);
        }
        /// <summary>
        /// Gets a field value from a record.
        /// </summary>
        /// <param name="record">The source record.</param>
        /// <param name="name">The field name to find.</param>
        /// <param name="defaultValue">The default in case the value is not assigned.</param>
        /// <param name="ignoreErrors">Specifies if a missing field should return the default value.</param>
        /// <returns></returns>
        public static int GetInt32(this IDataRecord record, string name, int defaultValue, bool ignoreErrors)
        {
            int index = FieldIndex(record, name);
            if (index < 0)
            {
                if (ignoreErrors)
                    return defaultValue;
                else
                    throw new ArgumentOutOfRangeException("name", name, "Field name does not exist.");
            }
            return GetInt32(record[index], defaultValue);
        }
        /// <summary>
        /// Gets a value from a database field value or output parameter.
        /// </summary>
        /// <param name="fieldValue">The value to convert.</param>
        /// <returns>The converted value.</returns>
        public static int GetInt32(object fieldValue)
        {
            return GetInt32(fieldValue, -1);
        }
        /// <summary>
        /// Gets a value from a database field value or output parameter.
        /// </summary>
        /// <param name="fieldValue">The value to convert.</param>
        /// <param name="defaultValue">The default value if the original is DbNull.</param>
        /// <returns>The converted value.</returns>
        public static int GetInt32(object fieldValue, int defaultValue)
        {
            if (fieldValue == DBNull.Value || fieldValue == null)
                return defaultValue;

            Type valType = fieldValue.GetType();
            if (valType == typeof(int))
                return (int)fieldValue;
            if (valType == typeof(short))
                return (int)(short)fieldValue;
            else if (valType == typeof(decimal))
                return (int)Math.Floor((decimal)fieldValue);
            else if (valType == typeof(double))
                return (int)Math.Floor((double)fieldValue);
            else
                return Convert.ToInt32(fieldValue);
        }
        /// <summary>
        /// Gets a field value from a record.
        /// </summary>
        /// <param name="record">The source record.</param>
        /// <param name="name">The field name to find.</param>
        /// <returns></returns>
        public static long GetInt64(this IDataRecord record, string name)
        {
            return GetInt64(record, name, -1, false);
        }
        /// <summary>
        /// Gets a field value from a record.
        /// </summary>
        /// <param name="record">The source record.</param>
        /// <param name="name">The field name to find.</param>
        /// <param name="defaultValue">The default in case the value is not assigned.</param>
        /// <returns></returns>
        public static long GetInt64(this IDataRecord record, string name, long defaultValue)
        {
            return GetInt64(record, name, defaultValue, false);
        }
        /// <summary>
        /// Gets a field value from a record.
        /// </summary>
        /// <param name="record">The source record.</param>
        /// <param name="name">The field name to find.</param>
        /// <param name="defaultValue">The default in case the value is not assigned.</param>
        /// <param name="ignoreErrors">Specifies if a missing field should return the default value.</param>
        /// <returns></returns>
        public static long GetInt64(this IDataRecord record, string name, long defaultValue, bool ignoreErrors)
        {
            int index = FieldIndex(record, name);
            if (index < 0)
            {
                if (ignoreErrors)
                    return defaultValue;
                else
                    throw new ArgumentOutOfRangeException("name", name, "Field name does not exist.");
            }
            return GetInt64(record[index], defaultValue);
        }
        /// <summary>
        /// Gets a value from a database field value or output parameter.
        /// </summary>
        /// <param name="fieldValue">The value to convert.</param>
        /// <returns>The converted value.</returns>
        public static long GetInt64(object fieldValue)
        {
            return GetInt64(fieldValue, -1);
        }
        /// <summary>
        /// Gets a value from a database field value or output parameter.
        /// </summary>
        /// <param name="fieldValue">The value to convert.</param>
        /// <param name="defaultValue">The default value if the original is DbNull.</param>
        /// <returns>The converted value.</returns>
        public static long GetInt64(object fieldValue, long defaultValue)
        {
            if (fieldValue == DBNull.Value || fieldValue == null)
                return defaultValue;

            Type valType = fieldValue.GetType();
            if (valType == typeof(long))
                return (long)fieldValue;
            else if (valType == typeof(int))
                return (long)(int)fieldValue;
            else if (valType == typeof(short))
                return (long)(short)fieldValue;
            else if (valType == typeof(decimal))
                return (long)Math.Floor((decimal)fieldValue);
            else if (valType == typeof(double))
                return (long)Math.Floor((double)fieldValue);
            else
                return Convert.ToInt64(fieldValue);
        }
        /// <summary>
        /// Gets a field value from a record.
        /// </summary>
        /// <param name="record">The source record.</param>
        /// <param name="name">The field name to find.</param>
        /// <returns></returns>
        public static double GetDouble(this IDataRecord record, string name)
        {
            return GetDouble(record, name, double.NaN, false);
        }
        /// <summary>
        /// Gets a field value from a record.
        /// </summary>
        /// <param name="record">The source record.</param>
        /// <param name="name">The field name to find.</param>
        /// <param name="defaultValue">The default in case the value is not assigned.</param>
        /// <returns></returns>
        public static double GetDouble(this IDataRecord record, string name, double defaultValue)
        {
            return GetDouble(record, name, defaultValue, false);
        }
        /// <summary>
        /// Gets a field value from a record.
        /// </summary>
        /// <param name="record">The source record.</param>
        /// <param name="name">The field name to find.</param>
        /// <param name="defaultValue">The default in case the value is not assigned.</param>
        /// <param name="ignoreErrors">Specifies if a missing field should return the default value.</param>
        /// <returns></returns>
        public static double GetDouble(this IDataRecord record, string name, double defaultValue, bool ignoreErrors)
        {
            int index = FieldIndex(record, name);
            if (index < 0)
            {
                if (ignoreErrors)
                    return defaultValue;
                else
                    throw new ArgumentOutOfRangeException("name", name, "Field name does not exist.");
            }
            return GetDouble(record[index], defaultValue);
        }
        /// <summary>
        /// Gets a value from a database field value or output parameter.
        /// </summary>
        /// <param name="fieldValue">The value to convert.</param>
        /// <returns>The converted value.</returns>
        public static double GetDouble(object fieldValue)
        {
            return GetDouble(fieldValue, double.NaN);
        }
        /// <summary>
        /// Gets a value from a database field value or output parameter.
        /// </summary>
        /// <param name="fieldValue">The value to convert.</param>
        /// <param name="defaultValue">The default value if the original is DbNull.</param>
        /// <returns>The converted value.</returns>
        public static double GetDouble(object fieldValue, double defaultValue)
        {
            if (fieldValue == DBNull.Value || fieldValue == null)
                return defaultValue;

            Type valType = fieldValue.GetType();
            if (valType == typeof(decimal))
                return (double)(decimal)fieldValue;
            else if (valType == typeof(double))
                return (double)fieldValue;
            else
                return Convert.ToDouble(fieldValue);
        }
        /// <summary>
        /// Gets a field value from a record.
        /// </summary>
        /// <param name="record">The source record.</param>
        /// <param name="name">The field name to find.</param>
        /// <returns></returns>
        public static decimal GetDecimal(this IDataRecord record, string name)
        {
            return GetDecimal(record, name, Decimal.Zero, false);
        }
        /// <summary>
        /// Gets a field value from a record.
        /// </summary>
        /// <param name="record">The source record.</param>
        /// <param name="name">The field name to find.</param>
        /// <param name="defaultValue">The default in case the value is not assigned.</param>
        /// <returns></returns>
        public static decimal GetDecimal(this IDataRecord record, string name, decimal defaultValue)
        {
            return GetDecimal(record, name, defaultValue, false);
        }
        /// <summary>
        /// Gets a field value from a record.
        /// </summary>
        /// <param name="record">The source record.</param>
        /// <param name="name">The field name to find.</param>
        /// <param name="defaultValue">The default in case the value is not assigned.</param>
        /// <param name="ignoreErrors">Specifies if a missing field should return the default value.</param>
        /// <returns></returns>
        public static decimal GetDecimal(this IDataRecord record, string name, decimal defaultValue, bool ignoreErrors)
        {
            int index = FieldIndex(record, name);
            if (index < 0)
            {
                if (ignoreErrors)
                    return defaultValue;
                else
                    throw new ArgumentOutOfRangeException("name", name, "Field name does not exist.");
            }
            return GetDecimal(record[index], defaultValue);
        }
        /// <summary>
        /// Gets a value from a database field value or output parameter.
        /// </summary>
        /// <param name="fieldValue">The value to convert.</param>
        /// <returns>The converted value.</returns>
        public static decimal GetDecimal(object fieldValue)
        {
            return GetDecimal(fieldValue, decimal.Zero);
        }
        /// <summary>
        /// Gets a value from a database field value or output parameter.
        /// </summary>
        /// <param name="fieldValue">The value to convert.</param>
        /// <param name="defaultValue">The default value if the original is DbNull.</param>
        /// <returns>The converted value.</returns>
        public static decimal GetDecimal(object fieldValue, decimal defaultValue)
        {
            if (fieldValue == DBNull.Value || fieldValue == null)
                return defaultValue;

            Type valType = fieldValue.GetType();
            if (valType == typeof(double))
                return (decimal)(double)fieldValue;
            else if (valType == typeof(decimal))
                return (decimal)fieldValue;
            else
                return Convert.ToDecimal(fieldValue);
        }
        /// <summary>
        /// Gets a field value from a record.
        /// </summary>
        /// <param name="record">The source record.</param>
        /// <param name="name">The field name to find.</param>
        /// <returns></returns>
        public static DateTime GetDateTime(this IDataRecord record, string name)
        {
            return GetDateTime(record, name, DateTime.MinValue, false);
        }
        /// <summary>
        /// Gets a field value from a record.
        /// </summary>
        /// <param name="record">The source record.</param>
        /// <param name="name">The field name to find.</param>
        /// <param name="defaultValue">The default in case the value is not assigned.</param>
        /// <returns></returns>
        public static DateTime GetDateTime(this IDataRecord record, string name, DateTime defaultValue)
        {
            return GetDateTime(record, name, defaultValue, false);
        }
        /// <summary>
        /// Gets a field value from a record.
        /// </summary>
        /// <param name="record">The source record.</param>
        /// <param name="name">The field name to find.</param>
        /// <param name="defaultValue">The default in case the value is not assigned.</param>
        /// <param name="ignoreErrors">Specifies if a missing field should return the default value.</param>
        /// <returns></returns>
        public static DateTime GetDateTime(this IDataRecord record, string name, DateTime defaultValue, bool ignoreErrors)
        {
            int index = FieldIndex(record, name);
            if (index < 0)
            {
                if (ignoreErrors)
                    return defaultValue;
                else
                    throw new ArgumentOutOfRangeException("name", name, "Field name does not exist.");
            }
            return GetDateTime(record[index], defaultValue);
        }
        /// <summary>
        /// Gets a value from a database field value or output parameter.
        /// </summary>
        /// <param name="fieldValue">The value to convert.</param>
        /// <returns>The converted value.</returns>
        public static DateTime GetDateTime(object fieldValue)
        {
            return GetDateTime(fieldValue, DateTime.MinValue);
        }
        /// <summary>
        /// Gets a value from a database field value or output parameter.
        /// </summary>
        /// <param name="fieldValue">The value to convert.</param>
        /// <param name="defaultValue">The default value if the original is DbNull.</param>
        /// <returns>The converted value.</returns>
        public static DateTime GetDateTime(object fieldValue, DateTime defaultValue)
        {
            if (fieldValue == DBNull.Value || fieldValue == null)
                return defaultValue;
            else
                return Convert.ToDateTime(fieldValue);
        }
        /// <summary>
        /// Gets a field value from a record.
        /// </summary>
        /// <param name="record">The source record.</param>
        /// <param name="name">The field name to find.</param>
        /// <returns></returns>
        public static DateTime GetUTCDateTime(this IDataRecord record, string name)
        {
            return GetUTCDateTime(record, name, DateTime.MinValue, false);
        }
        /// <summary>
        /// Gets a field value from a record.
        /// </summary>
        /// <param name="record">The source record.</param>
        /// <param name="name">The field name to find.</param>
        /// <param name="defaultValue">The default in case the value is not assigned.</param>
        /// <returns></returns>
        public static DateTime GetUTCDateTime(this IDataRecord record, string name, DateTime defaultValue)
        {
            return GetUTCDateTime(record, name, defaultValue, false);
        }
        /// <summary>
        /// Gets a field value from a record.
        /// </summary>
        /// <param name="record">The source record.</param>
        /// <param name="name">The field name to find.</param>
        /// <param name="defaultValue">The default in case the value is not assigned.</param>
        /// <param name="ignoreErrors">Specifies if a missing field should return the default value.</param>
        /// <returns></returns>
        public static DateTime GetUTCDateTime(this IDataRecord record, string name, DateTime defaultValue, bool ignoreErrors)
        {
            int index = FieldIndex(record, name);
            if (index < 0)
            {
                if (ignoreErrors)
                    return defaultValue;
                else
                    throw new ArgumentOutOfRangeException("name", name, "Field name does not exist.");
            }
            return GetUTCDateTime(record[index], defaultValue);
        }
        /// <summary>
        /// Gets a value from a database field value or output parameter.
        /// </summary>
        /// <param name="fieldValue">The value to convert.</param>
        /// <returns>The converted value.</returns>
        public static DateTime GetUTCDateTime(object fieldValue)
        {
            return GetUTCDateTime(fieldValue, DateTime.MinValue);
        }
        /// <summary>
        /// Gets a value from a database field value or output parameter.
        /// </summary>
        /// <param name="fieldValue">The value to convert.</param>
        /// <param name="defaultValue">The default value if the original is DbNull.</param>
        /// <returns>The converted value.</returns>
        public static DateTime GetUTCDateTime(object fieldValue, DateTime defaultValue)
        {
            if (fieldValue == DBNull.Value || fieldValue == null)
                return defaultValue;
            else
            {
                DateTime utc = Convert.ToDateTime(fieldValue);
                if (utc == DateTime.MinValue || utc == DateTime.MaxValue)
                    return utc;
                return new DateTime(utc.Year, utc.Month, utc.Day, utc.Hour, utc.Minute, utc.Second, utc.Millisecond, DateTimeKind.Utc);
            }
        }
        /// <summary>
        /// Gets a field value from a record.
        /// </summary>
        /// <param name="record">The source record.</param>
        /// <param name="name">The field name to find.</param>
        /// <param name="defaultValue">The default in case the value is not assigned.</param>
        /// <returns></returns>
        public static bool GetBoolean(this IDataRecord record, string name, bool defaultValue)
        {
            return GetBoolean(record, name, defaultValue, false);
        }
        /// <summary>
        /// Gets a field value from a record.
        /// </summary>
        /// <param name="record">The source record.</param>
        /// <param name="name">The field name to find.</param>
        /// <param name="defaultValue">The default in case the value is not assigned.</param>
        /// <param name="ignoreErrors">Specifies if a missing field should return the default value.</param>
        /// <returns></returns>
        public static bool GetBoolean(this IDataRecord record, string name, bool defaultValue, bool ignoreErrors)
        {
            int index = FieldIndex(record, name);
            if (index < 0)
            {
                if (ignoreErrors)
                    return defaultValue;
                else
                    throw new ArgumentOutOfRangeException("name", name, "Field name does not exist.");
            }
            return GetBoolean(record[index], defaultValue);
        }
        /// <summary>
        /// Gets a value from a database field value or output parameter.
        /// </summary>
        /// <param name="fieldValue">The value to convert.</param>
        /// <param name="defaultValue">The default value if the original is DbNull.</param>
        /// <returns>The converted value.</returns>
        public static bool GetBoolean(object fieldValue, bool defaultValue)
        {
            if (fieldValue == DBNull.Value || fieldValue == null)
                return defaultValue;
            else
                return Convert.ToBoolean(fieldValue);
        }
        /// <summary>
        /// Gets a field value from a record.
        /// </summary>
        /// <param name="record">The source record.</param>
        /// <param name="name">The field name to find.</param>
        /// <returns></returns>
        public static Guid GetGuid(this IDataRecord record, string name)
        {
            return GetGuid(record, name, Guid.Empty, false);
        }
        /// <summary>
        /// Gets a field value from a record.
        /// </summary>
        /// <param name="record">The source record.</param>
        /// <param name="name">The field name to find.</param>
        /// <param name="defaultValue">The default in case the value is not assigned.</param>
        /// <returns></returns>
        public static Guid GetGuid(this IDataRecord record, string name, Guid defaultValue)
        {
            return GetGuid(record, name, defaultValue, false);
        }
        /// <summary>
        /// Gets a field value from a record.
        /// </summary>
        /// <param name="record">The source record.</param>
        /// <param name="name">The field name to find.</param>
        /// <param name="defaultValue">The default in case the value is not assigned.</param>
        /// <param name="ignoreErrors">Specifies if a missing field should return the default value.</param>
        /// <returns></returns>
        public static Guid GetGuid(this IDataRecord record, string name, Guid defaultValue, bool ignoreErrors)
        {
            int index = FieldIndex(record, name);
            if (index < 0)
            {
                if (ignoreErrors)
                    return defaultValue;
                else
                    throw new ArgumentOutOfRangeException("name", name, "Field name does not exist.");
            }
            return GetGuid(record[index], defaultValue);
        }
        /// <summary>
        /// Gets a value from a database field value or output parameter.
        /// </summary>
        /// <param name="fieldValue">The value to convert.</param>
        /// <returns>The converted value.</returns>
        public static Guid GetGuid(object fieldValue)
        {
            return GetGuid(fieldValue, Guid.Empty);
        }
        /// <summary>
        /// Gets a value from a database field value or output parameter.
        /// </summary>
        /// <param name="fieldValue">The value to convert.</param>
        /// <param name="defaultValue">The default value if the original is DbNull.</param>
        /// <returns>The converted value.</returns>
        public static Guid GetGuid(object fieldValue, Guid defaultValue)
        {
            if (fieldValue == DBNull.Value || fieldValue == null)
                return defaultValue;

            Type valType = fieldValue.GetType();
            if (valType == typeof(string))
                return new Guid(GetString(fieldValue, null));
            else if (valType == typeof(byte[]))
                return new Guid((byte[])fieldValue);
            else
                return (Guid)fieldValue;
        }
    }
}

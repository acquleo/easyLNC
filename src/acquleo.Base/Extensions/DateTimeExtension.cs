using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    /// <summary>
    /// DateTime extension
    /// </summary>
    public static class DateTimeExtension
    {
        /// <summary>
        /// Converte un DateTime nella in una stringa con formato ISO 8601
        /// </summary>
        /// <param name="dateTimeStr"></param>
        /// <returns></returns>
        public static DateTime? ConvertFromISO8601(this string dateTimeStr)
        {
            if(DateTime.TryParseExact(dateTimeStr,"o",System.Globalization.CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            {
                return date;
            }
            return null;
        }
        /// <summary>
        /// Converte un DateTime nella in una stringa con formato ISO 8601 con il kind specificato
        /// </summary>
        /// <param name="dateTimeStr"></param>
        /// <param name="kind"></param>
        /// <returns></returns>
        public static DateTime? ConvertFromISO8601(this string dateTimeStr, DateTimeKind kind)
        {
            if (DateTime.TryParseExact(dateTimeStr, "o", System.Globalization.CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            {
                return date.ConvertTo(kind);
            }
            return null;
        }
        /// <summary>
        /// Converte un DateTime nella in una stringa con formato ISO 8601
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static string ConvertToISO8601(this DateTime dateTime)
        {
            return dateTime.ToString("o", CultureInfo.InvariantCulture);
        }
        /// <summary>
        /// Converte un DateTime nella in una stringa con formato ISO 8601
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static string ConvertToISO8601(this DateTime? dateTime)
        {
            if (!dateTime.HasValue) return default(string);

            return dateTime.Value.ToString("o", CultureInfo.InvariantCulture);
        }
        /// <summary>
        /// Converte un DateTime nella in una stringa con formato ISO 8601 utilizzando il kind specificato
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="kind"></param>
        /// <returns></returns>
        public static string ConvertToISO8601(this DateTime? dateTime, DateTimeKind kind)
        {
            if (!dateTime.HasValue) return default(string);

            return dateTime.ConvertTo(kind).Value.ToString("o", CultureInfo.InvariantCulture);
        }
        /// <summary>
        /// Converte un DateTime nella in una stringa con formato ISO 8601 utilizzando il kind specificato
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="kind"></param>
        /// <returns></returns>
        public static string ConvertToISO8601(this DateTime dateTime, DateTimeKind kind)
        {
            return dateTime.ConvertTo(kind).ToString("o", CultureInfo.InvariantCulture);
        }


        /// <summary>
        /// Converte un DateTime nella in una stringa con formato custom
        /// </summary>
        /// <param name="dateTimeStr"></param>
        /// <param name="customFormat"></param>
        /// <returns></returns>
        public static DateTime? ConvertFromCustomFormat(this string dateTimeStr, string customFormat)
        {
            if (DateTime.TryParseExact(dateTimeStr, customFormat, System.Globalization.CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            {
                return date;
            }
            return null;
        }
        /// <summary>
        /// Converte un DateTime nella in una stringa con formato custom con il kind specificato
        /// </summary>
        /// <param name="dateTimeStr"></param>
        /// <param name="customFormat"></param>
        /// <param name="kind"></param>
        /// <returns></returns>
        public static DateTime? ConvertFromCustomFormat(this string dateTimeStr, string customFormat, DateTimeKind kind)
        {
            if (DateTime.TryParseExact(dateTimeStr, customFormat, System.Globalization.CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            {
                return date.ConvertTo(kind);
            }
            return null;
        }
        /// <summary>
        /// Converte un DateTime nella in una stringa con formato custom
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="customFormat"></param>
        /// <returns></returns>
        public static string ConvertToCustomFormat(this DateTime dateTime, string customFormat)
        {
            return dateTime.ToString(customFormat, CultureInfo.InvariantCulture);
        }
        /// <summary>
        /// Converte un DateTime nella in una stringa con formato custom
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="customFormat"></param>
        /// <returns></returns>
        public static string ConvertToCustomFormat(this DateTime? dateTime, string customFormat)
        {
            if (!dateTime.HasValue) return default(string);

            return dateTime.Value.ToString(customFormat, CultureInfo.InvariantCulture);
        }
        /// <summary>
        /// Converte un DateTime nella in una stringa con formato custom utilizzando il kind specificato
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="kind"></param>
        /// <param name="customFormat"></param>
        /// <returns></returns>
        public static string ConvertToCustomFormat(this DateTime? dateTime, DateTimeKind kind, string customFormat)
        {
            if (!dateTime.HasValue) return default(string);

            return dateTime.ConvertTo(kind).Value.ToString(customFormat, CultureInfo.InvariantCulture);
        }
        /// <summary>
        /// Converte un DateTime nella in una stringa con formato custom utilizzando il kind specificato
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="kind"></param>
        /// <param name="customFormat"></param>
        /// <returns></returns>
        public static string ConvertToCustomFormat(this DateTime dateTime, DateTimeKind kind, string customFormat)
        {
            return dateTime.ConvertTo(kind).ToString(customFormat, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Converte un oggetto DateTime in uno con il kind specificato
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="kind"></param>
        /// <returns></returns>
        public static DateTime? ConvertTo(this DateTime? dateTime, DateTimeKind kind)
        {
            if (!dateTime.HasValue) return null;

            if (dateTime.Value.Kind == kind) return dateTime;
            if (kind == DateTimeKind.Utc) return dateTime.ConvertToUTC();
            if (kind == DateTimeKind.Local) return dateTime.ConvertToLocal();
            return dateTime;
        }
        /// <summary>
        /// Converte un oggetto DateTime in uno con il kind specificato
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="kind"></param>
        /// <returns></returns>
        public static DateTime ConvertTo(this DateTime dateTime, DateTimeKind kind)
        {
            if (dateTime.Kind == kind) return dateTime;
            if (kind == DateTimeKind.Utc) return dateTime.ConvertToUTC();
            if (kind == DateTimeKind.Local) return dateTime.ConvertToLocal();
            return dateTime;
        }
        /// <summary>
        /// Converte un oggetto DateTime in uno UTC
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static DateTime ConvertToUTC(this DateTime dateTime)
        {
            if (dateTime.Kind == DateTimeKind.Local)
            {
                return TimeZoneInfo.ConvertTimeToUtc(dateTime);
            }
            return dateTime;
        }
        /// <summary>
        /// Converte un oggetto DateTime in uno UTC
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static DateTime? ConvertToUTC(this DateTime? dateTime)
        {
            if (!dateTime.HasValue) return null;

            if (dateTime.Value.Kind == DateTimeKind.Local)
            {
                return TimeZoneInfo.ConvertTimeToUtc(dateTime.Value);
            }
            return dateTime;
        }
        /// <summary>
        /// Converte un oggetto DateTime in uno LOCAL
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static DateTime ConvertToLocal(this DateTime dateTime)
        {
            if (dateTime.Kind == DateTimeKind.Utc)
            {
                return TimeZoneInfo.ConvertTimeFromUtc(dateTime, TimeZoneInfo.Local);
            }
            return dateTime;
        }
        /// <summary>
        /// Converte un oggetto DateTime in uno LOCAL
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static DateTime? ConvertToLocal(this DateTime? dateTime)
        {
            if (!dateTime.HasValue) return null;

            if (dateTime.Value.Kind == DateTimeKind.Utc)
            {
                return TimeZoneInfo.ConvertTimeFromUtc(dateTime.Value, TimeZoneInfo.Local);
            }
            return dateTime;
        }
    }
}

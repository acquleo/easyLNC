using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Globalization;

namespace acquleo.Base.Json.NewtonSoft
{

    /// <summary>
    /// Implementa un json serializer con formato date time personalizzabile
    /// </summary>
    public static class CustomJsonSerialize
    {
        const string DefaultCustomDateTimeFormat = "yyyy-MM-ddTHH:mm:ss.fffzzz";
        const string ISO8601DateTimeFormat = "o";
        const string ISO8601DateTimeFormatMilli = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffK";

        /// <summary>
        /// Ritorna il json converter in base ai parametri di input
        /// </summary>
        /// <param name="kind">datetime kind (local/utc)</param>
        /// <param name="type">Tipo di formato</param>
        /// <param name="customFormatformat">Formato custom</param>
        /// <returns></returns>
        public static JsonConverter<DateTime> GetDateTimeConverter(string customFormatformat, DateTimeKind kind, DateTimeConverterTypes type)
        {
            if (type == DateTimeConverterTypes.Custom)
                return new CustomDateTimeConverter(customFormatformat, kind);
            if (type == DateTimeConverterTypes.ISO8601)
                return new Iso8601DateTimeConverter(kind);
            if (type == DateTimeConverterTypes.ISO8601_MILLI)
                return new CustomDateTimeConverter(ISO8601DateTimeFormatMilli, kind);

            throw new NotImplementedException();
        }

        /// <summary>
        /// Serializza l'oggetto in stringa
        /// </summary>
        /// <param name="obj">oggetto</param>
        /// <param name="kind">datetime kind (local/utc)</param>
        /// <param name="type">Tipo di formato</param>
        /// <param name="customFormatformat">Formato custom</param>
        /// <returns></returns>
        public static string Serialize(object obj,string customFormatformat,  DateTimeKind kind, DateTimeConverterTypes type)
        {
            var options = new JsonSerializerSettings() {  };
            if(type== DateTimeConverterTypes.Custom)
                options.Converters.Add(new CustomDateTimeConverter(customFormatformat, kind));
            if (type == DateTimeConverterTypes.ISO8601)
                options.Converters.Add(new Iso8601DateTimeConverter(kind));
            if (type == DateTimeConverterTypes.ISO8601_MILLI)
                options.Converters.Add(new CustomDateTimeConverter(ISO8601DateTimeFormatMilli, kind));


            options.Converters.Add(new StringEnumConverter());

            return JsonConvert.SerializeObject(obj, Formatting.None, options);
        }

        /// <summary>
        /// Serializza l'oggetto in stringa
        /// </summary>
        /// <param name="obj">oggetto</param>
        /// <param name="kind">datetime kind (local/utc)</param>
        /// <param name="type">Tipo di formato</param>
        /// <returns></returns>
        public static string Serialize(object obj, DateTimeKind kind, DateTimeConverterTypes type)
        {
            return Serialize(obj,DefaultCustomDateTimeFormat,kind,type);

        }
        /// <summary>
        /// Serializza l'oggetto in stringa - default utilizza "yyyy-MM-ddTHH:mm:ss.fffzzz" e kind local time
        /// </summary>
        /// <param name="obj">oggetto</param>
        /// <returns></returns>
        public static string Serialize(object obj)
        {
            return Serialize(obj, DefaultCustomDateTimeFormat,  DateTimeKind.Local, DateTimeConverterTypes.Custom);
        }

        /// <summary>
        /// Deserializza la stringa in oggetto
        /// </summary>
        /// <param name="objstr">oggetto</param>
        /// <param name="kind">datetime kind (local/utc)</param>
        /// <param name="type">Tipo di formato</param>
        /// <param name="customFormat">Formato custom</param>
        public static T Deserialize<T>(string objstr, string customFormat, DateTimeKind kind, DateTimeConverterTypes type)
        {
            var options = new JsonSerializerSettings() { };
            if (type == DateTimeConverterTypes.Custom)
                options.Converters.Add(new CustomDateTimeConverter(customFormat, kind));
            if (type == DateTimeConverterTypes.ISO8601)
                options.Converters.Add(new Iso8601DateTimeConverter(kind));
            if (type == DateTimeConverterTypes.ISO8601_MILLI)
                options.Converters.Add(new CustomDateTimeConverter(ISO8601DateTimeFormatMilli, kind));

            options.Converters.Add(new StringEnumConverter());

            return JsonConvert.DeserializeObject<T>(objstr, options);
        }

        /// <summary>
        /// Deserializza la stringa in oggetto
        /// </summary>
        /// <param name="objstr">oggetto</param>
        /// <param name="kind">datetime kind (local/utc)</param>
        /// <param name="type">Tipo di formato</param>
        public static T Deserialize<T>(string objstr, DateTimeKind kind, DateTimeConverterTypes type)
        {
            return Deserialize<T>(objstr, DefaultCustomDateTimeFormat,kind,type);
        }


        /// <summary>
        /// Deserializza la stringa in oggetto - default utilizza "yyyy-MM-ddTHH:mm:ss.fffzzz" e kind local time
        /// </summary>
        /// <param name="objstr">oggetto</param>
        public static T Deserialize<T>(string objstr)
        {
            return Deserialize<T>(objstr, DefaultCustomDateTimeFormat);
        }

        /// <summary>
        /// Deserializza la stringa in oggetto - default utilizza "yyyy-MM-ddTHH:mm:ss.fffzzz" e kind local time
        /// </summary>
        /// <param name="objstr">oggetto</param>
        /// <param name="format">datetime format</param>
        public static T Deserialize<T>(string objstr, string format)
        {
            return Deserialize<T>(objstr, format,  DateTimeKind.Local,  DateTimeConverterTypes.Custom);
        }

        private sealed class CustomDateTimeConverter : JsonConverter<DateTime>
        {
            private readonly string Format;
            private readonly DateTimeKind kind;
            public CustomDateTimeConverter(string format)
                :this(format, DateTimeKind.Local)
            {

            }
            public CustomDateTimeConverter(string format, DateTimeKind kind)
            {
                this.Format = format;
                this.kind = kind;
            }

            public override void WriteJson(JsonWriter writer, DateTime date, JsonSerializer serializer)
            {
                writer.WriteValue(date.ConvertTo(kind).ToString(Format, System.Globalization.CultureInfo.InvariantCulture));
            }

            public override DateTime ReadJson(JsonReader reader, Type objectType, DateTime existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                DateTime outVal;
                if (DateTime.TryParseExact(reader.ReadAsString(), Format, System.Globalization.CultureInfo.InvariantCulture, DateTimeStyles.None, out outVal))
                {
                    return outVal.ConvertTo(kind);
                }
                throw new ArgumentException($@"invalid datetime string {reader.ReadAsString()} format {Format}");
            }

        }
        private sealed class Iso8601DateTimeConverter : JsonConverter<DateTime>
        {
            private readonly DateTimeKind kind;
            
            public Iso8601DateTimeConverter(DateTimeKind kind)
            {
                this.kind = kind;
            }
            public override void WriteJson(JsonWriter writer, DateTime date, JsonSerializer serializer)
            {
                writer.WriteValue(date.ConvertTo(kind).ToString(ISO8601DateTimeFormat, System.Globalization.CultureInfo.InvariantCulture));
            }

            public override DateTime ReadJson(JsonReader reader, Type objectType, DateTime existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                DateTime outVal;
                if (DateTime.TryParseExact(reader.ReadAsString(), ISO8601DateTimeFormat, System.Globalization.CultureInfo.InvariantCulture, DateTimeStyles.None, out outVal))
                {
                    return outVal.ConvertTo(kind);
                }
                throw new ArgumentException($@"invalid datetime string {reader.ReadAsString()} format {ISO8601DateTimeFormat}");
            }
        }
    }
}

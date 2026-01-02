using System;
using System.Collections.Generic;
using System.Text;

namespace acquleo.Base.Json
{
    public enum DateTimeConverterTypes
    {
        /// <summary>
        /// uses the format "yyyy-MM-ddTHH:mm:ss.fffzzz"
        /// </summary>
        Custom,
        /// <summary>
        /// uses the ISO 8601 format
        /// </summary>
        ISO8601,
        /// <summary>
        /// uses the ISO 8601 format with milliseconds resolution
        /// </summary>
        ISO8601_MILLI
    }
}

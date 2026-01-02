using System;
using System.Collections.Generic;
using System.Text;

namespace acquleo.Protocol
{
    /// <summary>
    /// Represents a data content
    /// </summary>
    /// <typeparam name="Tdata"></typeparam>
    public class ContentData<Tdata>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="contentType"></param>
        /// <param name="data"></param>
        public ContentData(string contentType, Tdata data) { 
            this.ContentType = contentType;
            this.Data = data;
        }
        /// <summary>
        /// content type
        /// </summary>
        public string ContentType { get; set; }
        /// <summary>
        /// content data
        /// </summary>
        public Tdata Data { get; set; }
    }
}

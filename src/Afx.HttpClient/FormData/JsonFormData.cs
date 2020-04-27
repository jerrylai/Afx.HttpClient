using System;
using System.Collections.Generic;
using System.Text;

namespace Afx.HttpClient
{
    /// <summary>
    /// JsonFormData
    /// </summary>
    public class JsonFormData : ByteFormData
    {
        /// <summary>
        /// JsonFormData
        /// </summary>
        /// <param name="json">json 字符串</param>
        public JsonFormData(string json)
            : base(!string.IsNullOrEmpty(json) ? Encoding.UTF8.GetBytes(json) : new byte[0])
        {
            this.ContentType = "application/json";
            this.ContentEncoding = Encoding.UTF8;
        }
    }
}

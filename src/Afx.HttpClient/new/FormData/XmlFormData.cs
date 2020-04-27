using System;
using System.Collections.Generic;
using System.Text;

namespace Afx.HttpClient
{
    /// <summary>
    /// XmlFormData
    /// </summary>
    public class XmlFormData : BytesFormData
    {
        /// <summary>
        /// JsonFormData
        /// </summary>
        /// <param name="xml">xml 字符串</param>
        public XmlFormData(string xml)
            : base(!string.IsNullOrEmpty(xml) ? Encoding.UTF8.GetBytes(xml) : new byte[0])
        {
            this.ContentType = "application/xml";
            this.ContentEncoding = Encoding.UTF8;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Afx.HttpClient
{
    /// <summary>
    /// StringFormData application/string
    /// </summary>
    public class StringFormData : ByteFormData
    {
        /// <summary>
        /// StringFormData  application/string
        /// </summary>
        /// <param name="data"></param>
        public StringFormData(string data)
            : base(data != null ? Encoding.UTF8.GetBytes(data) : null)
        {
            this.ContentType = "application/string";
            this.ContentEncoding = Encoding.UTF8;
        }
    }
}

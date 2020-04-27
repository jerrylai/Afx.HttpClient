using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;

namespace Afx.HttpClient
{
    /// <summary>
    /// 原始byte[] 表单提交
    /// </summary>
    public class BytesFormData : FormData
    {
        /// <summary>
        /// buffer
        /// </summary>
        protected byte[] buffer;

        /// <summary>
        /// SetContentType
        /// </summary>
        /// <param name="contentType"></param>
        public void SetContentType(string contentType)
        {
            if(!string.IsNullOrEmpty(contentType))
            this.ContentType = contentType;
        }

        /// <summary>
        /// SetContentEncoding
        /// </summary>
        /// <param name="encoding"></param>
        public void SetContentEncoding(Encoding encoding)
        {
            if (encoding != null)
                this.ContentEncoding = encoding;
        }

        /// <summary>
        /// ByteFormData
        /// </summary>
        /// <param name="buffer"></param>
        public BytesFormData(byte[] buffer)
            : this(buffer, 0, buffer == null ? 0 : buffer.Length)
        {
        }
        /// <summary>
        /// ByteFormData
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        public BytesFormData(byte[] buffer, int offset, int count)
        {
            if (buffer != null && buffer.Length > 0 
                && offset >= 0 && count > 0
                && buffer.Length >= offset + count)
            {
                byte[] arr = new byte[count];
                for (int i = 0; i < count; i++)
                {
                    arr[i] = buffer[i + offset];
                }
                this.buffer = arr;
            }
        }

        public override HttpContent GetContent()
        {
            var result = new ByteArrayContent(this.buffer);
            this.AddDispose(result);
            if (!string.IsNullOrEmpty(this.ContentType))
            {
                result.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(this.ContentType);
                if (this.ContentEncoding != null) result.Headers.ContentType.CharSet = this.ContentEncoding.WebName;
            }

            return result;
        }

        /// <summary>
        /// Dispose
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.buffer = null;
            }
            base.Dispose(disposing);
        }

        
    }
}

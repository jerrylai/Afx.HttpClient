using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;

namespace Afx.HttpClient
{
    /// <summary>
    /// 原始流表单提交
    /// </summary>
    public class StreamFormData : FormData
    {
        private Stream stream;

        /// <summary>
        /// StreamFormData
        /// </summary>
        /// <param name="stream"></param>
        public StreamFormData(Stream stream)
        {
            this.stream = stream;
        }
        /// <summary>
        /// SetContentType
        /// </summary>
        /// <param name="contentType"></param>
        public void SetContentType(string contentType)
        {
            if (!string.IsNullOrEmpty(contentType))
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
        /// Dispose
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.stream != null) this.stream.Dispose();
                this.stream = null;
            }
            base.Dispose(disposing);
        }

        public override HttpContent GetContent()
        {
            if (this.stream.CanSeek) this.stream.Seek(0, SeekOrigin.Begin);
            var result = new StreamContent(this.stream);
            this.AddDispose(result);
            if (!string.IsNullOrEmpty(this.ContentType))
            {
                result.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(this.ContentType);
                if (this.ContentEncoding != null) result.Headers.ContentType.CharSet = this.ContentEncoding.WebName;
            }

            return result;
        }
    }
}

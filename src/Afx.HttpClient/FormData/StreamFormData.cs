using System;
using System.Collections.Generic;
using System.IO;
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
        /// Serialize
        /// </summary>
        /// <param name="stream"></param>
        public override void Serialize(Stream stream)
        {
            if (this.stream != null)
            {
                this.stream.Position = 0;
                byte[] buffer = new byte[4 * 1024];
                int count = 0;
                while ((count = this.stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    stream.Write(buffer, 0, count);
                }
            }
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
        /// GetLength
        /// </summary>
        /// <returns></returns>
        public override long GetLength()
        {
            return this.stream == null ? 0 : this.stream.Length;
        }
        /// <summary>
        /// Dispose
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
            if (this.stream != null) this.stream.Dispose();
            this.stream = null;
        }
    }
}

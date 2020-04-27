using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Afx.HttpClient
{
    /// <summary>
    /// 原始byte[] 表单提交
    /// </summary>
    public class ByteFormData : FormData
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
        public ByteFormData(byte[] buffer)
            : this(buffer, 0, buffer == null ? 0 : buffer.Length)
        {
        }
        /// <summary>
        /// ByteFormData
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        public ByteFormData(byte[] buffer, int offset, int count)
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

        /// <summary>
        /// Serialize
        /// </summary>
        /// <param name="stream"></param>
        public override void Serialize(Stream stream)
        {
            if (this.buffer != null && this.buffer.Length > 0)
                stream.Write(this.buffer, 0, this.buffer.Length);
        }
        /// <summary>
        /// GetLength
        /// </summary>
        /// <returns></returns>
        public override long GetLength()
        {
            return this.buffer == null ? 0 : this.buffer.LongLength;
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
            this.buffer = null;
        }
    }
}

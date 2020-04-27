using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Afx.HttpClient
{
    /// <summary>
    /// 请求 FormData
    /// </summary>
    public abstract class FormData : IDisposable
    {
        /// <summary>
        /// 请求 Content Encoding
        /// </summary>
        public Encoding ContentEncoding { get; protected set; }

        /// <summary>
        /// 请求 ContentType
        /// </summary>
        public string ContentType { get; protected set; }

        /// <summary>
        /// Serialize 请求 数据
        /// </summary>
        /// <param name="stream"></param>
        public abstract void Serialize(Stream stream);

        /// <summary>
        /// 获取请求数据byte 总长度
        /// </summary>
        /// <returns></returns>
        public abstract long GetLength();
        /// <summary>
        /// Dispose
        /// </summary>
        public virtual void Dispose()
        {
            this.ContentEncoding = null;
            this.ContentType = null;
        }
    }
}

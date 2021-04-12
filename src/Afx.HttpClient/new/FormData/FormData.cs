using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
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
        public abstract HttpContent GetContent();

        private List<IDisposable> disposables;
        protected void AddDispose(IDisposable dis)
        {
            if (dis == null) return;
            if (this.disposables == null) this.disposables = new List<IDisposable>();
           if(!disposables.Contains(dis)) this.disposables.Add(dis);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.ContentEncoding = null;
                this.ContentType = null;
                if (this.disposables != null)
                {
                    foreach (var dis in this.disposables)
                        dis.Dispose();
                    this.disposables = null;
                }
            }
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
        }
    }
}

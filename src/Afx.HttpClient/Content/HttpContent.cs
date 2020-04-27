using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Afx.HttpClient
{
    /// <summary>
    /// HttpContent
    /// </summary>
    public abstract class HttpContent : IDisposable
    {
        /// <summary>
        /// IsSucceed
        /// </summary>
        public virtual bool IsSucceed { get; internal set; }
        /// <summary>
        /// Error
        /// </summary>
        public virtual Exception Error { get; internal set; }
        /// <summary>
        /// HttpStatusCode
        /// </summary>
        public virtual HttpStatusCode StatusCode { get; internal set; }
        /// <summary>
        /// StatusDescription
        /// </summary>
        public virtual string StatusDescription { get; internal set; }
        /// <summary>
        /// CharacterSet
        /// </summary>
        public string CharacterSet { get; internal set; }
        /// <summary>
        /// ContentEncoding
        /// </summary>
        public string ContentEncoding { get; internal set; }
        /// <summary>
        /// ContentLength
        /// </summary>
        public long ContentLength { get; internal set; }
        /// <summary>
        /// ContentType
        /// </summary>
        public string ContentType { get; internal set; }
        /// <summary>
        /// CookieCollection
        /// </summary>
        public CookieCollection Cookies { get; internal set; }
        /// <summary>
        /// Headers
        /// </summary>
        public Dictionary<string, string> Headers { get; internal set; }
        /// <summary>
        /// LastModified
        /// </summary>
        public DateTime LastModified { get; internal set; }
        /// <summary>
        /// Method
        /// </summary>
        public string Method { get; internal set; }
        /// <summary>
        /// ProtocolVersion
        /// </summary>
        public Version ProtocolVersion { get; internal set; }
        /// <summary>
        /// ResponseUri
        /// </summary>
        public Uri ResponseUri { get; internal set; }
        /// <summary>
        /// Server
        /// </summary>
        public string Server { get; internal set; }

        /// <summary>
        /// HttpContent
        /// </summary>
        public HttpContent()
        {
            this.IsSucceed = false;
            this.StatusCode = HttpStatusCode.NotFound;
            this.StatusDescription = null;
            this.Error = null;
            this.Headers = new Dictionary<string, string>();
            this.Cookies = new CookieCollection();
        }
        /// <summary>
        /// Dispose
        /// </summary>
        public virtual void Dispose()
        {
            this.IsSucceed = false;
            this.StatusCode = HttpStatusCode.Continue;
            this.StatusDescription = null;
            this.Error = null;
            if(this.Headers != null) this.Headers.Clear();
            this.Headers = null;
        }
    }
}

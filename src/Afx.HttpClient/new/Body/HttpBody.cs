using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Linq;
using System.Threading.Tasks;

namespace Afx.HttpClient
{
    /// <summary>
    /// HttpContent
    /// </summary>
    public abstract class HttpBody : IDisposable
    {
        //private Task<HttpResponseMessage> task;
        private HttpResponseMessage httpResponse;
        protected HttpBody(HttpResponseMessage httpResponse)//Task<HttpResponseMessage> task)
        {
            //this.task = task;
            this.httpResponse = httpResponse;
            this.IsSucceed = false;
        }

        internal async Task Proc()
        {
            try
            {
                //this.httpResponse = await this.task;
                this.CharacterSet = this.httpResponse.Content.Headers?.ContentType?.CharSet;
                this.ContentLength = this.httpResponse.Content.Headers?.ContentLength;
                this.ContentType = this.httpResponse.Content.Headers?.ContentType?.MediaType;
                this.LastModified = this.httpResponse.Content.Headers?.LastModified;
                this.ProtocolVersion = this.httpResponse.Version;
                this.StatusCode = this.httpResponse.StatusCode;
                this.dic = new Dictionary<string, IEnumerable<string>>(this.httpResponse.Headers.Count() + this.httpResponse.Content.Headers.Count());
                foreach (KeyValuePair<string, IEnumerable<string>> kv in this.httpResponse.Headers)
                    dic[kv.Key] = kv.Value;
                foreach (KeyValuePair<string, IEnumerable<string>> kv in this.httpResponse.Content.Headers)
                    dic[kv.Key] = kv.Value;
                //if (this.httpResponse.IsSuccessStatusCode)
                {
                    this.IsSucceed = await this.Read(this.httpResponse) && this.httpResponse.IsSuccessStatusCode;
                }
            }
            catch (Exception ex)
            {
                this.Exception = ex;
            }
        }

        protected abstract Task<bool> Read(HttpResponseMessage httpResponse);

        /// <summary>
        /// IsSucceed
        /// </summary>
        public virtual bool IsSucceed { get; private set; }

        /// <summary>
        /// Error
        /// </summary>
        public virtual Exception Exception { get; private set; }

        /// <summary>
        /// HttpStatusCode
        /// </summary>
        public virtual HttpStatusCode StatusCode { get; private set; }
        /// <summary>
        /// CharacterSet
        /// </summary>
        public string CharacterSet { get; private set; }
        /// <summary>
        /// ContentLength
        /// </summary>
        public long? ContentLength { get; private set; }
        /// <summary>
        /// ContentType
        /// </summary>
        public string ContentType { get; protected set; }

        Dictionary<string, IEnumerable<string>> dic = null;
        /// <summary>
        /// Headers
        /// </summary>
        public Dictionary<string, IEnumerable<string>> Headers => dic;
        /// <summary>
        /// LastModified
        /// </summary>
        public DateTimeOffset? LastModified { get; private set; }

        /// <summary>
        /// ProtocolVersion
        /// </summary>
        public Version ProtocolVersion { get; private set; }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.httpResponse?.Content != null) this.httpResponse.Content.Dispose();
                if (this.httpResponse != null) this.httpResponse.Dispose();
                //if (this.task != null) this.task.Dispose();
                if (this.dic != null) this.dic.Clear();
                this.httpResponse = null;
                this.Exception = null;
                this.dic = null;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Afx.HttpClient
{
    /// <summary>
    /// StreamContent
    /// </summary>
    public sealed class StreamBody : HttpBody
    {
        /// <summary>
        /// Body
        /// </summary>
        public Stream Body { get; private set; }

        /// <summary>
        /// ByteContent
        /// </summary>
        internal StreamBody(HttpResponseMessage httpResponse)//Task<HttpResponseMessage> task)
            : base(httpResponse)//(task)
        {

        }

        protected override async Task<bool> Read(HttpResponseMessage httpResponse)
        {
            this.Body = await httpResponse.Content.ReadAsStreamAsync();

            return true;
        }

        /// <summary>
        /// Dispose
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.Body != null) this.Body.Dispose();
                this.Body = null;
            }
            base.Dispose(disposing);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Afx.HttpClient
{
    /// <summary>
    /// HtmlContent
    /// </summary>
    public sealed class StringBody : HttpBody
    {
        /// <summary>
        /// Body
        /// </summary>
        public string Body { get; private set; }

        /// <summary>
        /// ByteContent
        /// </summary>
        internal StringBody(HttpResponseMessage httpResponse)//Task<HttpResponseMessage> task)
            : base(httpResponse)//(task)
        {

        }

        protected override async Task<bool> Read(HttpResponseMessage httpResponse)
        {
            this.Body = await httpResponse.Content.ReadAsStringAsync();

            return true;
        }

        /// <summary>
        /// Dispose
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.Body = null;
            }
            base.Dispose(disposing);
        }
    }
}

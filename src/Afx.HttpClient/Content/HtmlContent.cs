using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Afx.HttpClient
{
    /// <summary>
    /// HtmlContent
    /// </summary>
    public sealed class HtmlContent : HttpContent
    {
        /// <summary>
        /// Body
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// HtmlContent
        /// </summary>
        public HtmlContent()
        {
            this.Body = null;
        }
        /// <summary>
        /// Dispose
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
            this.Body = null;
        }
    }
}

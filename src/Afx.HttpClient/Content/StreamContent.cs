using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;

namespace Afx.HttpClient
{
    /// <summary>
    /// StreamContent
    /// </summary>
    public sealed class StreamContent : HttpContent
    {
        /// <summary>
        /// Stream
        /// </summary>
        public Stream Stream { get; internal set; }
        internal HttpWebResponse response { get; set; }

        /// <summary>
        /// StreamContent
        /// </summary>
        internal StreamContent()
        {
            this.Stream = null;
        }
        /// <summary>
        /// Dispose
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
            if (this.Stream != null)
            {
                try { this.Stream.Dispose(); }
                catch { }
            }
            this.Stream = null;
            if(this.response != null)
            {
                try { this.response.Close();} catch { }
            }
            this.response = null;
        }
    }
}

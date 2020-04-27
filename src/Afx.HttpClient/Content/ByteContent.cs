using System;
using System.Collections.Generic;
using System.Text;

namespace Afx.HttpClient
{
    /// <summary>
    /// ByteContent
    /// </summary>
    public sealed class ByteContent : HttpContent
    {
        /// <summary>
        /// Buffer
        /// </summary>
        public byte[] Buffer { get; set; }

        /// <summary>
        /// ByteContent
        /// </summary>
        public ByteContent()
        {
            this.Buffer = null;
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
            this.Buffer = null;
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;

namespace Afx.HttpClient
{
    /// <summary>
    /// text/plain 表单提交
    /// </summary>
    public class TextPlainFormData : FormData
    {
        private Dictionary<string, string> paramDic;
        private int ver = 0;
        /// <summary>
        /// TextPlainFormData
        /// </summary>
        public TextPlainFormData()
        {
            this.ContentEncoding = Encoding.UTF8;
            this.paramDic = new Dictionary<string, string>();
            this.ContentType = "text/plain";
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void AddParam(string key, string value)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException("key");
            this.paramDic[key] = value ?? "";
            this.ver++;
        }
        public void AddParam(Dictionary<string, string> dic)
        {
            if (dic == null) throw new ArgumentNullException("dic");
            foreach (KeyValuePair<string, string> kv in dic)
            {
                this.paramDic[kv.Key] = kv.Value ?? "";
            }
            this.ver++;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetParam(string key)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException("key");
            if (this.paramDic.ContainsKey(key))
            {
                return this.paramDic[key];
            }

            return null;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        public void RemoveParam(string key)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException("key");
            if (this.paramDic.Remove(key))
                this.ver++;
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.paramDic != null)
                    this.paramDic.Clear();
                this.paramDic = null;
                this.formString = null;
            }
            base.Dispose(disposing);
        }

        private string formString;
        private int formVer = -1;
        public override string ToString()
        {
            if (this.ver != this.formVer)
            {
                StringBuilder text = new StringBuilder();
                foreach (var kv in this.paramDic)
                {
                    text.AppendFormat("{0}={1}\r\n", kv.Key, kv.Value);
                }

                this.formString = text.ToString();
                this.formVer = this.ver;
            }

            return this.formString ?? "";
        }

        public override HttpContent GetContent()
        {
            var s = this.ToString();
            var result = new StringContent(s, this.ContentEncoding, this.ContentType);
            this.AddDispose(result);
            
            return result;
        }
    }
}

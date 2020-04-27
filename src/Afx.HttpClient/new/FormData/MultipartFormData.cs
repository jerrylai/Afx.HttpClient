using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;

namespace Afx.HttpClient
{
    /// <summary>
    /// multipart/form-data 表单提交
    /// </summary>
    public class MultipartFormData : FormData
    {
        private Dictionary<string, string> paramDic;

        private Dictionary<string, string> fileDic;

        private const string BOUNDARY = "----------------afx0httpclient0formdata";

      
        /// <summary>
        /// MultipartFormData
        /// </summary>
        public MultipartFormData()
        {
            this.ContentEncoding = Encoding.UTF8;
            this.paramDic = new Dictionary<string, string>();
            this.fileDic = new Dictionary<string, string>();

            this.ContentType = "multipart/form-data";
        }
        /// <summary>
        /// 添加参数
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void AddParam(string key, string value)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException("key");
            this.paramDic[key] = value ?? "";
        }

        public void AddParam(Dictionary<string, string> dic)
        {
            if (dic == null) throw new ArgumentNullException("dic");
            foreach (KeyValuePair<string, string> kv in dic)
            {
                this.paramDic[kv.Key] = kv.Value ?? "";
            }
        }

        /// <summary>
        /// 获取参数
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
        /// 移除参数
        /// </summary>
        /// <param name="key"></param>
        public void RemoveParam(string key)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException("key");
            this.paramDic.Remove(key);
        }
        /// <summary>
        /// 添加上传文件
        /// </summary>
        /// <param name="key"></param>
        /// <param name="fileName"></param>
        public void AddFile(string key, string fileName)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException("key");
            if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException("fileName");
            if (!File.Exists(fileName)) throw new FileNotFoundException(fileName + " not found!", fileName);

            this.fileDic[key] = fileName;
        }
        /// <summary>
        /// 获取上传文件
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetFile(string key)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException("key");
            if (this.fileDic.ContainsKey(key))
            {
                return this.fileDic[key];
            }

            return null;
        }
        /// <summary>
        /// 移除上传文件
        /// </summary>
        /// <param name="key"></param>
        public void RemoveFile(string key)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException("key");
            this.fileDic.Remove(key);
        }
        /// <summary>
        /// Serialize
        /// </summary>
        /// <param name="stream"></param>

        /// <summary>
        /// Dispose
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.paramDic != null) this.paramDic.Clear();
                if (this.fileDic != null) this.fileDic.Clear();
                this.paramDic = null;
                this.fileDic = null;
            }
            base.Dispose(disposing);
        }

        public override HttpContent GetContent()
        {
            var result = new MultipartFormDataContent(BOUNDARY);
            this.AddDispose(result);
            foreach (KeyValuePair<string, string> kv in this.paramDic)
            {
                StringContent content = new StringContent(kv.Value, this.ContentEncoding);
                this.AddDispose(content);
                result.Add(content, kv.Key);
            }
            foreach (KeyValuePair<string, string> kv in this.fileDic)
            {
                var stream = System.IO.File.Open(kv.Value, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                this.AddDispose(stream);
                StreamContent content = new StreamContent(stream);
                this.AddDispose(content);
                result.Add(content, kv.Key, System.IO.Path.GetFileName(kv.Value));
            }

            return result;
        }
    }
}

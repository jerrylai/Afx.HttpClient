using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Cache;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web;

namespace Afx.HttpClient
{
    /// <summary>
    /// HttpClient
    /// </summary>
    public sealed class HttpClient : IDisposable
    {
        private static bool isSetServicePointManager = false;
        private static void SetServicePointManager()
        {
            if (isSetServicePointManager) return;
            if (ServicePointManager.DefaultConnectionLimit < 10)
                ServicePointManager.DefaultConnectionLimit = 512;
            if (ServicePointManager.ServerCertificateValidationCallback == null)
                ServicePointManager.ServerCertificateValidationCallback = OnServerCertificateValidationCallback;
#if NET20 || NET40
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls;
#else
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            }
            catch
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            }
#endif
            isSetServicePointManager = true;
        }

        private static bool OnServerCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
        /// <summary>
        /// Accept
        /// </summary>
        public string Accept { get; set; }
        /// <summary>
        /// AcceptLanguage
        /// </summary>
        public string AcceptLanguage { get; set; }
        /// <summary>
        /// AcceptCharset
        /// </summary>
        public string AcceptCharset { get; set; }
        /// <summary>
        /// UserAgent
        /// </summary>
        public string UserAgent { get; set; }
        /// <summary>
        /// Timeout
        /// </summary>
        public int? Timeout { get; set; }
        /// <summary>
        /// KeepAlive
        /// </summary>
        public bool? KeepAlive { get; set; }
        /// <summary>
        /// Cookies
        /// </summary>
        public CookieContainer Cookies { get; private set; }
        /// <summary>
        /// UseDefaultCredentials
        /// </summary>
        public bool? UseDefaultCredentials { get; set; }
#if !NET40 && !NET20
        /// <summary>
        /// ServerCertificateValidationCallback
        /// </summary>
        public RemoteCertificateValidationCallback ServerCertificateValidationCallback { get; set; }
#endif
        /// <summary>
        /// ClientCertificates
        /// </summary>
        public X509CertificateCollection ClientCertificates {  get; private set; }

        public Version ProtocolVersion { get; set; }

        public ICredentials Credentials { get; set; }

        public bool? AllowAutoRedirect { get; set; }

        private Dictionary<string, string> headersDic;

        private string _baseAddress = null;
        /// <summary>
        /// BaseAddress
        /// </summary>
        public string BaseAddress
        {
            get
            {
                return this._baseAddress;
            }
            set
            {
                if (string.IsNullOrEmpty(value)
                    || value.ToLower() == "http://"
                    || value.ToLower() == "https://"
                    || !(value.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                    || value.StartsWith("https://", StringComparison.OrdinalIgnoreCase)))
                {
                    throw new ArgumentException("BaseAddress is error!");
                }

                this._baseAddress = value;
            }
        }

        private bool _allowWriteStreamBuffering = true;
        /// <summary>
        /// AllowWriteStreamBuffering
        /// </summary>
        public bool AllowWriteStreamBuffering
        {
            get { return this._allowWriteStreamBuffering; }
            set { this._allowWriteStreamBuffering = value; }
        }

        /// <summary>
        /// HttpClient
        /// </summary>
        public HttpClient()
        {
            SetServicePointManager();
            this.Accept = "text/html, application/xhtml+xml, */*";
            this.AcceptLanguage = "zh-CN";
            this.AcceptCharset = "utf-8";
            this.UserAgent = "Afx.HttpClient";
            this.AllowAutoRedirect = true;
            this.KeepAlive = false;
            //this.Timeout = 10 * 1000;
            //this.UseDefaultCredentials = true;
            //this.Credentials = CredentialCache.DefaultCredentials;
            this.Cookies = new CookieContainer();
            this.headersDic = new Dictionary<string, string>();
            this.ClientCertificates = new X509CertificateCollection();
        }

        /// <summary>
        /// HttpClient
        /// </summary>
        /// <param name="baseAddress"></param>
        public HttpClient(string baseAddress)
            : this()
        {
            this.BaseAddress = baseAddress;
        }
        /// <summary>
        /// AddHeaders
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void AddHeader(string key, string value)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException("key");
            if(!string.IsNullOrEmpty(value))
            {
                this.headersDic[key] = value;
            }
        }

        /// <summary>
        /// GetHeaders
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetHeader(string key)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException("key");
            string value = null;
            this.headersDic.TryGetValue(key, out value);

            return value;
        }
        /// <summary>
        /// RemoveHeader
        /// </summary>
        /// <param name="key"></param>
        public void RemoveHeader(string key)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException("key");
            this.headersDic.Remove(key);
        }
        /// <summary>
        /// ClearHeader
        /// </summary>
        public void ClearHeader()
        {
            this.headersDic.Clear();
        }
        /// <summary>
        /// GetAllHeaders
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string> GetAllHeader()
        {
            Dictionary<string, string> dic = new Dictionary<string, string>(this.headersDic.Count);
            foreach(var kv in this.headersDic)
            {
                dic[kv.Key] = kv.Value;
            }

            return dic;
        }

        private string BuildUrl(string url)
        {
            if (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                || url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                return url;

            if (string.IsNullOrEmpty(this.BaseAddress))
                throw new ArgumentException("Request BaseAddress is empty!");

            if (string.IsNullOrEmpty(url)) return this.BaseAddress;
            
            if (url.StartsWith("/") && this.BaseAddress.EndsWith("/"))
                url = this.BaseAddress + url.TrimStart('/');
            else if (!url.StartsWith("/") && !this.BaseAddress.EndsWith("/"))
                url = this.BaseAddress + "/" + url;
            else
                url = this.BaseAddress + url;
            
            return url;
        }

        private HttpWebRequest CreateRequest(string url, string method)
        {
            url = this.BuildUrl(url);
            HttpWebRequest request = HttpWebRequest.Create(url) as HttpWebRequest;
            if(this.AllowAutoRedirect.HasValue) request.AllowAutoRedirect = this.AllowAutoRedirect.Value;
            request.AllowWriteStreamBuffering = this.AllowWriteStreamBuffering;
            request.CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);
           if(this.KeepAlive.HasValue)  request.KeepAlive = this.KeepAlive.Value;
            request.Method = method;
            request.CookieContainer = this.Cookies;
            if (!string.IsNullOrEmpty(this.Accept))
                request.Accept = this.Accept;
            if (this.Timeout.HasValue && this.Timeout.Value > 0)
                request.Timeout = this.Timeout.Value < 500 ? 500 : this.Timeout.Value;
            if (!string.IsNullOrEmpty(this.UserAgent))
                request.UserAgent = this.UserAgent;
            if (!string.IsNullOrEmpty(this.AcceptLanguage))
                request.Headers.Add(HttpRequestHeader.AcceptLanguage, this.AcceptLanguage);
            if (!string.IsNullOrEmpty(this.AcceptCharset))
                request.Headers.Add(HttpRequestHeader.AcceptCharset, this.AcceptCharset);
            if(this.ProtocolVersion != null) request.ProtocolVersion = this.ProtocolVersion;
            if (this.headersDic.Count > 0)
            {
                foreach(var kv in this.headersDic)
                {        
                    request.Headers.Add(kv.Key, kv.Value);
                }
            }
            if (this.UseDefaultCredentials.HasValue) request.UseDefaultCredentials = this.UseDefaultCredentials.Value;
            if(this.Credentials != null) request.Credentials = this.Credentials;

#if !NET40 && !NET20
            if (this.ServerCertificateValidationCallback != null) request.ServerCertificateValidationCallback = this.ServerCertificateValidationCallback;
#endif
            if (this.ClientCertificates.Count > 0)
            {
                if (request.ClientCertificates != null) request.ClientCertificates.AddRange(this.ClientCertificates);
                else request.ClientCertificates = this.ClientCertificates;
            }

            return request;
        }

        private HttpWebResponse GetResponse(HttpWebRequest request)
        {
            HttpWebResponse response = request.GetResponse() as HttpWebResponse;
            if (response.Cookies != null && response.Cookies.Count > 0)
            {
                this.Cookies.Add(response.Cookies);
            }

            return response;
        }

        private void SetFormData(HttpWebRequest request, FormData formData)
        {
            if (request != null)
            {
                if (formData != null)
                {
                    if (!string.IsNullOrEmpty(formData.ContentType))
                        request.ContentType = formData.ContentType;
                    if (formData.ContentEncoding != null)
                        request.Headers.Add(HttpRequestHeader.ContentEncoding, formData.ContentEncoding.WebName);
                    if (!this.AllowWriteStreamBuffering)
                    {
                        request.ContentLength = formData.GetLength();
                    }
                    using (Stream requestStream = request.GetRequestStream())
                    {
                        formData.Serialize(requestStream);
                        requestStream.Close();
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(request.ContentType))
                    {
                        request.ContentType = "application/x-www-form-urlencoded";
                    }
                    if (!this.AllowWriteStreamBuffering)
                    {
                        request.ContentLength = 0;
                    }
                }   
            }
        }

        private bool IsGZip(string contentEncoding)
        {
            if (!string.IsNullOrEmpty(contentEncoding) && contentEncoding.Contains("gzip"))
                return true;
            else
                return false;
        }

        private bool IsDeflate(string contentEncoding)
        {
            if (!string.IsNullOrEmpty(contentEncoding) && contentEncoding.Contains("deflate"))
                return true;
            else
                return false;
        }

        private Stream GetResponseStream(HttpWebResponse response)
        {
            Stream dataStream = response.GetResponseStream();

            string contentEncoding = (response.ContentEncoding ?? "").ToLower();
            if (IsGZip(contentEncoding))
                dataStream = new GZipStream(dataStream, CompressionMode.Decompress);
            else if (IsDeflate(contentEncoding))
                dataStream = new DeflateStream(dataStream, CompressionMode.Decompress);

            return dataStream;
        }

        private void SetContent(HttpWebResponse response, HttpContent content)
        {
            content.StatusCode = response.StatusCode;
            content.StatusDescription = response.StatusDescription;
            content.IsSucceed = true;
            content.CharacterSet = response.CharacterSet;
            content.ContentEncoding = response.ContentEncoding;
            content.ContentLength = response.ContentLength;
            content.ContentType = response.ContentType;
            if (response.Cookies != null && response.Cookies.Count > 0)
            {
                content.Cookies.Add(response.Cookies);
            }
            content.LastModified = response.LastModified;
            content.Method = response.Method;
            content.ProtocolVersion = response.ProtocolVersion;
            content.ResponseUri = response.ResponseUri;
            content.Server = response.Server;
            if (response.Headers != null && response.Headers.Count > 0)
            {
                foreach (string k in response.Headers.AllKeys)
                {
                    if (!string.IsNullOrEmpty(k))
                    {
                        content.Headers[k] = response.Headers[k];
                    }
                }
            }
        }
        /// <summary>
        /// GetHtmlContent
        /// </summary>
        /// <param name="url"></param>
        /// <param name="method"></param>
        /// <param name="formData"></param>
        /// <returns></returns>
        public HtmlContent GetHtmlContent(string url, string method, FormData formData, Encoding responseEncoding = null)
        {
            HtmlContent result = new HtmlContent();
            try
            {
                HttpWebRequest request = this.CreateRequest(url, method);
                this.SetFormData(request, formData);
                using (HttpWebResponse response = this.GetResponse(request))
                {
                    using (var responseStream = this.GetResponseStream(response))
                    {
                        Encoding encoding = responseEncoding != null ? responseEncoding : Encoding.UTF8;
                        if (responseEncoding == null && response.CharacterSet != null)
                        {
                            try { encoding = Encoding.GetEncoding(response.CharacterSet); }
                            catch { }
                        }

                        using (StreamReader sr = new StreamReader(responseStream, encoding))
                        {
                            result.Body = sr.ReadToEnd();
                        }
                        this.SetContent(response, result);
                    }
                }
            }
            catch (Exception ex)
            {
                result.Error = ex;
            }

            return result;
        }
        /// <summary>
        /// get
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public HtmlContent Get(string url, Encoding responseEncoding = null)
        {
            return this.GetHtmlContent(url, "GET", null, responseEncoding);
        }
        /// <summary>
        /// post
        /// </summary>
        /// <param name="url"></param>
        /// <param name="formData"></param>
        /// <returns></returns>
        public HtmlContent Post(string url, FormData formData, Encoding responseEncoding = null)
        {
            return this.GetHtmlContent(url, "POST", formData, responseEncoding);
        }
        /// <summary>
        /// delete
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public HtmlContent Delete(string url, Encoding responseEncoding = null)
        {
            return this.GetHtmlContent(url, "DELETE", null, responseEncoding);
        }
        /// <summary>
        /// put
        /// </summary>
        /// <param name="url"></param>
        /// <param name="formData"></param>
        /// <returns></returns>
        public HtmlContent Put(string url, FormData formData, Encoding responseEncoding = null)
        {
            return this.GetHtmlContent(url, "PUT", formData, responseEncoding);
        }
        /// <summary>
        /// DownloadFile
        /// </summary>
        /// <param name="url"></param>
        /// <param name="saveFileName"></param>
        /// <returns></returns>
        public EmptyContent DownloadFile(string url, string saveFileName)
        {
            return DownloadFile(url, "GET", null, saveFileName);
        }
        /// <summary>
        /// DownloadFile
        /// </summary>
        /// <param name="url"></param>
        /// <param name="method"></param>
        /// <param name="formData"></param>
        /// <param name="saveFileName"></param>
        /// <returns></returns>
        public EmptyContent DownloadFile(string url, string method, FormData formData, string saveFileName)
        {
            EmptyContent result = new EmptyContent();
            try
            {
                HttpWebRequest request = this.CreateRequest(url, method);
                this.SetFormData(request, formData);
                using (HttpWebResponse response = this.GetResponse(request))
                {
                    using (var responseStream = this.GetResponseStream(response))
                    {
                        using (var fs = File.Create(saveFileName))
                        {
#if NET20
                            var buffer = new byte[8 * 1024];
                            int count = 0;
                            while((count = responseStream.Read(buffer, 0, buffer.Length)) > 0
                                || response.ContentLength> fs.Position)
                            {
                                if (count > 0) fs.Write(buffer, 0, count);
                                else System.Threading.Thread.SpinWait(10);
                            }
#else
                            responseStream.CopyTo(fs);
#endif
                        }
                        this.SetContent(response, result);
                    }
                }
            }
            catch (Exception ex)
            {
                result.Error = ex;
            }

            return result;
        }
        /// <summary>
        /// GetBytes
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public ByteContent GetBytes(string url)
        {
            return this.GetBytes(url, "GET", null);
        }
        /// <summary>
        /// GetBytes
        /// </summary>
        /// <param name="url"></param>
        /// <param name="method"></param>
        /// <param name="formData"></param>
        /// <returns></returns>
        public ByteContent GetBytes(string url, string method, FormData formData)
        {
            ByteContent result = new ByteContent();
            try
            {
                HttpWebRequest request = this.CreateRequest(url, method);
                this.SetFormData(request, formData);
                using (HttpWebResponse response = this.GetResponse(request))
                {
                    using (var responseStream = this.GetResponseStream(response))
                    {
                        using (MemoryStream ms = new MemoryStream())
                        {
#if NET20
                            var buffer = new byte[8 * 1024];
                            int count = 0;
                            while ((count = responseStream.Read(buffer, 0, buffer.Length)) > 0
                                || response.ContentLength > ms.Position)
                            {
                                if (count > 0) ms.Write(buffer, 0, count);
                                else System.Threading.Thread.SpinWait(10);
                            }
#else
                            responseStream.CopyTo(ms);
#endif
                            result.Buffer = ms.ToArray();
                        }

                        this.SetContent(response, result);
                    }
                }
            }
            catch (Exception ex)
            {
                result.Error = ex;
            }

            return result;
        }
        /// <summary>
        /// GetStream
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public StreamContent GetStream(string url)
        {
            return this.GetStream(url, "GET", null);
        }
        /// <summary>
        /// GetStream
        /// </summary>
        /// <param name="url"></param>
        /// <param name="method"></param>
        /// <param name="formData"></param>
        /// <returns></returns>
        public StreamContent GetStream(string url, string method, FormData formData)
        {
            StreamContent result = new StreamContent();
            try
            {
                HttpWebRequest request = this.CreateRequest(url, method);
                this.SetFormData(request, formData);
                result.response = this.GetResponse(request);
                result.Stream = this.GetResponseStream(result.response);
                this.SetContent(result.response, result);
            }
            catch (Exception ex)
            {
                result.Error = ex;
            }

            return result;
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
#if NETCOREAPP || NETSTANDARD
            foreach (var cer in this.ClientCertificates) cer.Dispose();
#endif

            this.ClientCertificates.Clear();
            this.headersDic.Clear();
        }
    }
}
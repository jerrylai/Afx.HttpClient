using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Linq;
using System.Threading;
using System.Collections.Concurrent;

namespace Afx.HttpClient
{
    

    /// <summary>
    /// WebApiClient
    /// </summary>
    public sealed class WebApiClient : IDisposable
    {
        #region
        class LifetimeTrackingHttpMessageHandler : DelegatingHandler
        {
            public LifetimeTrackingHttpMessageHandler(HttpMessageHandler innerHandler)
                : base(innerHandler)
            {
            }

            protected override void Dispose(bool disposing)
            {
                
            }
        }

        class CacheModel
        {
            public LifetimeTrackingHttpMessageHandler handler;
            public DateTime createTime;
        }

        class ClearModel
        {
            public HttpClientHandler handler;
            public WeakReference weak;
        }

        private static int _handlerLifetime = 120;
        private static ConcurrentDictionary<string, Lazy<CacheModel>> _activeHandlers = new ConcurrentDictionary<string, Lazy<CacheModel>>(StringComparer.OrdinalIgnoreCase);
        private static Timer _clearTimer;
        private static object _clearObj = new object();
        private static volatile bool _isStartClear = false;
        private static ConcurrentQueue<ClearModel> _clearQueue = new ConcurrentQueue<ClearModel>();

        /// <summary>
        /// HttpClientHandler生存周期(秒)，默认120s
        /// </summary>
        public static int HandlerLifetime
        {
            get { return _handlerLifetime; }
            set
            {
                if (value < 1) throw new ArgumentOutOfRangeException("HandlerLifetime", "HandlerLifetime more than the 0!");
                _handlerLifetime = value;
            }
        }

        private static HttpMessageHandler GetHandler(string name, Action<HttpClientHandler> config)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
            var now = DateTime.Now;
            var m = _activeHandlers.GetOrAdd(name, (k) =>
            {
                return new Lazy<CacheModel>(() =>
                {
                    var h = new HttpClientHandler();
                    h.ServerCertificateCustomValidationCallback = ServerCertificateValidation;
                    try
                    {
                        h.SslProtocols = SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12;
                    }
                    catch (Exception ex) { Console.WriteLine(ex.Message + ex.StackTrace); }
                    if (config != null) try { config(h); } catch { }

                    return new CacheModel() { handler = new LifetimeTrackingHttpMessageHandler(h), createTime = DateTime.Now };
                }, LazyThreadSafetyMode.ExecutionAndPublication);
            }).Value;
            if((now - m.createTime).TotalSeconds > _handlerLifetime)
            {
                StartClear(name);
            }

            return m.handler;
        }

        private static void StartClear(string name)
        {
            if(_activeHandlers.TryRemove(name, out var v))
            {
                _clearQueue.Enqueue(new ClearModel { weak = new WeakReference(v.Value.handler), handler = v.Value.handler.InnerHandler as HttpClientHandler });
                StartCleanupTimer();
            }
        }

        private static void StartCleanupTimer()
        {
            if (_isStartClear) return;
            lock (_clearObj)
            {
                if (_isStartClear) return;
                bool restoreFlow = false;
                try
                {
                    if (!ExecutionContext.IsFlowSuppressed())
                    {
                        ExecutionContext.SuppressFlow();
                        restoreFlow = true;
                    }

                    _clearTimer = new Timer(CleanupTimer, null, TimeSpan.FromSeconds(10), System.Threading.Timeout.InfiniteTimeSpan);
                    _isStartClear = true;
                }
                finally
                {
                    if (restoreFlow)
                    {
                        ExecutionContext.RestoreFlow();
                    }
                }
            }
        }

        private static void CleanupTimer(object state)
        {
            _clearTimer.Dispose();
            _clearTimer = null;

            var count = _clearQueue.Count;
            for (var i = 0; i < count; i++)
            {
                _clearQueue.TryDequeue(out var cm);
                if (!cm.weak.IsAlive)
                {
                    try
                    {
                        foreach (var crt in cm.handler.ClientCertificates) try { crt.Dispose(); } catch { }
                        cm.handler.ClientCertificates.Clear();
                    }
                    catch { }
                    try { cm.handler.Dispose(); } catch { }
                    cm.handler = null;
                    cm.weak = null;
                }
                else
                {
                    _clearQueue.Enqueue(cm);
                }
            }

            _isStartClear = false;
            if (_clearQueue.Count > 0)
            {
                StartCleanupTimer();
            }
        }

        private static bool ServerCertificateValidation(HttpRequestMessage httpRequestMessage, X509Certificate2 x509Certificate2, X509Chain x509Chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
        #endregion

        private System.Net.Http.HttpClient m_client;

        /// <summary>
        /// 获取与每个请求一起发送的标题。
        /// </summary>
        public HttpRequestHeaders DefaultRequestHeaders
        {
            get { return m_client.DefaultRequestHeaders; }
        }

        /// <summary>
        /// 获取或设置请求超时前等待的毫秒数。
        /// </summary>
        public TimeSpan Timeout
        {
            get { return m_client.Timeout; }
            set { m_client.Timeout = value; }
        }

        /// <summary>
        /// 获取或设置读取响应内容时要缓冲的最大字节数。此属性的默认值为 2 GB。
        /// </summary>
        public long MaxResponseContentBufferSize
        {
            get { return m_client.MaxResponseContentBufferSize; }
            set { m_client.MaxResponseContentBufferSize = value; }
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

        public Version Version { get; set; }

        public string Host { get; set; }

        public string From { get; set; }

        public Uri Referrer { get; set; }

        public IDictionary<string, string> Headers { get; private set; }

        private string m_baseAddress = null;
        /// <summary>
        /// BaseAddress
        /// </summary>
        public string BaseAddress
        {
            get
            {
                return this.m_baseAddress;
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

                this.m_baseAddress = value;
            }
        }

        public bool IsDisposed { get; private set; }

        /// <summary>
        /// HttpClient
        /// </summary>
        public WebApiClient()
        {
            this.Init(nameof(WebApiClient), null);
        }

        private void Init(string name, Action<HttpClientHandler> config)
        {
            var handler = GetHandler(name, config);
            m_client = new System.Net.Http.HttpClient(handler, false);
            //text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3
            m_client.DefaultRequestHeaders.Accept.Clear();
            m_client.DefaultRequestHeaders.Accept.TryParseAdd("text/html");
            m_client.DefaultRequestHeaders.Accept.TryParseAdd("*/*");
            m_client.DefaultRequestHeaders.AcceptLanguage.Clear();
            m_client.DefaultRequestHeaders.AcceptLanguage.TryParseAdd("zh-CN");
            m_client.DefaultRequestHeaders.AcceptLanguage.TryParseAdd("zh");
            m_client.DefaultRequestHeaders.AcceptCharset.Clear();
            m_client.DefaultRequestHeaders.AcceptCharset.TryParseAdd("utf-8");
            m_client.DefaultRequestHeaders.UserAgent.Clear();
            m_client.DefaultRequestHeaders.UserAgent.TryParseAdd("Afx.HttpClient");

            m_client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue() { NoCache = true };

            this.Headers = new Dictionary<string, string>();
            this.IsDisposed = false;
        }

        public WebApiClient(string baseAddress)
        {
            this.BaseAddress = baseAddress;
            this.Init(nameof(WebApiClient), null);
        }

        public WebApiClient(string baseAddress, string name = null, Action<HttpClientHandler> config = null)
        {
            this.BaseAddress = baseAddress;
            this.Init(name ?? nameof(WebApiClient), config);
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

        private void SetDefault(HttpRequestMessage request)
        {
            if (!string.IsNullOrEmpty(this.Accept))  request.Headers.Accept.TryParseAdd(this.Accept);
            if (!string.IsNullOrEmpty(this.AcceptLanguage)) request.Headers.AcceptLanguage.TryParseAdd(this.AcceptLanguage);
            if (!string.IsNullOrEmpty(this.AcceptCharset)) request.Headers.AcceptCharset.TryParseAdd(this.AcceptCharset);
            if (!string.IsNullOrEmpty(this.UserAgent)) request.Headers.UserAgent.TryParseAdd(this.UserAgent);
            if (this.Version != null) request.Version = this.Version;
            if (!string.IsNullOrEmpty(this.Host)) request.Headers.Host = this.Host;
            if (!string.IsNullOrEmpty(this.From)) request.Headers.From = this.From;
            if (this.Referrer != null) request.Headers.Referrer = this.Referrer;
            foreach(KeyValuePair<string, string> kv in this.Headers)
            {
               if(!string.IsNullOrEmpty(kv.Key) && !string.IsNullOrEmpty(kv.Value))
                    request.Headers.Add(kv.Key, kv.Value);
            }
        }

        #region get
        public async Task<BytesBody> GetBytesAsync(string url)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, this.BuildUrl(url)))
            {
                this.SetDefault(request);
                var rp = await m_client.SendAsync(request);
                BytesBody result = new BytesBody(rp);
                this.AddDispose(result);
                await result.Proc();

                return result;
            }
        }

        public BytesBody GetBytes(string url)
        {
            var t = this.GetBytesAsync(url);
            t.Wait();

            return t.Result;
        }

        public async Task<StreamBody> GetStreamAsync(string url)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, this.BuildUrl(url)))
            {
                this.SetDefault(request);
                var rp = await m_client.SendAsync(request);
                StreamBody result = new StreamBody(rp);
                this.AddDispose(result);
                await result.Proc();

                return result;
            }
        }

        public StreamBody GetStream(string url)
        {
            var t = this.GetStreamAsync(url);
            t.Wait();

            return t.Result;
        }

        public async Task<StringBody> GetAsync(string url)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, this.BuildUrl(url)))
            {
                this.SetDefault(request);
                var rp = await m_client.SendAsync(request);
                StringBody result = new StringBody(rp);
                this.AddDispose(result);
                await result.Proc();

                return result;
            }
        }

        public StringBody Get(string url)
        {
            var t = this.GetAsync(url);
            t.Wait();

            return t.Result;
        }
        #endregion

        #region delete
        public async Task<BytesBody> DeleteBytesAsync(string url)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Delete, this.BuildUrl(url)))
            {
                this.SetDefault(request);
                var t = await m_client.SendAsync(request);
                BytesBody result = new BytesBody(t);
                this.AddDispose(result);
                await result.Proc();

                return result;
            }
        }

        public BytesBody DeleteBytes(string url)
        {
            var t = this.DeleteBytesAsync(url);
            t.Wait();

            return t.Result;
        }

        public async Task<StreamBody> DeleteStreamAsync(string url)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Delete, this.BuildUrl(url)))
            {
                this.SetDefault(request);
                var t = await m_client.SendAsync(request);
                StreamBody result = new StreamBody(t);
                this.AddDispose(result);
                await result.Proc();

                return result;
            }
        }

        public StreamBody DeleteStream(string url)
        {
            var t = this.DeleteStreamAsync(url);
            t.Wait();

            return t.Result;
        }

        public async Task<StringBody> DeleteAsync(string url)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Delete, this.BuildUrl(url)))
            {
                this.SetDefault(request);
                var t = await m_client.SendAsync(request);
                StringBody result = new StringBody(t);
                this.AddDispose(result);
                await result.Proc();

                return result;
            }
        }

        public StringBody Delete(string url)
        {
            var t = this.DeleteAsync(url);
            t.Wait();

            return t.Result;
        }
        #endregion

        #region post
        public async Task<BytesBody> PostBytesAsync(string url, FormData formData)
        {
            this.AddDispose(formData);
            using (var content = formData?.GetContent())
            {
                using (var request = new HttpRequestMessage(HttpMethod.Post, this.BuildUrl(url)))
                {
                    this.SetDefault(request);
                    request.Content = content;
                    var t = await m_client.SendAsync(request);
                    
                    BytesBody result = new BytesBody(t);
                    this.AddDispose(result);
                    await result.Proc();

                    return result;
                }
            }
        }

        public BytesBody PostBytes(string url, FormData formData)
        {
            var t = this.PostBytesAsync(url, formData);
            t.Wait();

            return t.Result;
        }

        public async Task<StreamBody> PostStreamAsync(string url, FormData formData)
        {
            this.AddDispose(formData);
            using (var content = formData?.GetContent())
            {
                using (var request = new HttpRequestMessage(HttpMethod.Post, this.BuildUrl(url)))
                {
                    this.SetDefault(request);
                    request.Content = content;
                    var t = await m_client.SendAsync(request);
                    
                    StreamBody result = new StreamBody(t);
                    this.AddDispose(result);
                    await result.Proc();

                    return result;
                }
            }
        }

        public StreamBody PostStream(string url, FormData formData)
        {
            var t = this.PostStreamAsync(url, formData);
            t.Wait();

            return t.Result;
        }

        public async Task<StringBody> PostAsync(string url, FormData formData)
        {
            this.AddDispose(formData);
            using (var content = formData?.GetContent())
            {
                using (var request = new HttpRequestMessage(HttpMethod.Post, this.BuildUrl(url)))
                {
                    this.SetDefault(request);
                    request.Content = content;
                    var t = await m_client.SendAsync(request);

                    StringBody result = new StringBody(t);
                    this.AddDispose(result);
                    await result.Proc();

                    return result;
                }
            }
        }

        public StringBody Post(string url, FormData formData)
        {
            var t = this.PostAsync(url, formData);
            t.Wait();

            return t.Result;
        }
        #endregion

        #region put
        public async Task<BytesBody> PutBytesAsync(string url, FormData formData)
        {
            this.AddDispose(formData);
            using (var content = formData?.GetContent())
            {
                using (var request = new HttpRequestMessage(HttpMethod.Put, this.BuildUrl(url)))
                {
                    this.SetDefault(request);
                    request.Content = content;
                    var t = await m_client.SendAsync(request);
                    
                    BytesBody result = new BytesBody(t);
                    this.AddDispose(result);
                    await result.Proc();

                    return result;
                }
            }
        }

        public BytesBody PutBytes(string url, FormData formData)
        {
            var t = this.PutBytesAsync(url, formData);
            t.Wait();

            return t.Result;
        }

        public async Task<StreamBody> PutStreamAsync(string url, FormData formData)
        {
            this.AddDispose(formData);
            using (var content = formData?.GetContent())
            {
                using (var request = new HttpRequestMessage(HttpMethod.Put, this.BuildUrl(url)))
                {
                    this.SetDefault(request);
                    request.Content = content;
                    var t = await m_client.SendAsync(request);
                    
                    StreamBody result = new StreamBody(t);
                    this.AddDispose(result);
                    await result.Proc();

                    return result;
                }
            }
        }

        public StreamBody PutStream(string url, FormData formData)
        {
            var t = this.PutStreamAsync(url, formData);
            t.Wait();

            return t.Result;
        }

        public async Task<StringBody> PutAsync(string url, FormData formData)
        {
            this.AddDispose(formData);
            using (var content = formData?.GetContent())
            {
                using (var request = new HttpRequestMessage(HttpMethod.Put, this.BuildUrl(url)))
                {
                    this.SetDefault(request);
                    request.Content = content;
                    var t = await m_client.SendAsync(request);
                    
                    StringBody result = new StringBody(t);
                    this.AddDispose(result);
                    await result.Proc();

                    return result;
                }
            }
        }

        public StringBody Put(string url, FormData formData)
        {
            var t = this.PutAsync(url, formData);
            t.Wait();

            return t.Result;
        }
        #endregion

        private List<IDisposable> disposables;
        private void AddDispose(IDisposable dis)
        {
            if (dis == null) return;
            if (this.disposables == null) this.disposables = new List<IDisposable>();
            this.disposables.Add(dis);
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            if (this.m_client != null) try { this.m_client.Dispose(); } catch { }
            if (this.disposables != null)
            {
                foreach (var dis in this.disposables)
                    try { dis.Dispose(); } catch { }
                this.disposables.Clear();
                this.disposables = null;
            }
            this.m_client = null;
            if (this.Headers != null) this.Headers.Clear();
            this.Headers = null;
            this.m_baseAddress = null;
            this.Accept = null;
            this.AcceptCharset = null;
            this.AcceptLanguage = null;
            this.From = null;
            this.Host = null;
            this.Referrer = null;
            this.UserAgent = null;
            this.Version = null;
        }
    }

}
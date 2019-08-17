using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using PROXY_MELI_DATABASE.Mongo;

namespace PROXY_MELI.ReverseProxy
{
    public class ReverseProxyMiddleware
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private readonly RequestDelegate _next;
        private readonly IProxyMeliMongoDatabaseSettings _proxyMeliMongoDatabaseSettings;
        private readonly IMongoDatabase _database;
        private Stopwatch stopwatch;


        public string Ip { get; set; }
        public string Path { get; set; }

        public ReverseProxyMiddleware(RequestDelegate next, IOptions<ProxyMeliMongoDatabaseSettings> settings)
        {
            _next = next;
            _proxyMeliMongoDatabaseSettings = settings.Value;

            var client = new MongoClient(_proxyMeliMongoDatabaseSettings.ConnectionString);
            _database = client.GetDatabase(_proxyMeliMongoDatabaseSettings.DataBaseName);
        }

        public async Task Invoke(HttpContext context)
        {
            var targetUri = BuildTargetUri(context.Request);

            if (targetUri != null)
            {
                var request = context.Request;
                Path = $"{request.Path}{request.QueryString}";
                Ip = !request.IpIsLocal() ? request.GetClientSystemInfo().IpAddress : "";

                var rule = ContainsRules(Ip, Path);
                if(rule != null)
                {
                    //Tem algum controle
                }
                

                stopwatch = new Stopwatch();
                stopwatch.Start();
                HttpRequestMessage targetRequestMessage = CreateTargetMessage(context, targetUri);

                using (var responseMessage = await _httpClient.SendAsync(targetRequestMessage, HttpCompletionOption.ResponseHeadersRead, context.RequestAborted))
                {
                    context.Response.StatusCode = (int)responseMessage.StatusCode;

                    CopyFromTargetResponseHeaders(context, responseMessage);

                    await ProcessResponseContent(context, responseMessage);
                }

                return;
            }

            await _next(context);
        }

        private async Task ProcessResponseContent(HttpContext context, HttpResponseMessage responseMessage)
        {
            var httpContent = responseMessage.Content;
            var content = await httpContent.ReadAsByteArrayAsync();
            await context.Response.Body.WriteAsync(content);

            stopwatch.Stop();
            await LogRequest(context);
        }

        private async Task LogRequest(HttpContext context)
        {
            var request = context.Request;
            var response = context.Response;

            var requestId = request.Headers.FirstOrDefault(x => x.Key.Equals("Request-Id")).Value.ToString();

            var requests = _database.GetCollection<RequestMELI>(_proxyMeliMongoDatabaseSettings.RequestsCollectionName);

            await requests.InsertOneAsync(new RequestMELI
            {
                TotalTime = stopwatch.Elapsed,
                Ip = Ip,
                StatusCode = response.StatusCode,
                Path = Path,
                Date = DateTime.Now
            });
        }

        private Rule ContainsRules(string ip, string path)
        {
            var collection = _database.GetCollection<Rule>(_proxyMeliMongoDatabaseSettings.ConfigCollectionName);
            return collection.Find(r => r.Ip.Equals(ip) || r.Path.Equals(path)).FirstOrDefault();

        }

        private HttpRequestMessage CreateTargetMessage(HttpContext context, Uri targetUri)
        {
            var requestMessage = new HttpRequestMessage();
            CopyFromOriginalRequestContentAndHeaders(context, requestMessage);

            requestMessage.RequestUri = targetUri;
            requestMessage.Headers.Host = targetUri.Host;
            requestMessage.Method = GetMethod(context.Request.Method);

            return requestMessage;
        }

        private void CopyFromOriginalRequestContentAndHeaders(HttpContext context, HttpRequestMessage requestMessage)
        {
            var requestMethod = context.Request.Method;

            if (!HttpMethods.IsGet(requestMethod) &&
                !HttpMethods.IsHead(requestMethod) &&
                !HttpMethods.IsDelete(requestMethod) &&
                !HttpMethods.IsTrace(requestMethod))
            {
                var streamContent = new StreamContent(context.Request.Body);
                requestMessage.Content = streamContent;
            }

            foreach (var header in context.Request.Headers)
            {
                requestMessage.Content?.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
            }
        }

        private void CopyFromTargetResponseHeaders(HttpContext context, HttpResponseMessage responseMessage)
        {
            foreach (var header in responseMessage.Headers)
            {
                context.Response.Headers[header.Key] = header.Value.ToArray();
            }

            foreach (var header in responseMessage.Content.Headers)
            {
                context.Response.Headers[header.Key] = header.Value.ToArray();
            }
            context.Response.Headers.Remove("transfer-encoding");
        }

        private static HttpMethod GetMethod(string method)
        {
            if (HttpMethods.IsDelete(method)) return HttpMethod.Delete;
            if (HttpMethods.IsGet(method)) return HttpMethod.Get;
            if (HttpMethods.IsHead(method)) return HttpMethod.Head;
            if (HttpMethods.IsOptions(method)) return HttpMethod.Options;
            if (HttpMethods.IsPost(method)) return HttpMethod.Post;
            if (HttpMethods.IsPut(method)) return HttpMethod.Put;
            if (HttpMethods.IsTrace(method)) return HttpMethod.Trace;
            return new HttpMethod(method);
        }

        private Uri BuildTargetUri(HttpRequest request)
        {
            //TODO: pegar de config
            var path = request.Path;


            return !"/".Equals(path.Value) && !path.Value.Contains(".") ? new Uri("https://api.mercadolibre.com" + request.Path) : null;
        }
    }
}

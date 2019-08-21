using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Newtonsoft.Json;
using PROXY_MELI_DATABASE.Models;
using PROXY_MELI_DATABASE.Mongo;

namespace PROXY_MELI.ReverseProxy
{
    public class ReverseProxyMiddleware
    {
        private static HttpClient _httpClient;
        private readonly RequestDelegate _next;
        private readonly IProxyMeliMongoDatabaseSettings _proxyMeliMongoDatabaseSettings;
        private readonly IDistributedCache _redisCache;
        private readonly IMongoDatabase _database;
        private Stopwatch stopwatch;

        public ReverseProxyMiddleware(RequestDelegate next,
            IOptions<ProxyMeliMongoDatabaseSettings> settings,
            IDistributedCache redisCache,
            HttpClient httpClient
           )
        {
            _next = next;
            _proxyMeliMongoDatabaseSettings = settings.Value;
            _redisCache = redisCache;
            _httpClient = httpClient;

            var clientMongo = new MongoClient(_proxyMeliMongoDatabaseSettings.ConnectionString);
            _database = clientMongo.GetDatabase(_proxyMeliMongoDatabaseSettings.DataBaseName);
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                context.Response.StatusCode = (int)HttpStatusCode.OK;

                var targetUri = BuildTargetUri(context.Request);

                if (targetUri != null)
                {
                    stopwatch = new Stopwatch();
                    stopwatch.Start();

                    var request = context.Request;
                    var path = string.Concat('/', request.Path.Value.Split('/')[1]);
                    var ip = !request.IpIsLocal() ? request.GetClientSystemInfo().IpAddress : "";

                    if (!await CanPass(ip, path))
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
                        await LogRequest(context);
                        return;
                    }

                    HttpRequestMessage targetRequestMessage = CreateTargetMessage(context,
                  targetUri);

                    using (var responseMessage = await _httpClient.SendAsync(targetRequestMessage, HttpCompletionOption.ResponseHeadersRead, context.RequestAborted))
                    {
                        context.Response.StatusCode = (int)responseMessage.StatusCode;

                        CopyFromTargetResponseHeaders(context, responseMessage);

                        await ProcessResponseContent(context, responseMessage);
                    }

                    return;
                }

            }
            catch (Exception ex)
            {
                await LogError(ex.Message);
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }

            await _next(context);
        }

        private async Task<string> GetCacheRedis(string key)
            => await _redisCache.GetStringAsync(key).ConfigureAwait(false);

        private async Task<Rule> GetCacheRuleRedis(string key)
        {
            var _rule = await _redisCache.GetStringAsync(key).ConfigureAwait(false);
            return _rule != null ? JsonConvert.DeserializeObject<Rule>(_rule) : null;
        }

        private async Task SetCacheRuleRedis(Rule rule)
        {
            var _rule = JsonConvert.SerializeObject(rule);
            await _redisCache.SetStringAsync(rule.KeyRuleRedis, _rule).ConfigureAwait(false);
        }

        private async Task SetCacheRedis(string ip, string path)
        {
            var ttl = new DistributedCacheEntryOptions();
            await _redisCache.SetStringAsync(ip + "_" + path, DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss"), ttl).ConfigureAwait(false);
        }

        private async Task IncrementCacheRateLimit(Rule rule, int qtd)
        {
            qtd++;
            await _redisCache.SetStringAsync(rule.KeyRateLimitRedis, qtd.ToString()).ConfigureAwait(false);

            if (qtd == rule.RateLimit)
            {
                var ttl = new DistributedCacheEntryOptions();
                if (rule.BlockedTime > 0)
                {
                    ttl.SetAbsoluteExpiration(TimeSpan.FromSeconds(rule.BlockedTime));
                }
                await _redisCache.SetStringAsync(rule.KeyRateLimitRedis, "-1", ttl).ConfigureAwait(false);
            }
        }

        private async Task ProcessResponseContent(HttpContext context, HttpResponseMessage responseMessage)
        {
            var httpContent = responseMessage.Content;
            var content = await httpContent.ReadAsByteArrayAsync();
            await context.Response.Body.WriteAsync(content);

            await LogRequest(context);
        }

        private async Task LogRequest(HttpContext context)
        {
            stopwatch.Stop();

            var response = context.Response;
            var request = context.Request;
            var path = request.Path.Value;
            var ip = !request.IpIsLocal() ? request.GetClientSystemInfo().IpAddress : "";

            var requests = _database.GetCollection<RequestMELI>(_proxyMeliMongoDatabaseSettings.RequestsCollectionName);

            await requests.InsertOneAsync(new RequestMELI
            {
                TotalTime = stopwatch.Elapsed,
                Ip = ip,
                StatusCode = response.StatusCode,
                Path = path,
                Date = DateTime.Now
            });
        }

        private async Task LogError(string msg)
        {
            stopwatch.Stop();

            var requests = _database.GetCollection<ErrorMeli>(_proxyMeliMongoDatabaseSettings.ErrorsCollectionName);

            await requests.InsertOneAsync(new ErrorMeli
            {
                Msg = msg,
                Date = DateTime.Now
            });
        }

        private async Task<bool> CanPass(string ip, string path)
        {
            path = path.Replace("/", "");
            var ruleIp = await GetCacheRuleRedis(Rule.PrefixKeyNameRedis + ip).ConfigureAwait(false);
            if (ruleIp != null)
                return await OkRateLimit(ruleIp);

            var rulePath = await GetCacheRuleRedis(Rule.PrefixKeyNameRedis + path).ConfigureAwait(false);

            if (rulePath != null)
                return await OkRateLimit(rulePath);

            var ruleIpPath = await GetCacheRuleRedis(Rule.PrefixKeyNameRedis + ip + path).ConfigureAwait(false);
            if (ruleIpPath != null)
                return await OkRateLimit(rulePath);

            return true;
        }

        private async Task<bool> OkRateLimit(Rule rule)
        {
            if (rule.RateLimit == 0)
                return false;

            var rateLimit = await GetCacheRedis(rule.KeyRateLimitRedis).ConfigureAwait(false);
            var qtd = 0;
            if (rateLimit != null)
                qtd = Int32.Parse(rateLimit);

            if (qtd < 0)
                return false;

            await IncrementCacheRateLimit(rule, qtd);
            return true;
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

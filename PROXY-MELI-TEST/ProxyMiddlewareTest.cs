using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using MongoDB.Driver;
using NSubstitute;
using NUnit.Framework;
using PROXY_MELI.ReverseProxy;
using PROXY_MELI_DATABASE.Models;
using PROXY_MELI_DATABASE.Mongo;
using PROXYMELITEST.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;


namespace Tests
{
    [ExcludeFromCodeCoverage]
    internal class ProxyMiddlewareTest
    {
        private RequestDelegate _requestDelegate;
        private IOptions<ProxyMeliMongoDatabaseSettings> _proxyMeliMongoDatabaseSettings;
        private IDistributedCache _redisCache;

        [SetUp]
        public void Setup()
        {
            _requestDelegate = Substitute.For<RequestDelegate>();
            _redisCache = Substitute.For<IDistributedCache>();

            _proxyMeliMongoDatabaseSettings = Options.Create(new ProxyMeliMongoDatabaseSettings
            {
                ConnectionString = "mongodb://localhost:27017",
                DataBaseName = "proxy-meli",
                RequestsCollectionName = "requests",
                ErrorsCollectionName = "errors"
            });
        }

        [Test]
        public async Task CreateItensTests()
        {
            var mongo = _proxyMeliMongoDatabaseSettings.Value;

            var clientMongo = new MongoClient(mongo.ConnectionString);
            var _database = clientMongo.GetDatabase(mongo.DataBaseName);

            var requests = _database.GetCollection<RequestMELI>(mongo.RequestsCollectionName);

            var date = DateTime.Now.TimeMin();
            var random = new Random();
            for (var i=0; i< 5000; i++)
            {
                var hour = random.Next(0, 24);
                var time = random.Next(10, 200);
                var randomStatudCode = random.Next();
                var statusCode = 200;

                if (randomStatudCode % 3 == 0)
                    statusCode = 429;

                if (randomStatudCode % 7 == 0)
                    statusCode = 500;

                if (randomStatudCode % 11 == 0)
                    statusCode = 404;

                await requests.InsertOneAsync(new RequestMELI
                {
                    TotalTime = new TimeSpan(time),
                    Ip = "192.168.0.1",
                    StatusCode = statusCode,
                    Path = "/teste",
                    Date = date.AddHours(hour)
                });
            }
        }


        [Test]
        public async Task Should_ReturnOk_When_Initialize()
        {
            const string expectedContent = "string content mock";

            var context = CreateBasicContext("/", true);

            const HttpStatusCode expectedStatusCode = HttpStatusCode.OK;

            var sut = CreateSut(expectedStatusCode, expectedContent);

            await sut.Invoke(context).ConfigureAwait(false);
            Assert.AreEqual((int)expectedStatusCode, context.Response.StatusCode);
        }

        [Test]
        public async Task Should_ReturnOk_When_URLisValid()
        {
            const string expectedContent = "string content mock";

            var context = CreateBasicContext("/categories/MLB1430", true);

            const HttpStatusCode expectedStatusCode = HttpStatusCode.OK;

            var sut = CreateSut(expectedStatusCode, expectedContent);

            await sut.Invoke(context).ConfigureAwait(false);
            Assert.AreEqual((int)expectedStatusCode, context.Response.StatusCode);
        }

        [Test]
        public async Task Should_ReturnOk_When_URLisinValid()
        {
            const string expectedContent = "string content mock";

            var context = CreateBasicContext("/categories/xxx", true);

            const HttpStatusCode expectedStatusCode = HttpStatusCode.NotFound;

            var sut = CreateSut(expectedStatusCode, expectedContent);

            await sut.Invoke(context).ConfigureAwait(false);
            Assert.AreEqual((int)expectedStatusCode, context.Response.StatusCode);
        }

        private static HttpContext CreateBasicContext(string path, bool authenticatedUser)
        {
            var context = Substitute.For<HttpContext>();

            var request = Substitute.For<HttpRequest>();
            request.Path.Returns(new PathString(path));
            request.Method.Returns("GET");
            request.Headers.Returns(new HeaderDictionary()
    {
{HeaderNames.UserAgent, "user-agent"}
    });

            request.HttpContext.Connection.RemoteIpAddress.Returns(IPAddress.Parse("192.168.1.111"));

            context.Request.Returns((req) => request);

            context.Response.Body.Returns(Stream.Null);

            var claimsIdentity = Substitute.For<ClaimsIdentity>();
            claimsIdentity
        .Claims
        .Returns(new List<Claim>() {
    new Claim("cia", IPAddress.Any.ToString()),
    new Claim(ClaimTypes.Name, "12930129301")
        });
            claimsIdentity.IsAuthenticated.Returns(authenticatedUser);

            context.User.Returns(new ClaimsPrincipal(claimsIdentity));

            return context;
        }

        private ReverseProxyMiddleware CreateSut(HttpStatusCode expectedStatusCode, string expectedContent)
            => new ReverseProxyMiddleware(_requestDelegate, _proxyMeliMongoDatabaseSettings, _redisCache, new HttpClient(new FakeDelegatingHandler(expectedStatusCode, expectedContent)));
    }
}

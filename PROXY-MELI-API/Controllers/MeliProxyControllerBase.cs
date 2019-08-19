using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using PROXY_MELI_DATABASE.Models;
using PROXY_MELI_DATABASE.Mongo;
using StackExchange.Redis;

namespace PROXY_MELI_API.Controllers
{
    public class MeliProxyControllerBase : ControllerBase
    {
        protected readonly IProxyMeliMongoDatabaseSettings _proxyMeliMongoDatabaseSettings;
        private readonly IDistributedCache _redisCache;
        protected readonly IMongoDatabase _database;
        private readonly IConnectionMultiplexer _connectionMultiplexer;

        public MeliProxyControllerBase(
            IOptions<ProxyMeliMongoDatabaseSettings> settings,
            IDistributedCache redisCache,
            IConnectionMultiplexer connectionMultiplexer
           )
        {
            _proxyMeliMongoDatabaseSettings = settings.Value;
            _redisCache = redisCache;
            _connectionMultiplexer = connectionMultiplexer;

            var clientMongo = new MongoClient(_proxyMeliMongoDatabaseSettings.ConnectionString);
            _database = clientMongo.GetDatabase(_proxyMeliMongoDatabaseSettings.DataBaseName);
        }

        public IList<string> GetRulesAllKeysRedis()
        {
            var server = _connectionMultiplexer.GetServer(_connectionMultiplexer.GetEndPoints().First());
            var keys = server.Keys(pattern: Rule.PrefixKeyNameRedis + "*");
            return keys.Select(key => (string)key).ToList();
        }
    }
}

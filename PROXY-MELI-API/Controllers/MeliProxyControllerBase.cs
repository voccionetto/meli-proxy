using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Newtonsoft.Json;
using PROXY_MELI_DATABASE.Models;
using PROXY_MELI_DATABASE.Mongo;
using StackExchange.Redis;

namespace PROXY_MELI_API.Controllers
{
    [Produces("application/json")]
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

        protected async Task<string> GetStringCacheRedis(string key)
           => await _redisCache.GetStringAsync(key).ConfigureAwait(false);

        protected async Task<Rule> GetCacheRuleRedis(string key)
        {
            var _rule = await _redisCache.GetStringAsync(key).ConfigureAwait(false);
            return _rule != null ? JsonConvert.DeserializeObject<Rule>(_rule) : null;
        }

        protected async Task SetCacheRuleRedis(Rule rule)
        {
            var _rule = JsonConvert.SerializeObject(rule);
            await _redisCache.SetStringAsync(rule.KeyRuleRedis, _rule).ConfigureAwait(false);
        }

        protected async Task DeleteCacheRuleRedis(string key)
        {
            await _redisCache.RemoveAsync(key).ConfigureAwait(false);
        }

        public IList<string> GetRulesAllKeysRedis()
        {
            var server = _connectionMultiplexer.GetServer(_connectionMultiplexer.GetEndPoints().First());
            var keys = server.Keys(pattern: Rule.PrefixKeyNameRedis + "*");
            return keys.Select(key => (string)key).ToList();
        }
    }
}

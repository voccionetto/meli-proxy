using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PROXY_MELI_DATABASE.Models;
using PROXY_MELI_DATABASE.Mongo;
using StackExchange.Redis;

namespace PROXY_MELI_API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class AdminControlController : MeliProxyControllerBase
    {
        private readonly ILogger<AdminControlController> _logger;

        public AdminControlController(
            IOptions<ProxyMeliMongoDatabaseSettings> settings,
            IDistributedCache redisCache,
            IConnectionMultiplexer _connectionMultiplexer, ILogger<AdminControlController> logger
           ) : base(settings, redisCache, _connectionMultiplexer)
        {
            _logger = logger;
        }

        [HttpGet("AllRules")]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetAllRulesAsync()
        {
            var keys = GetRulesAllKeysRedis();
            _logger.LogDebug($"Listing Rules {keys.Count}");

            var responseKeys = new List<Rule>();
            foreach (var key in keys)
            {
                responseKeys.Add(await GetCacheRuleRedis(key).ConfigureAwait(false));
            }

            return Created("", responseKeys);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetAsync(string id)
        {
            var _key = await GetCacheRuleRedis(id).ConfigureAwait(false);
            if (_key != null)
            {
                _logger.LogDebug($"Get Rule {_key.KeyRuleRedis}");
            }
            else
            {
                _logger.LogDebug($"Get Rule {id} not found");
            }

            return Created("", _key);
        }

        // POST api/values
        [HttpPost]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        public async Task<ActionResult> PostAsync([FromBody] Rule newRule)
        {
            if (newRule != null)
            {
                await SetCacheRuleRedis(newRule).ConfigureAwait(false);

                _logger.LogDebug($"Rule insert/update {newRule.KeyRuleRedis}!");

            }
            return Ok(newRule);
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public async Task<ActionResult> PutAsync(string id, [FromBody] Rule newRule)
        {
            if (newRule != null && !string.IsNullOrEmpty(id) && await GetCacheRuleRedis(id).ConfigureAwait(false) != null)
            {
                await SetCacheRuleRedis(newRule).ConfigureAwait(false);
            }
            return Ok(newRule);
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        public async Task<ActionResult> DeleteAsync(string id)
        {
            try
            {
                var rule = await GetCacheRuleRedis(id).ConfigureAwait(false);
                if (rule != null)
                {
                    var keyRuleLimit = rule.KeyRateLimitRedis;

                    var keyRateLimit = await GetStringCacheRedis(keyRuleLimit).ConfigureAwait(false);
                    if (!string.IsNullOrEmpty(keyRateLimit))
                    {
                        await DeleteCacheRuleRedis(keyRuleLimit).ConfigureAwait(false);
                        _logger.LogDebug($"Key Rate Limit {rule.KeyRateLimitRedis} deleted!");
                    }
                    await DeleteCacheRuleRedis(id).ConfigureAwait(false);
                    _logger.LogDebug($"Rule {id} deleted!");
                }

                return Ok(true);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting rule {id} error {ex.Message}");
                return Ok(false);
            }
        }
    }
}

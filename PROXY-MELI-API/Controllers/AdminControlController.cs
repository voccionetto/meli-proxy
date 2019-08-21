using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
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
        public AdminControlController(
            IOptions<ProxyMeliMongoDatabaseSettings> settings,
            IDistributedCache redisCache,
            IConnectionMultiplexer _connectionMultiplexer
           ) : base(settings, redisCache, _connectionMultiplexer)
        {
        }

        [HttpGet("AllRules")]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetAllRulesAsync()
        {
            var keys = GetRulesAllKeysRedis();

            var responseKeys = new List<Rule>();
            foreach(var key in keys)
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
                //var _key = await GetCacheRuleRedis(newRule.KeyRuleRedis).ConfigureAwait(false);
                await SetCacheRuleRedis(newRule).ConfigureAwait(false);
            }
            return Ok(newRule);
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        public async Task<ActionResult> DeleteAsync(string id)
        {
            await DeleteCacheRuleRedis(id).ConfigureAwait(false);
            return Ok(id);
        }
    }
}

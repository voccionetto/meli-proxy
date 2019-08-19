using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
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

        [HttpGet("Rule")]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetRulesAsync()
        {
            var keys = GetRulesAllKeysRedis();

            return Created("", keys);
        }


        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}

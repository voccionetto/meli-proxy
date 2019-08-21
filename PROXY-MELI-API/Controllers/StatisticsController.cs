using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using PROXY_MELI_API.Models;
using PROXY_MELI_DATABASE.Models;
using PROXY_MELI_DATABASE.Mongo;
using StackExchange.Redis;

namespace PROXY_MELI_API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class StatisticsController : MeliProxyControllerBase
    {
        public StatisticsController(
            IOptions<ProxyMeliMongoDatabaseSettings> settings,
            IDistributedCache redisCache,
            IConnectionMultiplexer _connectionMultiplexer
           ) : base(settings, redisCache, _connectionMultiplexer)
        {
        }

        private CreatedResult CreateResult(IList<RequestMELI> requests)
        {
            return Created("", requests.Select(h => new HitResponse
            {
                TotalTime = h.TotalTime,
                StatusCode = h.StatusCode,
                Ip = h.Ip,
                Path = h.Path,
                Date = h.Date
            }));
        }

        [HttpGet("AllHits")]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetAllHitsAsync(DateTime? date = null)
        {
            var requests = _database.GetCollection<RequestMELI>(_proxyMeliMongoDatabaseSettings.RequestsCollectionName);
            var _date = DateTime.Now;
            if (date != null)
                _date = date.Value;

            var _min = _date.TimeMin();
            var _max = _date.TimeMax();

            var hits = await requests.Find(r=> r.Date >= _min && r.Date <= _max).ToListAsync();
            return CreateResult(hits);
        }

        [HttpGet("OKHits")]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetOKHitsAsync(DateTime? date = null)
        {
            var requests = _database.GetCollection<RequestMELI>(_proxyMeliMongoDatabaseSettings.RequestsCollectionName);
            var _date = DateTime.Now;
            if (date != null)
                _date = date.Value;

            var _min = _date.TimeMin();
            var _max = _date.TimeMax();

            var hits = await requests.Find(r=> r.Date >= _min && r.Date <= _max && r.StatusCode == 200).ToListAsync();
            return CreateResult(hits);
        }

        [HttpGet("TooManyRequestsHits")]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetTooManyRequestsHitsAsync(DateTime? date = null)
        {
            var requests = _database.GetCollection<RequestMELI>(_proxyMeliMongoDatabaseSettings.RequestsCollectionName);
            var _date = DateTime.Now;
            if (date != null)
                _date = date.Value;

            var _min = _date.TimeMin();
            var _max = _date.TimeMax();

            var hits = await requests.Find(r => r.Date >= _min && r.Date <= _max && r.StatusCode == 429).ToListAsync();
            return CreateResult(hits);
        }

        [HttpGet("NotFoundRequestsHits")]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetNotFoundRequestsHitsAsync(DateTime? date = null)
        {
            var requests = _database.GetCollection<RequestMELI>(_proxyMeliMongoDatabaseSettings.RequestsCollectionName);
            var _date = DateTime.Now;
            if (date != null)
                _date = date.Value;

            var _min = _date.TimeMin();
            var _max = _date.TimeMax();

            var hits = await requests.Find(r => r.Date >= _min && r.Date <= _max && r.StatusCode == 404).ToListAsync();
            return CreateResult(hits);
        }

        [HttpGet("ErrorsRequestsHits")]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetErrorsRequestsHitsAsync(DateTime? date = null)
        {
            var requests = _database.GetCollection<RequestMELI>(_proxyMeliMongoDatabaseSettings.RequestsCollectionName);
            var _date = DateTime.Now;
            if (date != null)
                _date = date.Value;

            var _min = _date.TimeMin();
            var _max = _date.TimeMax();

            var hits = await requests.Find(r => r.Date >= _min && r.Date <= _max && r.StatusCode == 500).ToListAsync();
            return CreateResult(hits);
        }
    }
}

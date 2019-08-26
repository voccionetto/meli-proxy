using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using PROXY_MELI_DATABASE.Models;
using PROXY_MELI_WEB.Models;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace PROXY_MELI_WEB.Controllers
{
    public class HomeController : ControllerBase
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(IOptions<ApiCaller> api, ILogger<HomeController> logger)
            : base(api, logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        private IEnumerable<int> OrderHits(IList<HitResponse> hits)
        {
            var hitsReturn = new List<int>();
            var hitsOrder = hits.OrderBy(g => g.Date);

            for (var i = 0; i < 24; i++)
            {
                var hitsSelect = hitsOrder.Where(h => h.Date.Hour == i);
                hitsReturn.Add(hitsSelect.Count());
            }
            return hitsReturn;
        }

        private IEnumerable<double> CalculateAverageTime(IList<HitResponse> hits)
        {
            var responses = new List<double>();
            var hitsOrder = hits.OrderBy(g => g.Date);

            for (var i = 0; i < 24; i++)
            {
                var hitsSelect = hitsOrder.Where(h => h.Date.Hour == i);
                var list = hitsSelect.Select(h => h.TotalTime.TotalMilliseconds);
                var average = list.Count() > 0 ? list.Average() : 0;
                responses.Add(average);
            }

            return responses;
        }

        public JsonResult GetHits(DateTime date)
        {
            var hits = new List<HitResponse>();
            try
            {
                _logger.LogDebug($"get all hits");

                var allHits = CallGet<IList<HitResponse>>("statistics/AllHits");
                hits = allHits.OrderBy(h => h.Ip).ThenBy(h => h.Path).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError($"error get all hits {ex.Message}");
            }
            return Json(hits);
        }


        public JsonResult GetItensChart(DateTime date, GraphTypes type)
        {
            object itens = new List<int>();
            var title = "";
            var yAxesTitle = "Hit";
            var color = "#4169E1";
            try
            {
                switch (type)
                {
                    case GraphTypes.TotalHits:
                        title = "Total Hits";
                        var allHits = CallGet<IList<HitResponse>>("statistics/AllHits");
                        itens = OrderHits(allHits);
                        break;
                    case GraphTypes.StatusOK:
                        title = "Status 200";
                        var hitsOK = CallGet<IList<HitResponse>>("statistics/OKHits");
                        itens = OrderHits(hitsOK);
                        break;
                    case GraphTypes.TooManyRequests:
                        title = "Status 429";
                        color = "#008000";
                        var hitsTooManyRequests = CallGet<IList<HitResponse>>("statistics/TooManyRequestsHits");
                        itens = OrderHits(hitsTooManyRequests);
                        break;
                    case GraphTypes.NotFound:
                        title = "Status 404";
                        color = "#FF0000";
                        var hitsNotFound = CallGet<IList<HitResponse>>("statistics/NotFoundRequestsHits");
                        itens = OrderHits(hitsNotFound);
                        break;
                    case GraphTypes.Errors:
                        title = "Status 500";
                        color = "#FF0000";
                        var hitsErrors = CallGet<IList<HitResponse>>("statistics/ErrorsRequestsHits");
                        itens = OrderHits(hitsErrors);
                        break;
                    case GraphTypes.AverageTime:
                        title = "Average Time";
                        yAxesTitle = "milliseconds";
                        var hitsAverage = CallGet<IList<HitResponse>>("statistics/AllHits");
                        itens = CalculateAverageTime(hitsAverage);
                        break;
                    default:
                        title = "Page Setup Error <br> Contact us -> voccio@gmail.com    ;)";
                        break;
                }
            }

            catch (Exception ex)
            {
                title = "Ops... Error trying to load page <br>" + ex.Message;
            }

            return Json(
            new
            {
                response = itens,
                title,
                yAxesTitle,
                color
            });
        }
    }
}

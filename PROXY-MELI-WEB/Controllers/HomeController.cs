using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PROXY_MELI_DATABASE.Models;
using PROXY_MELI_WEB.Models;
using System.Net;

namespace PROXY_MELI_WEB.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        private IList<RequestMELI> GetRequests()
        {
            var requests = new List<RequestMELI>();
            var random = new Random();
            for (var i = 0; i < 1000; i++)
            {
                var aleatorio = random.Next();
                requests.Add(new RequestMELI
                {
                    TotalTime = new TimeSpan(random.Next()),
                    StatusCode = aleatorio % 2 == 0 ? 200 : 429,
                    Path = "/teste",
                    Date = new DateTime(2019, 8, 19, random.Next(0, 24), 0, 0)
                });
            }
            return requests;
        }

        private IList<DataPoint> GetJsonItens(IEnumerable<IGrouping<int, RequestMELI>> requests)
        {
            var itens = new List<DataPoint>();
            foreach (var r in requests)
            {
                itens.Add(new DataPoint(r.Key, r.ToList().Count()));
            }
            return itens;
        }

        public JsonResult GetItens(DateTime date, GraphTypes type)
        {
            IList<DataPoint> itens = new List<DataPoint>();
            var title = "";
            var subTitle = "";

            try
            {
                switch (type)
                {
                    case GraphTypes.TotalHits:
                        title = "Total Hits";
                        itens = GetJsonItens(GetRequests().OrderBy(g => g.Date).GroupBy(g => g.Date.Hour));
                        break;
                    case GraphTypes.StatusOK:
                        title = "Status 200";
                        itens = GetJsonItens(GetRequests().Where(g => g.StatusCode == (int)HttpStatusCode.OK).OrderBy(g => g.Date).GroupBy(g => g.Date.Hour));
                        break;
                    case GraphTypes.TooManyRequests:
                        title = "Status 429";
                        itens = GetJsonItens(GetRequests().Where(g => g.StatusCode == (int)HttpStatusCode.TooManyRequests).OrderBy(g => g.Date).GroupBy(g => g.Date.Hour));
                        break;
                    case GraphTypes.NotFound:
                        title = "Status 404";
                        itens = GetJsonItens(GetRequests().Where(g => g.StatusCode == (int)HttpStatusCode.NotFound).OrderBy(g => g.Date).GroupBy(g => g.Date.Hour));
                        break;
                    case GraphTypes.Errors:
                        title = "Status 500";
                        itens = GetJsonItens(GetRequests().Where(g => g.StatusCode == (int)HttpStatusCode.InternalServerError).OrderBy(g => g.Date).GroupBy(g => g.Date.Hour));
                        break;
                    case GraphTypes.AverageTime:
                        title = "Average Time";
                        //itens = GetJsonItens(GetRequests().Where(g => g.StatusCode == (int)HttpStatusCode.InternalServerError).OrderBy(g => g.Date).GroupBy(g => g.Date.Hour));
                        break;
                    default:
                        title = "Page Setup Error";
                        subTitle = "Contact us -> voccio@gmail.com    ;)";
                        break;
                }
            }

            catch (Exception ex)
            {
                title = ex.Message;
                subTitle = "Ops... Error trying to load page";
            }

            return Json(
        new
        {
            response = itens,
            title,
            subTitle
        });
        }
    }
}

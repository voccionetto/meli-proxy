using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using PROXY_MELI_WEB.Models;

namespace PROXY_MELI_WEB.Controllers
{
    public class ControllerBase : Controller
    {
        protected readonly ApiCaller _api;

        public ControllerBase(IOptions<ApiCaller> api)
        {
            _api = api.Value;
        }


        protected T CallGet<T>(string path)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                var url = _api.ApiPath + path;

                HttpResponseMessage response = client.GetAsync(url).Result;

                response.EnsureSuccessStatusCode();
                string content =
                    response.Content.ReadAsStringAsync().Result;
                dynamic json = JsonConvert.DeserializeObject(content);
                return json.ToObject<T>();
            }
        }

    }
}

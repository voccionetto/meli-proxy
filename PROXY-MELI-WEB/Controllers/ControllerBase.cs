﻿using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using PROXY_MELI_WEB.Models;

namespace PROXY_MELI_WEB.Controllers
{
    public class ControllerBase : Controller
    {
        protected readonly ApiCaller _api;
        private readonly ILogger<ControllerBase> _logger;

        public ControllerBase(IOptions<ApiCaller> api, ILogger<ControllerBase> logger)
        {
            _logger = logger;
            _api = api.Value;
        }


        protected T CallGet<T>(string path)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri($"{this.Request.Scheme}://{this.Request.Host}");

                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                var url = _api.ApiPath + path;

                _logger.LogDebug($"executing get {url}");

                HttpResponseMessage response = client.GetAsync(url).Result;

                response.EnsureSuccessStatusCode();
                string content =
                    response.Content.ReadAsStringAsync().Result;
                dynamic json = JsonConvert.DeserializeObject(content);
                return json.ToObject<T>();
            }
        }

        protected HttpResponseMessage CallPost(string path, object data)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri($"{this.Request.Scheme}://{this.Request.Host}");

                var parametros = JsonConvert.SerializeObject(data);
                var content = new StringContent(parametros, System.Text.Encoding.UTF8, "application/json");

                var url = _api.ApiPath + path;
                _logger.LogDebug($"executing post {url}");

                var result = httpClient.PostAsync(url, content).Result;
                return result;
            }
        }

        protected HttpResponseMessage CallDelete(string path)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri($"{this.Request.Scheme}://{this.Request.Host}");

                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                var url = _api.ApiPath + path;
                _logger.LogDebug($"executing delete {url}");

                HttpResponseMessage response = client.DeleteAsync(url).Result;

                string content =
                    response.Content.ReadAsStringAsync().Result;
                dynamic json = JsonConvert.DeserializeObject(content);
                return json != null ? json.ToObject<bool>() : null;
            }
        }

    }
}

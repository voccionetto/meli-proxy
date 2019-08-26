using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using PROXY_MELI_DATABASE.Models;
using PROXY_MELI_WEB.Models;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace PROXY_MELI_WEB.Controllers
{
    public class AdminController : ControllerBase
    {
        private readonly ILogger<AdminController> _logger;

        public AdminController(IOptions<ApiCaller> api, ILogger<AdminController> logger)
            : base(api, logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public JsonResult GetRules()
        {
            IList<Rule> rules = new List<Rule>();
            try
            {
                _logger.LogDebug("get all rules ");
                rules = CallGet<IList<Rule>>("adminControl/allrules");
            }
            catch(Exception ex)
            {
                _logger.LogError($"error get all rules {ex.Message}");
            }
            return Json(rules);
        }

        public JsonResult GetRule(string id)
        {
            var rule = new Rule();
            try
            {
                rule = CallGet<Rule>("adminControl/" + id);
            }
            catch (Exception ex)
            {
                _logger.LogError($"error get rule {ex.Message}");
            }
            return Json(rule);
        }

        public JsonResult SaveUpdateKey(Rule rule)
        {
            var ok = true;
            try
            {
                CallPost("adminControl", rule);
            }
            catch(Exception ex)
            {
                ok = false;
                _logger.LogError($"error SaveUpdate key {ex.Message}");
            }

            return Json(ok);
        }

        public JsonResult DeleteRule(string id)
        {
            var ok = true;
            try
            {
                CallDelete("adminControl/" + id);
            }
            catch (Exception ex)
            {
                _logger.LogError($"error delete rule {ex.Message}");

                ok = false;
            }

            return Json(ok);
        }
    }
}

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
using Microsoft.Extensions.Options;

namespace PROXY_MELI_WEB.Controllers
{
    public class AdminController : ControllerBase
    {

        public AdminController(IOptions<ApiCaller> api)
            : base(api)
        {
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
                rules = CallGet<IList<Rule>>("adminControl/allrules");
            }
            catch(Exception ex)
            {
                //TODO: logar
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
                //TODO: logar
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
                ok = false;
            }

            return Json(ok);
        }
    }
}

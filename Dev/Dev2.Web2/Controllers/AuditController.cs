#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.ExtMethods;
using Dev2.Communication;
using Dev2.Web2.Models.Auditing;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http.Cors;
using System.Web.Mvc;
using Warewolf.Auditing;
using Warewolf.Common;

namespace Dev2.Web2.Controllers
{
    public class AllowCrossSiteJsonAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var ctx = filterContext.RequestContext.HttpContext;
            var origin = ctx.Request.Headers["Origin"];
            var allowOrigin = !string.IsNullOrWhiteSpace(origin) ? origin : "*";
            ctx.Response.AddHeader("Access-Control-Allow-Origin", allowOrigin);
            ctx.Response.AddHeader("Access-Control-Allow-Headers", "*");
            ctx.Response.AddHeader("Access-Control-Allow-Credentials", "true");
            base.OnActionExecuting(filterContext);
        }
    }

    [EnableCors("*", "*", "*", PreflightMaxAge = 10000, SupportsCredentials = true)]
    public class AuditController : Controller
    {
        private readonly IWebClientFactory _clientFactory;
        public AuditController()
            :this(new WebClientFactory())
        {

        }

        public AuditController(IWebClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        // GET: Audit
        [AllowCrossSiteJson]
        public ActionResult Index()
        {
            var request = CheckRequest(null);
            var model = new Tuple<List<Audit>, AuditingViewModel>(new List<Audit>(), request);
            return View(model);
        }

        [HttpGet]
        [AllowCrossSiteJson]
        public ActionResult Index(string server)
        {
            var servername = server;
            var request = CheckRequest(null);
            if (string.IsNullOrEmpty(servername))
            {
                servername = "localhost";
            }
            request.Server = servername;
            var model = new Tuple<List<Audit>, AuditingViewModel>(new List<Audit>(), request);
            return View(model);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult AuditList(string jsonData)
        {
            var serializer = new Dev2JsonSerializer();
            var request = CheckRequest(null);
            if (jsonData.IsJSON() & !jsonData.Contains("[null]"))
            {
                var logEntries = serializer.Deserialize<List<Audit>>(jsonData);
                var model = new Tuple<List<Audit>, AuditingViewModel>(logEntries, request);
                return PartialView("AuditList", model.Item1);
            }
            else
            {
                var model = new Tuple<List<Audit>, AuditingViewModel>(new List<Audit>(), request);
                return PartialView("AuditList", model.Item1);
            }
        }

        [HttpPost]
        public ActionResult PerformResume(string resourceID, string environment, string startActivityId, string wareWolfResumeUrl)
        {
            if (TempData.ContainsKey("allowLogin"))
            {
                // See if they've supplied credentials
                var authHeader = Request.Headers["Authorization"];
                if ((authHeader != null) && (authHeader.StartsWith("Basic")))
                {
                    // Parse username and password out of the HTTP headers
                    authHeader = authHeader.Substring("Basic".Length).Trim();
                    var authHeaderBytes = Convert.FromBase64String(authHeader);
                    authHeader = Encoding.UTF7.GetString(authHeaderBytes);
                    var userName = authHeader.Split(':')[0];
                    var password = authHeader.Split(':')[1];
                    return UploadValues(resourceID, environment, startActivityId, wareWolfResumeUrl, userName, password);
                }
            }

            // Force the browser to pop up the login prompt
            Response.StatusCode = 401;
            Response.AppendHeader("WWW-Authenticate", "Basic");
            TempData["allowLogin"] = true;

            // This gets shown if they click "Cancel" to the login prompt
            Response.Write("You must log in to access this URL.");

            return Json("Success");
        }

        internal ActionResult UploadValues(string resourceID, string environment, string startActivityId, string wareWolfResumeUrl, string userName, string password)
        {
            using (var client = _clientFactory.New(userName, password))
            {
                var nameValueCollection = new NameValueCollection
                        {
                            { "resourceID",new StringBuilder(resourceID).ToString() },
                            { "startActivityId",new StringBuilder(startActivityId).ToString() },
                            { "environment",new StringBuilder(environment).ToString() },
                        };

                TempData.Remove("allowLogin");
                var returnValue = client.UploadValues(wareWolfResumeUrl, "POST", nameValueCollection);
                return Json(returnValue);
            }
        }

        AuditingViewModel CheckRequest(AuditingViewModel Request)
        {
            AuditingViewModel toReturn;
            if (Request != null)
            {
                toReturn = Request;
                toReturn.Server = toReturn.Server ?? "localhost";
                toReturn.Protocol = toReturn.Protocol ?? "http";
                toReturn.Port = toReturn.Port ?? "3142";
            }
            else
            {
                toReturn = new AuditingViewModel { Protocol = "http", Server = "localhost", Port = "3142" };
            }
            return toReturn;
        }
    }
}

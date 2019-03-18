#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
﻿using Dev2.Communication;
using Dev2.Runtime.Auditing;
using Dev2.Web2.Models.Auditing;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Text;
using System.Web.Http.Cors;
using System.Web.Mvc;

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
        // GET: Audit
        [AllowCrossSiteJson]
        public ActionResult Index()
        {
            var request = CheckRequest(null);
            var model = new Tuple<List<AuditLog>, AuditingViewModel>(new List<AuditLog>(), request);
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
            var model = new Tuple<List<AuditLog>, AuditingViewModel>(new List<AuditLog>(), request);
            return View(model);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult AuditList(string jsonData)
        {
            var serializer = new Dev2JsonSerializer();
            var request = CheckRequest(null);
            if (jsonData != null)
            {
                var logEntries = serializer.Deserialize<List<AuditLog>>(jsonData);
                var model = new Tuple<List<AuditLog>, AuditingViewModel>(logEntries, request);
                return PartialView("AuditList", model.Item1);
            }
            else
            {
                var model = new Tuple<List<AuditLog>, AuditingViewModel>(new List<AuditLog>(), request);
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

                    var credential = new NetworkCredential(userName, password);
                    using (var client = new WebClient
                    {
                        Credentials = credential
                    })
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
            }

            // Force the browser to pop up the login prompt
            Response.StatusCode = 401;
            Response.AppendHeader("WWW-Authenticate", "Basic");
            TempData["allowLogin"] = true;

            // This gets shown if they click "Cancel" to the login prompt
            Response.Write("You must log in to access this URL.");

            return Json("Success");
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

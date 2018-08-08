using Dev2.Communication;
using Dev2.Runtime.Auditing;
using Dev2.Web2.Models.Auditing;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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

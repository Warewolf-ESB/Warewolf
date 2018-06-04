﻿using Dev2.Common;
using Dev2.Web2.Models.ExecutionLogging;
using System;
using System.Collections.Generic;
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

    [EnableCors("*","*","*",PreflightMaxAge =10000,SupportsCredentials = true)]
    public class ExecutionLoggingController : Controller
    {
        // GET: ExecutionLogging
        [AllowCrossSiteJson]
        public ActionResult Index()
        {
            var request = CheckRequest(null);
            var model = new Tuple<List<LogEntry>, ExecutionLoggingRequestViewModel>(new List<LogEntry>(), request);
            return View(model);
        }

        [HttpGet]
        [AllowCrossSiteJson]
        public ActionResult Index(string server)
        {
            var request = CheckRequest(null);
            if (string.IsNullOrEmpty(server))
            {
                server = "localhost";
            }
            request.Server = server;
            var model = new Tuple<List<LogEntry>, ExecutionLoggingRequestViewModel>(new List<LogEntry>(), request);
            return View(model);
        }


        [HttpPost]
        [ValidateInput(false)]
        [AllowCrossSiteJson]
        public ActionResult ExecutionList(LogEntry[] logEntries)
        {
            var emptyModel = new List<LogEntry>();
            if (logEntries != null)
            {
                try
                {
                    var request = CheckRequest(null);
                    var model = new Tuple<LogEntry[], ExecutionLoggingRequestViewModel>(logEntries, request);

                    return PartialView("ExecutionList", model.Item1);
                }
                catch (Exception ex)
                {
                    return PartialView("ExecutionList", emptyModel);
                }
            }
            return PartialView("ExecutionList", emptyModel);
        }

        ExecutionLoggingRequestViewModel CheckRequest(ExecutionLoggingRequestViewModel Request)
        {
            ExecutionLoggingRequestViewModel toReturn;
            if (Request != null)
            {
                toReturn = Request;
                toReturn.Server = toReturn.Server ?? "localhost";
                toReturn.Protocol = toReturn.Protocol ?? "http";
                toReturn.Port = toReturn.Port ?? "3142";
            }
            else
            {
                toReturn = new ExecutionLoggingRequestViewModel { Protocol = "http", Server = "localhost", Port = "3142" };
            }
            return toReturn;
        }

        // GET: ExecutionLogging/Create
        public ActionResult Create() => View();

        // POST: ExecutionLogging/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return View();
            }
        }

        // GET: ExecutionLogging/Edit/5
        public ActionResult Edit(int id) => View();

        // POST: ExecutionLogging/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return View();
            }
        }

        // GET: ExecutionLogging/Delete/5
        public ActionResult Delete(int id) => View();

        // POST: ExecutionLogging/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return View();
            }
        }
    }
}

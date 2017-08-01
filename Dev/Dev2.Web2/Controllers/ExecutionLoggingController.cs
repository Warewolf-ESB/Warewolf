using Dev2.Common;
using Dev2.Web2.Models.ExecutionLogging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Dev2.Web2.Controllers
{
    public class ExecutionLoggingController : Controller
    {
       // GET: ExecutionLogging
        public ActionResult Index()
        {
            var request = CheckRequest(null);
            var model = new Tuple<List<LogEntry>, ExecutionLoggingRequestViewModel>(new List<LogEntry>(), request);
            return View(model);
        }

        [HttpGet]
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
                catch
                {
                    return PartialView("ExecutionList", emptyModel);
                }
            }
            return PartialView("ExecutionList", emptyModel);
        }

        private ExecutionLoggingRequestViewModel CheckRequest(ExecutionLoggingRequestViewModel Request)
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
        public ActionResult Create()
        {
            return View();
        }

        // POST: ExecutionLogging/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: ExecutionLogging/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: ExecutionLogging/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: ExecutionLogging/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: ExecutionLogging/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}

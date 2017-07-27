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

        [HttpPost]
        public ActionResult ExecutionList(string jsonData)
        {
            var logEntries = JsonConvert.DeserializeObject<List<LogEntry>>(jsonData.ToString());
            var request = CheckRequest(null);
            var model = new Tuple<List<LogEntry>, ExecutionLoggingRequestViewModel>(logEntries, request);

            return View("Index", model);
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

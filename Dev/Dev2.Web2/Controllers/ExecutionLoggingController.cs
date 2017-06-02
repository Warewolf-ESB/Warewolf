using Dev2.Common;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Dev2.Web2.Controllers
{
    public class ExecutionLoggingController : Controller
    {
        // GET: ExecutionLogging
        public ActionResult Index()
        {
            var model = new List<LogEntry>();

            var client = new RestClient("https://localhost:3143");
            client.Authenticator = new NtlmAuthenticator();
            var request = new RestRequest("services/GetLogDataService", Method.GET);
            request.UseDefaultCredentials = true;
            var response = client.Execute<List<LogEntry>>(request);
            model.AddRange(response.Data);

            return View(model);
        }

        // GET: ExecutionLogging/Details/5
        public ActionResult Details(int id)
        {
            return View();
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

using Dev2.Common;
using Dev2.Web2.Models.ExecutionLogging;
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
        public ActionResult Index(ExecutionLoggingRequestViewModel Request)
        {
            var request = CheckRequest(Request);

            var serverUrl = String.Format("{0}://{1}:{2}", request.Protocol, request.Server, request.Port);
            var client = new RestClient(serverUrl);
            client.Authenticator = new NtlmAuthenticator();
            var serverRequest = BuildRequest(request);
            var response = client.Execute<List<LogEntry>>(serverRequest);

            var data = response.Data ?? new List<LogEntry>();

            var model = new Tuple<List<LogEntry>, ExecutionLoggingRequestViewModel>(data, request);

            return View(model);
        }

        private RestRequest BuildRequest(ExecutionLoggingRequestViewModel Request)
        {
            var serverRequest = new RestRequest("services/GetLogDataService", Method.GET);
            serverRequest.UseDefaultCredentials = true;            
            //serverRequest.AddQueryParameter("StartDateTime", Request.StartDateTime.ToString());
            //serverRequest.AddQueryParameter("CompletedDateTime", Request.CompletedDateTime.ToString());
            serverRequest.AddQueryParameter("Status", Request.Status);
            serverRequest.AddQueryParameter("User", Request.User);
            serverRequest.AddQueryParameter("Url", Request.Url);
            serverRequest.AddQueryParameter("ExecutionId", Request.ExecutionId);
            serverRequest.AddQueryParameter("ExecutionTime", Request.ExecutionTime);

            return serverRequest;
        }

        private ExecutionLoggingRequestViewModel CheckRequest(ExecutionLoggingRequestViewModel Request)
        {
            ExecutionLoggingRequestViewModel toReturn;
            if (Request != null)
            {
                toReturn = Request;
                toReturn.Server = toReturn.Server ?? "localhost";
                toReturn.Protocol = toReturn.Protocol ?? "https";
                toReturn.Port = toReturn.Port ?? "3143";
            }
            else
            {
                toReturn = new ExecutionLoggingRequestViewModel { Protocol = "https", Server = "localhost", Port = "3143" };
            }
            return toReturn;
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

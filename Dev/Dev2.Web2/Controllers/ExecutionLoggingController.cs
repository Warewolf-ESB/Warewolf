using Dev2.Common;
using Dev2.Web2.Models.ExecutionLogging;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            serverRequest.AddQueryParameter("StartDateTime", Request.StartDateTime.ToString("yyyy-MM-dd HH:mm:ss,fff"));
            serverRequest.AddQueryParameter("CompletedDateTime", Request.CompletedDateTime.ToString("yyyy-MM-dd HH:mm:ss,fff"));
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
        public ActionResult Details(string executionId)
        {
            var serverUrl = String.Format("{0}://{1}:{2}", "https", "localhost", "3143");
            var client = new RestClient(serverUrl);
            client.Authenticator = new NtlmAuthenticator();

            var serverRequest = GetResult(executionId);

            var response = client.Execute<LogEntry>(serverRequest);
            var data = response.Data ?? new LogEntry();

            return PartialView("Details", data);
        }

        private RestRequest GetResult(string executionId)
        {
            var serverRequest = new RestRequest("services/GetServiceExecutionResult", Method.GET);
            serverRequest.UseDefaultCredentials = true;
            serverRequest.AddQueryParameter("ExecutionId", executionId);

            return serverRequest;
        }

        // GET: ExecutionLogging/Create
        public ActionResult Create()
        {
            return View();
        }

        public ActionResult OpenUrl(string url)
        {
            try
            {
                OpenInBrowser(new Uri(url));
                return View("Index");
            }
            catch (Exception ex)
            {
                return View("Index");
            }
        }

        public void OpenInBrowser(Uri url)
        {
            Process start = null;
            try
            {
                start = Process.Start(url.ToString());
            }
            catch (TimeoutException)
            {
                start?.Kill();
                start?.Dispose();
            }
            catch (System.Runtime.InteropServices.COMException)
            {
                start?.Kill();
                start?.Dispose();
            }
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

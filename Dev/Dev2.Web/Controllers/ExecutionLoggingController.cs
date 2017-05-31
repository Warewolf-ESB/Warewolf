using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Dev2.Common;
using Newtonsoft.Json;
using System.Net.Http;
using System.Security.Authentication;

namespace Dev2.Web.Controllers
{
    public class ExecutionLoggingController : Controller
    {
        // GET: ExecutionLogging
        public ActionResult Index()
        {
            var clientHandler = new HttpClientHandler();
            clientHandler.UseDefaultCredentials = true;
            clientHandler.PreAuthenticate = true;
            clientHandler.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls11 | SslProtocols.Tls;
            clientHandler.ClientCertificateOptions = ClientCertificateOption.Automatic;
            clientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;

            var model = new List<LogEntry>();
            using (HttpClient client = new HttpClient(clientHandler))
            {
                var authToken = HttpContext.Request.Headers["Authorization"].ToString();
                client.DefaultRequestHeaders.Add("Authorization",authToken);
                var data = client.GetAsync("https://localhost:3143/services/GetLogDataService").Result;
                var jsonResponse = data.Content.ReadAsStringAsync().Result;
                if (jsonResponse != null)
                {
                    model = JsonConvert.DeserializeObject<List<LogEntry>>(jsonResponse);
                }
            }
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
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
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
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
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
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
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
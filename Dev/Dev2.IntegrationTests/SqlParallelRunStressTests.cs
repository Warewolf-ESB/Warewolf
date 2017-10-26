using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Dev2.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Integration.Tests.Server_Refresh
{
    [TestClass]
    public class SqlParallelRunStressTests
    {
        [TestMethod]
        public void Run_a_Tests_to_Verify_ParallelSqlExecutionONAllDatabaseTools()
        {
            var url1 = "http://localhost:3142/secure/AllDatabaseTests.tests";
            List<Task> list = new List<Task>();

            Parallel.For(1, 10, a =>
            {
                var passRequest = ExececuteRequest(new Uri(url1));
                list.Add(passRequest);
                passRequest.ContinueWith((b) => 
                {
                    StringAssert.Contains(b.Result, "\"Result\": \"Passed\"");
                });
               
            });
            Task.WaitAll(list.ToArray());
           
        }

        private class PatientWebClient : WebClient
        {
            protected override WebRequest GetWebRequest(Uri uri)
            {
                WebRequest w = base.GetWebRequest(uri);
                // ReSharper disable once PossibleNullReferenceException
                w.Timeout = 20 * 60 * 1000;
                return w;
            }
        }       

        private Task<string> ExececuteRequest(Uri url)
        {
            try
            {
                var client = new PatientWebClient { Credentials = CredentialCache.DefaultNetworkCredentials };
                using (client)
                {
                    var task = Task.Run(() => client.DownloadString(url));
                    return task;
                }

            }
            catch (Exception e)
            {
                Dev2Logger.Error(e, "Warewolf Error");
                return new Task<string>((() => e.Message));
            }
        }        
    }
}

using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Dev2.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using Newtonsoft.Json.Linq;

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

        [TestMethod]
        public void TestUsingQlinkTrailerCreationWorkflow()
        {
            var url1 = "http://localhost:3142/secure/QLINK/WriteProcess/QlinkTrailerCreation.tests";
            List<Task> list = new List<Task>();


            /*  string substring = "";
              using (var stream = File.OpenRead("TestData\\testresult.json"))
              {
                  using (StreamReader streamReader = new StreamReader(stream))
                  {
                      substring = streamReader.ReadToEnd();
                  }
              }
              JArray obj = new JArray(substring);*/

            var passRequest = ExececuteRequest(new Uri(url1));
            list.Add(passRequest);
            passRequest.ContinueWith((b) =>
            {
                try
                {
                    var item1 = "{\"TestName\":\"Test2\",\"Result\":\"Passed\"}";
                    var item2 = "{\"TestName\":\"Test1\",\"Result\":\"Passed\"}";
                    var item3 = "{\"TestName\":\"Test3\",\"Result\":\"Passed\"}";
                    var item4 = "{\"TestName\":\"Test4\",\"Result\":\"Passed\"}";
                    var item5 = "{\"TestName\":\"Test5\",\"Result\":\"Passed\"}";
                    var stringResult = b.Result.Replace(Environment.NewLine, "").Replace(" ", "");

                    var hasTestResult = stringResult.Contains(item1.ToString());
                    Assert.IsTrue(hasTestResult);
                    var hasTestResult1 = stringResult.Contains(item2.ToString());
                    Assert.IsTrue(hasTestResult);
                    var hasTestResult2 = stringResult.Contains(item3.ToString());
                    Assert.IsTrue(hasTestResult);
                    var hasTestResult3 = stringResult.Contains(item4.ToString());
                    Assert.IsTrue(hasTestResult);
                    var hasTestResult4 = stringResult.Contains(item5.ToString());
                    Assert.IsTrue(hasTestResult);
                }
                catch (Exception ex)
                {
                    Assert.Fail(ex.StackTrace);
                }

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

using Dev2.Integration.Tests.Server_Refresh;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace Dev2.Integration.Tests
{
    [TestClass]
    public class ApisJson
    {
        [TestMethod]
        public void ApisDotJson_Public_HelloWorld()
        {
            var passRequest = SqlParallelRunStressTests.ExecuteRequest(new Uri("http://localhost:3142/public/Hello%20World/apis.json"));
            passRequest.ContinueWith((b) =>
            {
                StringAssert.Contains(b.Result, "\"Url\": \"localhost:3142/Hello World/apis.json\"");
            });
            Task.WaitAll(passRequest);
        }
    }
}
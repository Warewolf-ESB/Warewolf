
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Collections.Specialized;
using System.Net;
using Dev2.Runtime.WebServer;
using Dev2.Runtime.WebServer.Responses;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Runtime.WebServer.Responses
{
    [TestClass]
    public class StatusResponseWriterTests
    {
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("StatusResponseWriter_Write")]
        public void StatusResponseWriter_Write_WebServerContext_WritesTheStatus()
        {
            //------------Setup for test--------------------------
            const HttpStatusCode Expected = HttpStatusCode.PaymentRequired;

            string content;
            NameValueCollection boundVars;
            NameValueCollection queryStr;
            NameValueCollection headers;
            var request = WebServerRequestTests.CreateHttpRequest(out content, out boundVars, out queryStr, out headers);

            var context = new WebServerContext(request, boundVars);

            var responseWriter = new StatusResponseWriter(Expected);

            //------------Execute Test---------------------------
            responseWriter.Write(context);

            //------------Assert Results-------------------------
            Assert.AreEqual(Expected, context.ResponseMessage.StatusCode);
        }
    }
}

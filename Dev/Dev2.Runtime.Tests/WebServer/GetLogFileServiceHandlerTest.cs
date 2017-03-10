/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Specialized;
using Dev2.Runtime.WebServer;
using Dev2.Runtime.WebServer.Handlers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Tests.Runtime.WebServer
{
    /// <summary>
    /// Summary description for WebsiteResourceHandlerTest
    /// </summary>
    [TestClass]
    public class GetLogFileServiceHandlerTest
    {
        private NameValueCollection LocalQueryString => new NameValueCollection
        {
            { "Name", "the_name" },
            { "numLines", "5" },
            { "wid", "workflowid" },
            { "rid", "resourceid" }
        };

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void ProcessRequest_GiveQueryStrignHasNoKeys()
        {
            var communicationContext = new Mock<ICommunicationContext>();
            var request = new Mock<ICommunicationRequest>();
            request.Setup(communicationRequest => communicationRequest.QueryString).Returns(new NameValueCollection());
            communicationContext.Setup(context => context.Request).Returns(request.Object);
            //------------Setup for test-------------------------
            var handler = new GetLogFileServiceHandler();
            //------------Execute Test---------------------------
            handler.ProcessRequest(communicationContext.Object);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void ProcessRequest_GiveQueryStrignHasKeys()
        {
            var communicationContext = new Mock<ICommunicationContext>();
            var request = new Mock<ICommunicationRequest>();
            request.Setup(communicationRequest => communicationRequest.QueryString).Returns(LocalQueryString);
            communicationContext.Setup(context => context.Request).Returns(request.Object);
            //------------Setup for test-------------------------
            var handler = new GetLogFileServiceHandler();
            //------------Execute Test---------------------------
            handler.ProcessRequest(communicationContext.Object);
            //------------Assert Results-------------------------
        }
    }
}

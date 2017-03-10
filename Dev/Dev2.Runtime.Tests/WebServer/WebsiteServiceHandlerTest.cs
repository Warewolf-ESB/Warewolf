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
using System.IO;
using System.Security.Principal;
using System.Text;
using Dev2.Runtime.WebServer;
using Dev2.Runtime.WebServer.Handlers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Tests.Runtime.WebServer
{
    [TestClass]
    public class WebsiteServiceHandlerTest
    {
        private NameValueCollection LocalBoundVariables => new NameValueCollection
        {
            { "Bookmark", "the_bookmark" },
            { "Instanceid", "the_instanceid" },
            { "Website", "the_website" },
            { "Path", "the_path" },
            { "Name", "the_name" },
            { "Action", "the_action" },
            { "Servicename", "the_servicename" },
            { "wid", "the_workflowid" }
        };

        private NameValueCollection LocalQueryString => new NameValueCollection
        {
            { "wid", "the_workflowid" },
            { "dlid", "<DataList></DataList>" }
        };

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void ProcessRequest_GivenValidInputStream()
        {
            //------------Setup for test--------------------------
            var context = new Mock<ICommunicationContext>();
            var request = new Mock<ICommunicationRequest>();
            var inputStream = new Mock<Stream>();
            var user = new Mock<IPrincipal>();
            var identity = new Mock<IIdentity>();
            identity.Setup(id => id.Name).Returns("User1");
            identity.Setup(id => id.AuthenticationType).Returns("SuperUser");
            identity.Setup(id => id.IsAuthenticated).Returns(true);
            inputStream.Setup(stream => stream.CanRead).Returns(true);
            user.Setup(principal => principal.Identity).Returns(identity.Object);
            request.Setup(communicationRequest => communicationRequest.User).Returns(user.Object);
            request.Setup(communicationRequest => communicationRequest.BoundVariables).Returns(LocalBoundVariables);
            request.Setup(communicationRequest => communicationRequest.QueryString).Returns(LocalQueryString);
            request.Setup(communicationRequest => communicationRequest.InputStream).Returns(inputStream.Object);
            request.Setup(communicationRequest => communicationRequest.ContentEncoding).Returns(Encoding.Default);
            context.Setup(communicationContext => communicationContext.Request).Returns(request.Object);
            //------------Execute Test---------------------------
            var handler = new WebsiteServiceHandler();
            handler.ProcessRequest(context.Object);
            //------------Assert Results-------------------------
        }
    }
}

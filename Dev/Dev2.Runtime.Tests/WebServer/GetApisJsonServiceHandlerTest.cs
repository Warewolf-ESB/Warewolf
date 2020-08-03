/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Dev2.Common.Interfaces.Data;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.WebServer;
using Dev2.Runtime.WebServer.Handlers;
using Dev2.Runtime.WebServer.Responses;
using Dev2.Services.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Tests.Runtime.WebServer
{
    /// <summary>
    /// Summary description for WebsiteResourceHandlerTest
    /// </summary>
    [TestClass]
    [TestCategory("Runtime WebServer")]
    public class GetApisJsonServiceHandlerTest
    {
        NameValueCollection LocalQueryString => new NameValueCollection
        {
            { "Name", "the_name" },
            { "isPublic", "" },
            { "wid", "workflowid" },
            { "rid", "resourceid" }
        };

            [TestMethod]
            [Owner("Sanele Mthembu")]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ProcessRequest_GiveNullCommunicationContext_ThrowsException()
            {
                //------------Setup for test-------------------------
                var handler = new GetApisJsonServiceHandler();
                //------------Execute Test---------------------------
                handler.ProcessRequest(null);
                //------------Assert Results-------------------------
            }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void ProcessRequest_GiveNoPathAndNonPublicRequest()
        {
            var communicationContext = new Mock<ICommunicationContext>();
            var request = new Mock<ICommunicationRequest>();
            request.Setup(communicationRequest => communicationRequest.BoundVariables).Returns(LocalQueryString);
            communicationContext.Setup(context => context.Request).Returns(request.Object);
            //------------Setup for test-------------------------
            var auth = new Mock<IAuthorizationService>();
            var cat = new Mock<IResourceCatalog>();
            cat.Setup(catalog => catalog.GetResourceList(It.IsAny<Guid>())).Returns(new List<IResource>());
            var handler = new GetApisJsonServiceHandler(cat.Object, auth.Object);
            //------------Execute Test---------------------------
            handler.ProcessRequest(communicationContext.Object);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void ProcessRequest_GivePublicRequest()
        {
            var collection = new NameValueCollection
            {
                { "Name", "the_name" },
                { "isPublic", "true" },
                { "path", "" },
            };
            var communicationContext = new Mock<ICommunicationContext>();
            var request = new Mock<ICommunicationRequest>();
            request.Setup(communicationRequest => communicationRequest.BoundVariables).Returns(collection);
            communicationContext.Setup(context => context.Request).Returns(request.Object);
            //------------Setup for test-------------------------
            var handler = new GetApisJsonServiceHandler();
            //------------Execute Test---------------------------
            handler.ProcessRequest(communicationContext.Object);
            //------------Assert Results-------------------------
            communicationContext.Verify(context => context.Send(It.IsAny<IResponseWriter>()), Times.AtLeastOnce);
        }
    }
}

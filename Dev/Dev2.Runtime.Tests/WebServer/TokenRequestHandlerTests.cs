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
using System.Collections.Specialized;
using System.Net.Http;
using System.Security.Principal;
using System.Text;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Enums;
using Dev2.Interfaces;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.WebServer;
using Dev2.Runtime.WebServer.Handlers;
using Dev2.Runtime.WebServer.Responses;
using Dev2.Runtime.WebServer.TransferObjects;
using Dev2.Services.Security;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Warewolf.Data;
using Warewolf.Storage;

namespace Dev2.Tests.Runtime.WebServer
{
    [TestClass]
    [TestCategory("Runtime WebServer")]
    public class TokenRequestHandlerTests
    {
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(TokenRequestHandler))]
        [ExpectedException(typeof(NullReferenceException))]
        public void TokenRequestHandler_ProcessRequest_GiveNullCommunicationContext_ThrowsException()
        {
            //------------Setup for test-------------------------
            var handler = new TokenRequestHandler();
            //------------Execute Test---------------------------
            handler.ProcessRequest(null);
            //------------Assert Results-------------------------
        }
        [TestMethod]
        [TestCategory(nameof(TokenRequestHandler))]
        [Owner("Candice Daniel")]
        public void TokenRequestHandler_ProcessRequest_ReturnToken()
        {
            NameValueCollection localQueryString = new NameValueCollection();

            var communicationContext = new Mock<ICommunicationContext>();
            var request = new Mock<ICommunicationRequest>();
            request.Setup(communicationRequest => communicationRequest.BoundVariables).Returns(localQueryString);
            var qs = new NameValueCollection(1);
            qs.Add("wid", Guid.NewGuid().ToString());
            request.Setup(o => o.QueryString).Returns(qs);
            request.Setup(o => o.Uri).Returns(new Uri("http://localhost:4321/public/something.json"));

            var response = new Mock<ICommunicationResponse>();
            request.Setup(communicationResponse => communicationResponse.ContentType).Returns(ContentTypes.Json.ToString);

            communicationContext.Setup(context => context.Request).Returns(request.Object);
            communicationContext.Setup(context => context.Response).Returns(response.Object);
            //------------Setup for test-------------------------
            var handler = new TokenRequestHandler();
            //------------Execute Test---------------------------
            handler.ProcessRequest(communicationContext.Object);
            var result = communicationContext.Object.Response;
            //------------Assert Results-------------------------
            Assert.IsNotNull(result);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(TokenRequestHandler))]
        public void TokenRequestHandler_Return_EncryptedUserGroups_Token()
        {
            var principal = new Mock<IPrincipal>();
            GetExecutingUser(principal);

            var outerEnv = new ExecutionEnvironment();
            outerEnv.Assign("[[UserGroups().Name]]", "public", 0);
            outerEnv.Assign("[[UserGroups().Name]]", "whatever", 0);

            var dataObject = new Mock<IDSFDataObject>();
            dataObject.SetupAllProperties();
            dataObject.Setup(o => o.Environment).Returns(outerEnv);
            dataObject.Setup(o => o.RawPayload).Returns(new StringBuilder("<raw>SomeData</raw>"));
            dataObject.Setup(p => p.ExecutingUser).Returns(principal.Object);

            var resourceId = Guid.NewGuid();
            var resourceName = "loginAuth";

            var authorizationService = new Mock<IAuthorizationService>();
            authorizationService.Setup(service => service.IsAuthorized(It.IsAny<IPrincipal>(), It.IsAny<AuthorizationContext>(), It.IsAny<string>())).Returns(true);

            var mockResource = new Mock<IResource>();
            mockResource.SetupGet(resource1 => resource1.ResourceID).Returns(resourceId);
            mockResource.Setup(o => o.ResourceName).Returns(resourceName);
            mockResource.SetupGet(a => a.DataList).Returns(new StringBuilder("<DataList><UserGroups Description='' IsEditable='True' ColumnIODirection='Output'><Name Description='' IsEditable='True' ColumnIODirection='Output'>public</Name></UserGroups></DataList>"));

            var mockWarewolfResource = mockResource.As<IWarewolfResource>();
            mockWarewolfResource.Setup(o => o.ResourceID).Returns(resourceId);
            mockWarewolfResource.Setup(o => o.ResourceName).Returns(resourceName);
            mockWarewolfResource.SetupGet(a => a.DataList).Returns(new StringBuilder("<DataList><UserGroups Description='' IsEditable='True' ColumnIODirection='Output'><Name Description='' IsEditable='True' ColumnIODirection='Output'>public</Name></UserGroups></DataList>"));

            var resourceCatalog = new Mock<IResourceCatalog>();
            resourceCatalog.Setup(o => o.GetResource(It.IsAny<Guid>(), It.IsAny<string>())).Returns(mockResource.Object);

            var wRepo = new Mock<IWorkspaceRepository>();
            wRepo.SetupGet(repository => repository.ServerWorkspace).Returns(new Workspace(Guid.Empty));

            var doFactory = new TestTokenRequestDataObjectFactory(dataObject.Object);

            //---------------Assert Precondition----------------
            var handlerMock = new TokenRequestHandlerMock(resourceCatalog.Object, wRepo.Object, authorizationService.Object, doFactory);
            var headers = new Mock<NameValueCollection>();
            var webRequestTo = new WebRequestTO()
            {
                ServiceName = "loginAuth",
                WebServerUrl = "http://localhost:3142/public/loginAuth"
            };

            //---------------Execute Test ----------------------
            var responseWriter = handlerMock.CreateFromMock(webRequestTo, "loginAuth", Guid.Empty.ToString(), headers.Object, principal.Object);

            //------------Assert Results-------------------------
            Assert.IsNotNull(responseWriter);

            var response = new HttpResponseMessage();
            var mockMessageContext = new Mock<IResponseMessageContext>();
            mockMessageContext.Setup(o => o.ResponseMessage).Returns(response);
            responseWriter.Write(mockMessageContext.Object);
            mockMessageContext.Verify(o => o.ResponseMessage, Times.AtLeast(1));

            var responseText = response.Content.ReadAsStringAsync().Result;
            Assert.IsTrue(!string.IsNullOrWhiteSpace(responseText), "expected non empty token");
            var bodyJson = JsonConvert.DeserializeObject<JObject>(responseText);
            Assert.IsNotNull(bodyJson);

            var text = JwtManager.ValidateToken(bodyJson["token"].ToString());
            Assert.IsTrue(!string.IsNullOrWhiteSpace(responseText), "expected valid token that can be decrypted");
            var json = JsonConvert.DeserializeObject<JObject>(text);

            var group1 = json["UserGroups"][0]["Name"].ToString();
            var group2 = json["UserGroups"][1]["Name"].ToString();
            var hasBothGroups =
                (group1 == "public" && group2 == "whatever") || (group2 == "public" && group1 == "whatever");
            Assert.IsTrue(hasBothGroups, "groups not found in response token");

            Assert.AreEqual("application/json", response.Content.Headers.ContentType.MediaType, "application/json media type expected");
        }

        static void GetExecutingUser(Mock<IPrincipal> principal)
        {
            var identity = new Mock<IIdentity>();
            identity.Setup(p => p.Name).Returns("User1");
            principal.Setup(p => p.Identity).Returns(identity.Object);
        }
    }

    class TestTokenRequestDataObjectFactory : TokenRequestHandler.IDataObjectFactory
    {
        readonly IDSFDataObject _dataObject;

        public TestTokenRequestDataObjectFactory(IDSFDataObject dataObject)
        {
            _dataObject = dataObject;
        }

        public IDSFDataObject New(Guid workspaceGuid, IPrincipal user, string serviceName, WebRequestTO webRequest) => _dataObject;
    }

    class TokenRequestHandlerMock : TokenRequestHandler
    {
        public TokenRequestHandlerMock(IResourceCatalog resourceCatalog, IWorkspaceRepository workspaceRepository, IAuthorizationService authorizationService, IDataObjectFactory dataObjectFactory)
            : base(resourceCatalog, workspaceRepository, authorizationService, dataObjectFactory)
        {
        }

        public IResponseWriter CreateFromMock(WebRequestTO webRequest, string serviceName, string workspaceId, NameValueCollection headers, IPrincipal user = null)
        {
            return ExecuteWorkflow(webRequest, serviceName, workspaceId, headers, user);
        }

        public override void ProcessRequest(ICommunicationContext ctx)
        {
            throw new NotImplementedException();
        }
    }
}
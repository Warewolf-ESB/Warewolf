using System;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using System.Security.Principal;
using Dev2.Interfaces;
using Dev2.Runtime.WebServer;
using Dev2.Runtime.WebServer.Handlers;
using Dev2.Runtime.WebServer.Responses;
using Dev2.Runtime.WebServer.TransferObjects;
using Dev2.Services.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Storage;

namespace Dev2.Tests.Runtime.WebServer
{
    [TestClass]
    public class AbstractWebRequestHandlerTests
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void CreateForm_GivenValidArgs_ShouldreturnWriter()
        {
            //---------------Set up test pack-------------------
            var principal = new Mock<IPrincipal>();
            var authorizationService = new Mock<IAuthorizationService>();
            authorizationService.Setup(service => service.IsAuthorized(It.IsAny<AuthorizationContext>(), It.IsAny<string>())).Returns(true);
            var mock = new Mock<IDSFDataObject>();
            mock.SetupAllProperties();
            var env = new Mock<IExecutionEnvironment>();
            env.SetupAllProperties();
            mock.SetupGet(o => o.Environment).Returns(env.Object);
            var handlerMock = new AbstractWebRequestHandlerMock(mock.Object, authorizationService.Object);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var responseWriter = handlerMock.CreateFromMock(new WebRequestTO(), "Hello World", Guid.Empty.ToString(), new NameValueCollection(), principal.Object);
            //---------------Test Result -----------------------
            Assert.IsNotNull(responseWriter);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void CreateForm_GivenValidArgsContainsIsDebug_ShouldSetDataObjectAsDebug()
        {
            //---------------Set up test pack-------------------
            var principal = new Mock<IPrincipal>();
            var authorizationService = new Mock<IAuthorizationService>();
            authorizationService.Setup(service => service.IsAuthorized(It.IsAny<AuthorizationContext>(), It.IsAny<string>())).Returns(true);
            var dataObject = new Mock<IDSFDataObject>();
            dataObject.SetupAllProperties();
            var env = new Mock<IExecutionEnvironment>();
            env.SetupAllProperties();
            dataObject.SetupGet(o => o.Environment).Returns(env.Object);
            var handlerMock = new AbstractWebRequestHandlerMock(dataObject.Object, authorizationService.Object);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var webRequestTO = new WebRequestTO()
            {
                Variables = new NameValueCollection()
                {
                    {"IsDebug","true"}
                }
            };
            var responseWriter = handlerMock.CreateFromMock(webRequestTO, "Hello World", Guid.Empty.ToString(), new NameValueCollection(), principal.Object);
            //---------------Test Result -----------------------
            Assert.IsNotNull(responseWriter);
            dataObject.VerifySet(o => o.IsDebug = true, Times.Once);
            dataObject.VerifySet(o => o.IsDebugFromWeb = true, Times.Once);
            dataObject.VerifySet(o => o.ClientID = It.IsAny<Guid>(), Times.Once);
            dataObject.VerifySet(o => o.DebugSessionID = It.IsAny<Guid>(), Times.Once);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void BindRequestVariablesToDataObject_GivenHasBookMark_ShouldSetDataObjectBookmark()
        {
            //---------------Set up test pack-------------------
            var authorizationService = new Mock<IAuthorizationService>();
            authorizationService.Setup(service => service.IsAuthorized(It.IsAny<AuthorizationContext>(), It.IsAny<string>())).Returns(true);
            var dataObject = new Mock<IDSFDataObject>();
            dataObject.SetupAllProperties();
            var env = new Mock<IExecutionEnvironment>();
            env.SetupAllProperties();
            dataObject.SetupGet(o => o.Environment).Returns(env.Object);
            var dsfDataObject = dataObject.Object;
            var handlerMock = new AbstractWebRequestHandlerMock(dsfDataObject, authorizationService.Object);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var instanceID = Guid.NewGuid();
            var webRequestTO = new WebRequestTO()
            {
                Bookmark = "bookmark",
                InstanceID = instanceID.ToString(),
                ServiceName = "a"
            };
            handlerMock.BindRequestVariablesToDataObjectMock(webRequestTO, ref dsfDataObject);
            //---------------Test Result -----------------------
            dataObject.VerifySet(o => o.CurrentBookmarkName = "bookmark", Times.Once);
            dataObject.VerifySet(o => o.WorkflowInstanceId = instanceID, Times.Once);
            dataObject.VerifySet(o => o.ServiceName = "a", Times.Once);

        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GetPostData_GivenUrlWithPostData_ShouldReturnPostData()
        {
            //---------------Set up test pack-------------------
            var mock = new Mock<ICommunicationContext>();
            mock.SetupGet(context => context.Request.Uri)
                .Returns(new Uri("https://warewolf.atlassian.net/secure/RapidBoard.jspa?rapidView=6&view=planning&selectedIssue=WOLF-2416"));
            var dataObject = new Mock<IDSFDataObject>();
            var dsfDataObject = dataObject.Object;
            var authorizationService = new Mock<IAuthorizationService>();
            var handlerMock = new AbstractWebRequestHandlerMock(dsfDataObject, authorizationService.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(handlerMock);
            //---------------Execute Test ----------------------
            var postDataMock = handlerMock.GetPostDataMock(mock.Object);
            //---------------Test Result -----------------------
            Assert.AreEqual("rapidView=6&view=planning&selectedIssue=WOLF-2416", postDataMock);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Location_GivenReturnsCorrectly()
        {
            //---------------Set up test pack-------------------
            var authorizationService = new Mock<IAuthorizationService>();
            authorizationService.Setup(service => service.IsAuthorized(It.IsAny<AuthorizationContext>(), It.IsAny<string>())).Returns(true);
            var dataObject = new Mock<IDSFDataObject>();
            dataObject.SetupAllProperties();
            var env = new Mock<IExecutionEnvironment>();
            env.SetupAllProperties();
            dataObject.SetupGet(o => o.Environment).Returns(env.Object);
            var handlerMock = new AbstractWebRequestHandlerMock(dataObject.Object, authorizationService.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(handlerMock);
            //---------------Execute Test ----------------------
            var location = handlerMock.Location;
            //---------------Test Result -----------------------
            var locationCurrent = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Assert.AreEqual(locationCurrent, location);
        }
    }

    internal class AbstractWebRequestHandlerMock : AbstractWebRequestHandler
    {
        public AbstractWebRequestHandlerMock(IDSFDataObject dataObject, IAuthorizationService service)
        {
            TestDataObject = dataObject;
            TestServerAuthorizationService = service;
        }

        #region Overrides of AbstractWebRequestHandler
        // protected static IResponseWriter CreateForm(WebRequestTO webRequest, string serviceName, string workspaceId, NameValueCollection headers, IPrincipal user = null)
        public IResponseWriter CreateFromMock(WebRequestTO webRequest, string serviceName, string workspaceId, NameValueCollection headers, IPrincipal user = null)
        {
            return CreateForm(webRequest, serviceName, workspaceId, headers, user);
        }
        //protected static string GetPostData(ICommunicationContext ctx)
        public string GetPostDataMock(ICommunicationContext ctx)
        {
            return GetPostData(ctx);
        }
        //BindRequestVariablesToDataObject(WebRequestTO request, ref IDSFDataObject dataObject)
        public void BindRequestVariablesToDataObjectMock(WebRequestTO request, ref IDSFDataObject dataObject)
        {
            BindRequestVariablesToDataObject(request, ref dataObject);
        }
        public override void ProcessRequest(ICommunicationContext ctx)
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }
}

using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using Dev2.Common;
using Dev2.Interfaces;
using Dev2.Runtime.Interfaces;
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
        private NameValueCollection LocalBoundVariables => new NameValueCollection
        {
            { "Bookmark", "the_bookmark" },
            { "Instanceid", "the_instanceid" },
            { "Website", "the_website" },
            { "Path", "the_path" },
            { "Name", "the_name" },
            { "Action", "the_action" },
            { "Servicename", "the_servicename" }
        };

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void CreateFormGivenValidArgsShouldreturnWriter()
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
            mock.SetupGet(o => o.RawPayload).Returns(new StringBuilder("<raw>SomeData</raw>"));
            var resourceCatalog = new Mock<IResourceCatalog>();
            var handlerMock = new AbstractWebRequestHandlerMock(mock.Object, authorizationService.Object, resourceCatalog.Object);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------            
            var responseWriter = handlerMock.CreateFromMock(new WebRequestTO(), "Hello World", Guid.Empty.ToString(), new NameValueCollection(), principal.Object);
            //---------------Test Result -----------------------
            Assert.IsNotNull(responseWriter);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void CreateFormGivenValidArgsContainsIsDebugShouldSetDataObjectAsDebug()
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
            dataObject.SetupGet(o => o.RawPayload).Returns(new StringBuilder("<raw>SomeData</raw>"));
            var resourceCatalog = new Mock<IResourceCatalog>();
            var handlerMock = new AbstractWebRequestHandlerMock(dataObject.Object, authorizationService.Object, resourceCatalog.Object);
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
        public void BindRequestVariablesToDataObjectGivenHasBookMarkShouldSetDataObjectBookmark()
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
            var resourceCatalog = new Mock<IResourceCatalog>();
            var handlerMock = new AbstractWebRequestHandlerMock(dsfDataObject, authorizationService.Object, resourceCatalog.Object);
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
        public void GetPostDataGivenUrlWithGetDataShouldReturnPostData()
        {
            //---------------Set up test pack-------------------
            var communicationContext = new Mock<ICommunicationContext>();            
            const string UriString = "https://warewolf.atlassian.net/secure/RapidBoard.jspa?rapidView=6&view=planning&selectedIssue=WOLF-2416";
            communicationContext.SetupGet(context => context.Request.Uri)
                .Returns(new Uri(UriString));
            communicationContext.Setup(context => context.Request.Method).Returns("GET");
            communicationContext.Setup(context => context.Request.ContentEncoding).Returns(Encoding.Default);
            var inputStream = new Mock<Stream>();
            inputStream.Setup(stream => stream.CanRead).Returns(true);
            communicationContext.Setup(context => context.Request.InputStream).Returns(inputStream.Object);
            communicationContext.Setup(context => context.Request.BoundVariables).Returns(new NameValueCollection());
            communicationContext.Setup(context => context.Request.QueryString).Returns(new NameValueCollection());
            var dataObject = new Mock<IDSFDataObject>();
            var dsfDataObject = dataObject.Object;
            var authorizationService = new Mock<IAuthorizationService>();
            var resourceCatalog = new Mock<IResourceCatalog>();
            var handlerMock = new AbstractWebRequestHandlerMock(dsfDataObject, authorizationService.Object, resourceCatalog.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(handlerMock);
            //---------------Execute Test ----------------------
            var postDataMock = handlerMock.GetPostDataMock(communicationContext.Object);
            //---------------Test Result -----------------------
            Assert.AreEqual("rapidView=6&view=planning&selectedIssue=WOLF-2416", postDataMock);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GetPostDataGivenUrlWithPostDataShouldReturnPostData()
        {
            //---------------Set up test pack-------------------
            var communicationContext = new Mock<ICommunicationContext>();
            communicationContext.SetupGet(context => context.Request.Uri)
                .Returns(new Uri("https://warewolf.atlassian.net/secure/RapidBoard.jspa?rapidView=6&view=planning&selectedIssue=WOLF-2416"));
            communicationContext.Setup(context => context.Request.Method).Returns("POST");
            communicationContext.Setup(context => context.Request.ContentEncoding).Returns(Encoding.Default);
            var inputStream = new Mock<Stream>();
            inputStream.Setup(stream => stream.CanRead).Returns(true);
            communicationContext.Setup(context => context.Request.InputStream).Returns(inputStream.Object);
            communicationContext.Setup(context => context.Request.BoundVariables).Returns(new NameValueCollection());
            communicationContext.Setup(context => context.Request.QueryString).Returns(new NameValueCollection());
            var dataObject = new Mock<IDSFDataObject>();
            var dsfDataObject = dataObject.Object;
            var authorizationService = new Mock<IAuthorizationService>();
            var resourceCatalog = new Mock<IResourceCatalog>();
            var handlerMock = new AbstractWebRequestHandlerMock(dsfDataObject, authorizationService.Object, resourceCatalog.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(handlerMock);
            //---------------Execute Test ----------------------
            var postDataMock = handlerMock.GetPostDataMock(communicationContext.Object);
            //---------------Test Result -----------------------
            Assert.AreEqual("rapidView=6&view=planning&selectedIssue=WOLF-2416", postDataMock);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void LocationGivenReturnsCorrectly()
        {
            //---------------Set up test pack-------------------
            var authorizationService = new Mock<IAuthorizationService>();
            authorizationService.Setup(service => service.IsAuthorized(It.IsAny<AuthorizationContext>(), It.IsAny<string>())).Returns(true);
            var dataObject = new Mock<IDSFDataObject>();
            dataObject.SetupAllProperties();
            var env = new Mock<IExecutionEnvironment>();
            env.SetupAllProperties();
            dataObject.SetupGet(o => o.Environment).Returns(env.Object);
            var resourceCatalog = new Mock<IResourceCatalog>();
            var handlerMock = new AbstractWebRequestHandlerMock(dataObject.Object, authorizationService.Object, resourceCatalog.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(handlerMock);
            //---------------Execute Test ----------------------
            var location = handlerMock.Location;
            //---------------Test Result -----------------------
            var locationCurrent = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Assert.AreEqual(locationCurrent, location);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void RemoteInove_GivenServerInvoke_ShouldSetRemoteIdOnTheDataObjectAndRemoteInvokeTo_True()
        {
            const string Someremoteid = "SomeRemoteServerId";
            var authorizationService = new Mock<IAuthorizationService>();
            authorizationService.Setup(service => service.IsAuthorized(It.IsAny<AuthorizationContext>(), It.IsAny<string>())).Returns(true);
            var dataObject = new Mock<IDSFDataObject>();
            dataObject.SetupAllProperties();
            var env = new Mock<IExecutionEnvironment>();
            env.SetupAllProperties();
            dataObject.SetupGet(o => o.Environment).Returns(env.Object);
            dataObject.SetupGet(o => o.RawPayload).Returns(new StringBuilder("<raw>SomeData</raw>"));
            var resourceCatalog = new Mock<IResourceCatalog>();
            //------------Setup for test-------------------------
            var handlerMock = new AbstractWebRequestHandlerMock(dataObject.Object, authorizationService.Object, resourceCatalog.Object);
            //------------Execute Test---------------------------
            var mock = new Mock<NameValueCollection>();
            mock.Setup(collection => collection.Get(HttpRequestHeader.Cookie.ToString())).Returns(GlobalConstants.RemoteServerInvoke);
            mock.Setup(collection => collection.Get(HttpRequestHeader.From.ToString())).Returns(Someremoteid);
            handlerMock.RemoteInvokeMock(mock.Object, dataObject.Object);
            //------------Assert Results-------------------------
            Assert.IsTrue(dataObject.Object.RemoteInvoke);
            Assert.AreEqual(Someremoteid, dataObject.Object.RemoteInvokerID);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void RemoteInvoke_GivenRemoteDebugInvoke_ShouldSetRemoteIdOnTheDataObjectAndRemoteInvokeTo_True()
        {
            const string Someremoteid = "SomeRemoteDebugServerId";
            var authorizationService = new Mock<IAuthorizationService>();
            authorizationService.Setup(service => service.IsAuthorized(It.IsAny<AuthorizationContext>(), It.IsAny<string>())).Returns(true);
            var dataObject = new Mock<IDSFDataObject>();
            dataObject.SetupAllProperties();
            var env = new Mock<IExecutionEnvironment>();
            env.SetupAllProperties();
            dataObject.SetupGet(o => o.Environment).Returns(env.Object);
            dataObject.SetupGet(o => o.RawPayload).Returns(new StringBuilder("<raw>SomeData</raw>"));
            var resourceCatalog = new Mock<IResourceCatalog>();
            //------------Setup for test-------------------------
            var handlerMock = new AbstractWebRequestHandlerMock(dataObject.Object, authorizationService.Object, resourceCatalog.Object);
            //------------Execute Test---------------------------
            var mock = new Mock<NameValueCollection>();
            mock.Setup(collection => collection.Get(HttpRequestHeader.Cookie.ToString())).Returns(GlobalConstants.RemoteDebugServerInvoke);
            mock.Setup(collection => collection.Get(HttpRequestHeader.From.ToString())).Returns(Someremoteid);
            handlerMock.RemoteInvokeMock(mock.Object, dataObject.Object);
            //------------Assert Results-------------------------
            Assert.IsTrue(dataObject.Object.RemoteNonDebugInvoke);
            Assert.IsFalse(dataObject.Object.RemoteInvoke);
            Assert.AreEqual(Someremoteid, dataObject.Object.RemoteInvokerID);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GetBookmark_GivenBoundVariables_ShouldReturnBookmark()
        {
            const string ExpectedResult = "the_bookmark";
            var communicationContext = new Mock<ICommunicationContext>();
            communicationContext.Setup(context => context.Request.BoundVariables).Returns(LocalBoundVariables);
            //------------Setup for test-------------------------
            var handlerMock = new AbstractWebRequestHandlerMock();
            //------------Execute Test---------------------------
            var bookmark = handlerMock.GetBookmarkMock(communicationContext.Object);
            //------------Assert Results-------------------------
            Assert.IsNotNull(bookmark);
            Assert.AreEqual(ExpectedResult, bookmark);
        }
        

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GetClassName_GivenBoundVariables_ShouldReturnName()
        {
            const string ExpectedResult = "the_name";
            var communicationContext = new Mock<ICommunicationContext>();
            communicationContext.Setup(context => context.Request.BoundVariables).Returns(LocalBoundVariables);
            //------------Setup for test-------------------------
            var handlerMock = new AbstractWebRequestHandlerMock();
            //------------Execute Test---------------------------
            var className = handlerMock.GetClassNameMock(communicationContext.Object);
            //------------Assert Results-------------------------
            Assert.IsNotNull(className);
            Assert.AreEqual(ExpectedResult, className);
        }
        

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GetMethodName_GivenBoundVariables_ShouldReturnMethodName()
        {
            const string ExpectedResult = "the_action";
            var communicationContext = new Mock<ICommunicationContext>();
            communicationContext.Setup(context => context.Request.BoundVariables).Returns(LocalBoundVariables);
            //------------Setup for test-------------------------
            var handlerMock = new AbstractWebRequestHandlerMock();
            //------------Execute Test---------------------------
            var methodName = handlerMock.GetMethodNameMock(communicationContext.Object);
            //------------Assert Results-------------------------
            Assert.IsNotNull(methodName);
            Assert.AreEqual(ExpectedResult, methodName);
        }
        

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GetPath_GivenBoundVariables_ShouldReturnPath()
        {
            const string ExpectedResult = "the_path";
            var communicationContext = new Mock<ICommunicationContext>();
            communicationContext.Setup(context => context.Request.BoundVariables).Returns(LocalBoundVariables);
            //------------Setup for test-------------------------
            var handlerMock = new AbstractWebRequestHandlerMock();
            //------------Execute Test---------------------------
            var path = handlerMock.GetPathMock(communicationContext.Object);
            //------------Assert Results-------------------------
            Assert.IsNotNull(path);
            Assert.AreEqual(ExpectedResult, path);
        }
        

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GetWebsite_GivenBoundVariables_ShouldReturnWebsite()
        {
            const string ExpectedResult = "the_website";
            var communicationContext = new Mock<ICommunicationContext>();
            communicationContext.Setup(context => context.Request.BoundVariables).Returns(LocalBoundVariables);
            //------------Setup for test-------------------------
            var handlerMock = new AbstractWebRequestHandlerMock();
            //------------Execute Test---------------------------
            var website = handlerMock.GetWebsiteMock(communicationContext.Object);
            //------------Assert Results-------------------------
            Assert.IsNotNull(website);
            Assert.AreEqual(ExpectedResult, website);
        }
        

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GetInstanceID_GivenBoundVariables_ShouldReturnInstanceId()
        {
            const string ExpectedResult = "the_instanceid";
            var communicationContext = new Mock<ICommunicationContext>();
            communicationContext.Setup(context => context.Request.BoundVariables).Returns(LocalBoundVariables);
            //------------Setup for test-------------------------
            var handlerMock = new AbstractWebRequestHandlerMock();
            //------------Execute Test---------------------------
            var instanceId = handlerMock.GetInstanceIDMock(communicationContext.Object);
            //------------Assert Results-------------------------
            Assert.IsNotNull(instanceId);
            Assert.AreEqual(ExpectedResult, instanceId);
        }
        

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GetServiceName_GivenBoundVariables_ShouldReturnServiceName()
        {
            const string ExpectedResult = "the_servicename";
            var communicationContext = new Mock<ICommunicationContext>();
            communicationContext.Setup(context => context.Request.BoundVariables).Returns(LocalBoundVariables);
            //------------Setup for test-------------------------
            var handlerMock = new AbstractWebRequestHandlerMock();
            //------------Execute Test---------------------------
            var serviceName = handlerMock.GetServiceNameMock(communicationContext.Object);
            //------------Assert Results-------------------------
            Assert.IsNotNull(serviceName);
            Assert.AreEqual(ExpectedResult, serviceName);
        }
        
        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GetWorkspaceID_GivenQueryString_ShouldReturnId()
        {
            const string ExpectedResult = "the_wid";
            var communicationContext = new Mock<ICommunicationContext>();
            communicationContext.Setup(context => context.Request.QueryString).Returns(new NameValueCollection { {"wid", "the_wid"} });
            var handlerMock = new AbstractWebRequestHandlerMock();
            //------------Execute Test---------------------------
            var workspaceID = handlerMock.GetWorkspaceIDMock(communicationContext.Object);
            //------------Assert Results-------------------------
            Assert.IsNotNull(workspaceID);
            Assert.AreEqual(ExpectedResult, workspaceID);
        }
        
        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GetDataListID_GivenQueryString_ShouldReturnDatalsi()
        {
            const string ExpectedResult = "the_datalist";
            var communicationContext = new Mock<ICommunicationContext>();
            communicationContext.Setup(context => context.Request.QueryString).Returns(new NameValueCollection { { "dlid", "the_datalist"} });
            //------------Setup for test-------------------------
            var handlerMock = new AbstractWebRequestHandlerMock();
            //------------Execute Test---------------------------
            var dataListID = handlerMock.GetDataListIDMock(communicationContext.Object);
            //------------Assert Results-------------------------
            Assert.IsNotNull(dataListID);
            Assert.AreEqual(ExpectedResult, dataListID);
        }
        
    }

    internal class AbstractWebRequestHandlerMock : AbstractWebRequestHandler
    {
        public AbstractWebRequestHandlerMock(IDSFDataObject dataObject, IAuthorizationService service, IResourceCatalog catalog)
            :base(catalog)
        {
            TestDataObject = dataObject;
            TestServerAuthorizationService = service;
        }
        public AbstractWebRequestHandlerMock()         
        {         
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
            throw new NotImplementedException();
        }
        public void RemoteInvokeMock(NameValueCollection headers, IDSFDataObject dataObject)
        {
            RemoteInvoke(headers, dataObject);
        }
        public string GetServiceNameMock(ICommunicationContext ctx)
        {
            return GetServiceName(ctx);
        }

        public string GetWorkspaceIDMock(ICommunicationContext ctx)
        {
            return GetWorkspaceID(ctx);
        }

        public string GetDataListIDMock(ICommunicationContext ctx)
        {
            return GetDataListID(ctx);
        }

        public string GetBookmarkMock(ICommunicationContext ctx)
        {
            return GetBookmark(ctx);
        }

        public string GetInstanceIDMock(ICommunicationContext ctx)
        {
            return GetInstanceID(ctx);
        }

        public string GetWebsiteMock(ICommunicationContext ctx)
        {
            return GetWebsite(ctx);
        }

        public string GetPathMock(ICommunicationContext ctx)
        {
            return GetPath(ctx);
        }

        public string GetClassNameMock(ICommunicationContext ctx)
        {
            return GetClassName(ctx);
        }

        public string GetMethodNameMock(ICommunicationContext ctx)
        {            
            return GetMethodName(ctx);
        }
        
        #endregion
    }
}

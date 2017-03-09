using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Security.Principal;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.Common.ExtMethods;
using Dev2.Common.Interfaces;
using Dev2.Data;
using Dev2.Interfaces;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.WebServer;
using Dev2.Runtime.WebServer.Handlers;
using Dev2.Runtime.WebServer.Responses;
using Dev2.Runtime.WebServer.TransferObjects;
using Dev2.Services.Security;
using Dev2.Web;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
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
            { "Servicename", "the_servicename" },
            { "wid", "the_workflowid" }
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
            var testCatalog = new Mock<ITestCatalog>();
            var handlerMock = new AbstractWebRequestHandlerMock(mock.Object, authorizationService.Object, resourceCatalog.Object, testCatalog.Object);
            var webRequestTO = new WebRequestTO
            {
                WebServerUrl = "http://localhost:3142/secure/Hello%20World.tests/Blank%20Input"
            };
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------            
            var responseWriter = handlerMock.CreateFromMock(webRequestTO, "Hello World", Guid.Empty.ToString(), new NameValueCollection(), principal.Object);
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
            var testCatalog = new Mock<ITestCatalog>();
            var handlerMock = new AbstractWebRequestHandlerMock(dataObject.Object, authorizationService.Object, resourceCatalog.Object, testCatalog.Object);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var webRequestTO = new WebRequestTO()
            {
                Variables = new NameValueCollection()
                {
                    {"IsDebug","true"}
                }
            };
            var responseWriter = handlerMock.CreateFromMock(webRequestTO, "Hello World", string.Empty, new NameValueCollection(), principal.Object);
            //---------------Test Result -----------------------
            Assert.IsNotNull(responseWriter);
            dataObject.VerifySet(o => o.IsDebug = true, Times.Once);
            dataObject.VerifySet(o => o.IsDebugFromWeb = true, Times.Once);
            dataObject.VerifySet(o => o.ClientID = It.IsAny<Guid>(), Times.Once);
            dataObject.VerifySet(o => o.DebugSessionID = It.IsAny<Guid>(), Times.Once);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void CreateFormGivenEmitionTypeTESTShould()
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
            dataObject.SetupGet(o => o.ReturnType).Returns(EmitionTypes.TEST);
            dataObject.SetupGet(o => o.TestName).Returns("*");
            dataObject.Setup(o => o.Clone()).Returns(dataObject.Object);
            var resourceCatalog = new Mock<IResourceCatalog>();
            var testCatalog = new Mock<ITestCatalog>();
            var serviceTestModelTO = new Mock<IServiceTestModelTO>();
            serviceTestModelTO.Setup(to => to.Enabled).Returns(true);
            serviceTestModelTO.Setup(to => to.TestName).Returns("Test1");
            var tests = new List<IServiceTestModelTO>
            {
                serviceTestModelTO.Object
            };
            testCatalog.Setup(catalog => catalog.Fetch(Guid.Empty)).Returns(tests);
            var handlerMock = new AbstractWebRequestHandlerMock(dataObject.Object, authorizationService.Object, resourceCatalog.Object, testCatalog.Object);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var webRequestTO = new WebRequestTO()
            {
                Variables = new NameValueCollection()
                {
                    {"IsDebug","true"}
                }
            };
            var responseWriter = handlerMock.CreateFromMock(webRequestTO, "Hello World", null, new NameValueCollection(), principal.Object);
            //---------------Test Result -----------------------
            Assert.IsNotNull(responseWriter);
            testCatalog.Verify(o => o.Fetch(Guid.Empty), Times.Once);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void CreateFormGivenEmitionTypeTESTAndIsRunAllTestsRequestTrue_Should()
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
            dataObject.SetupGet(o => o.ReturnType).Returns(EmitionTypes.TEST);
            dataObject.SetupGet(o => o.TestName).Returns("*");
            dataObject.Setup(o => o.Clone()).Returns(dataObject.Object);
            var resourceCatalog = new Mock<IResourceCatalog>();
            var testCatalog = new Mock<ITestCatalog>();
            var serviceTestModelTO = new Mock<IServiceTestModelTO>();
            serviceTestModelTO.Setup(to => to.Enabled).Returns(true);
            serviceTestModelTO.Setup(to => to.TestName).Returns("Test1");
            var tests = new List<IServiceTestModelTO>
            {
                serviceTestModelTO.Object
            };
            testCatalog.Setup(catalog => catalog.Fetch(Guid.Empty)).Returns(tests);
            var handlerMock = new AbstractWebRequestHandlerMock(dataObject.Object, authorizationService.Object, resourceCatalog.Object, testCatalog.Object);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var webRequestTO = new WebRequestTO()
            {
                Variables = new NameValueCollection()
                {
                    {"IsDebug","true"}
                }
            };
            var responseWriter = handlerMock.CreateFromMock(webRequestTO, "Hello World", null, new NameValueCollection(), principal.Object);
            //---------------Test Result -----------------------
            Assert.IsNotNull(responseWriter);
            testCatalog.Verify(o => o.Fetch(Guid.Empty), Times.Once);
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
            var testCatalog = new Mock<ITestCatalog>();
            var handlerMock = new AbstractWebRequestHandlerMock(dataObject.Object, authorizationService.Object, resourceCatalog.Object, testCatalog.Object);
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
        public void GetPostDataGivenUrlWithGetJsonDataShouldReturnPostData()
        {
            //---------------Set up test pack-------------------
            var communicationContext = new Mock<ICommunicationContext>();
            string payLoad = this.SerializeToJsonString(new DefaultSerializationBinder());
            string uriString = $"https://warewolf.atlassian.net/secure/RapidBoard.jspa?{payLoad}";
            communicationContext.SetupGet(context => context.Request.Uri)
                .Returns(new Uri(uriString));
            communicationContext.Setup(context => context.Request.Method).Returns("GET");
            communicationContext.Setup(context => context.Request.ContentEncoding).Returns(Encoding.Default);
            var inputStream = new Mock<Stream>();
            inputStream.Setup(stream => stream.CanRead).Returns(true);
            communicationContext.Setup(context => context.Request.InputStream).Returns(inputStream.Object);
            communicationContext.Setup(context => context.Request.BoundVariables).Returns(new NameValueCollection());
            communicationContext.Setup(context => context.Request.QueryString).Returns(new NameValueCollection());
            var dataObject = new Mock<IDSFDataObject>();
            var authorizationService = new Mock<IAuthorizationService>();
            var resourceCatalog = new Mock<IResourceCatalog>();
            var testCatalog = new Mock<ITestCatalog>();
            var handlerMock = new AbstractWebRequestHandlerMock(dataObject.Object, authorizationService.Object, resourceCatalog.Object, testCatalog.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(handlerMock);
            //---------------Execute Test ----------------------
            var postDataMock = handlerMock.GetPostDataMock(communicationContext.Object);
            //---------------Test Result -----------------------
            Assert.AreEqual(payLoad, postDataMock);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GetPostDataGivenUrlWithJsonPostDataShouldReturnPostData()
        {
            //---------------Set up test pack-------------------
            var communicationContext = new Mock<ICommunicationContext>();
            string payLoad = this.SerializeToJsonString(new DefaultSerializationBinder());
            string uriString = $"https://warewolf.atlassian.net/secure/RapidBoard.jspa?{payLoad}";
            communicationContext.SetupGet(context => context.Request.Uri)
                .Returns(new Uri(uriString));
            communicationContext.Setup(context => context.Request.Method).Returns("POST");
            communicationContext.Setup(context => context.Request.ContentEncoding).Returns(Encoding.Default);
            var inputStream = new Mock<Stream>();
            inputStream.Setup(stream => stream.CanRead).Returns(true);
            communicationContext.Setup(context => context.Request.InputStream).Returns(inputStream.Object);
            communicationContext.Setup(context => context.Request.BoundVariables).Returns(new NameValueCollection());
            communicationContext.Setup(context => context.Request.QueryString).Returns(new NameValueCollection());
            var dataObject = new Mock<IDSFDataObject>();
            var authorizationService = new Mock<IAuthorizationService>();
            var resourceCatalog = new Mock<IResourceCatalog>();
            var testCatalog = new Mock<ITestCatalog>();
            var handlerMock = new AbstractWebRequestHandlerMock(dataObject.Object, authorizationService.Object, resourceCatalog.Object, testCatalog.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(handlerMock);
            //---------------Execute Test ----------------------
            var postDataMock = handlerMock.GetPostDataMock(communicationContext.Object);
            //---------------Test Result -----------------------
            Assert.AreEqual(payLoad, postDataMock);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GetPostData_GivenPostDataInContext_ShouldReturnEmpty()
        {
            //---------------Set up test pack-------------------
            var communicationContext = new Mock<ICommunicationContext>();
            const string UriString = "https://warewolf.atlassian.net/secure/RapidBoard.jspa";
            communicationContext.SetupGet(context => context.Request.Uri).Returns(new Uri(UriString));
            communicationContext.Setup(context => context.Request.Method).Returns("POST");
            communicationContext.Setup(context => context.Request.ContentEncoding).Returns(Encoding.Default);
            var stringInMemoryStream = new MemoryStream(Encoding.Default.GetBytes("PostData"));
            communicationContext.Setup(context => context.Request.InputStream).Returns(stringInMemoryStream);
            communicationContext.Setup(context => context.Request.BoundVariables).Returns(new NameValueCollection());
            communicationContext.Setup(context => context.Request.QueryString).Returns(new NameValueCollection());
            var dataObject = new Mock<IDSFDataObject>();
            var authorizationService = new Mock<IAuthorizationService>();
            var resourceCatalog = new Mock<IResourceCatalog>();
            var testCatalog = new Mock<ITestCatalog>();
            var handlerMock = new AbstractWebRequestHandlerMock(dataObject.Object, authorizationService.Object, resourceCatalog.Object, testCatalog.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(handlerMock);
            //---------------Execute Test ----------------------
            var postDataMock = handlerMock.GetPostDataMock(communicationContext.Object);
            //---------------Test Result -----------------------
            Assert.AreEqual("", postDataMock);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GetPostData_GivenPostJsonDataInContext_ShouldReturnJsonData()
        {
            //---------------Set up test pack-------------------
            var communicationContext = new Mock<ICommunicationContext>();
            const string UriString = "https://warewolf.atlassian.net/secure/RapidBoard.jspa";
            communicationContext.SetupGet(context => context.Request.Uri).Returns(new Uri(UriString));
            communicationContext.Setup(context => context.Request.Method).Returns("POST");
            communicationContext.Setup(context => context.Request.ContentEncoding).Returns(Encoding.Default);
            var data = this.SerializeToJsonString(new DefaultSerializationBinder());
            var stringInMemoryStream = new MemoryStream(Encoding.Default.GetBytes(data));
            communicationContext.Setup(context => context.Request.InputStream).Returns(stringInMemoryStream);
            communicationContext.Setup(context => context.Request.BoundVariables).Returns(new NameValueCollection());
            communicationContext.Setup(context => context.Request.QueryString).Returns(new NameValueCollection());
            var dataObject = new Mock<IDSFDataObject>();
            var authorizationService = new Mock<IAuthorizationService>();
            var resourceCatalog = new Mock<IResourceCatalog>();
            var testCatalog = new Mock<ITestCatalog>();
            var handlerMock = new AbstractWebRequestHandlerMock(dataObject.Object, authorizationService.Object, resourceCatalog.Object, testCatalog.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(handlerMock);
            //---------------Execute Test ----------------------
            var postDataMock = handlerMock.GetPostDataMock(communicationContext.Object);
            //---------------Test Result -----------------------
            var json = postDataMock.IsJSON();
            Assert.IsTrue(json);
            Assert.AreEqual(data, postDataMock);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GetPostData_GivenJsonDataInContextAndUnknownWebMethod_ShouldReturnEmpty()
        {
            //---------------Set up test pack-------------------
            var communicationContext = new Mock<ICommunicationContext>();
            const string UriString = "https://warewolf.atlassian.net/secure/RapidBoard.jspa";
            communicationContext.SetupGet(context => context.Request.Uri).Returns(new Uri(UriString));
            communicationContext.Setup(context => context.Request.Method).Returns("unknown");
            communicationContext.Setup(context => context.Request.ContentEncoding).Returns(Encoding.Default);
            var data = this.SerializeToJsonString(new DefaultSerializationBinder());
            var stringInMemoryStream = new MemoryStream(Encoding.Default.GetBytes(data));
            communicationContext.Setup(context => context.Request.InputStream).Returns(stringInMemoryStream);
            communicationContext.Setup(context => context.Request.BoundVariables).Returns(new NameValueCollection());
            communicationContext.Setup(context => context.Request.QueryString).Returns(new NameValueCollection());
            var dataObject = new Mock<IDSFDataObject>();
            var authorizationService = new Mock<IAuthorizationService>();
            var resourceCatalog = new Mock<IResourceCatalog>();
            var testCatalog = new Mock<ITestCatalog>();
            var handlerMock = new AbstractWebRequestHandlerMock(dataObject.Object, authorizationService.Object, resourceCatalog.Object, testCatalog.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(handlerMock);
            //---------------Execute Test ----------------------
            var postDataMock = handlerMock.GetPostDataMock(communicationContext.Object);
            //---------------Test Result -----------------------
            Assert.AreEqual(string.Empty, postDataMock);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GetPostData_GivenPostXmlDataInContext_ShouldReturnXmlData()
        {
            //---------------Set up test pack-------------------
            var communicationContext = new Mock<ICommunicationContext>();
            const string UriString = "https://warewolf.atlassian.net/secure/RapidBoard.jspa";
            communicationContext.SetupGet(context => context.Request.Uri).Returns(new Uri(UriString));
            communicationContext.Setup(context => context.Request.Method).Returns("POST");
            communicationContext.Setup(context => context.Request.ContentEncoding).Returns(Encoding.Default);
            var data = this.SerializeToJsonString(new DefaultSerializationBinder());
            var xmlData = ConvertJsonToXml(data);
            var stringInMemoryStream = new MemoryStream(Encoding.Default.GetBytes(xmlData));
            communicationContext.Setup(context => context.Request.InputStream).Returns(stringInMemoryStream);
            communicationContext.Setup(context => context.Request.BoundVariables).Returns(new NameValueCollection());
            communicationContext.Setup(context => context.Request.QueryString).Returns(new NameValueCollection());
            var dataObject = new Mock<IDSFDataObject>();
            var authorizationService = new Mock<IAuthorizationService>();
            var resourceCatalog = new Mock<IResourceCatalog>();
            var testCatalog = new Mock<ITestCatalog>();
            var handlerMock = new AbstractWebRequestHandlerMock(dataObject.Object, authorizationService.Object, resourceCatalog.Object, testCatalog.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(handlerMock);
            //---------------Execute Test ----------------------
            var postDataMock = handlerMock.GetPostDataMock(communicationContext.Object);
            //---------------Test Result -----------------------
            var isXml = postDataMock.IsXml();
            Assert.IsTrue(isXml);
            Assert.AreEqual(xmlData, postDataMock);
        }

        private static string ConvertJsonToXml(string data)
        {
            var xml =
                XDocument.Load(JsonReaderWriterFactory.CreateJsonReader(Encoding.ASCII.GetBytes(data),
                    new XmlDictionaryReaderQuotas()));
            var xmlData = xml.ToString();
            return xmlData;
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GetPostData_GivenGetJsonDataInContext_ShouldReturnJsonData()
        {
            //---------------Set up test pack-------------------
            var communicationContext = new Mock<ICommunicationContext>();
            var data = this.SerializeToJsonString(new DefaultSerializationBinder());
            string uriString = $"https://warewolf.atlassian.net/secure/RapidBoard.jspa?&{data}";
            communicationContext.SetupGet(context => context.Request.Uri).Returns(new Uri(uriString));
            communicationContext.Setup(context => context.Request.Method).Returns("GET");
            communicationContext.Setup(context => context.Request.ContentEncoding).Returns(Encoding.Default);

            communicationContext.Setup(context => context.Request.BoundVariables).Returns(new NameValueCollection());
            communicationContext.Setup(context => context.Request.QueryString).Returns(new NameValueCollection());
            var dataObject = new Mock<IDSFDataObject>();
            var authorizationService = new Mock<IAuthorizationService>();
            var resourceCatalog = new Mock<IResourceCatalog>();
            var testCatalog = new Mock<ITestCatalog>();
            var handlerMock = new AbstractWebRequestHandlerMock(dataObject.Object, authorizationService.Object, resourceCatalog.Object, testCatalog.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(handlerMock);
            //---------------Execute Test ----------------------
            var postDataMock = handlerMock.GetPostDataMock(communicationContext.Object);
            //---------------Test Result -----------------------
            var isJson = postDataMock.IsJSON();
            Assert.IsTrue(isJson);
            Assert.AreEqual(data, postDataMock);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GetPostData_GivenGetXmlDataInContext_ShouldReturnXmlData()
        {
            //---------------Set up test pack-------------------
            var communicationContext = new Mock<ICommunicationContext>();
            var data = this.SerializeToJsonString(new DefaultSerializationBinder());
            var xmlData = ConvertJsonToXml(data);
            string uriString = $"https://warewolf.atlassian.net/secure/RapidBoard.jspa?&{xmlData}";
            communicationContext.SetupGet(context => context.Request.Uri).Returns(new Uri(uriString));
            communicationContext.Setup(context => context.Request.Method).Returns("GET");
            communicationContext.Setup(context => context.Request.ContentEncoding).Returns(Encoding.Default);

            communicationContext.Setup(context => context.Request.BoundVariables).Returns(new NameValueCollection());
            communicationContext.Setup(context => context.Request.QueryString).Returns(new NameValueCollection());
            var dataObject = new Mock<IDSFDataObject>();
            var authorizationService = new Mock<IAuthorizationService>();
            var resourceCatalog = new Mock<IResourceCatalog>();
            var testCatalog = new Mock<ITestCatalog>();
            var handlerMock = new AbstractWebRequestHandlerMock(dataObject.Object, authorizationService.Object, resourceCatalog.Object, testCatalog.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(handlerMock);
            //---------------Execute Test ----------------------
            var postDataMock = handlerMock.GetPostDataMock(communicationContext.Object);
            //---------------Test Result -----------------------
            var isXml = postDataMock.IsXml();
            Assert.IsTrue(isXml);
            Assert.AreEqual(xmlData, postDataMock);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GetPostData_GivenGetDataListXmlDataInContext_ShouldReturnXmlData()
        {
            //---------------Set up test pack-------------------
            var communicationContext = new Mock<ICommunicationContext>();
            var data = this.SerializeToJsonString(new DefaultSerializationBinder());
            XmlDocument myXmlNode = JsonConvert.DeserializeXmlNode(data, "DataList");
            var xmlData = myXmlNode.InnerXml;
            string uriString = $"https://warewolf.atlassian.net/secure/RapidBoard.jspa?&{xmlData}";
            communicationContext.SetupGet(context => context.Request.Uri).Returns(new Uri(uriString));
            communicationContext.Setup(context => context.Request.Method).Returns("GET");
            communicationContext.Setup(context => context.Request.ContentEncoding).Returns(Encoding.Default);

            communicationContext.Setup(context => context.Request.BoundVariables).Returns(new NameValueCollection());
            communicationContext.Setup(context => context.Request.QueryString).Returns(new NameValueCollection());
            var dataObject = new Mock<IDSFDataObject>();
            var authorizationService = new Mock<IAuthorizationService>();
            var resourceCatalog = new Mock<IResourceCatalog>();
            var testCatalog = new Mock<ITestCatalog>();
            var handlerMock = new AbstractWebRequestHandlerMock(dataObject.Object, authorizationService.Object, resourceCatalog.Object, testCatalog.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(handlerMock);
            //---------------Execute Test ----------------------
            var postDataMock = handlerMock.GetPostDataMock(communicationContext.Object);
            //---------------Test Result -----------------------
            var isXml = postDataMock.IsXml();
            Assert.IsTrue(isXml);
            Assert.AreEqual(xmlData, postDataMock);
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
            var testCatalog = new Mock<ITestCatalog>();
            var handlerMock = new AbstractWebRequestHandlerMock(dataObject.Object, authorizationService.Object, resourceCatalog.Object, testCatalog.Object);
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
            var testCatalog = new Mock<ITestCatalog>();
            //------------Setup for test-------------------------
            var handlerMock = new AbstractWebRequestHandlerMock(dataObject.Object, authorizationService.Object, resourceCatalog.Object, testCatalog.Object);
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
            var testCatalog = new Mock<ITestCatalog>();
            //------------Setup for test-------------------------
            var handlerMock = new AbstractWebRequestHandlerMock(dataObject.Object, authorizationService.Object, resourceCatalog.Object, testCatalog.Object);
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
            communicationContext.Setup(context => context.Request.QueryString).Returns(new NameValueCollection { { "wid", "the_wid" } });
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
            communicationContext.Setup(context => context.Request.QueryString).Returns(new NameValueCollection { { "dlid", "the_datalist" } });
            //------------Setup for test-------------------------
            var handlerMock = new AbstractWebRequestHandlerMock();
            //------------Execute Test---------------------------
            var dataListID = handlerMock.GetDataListIDMock(communicationContext.Object);
            //------------Assert Results-------------------------
            Assert.IsNotNull(dataListID);
            Assert.AreEqual(ExpectedResult, dataListID);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void BuildTestResultForWebRequest_GivenTestResultPassed_ShouldSetMessage()
        {
            //------------Setup for test-------------------------
            IServiceTestModelTO to = new ServiceTestModelTO();
            to.Result = new TestRunResult
            {
                RunTestResult = RunResult.TestPassed
            };
            var privateObject = new PrivateType(typeof(AbstractWebRequestHandler));
            //------------Execute Test---------------------------            
            var result = privateObject.InvokeStatic("BuildTestResultForWebRequest", to);
            //------------Assert Results-------------------------
            Assert.IsTrue(result.ToString().Contains("\"Result\": \"Passed\""));
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void BuildTestResultForWebRequest_GivenTestResultFailed_ShouldSetMessage()
        {
            //------------Setup for test-------------------------
            IServiceTestModelTO to = new ServiceTestModelTO();
            to.Result = new TestRunResult
            {
                RunTestResult = RunResult.TestFailed,
                Message = ""
            };
            var privateObject = new PrivateType(typeof(AbstractWebRequestHandler));
            //------------Execute Test---------------------------            
            var result = privateObject.InvokeStatic("BuildTestResultForWebRequest", to);
            //------------Assert Results-------------------------
            Assert.IsTrue(result.ToString().Contains("\"Result\": \"Failed\""));
        }
        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void BuildTestResultForWebRequest_GivenTestResultInvalid_ShouldSetMessage()
        {
            //------------Setup for test-------------------------
            IServiceTestModelTO to = new ServiceTestModelTO();
            to.Result = new TestRunResult
            {
                RunTestResult = RunResult.TestInvalid,
                Message = ""
            };
            var privateObject = new PrivateType(typeof(AbstractWebRequestHandler));
            //------------Execute Test---------------------------            
            var result = privateObject.InvokeStatic("BuildTestResultForWebRequest", to);
            //------------Assert Results-------------------------
            Assert.IsTrue(result.ToString().Contains("\"Result\": \"Invalid\""));
        }
        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void BuildTestResultForWebRequest_GivenTestResultTestResourceDeleted_ShouldSetMessage()
        {
            //------------Setup for test-------------------------
            IServiceTestModelTO to = new ServiceTestModelTO();
            to.Result = new TestRunResult
            {
                RunTestResult = RunResult.TestResourceDeleted,
                Message = ""
            };
            var privateObject = new PrivateType(typeof(AbstractWebRequestHandler));
            //------------Execute Test---------------------------            
            var result = privateObject.InvokeStatic("BuildTestResultForWebRequest", to);
            //------------Assert Results-------------------------
            Assert.IsTrue(result.ToString().Contains("\"Result\": \"ResourceDelete\""));
        }
        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void BuildTestResultForWebRequest_GivenTestResultTestResourcePathUpdated_ShouldSetMessage()
        {
            //------------Setup for test-------------------------
            IServiceTestModelTO to = new ServiceTestModelTO();
            to.Result = new TestRunResult
            {
                RunTestResult = RunResult.TestResourcePathUpdated,
                Message = ""
            };
            var privateObject = new PrivateType(typeof(AbstractWebRequestHandler));
            //------------Execute Test---------------------------            
            var result = privateObject.InvokeStatic("BuildTestResultForWebRequest", to);
            //------------Assert Results-------------------------
            Assert.IsTrue(result.ToString().Contains("\"Result\": \"ResourcpathUpdated\""));
        }
        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void BuildTestResultForWebRequest_GivenTestResultTestPending_ShouldSetMessage()
        {
            //------------Setup for test-------------------------
            IServiceTestModelTO to = new ServiceTestModelTO();
            to.Result = new TestRunResult
            {
                RunTestResult = RunResult.TestPending,
                Message = ""
            };
            var privateObject = new PrivateType(typeof(AbstractWebRequestHandler));
            //------------Execute Test---------------------------            
            var result = privateObject.InvokeStatic("BuildTestResultForWebRequest", to);
            //------------Assert Results-------------------------
            Assert.IsTrue(result.ToString().Contains("\"Result\": \"Pending\""));
        }
        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void ExtractKeyValuePairs_GivenKeyvaluePairs_ShouldCloneKeyValuePair()
        {
            //------------Setup for test-------------------------
            var boundVariables = new NameValueCollection();
            var privateObject = new PrivateType(typeof(AbstractWebRequestHandler));
            //------------Execute Test---------------------------            
            privateObject.InvokeStatic("ExtractKeyValuePairs", LocalBoundVariables, boundVariables);
            //------------Assert Results-------------------------
            //The WID is skipped
            Assert.AreEqual(LocalBoundVariables.Count - 1, boundVariables.Count);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GetForAllResources_GivenRequestIsPublic()
        {
            const string UriString = "http://localhost:3142/secure/Hello%20World.tests/Blank%20Input";
            //------------Setup for test-------------------------
            var webRequestTO = new WebRequestTO();
            webRequestTO.Variables.Add("isPublic", "false");
            webRequestTO.WebServerUrl = UriString;
            var privateObject = new PrivateType(typeof(AbstractWebRequestHandler));
            //------------Execute Test---------------------------            
            var result = privateObject.InvokeStatic("GetForAllResources", webRequestTO);
            //------------Assert Results-------------------------
            Assert.IsNotNull(result);
            Assert.IsTrue(result.ToString().Contains(".tests"));
        }
        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void SetContentType_GivenJsonType_ShouldSetDataObjectReturnType()
        {
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
            var testCatalog = new Mock<ITestCatalog>();
            var handlerMock = new AbstractWebRequestHandlerMock(dataObject.Object, authorizationService.Object, resourceCatalog.Object, testCatalog.Object);
            //---------------Assert Precondition----------------
            var headers = new Mock<NameValueCollection>();
            headers.Setup(collection => collection.Get("Content-Type")).Returns("application/json");
            handlerMock.CreateFromMock(new WebRequestTO(), "Hello World", Guid.Empty.ToString(), headers.Object, principal.Object);
            var privateType = new PrivateType(typeof(AbstractWebRequestHandler));
            //---------------Execute Test ----------------------
            privateType.InvokeStatic("SetContentType", headers.Object, dataObject.Object);
            //------------Assert Results-------------------------
            Assert.AreEqual(EmitionTypes.JSON, dataObject.Object.ReturnType);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void SetContentType_GivenXMLType_ShouldSetDataObjectReturnType()
        {
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
            var testCatalog = new Mock<ITestCatalog>();
            var handlerMock = new AbstractWebRequestHandlerMock(dataObject.Object, authorizationService.Object, resourceCatalog.Object, testCatalog.Object);
            //---------------Assert Precondition----------------
            var headers = new Mock<NameValueCollection>();
            headers.Setup(collection => collection.Get("Content-Type")).Returns("application/xml");
            handlerMock.CreateFromMock(new WebRequestTO(), "Hello World", Guid.Empty.ToString(), headers.Object, principal.Object);
            var privateType = new PrivateType(typeof(AbstractWebRequestHandler));
            //---------------Execute Test ----------------------            
            privateType.InvokeStatic("SetContentType", headers.Object, dataObject.Object);
            //------------Assert Results-------------------------
            Assert.AreEqual(EmitionTypes.XML, dataObject.Object.ReturnType);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void SetEmitionType_GivenHeaderContentTypeJson_ShouldSetDataObjectContentTypeJson()
        {
            //---------------Set up test pack-------------------
            var setEmitionTypeMethod = typeof(AbstractWebRequestHandler).GetMethod("SetEmitionType", BindingFlags.NonPublic | BindingFlags.Static);
            const string ServiceName = "hello World";
            NameValueCollection collection = new NameValueCollection
            {
                {"Content-Type", "Json"}
            };
            var dataObject = new Mock<IDSFDataObject>();
            dataObject.SetupProperty(o => o.ReturnType);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(setEmitionTypeMethod);
            //---------------Execute Test ----------------------
            var invoke = setEmitionTypeMethod.Invoke(null, new object[] { ServiceName, collection, dataObject.Object });
            //---------------Test Result -----------------------
            dataObject.VerifySet(o => o.ReturnType = EmitionTypes.JSON, Times.Exactly(1));
            Assert.AreEqual(ServiceName, invoke.ToString());
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void SetEmitionType_GivenHeaderContentTypeXml_ShouldSetDataObjectContentTypeXml()
        {
            //---------------Set up test pack-------------------
            var setEmitionTypeMethod = typeof(AbstractWebRequestHandler).GetMethod("SetEmitionType", BindingFlags.NonPublic | BindingFlags.Static);
            const string ServiceName = "hello World";
            NameValueCollection collection = new NameValueCollection
            {
                {"Content-Type", "xml"}
            };
            var dataObject = new Mock<IDSFDataObject>();
            dataObject.SetupProperty(o => o.ReturnType);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(setEmitionTypeMethod);
            //---------------Execute Test ----------------------
            var invoke = setEmitionTypeMethod.Invoke(null, new object[] { ServiceName, collection, dataObject.Object });
            //---------------Test Result -----------------------
            dataObject.VerifySet(o => o.ReturnType = EmitionTypes.XML, Times.Exactly(1));
            Assert.AreEqual(ServiceName, invoke.ToString());
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void SetEmitionType_GivenNoHeaders_ShouldSetDataObjectContentTypeXml()
        {
            //---------------Set up test pack-------------------
            var setEmitionTypeMethod = typeof(AbstractWebRequestHandler).GetMethod("SetEmitionType", BindingFlags.NonPublic | BindingFlags.Static);
            const string ServiceName = "hello World";
            NameValueCollection collection = default(NameValueCollection);

            var dataObject = new Mock<IDSFDataObject>();
            dataObject.SetupProperty(o => o.ReturnType);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(setEmitionTypeMethod);
            //---------------Execute Test ----------------------
            var invoke = setEmitionTypeMethod.Invoke(null, new object[] { ServiceName, collection, dataObject.Object });
            //---------------Test Result -----------------------
            dataObject.VerifySet(o => o.ReturnType = EmitionTypes.XML, Times.Exactly(1));
            Assert.AreEqual(ServiceName, invoke.ToString());
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void SetEmitionType_GivenServiceNameEndsWithapi_ShouldSetDataObjectContentTypeSwagger()
        {
            //---------------Set up test pack-------------------
            var setEmitionTypeMethod = typeof(AbstractWebRequestHandler).GetMethod("SetEmitionType", BindingFlags.NonPublic | BindingFlags.Static);
            const string ServiceName = "hello World.api";
            NameValueCollection collection = new NameValueCollection();

            var dataObject = new Mock<IDSFDataObject>();
            dataObject.SetupProperty(o => o.ReturnType);
            dataObject.SetupProperty(o => o.ServiceName);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(setEmitionTypeMethod);
            //---------------Execute Test ----------------------
            var invoke = setEmitionTypeMethod.Invoke(null, new object[] { ServiceName, collection, dataObject.Object });
            //---------------Test Result -----------------------
            dataObject.VerifySet(o => o.ReturnType = EmitionTypes.SWAGGER, Times.Exactly(1));
            dataObject.VerifySet(o => o.ReturnType = EmitionTypes.XML, Times.Exactly(1));
            Assert.AreEqual("hello World", invoke.ToString());
            Assert.AreEqual("hello World", dataObject.Object.ServiceName);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void SetEmitionType_GivenServiceNameEndsWithtests_ShouldSetDataObjectContentTypeTests()
        {
            //---------------Set up test pack-------------------
            var setEmitionTypeMethod = typeof(AbstractWebRequestHandler).GetMethod("SetEmitionType", BindingFlags.NonPublic | BindingFlags.Static);
            const string ServiceName = "hello World.tests";
            NameValueCollection collection = new NameValueCollection();

            var dataObject = new Mock<IDSFDataObject>();
            dataObject.SetupProperty(o => o.ReturnType);
            dataObject.SetupProperty(o => o.ServiceName);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(setEmitionTypeMethod);
            //---------------Execute Test ----------------------
            var invoke = setEmitionTypeMethod.Invoke(null, new object[] { ServiceName, collection, dataObject.Object });
            //---------------Test Result -----------------------
            dataObject.VerifySet(o => o.ReturnType = EmitionTypes.TEST, Times.Exactly(1));
            dataObject.VerifySet(o => o.ReturnType = EmitionTypes.XML, Times.Exactly(1));
            Assert.AreEqual("hello World", invoke.ToString());
            Assert.AreEqual("hello World", dataObject.Object.ServiceName);
        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void SetEmitionType_GivenServiceNameEndsWithJson_ShouldSetDataObjectContentTypeJson()
        {
            //---------------Set up test pack-------------------
            var setEmitionTypeMethod = typeof(AbstractWebRequestHandler).GetMethod("SetEmitionType", BindingFlags.NonPublic | BindingFlags.Static);
            const string ServiceName = "hello World.JSON";
            NameValueCollection collection = new NameValueCollection();

            var dataObject = new Mock<IDSFDataObject>();
            dataObject.SetupProperty(o => o.ReturnType);
            dataObject.SetupProperty(o => o.ServiceName);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(setEmitionTypeMethod);
            //---------------Execute Test ----------------------
            var invoke = setEmitionTypeMethod.Invoke(null, new object[] { ServiceName, collection, dataObject.Object });
            //---------------Test Result -----------------------
            dataObject.VerifySet(o => o.ReturnType = EmitionTypes.JSON, Times.Exactly(1));
            dataObject.VerifySet(o => o.ReturnType = EmitionTypes.XML, Times.Exactly(1));
            Assert.AreEqual("hello World", invoke.ToString());
            Assert.AreEqual("hello World", dataObject.Object.ServiceName);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void SetEmitionType_GivenTestsInFolder_ShouldSetDataObjectContentTypeTests()
        {
            //---------------Set up test pack-------------------
            var setEmitionTypeMethod = typeof(AbstractWebRequestHandler).GetMethod("SetEmitionType", BindingFlags.NonPublic | BindingFlags.Static);
            const string ServiceName = "hello World.tests/";
            NameValueCollection collection = new NameValueCollection();

            var dataObject = new Mock<IDSFDataObject>();
            dataObject.SetupProperty(o => o.ReturnType);
            dataObject.SetupProperty(o => o.ServiceName);
            dataObject.SetupProperty(o => o.TestName);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(setEmitionTypeMethod);
            //---------------Execute Test ----------------------
            var invoke = setEmitionTypeMethod.Invoke(null, new object[] { ServiceName, collection, dataObject.Object });
            //---------------Test Result -----------------------
            dataObject.VerifySet(o => o.ReturnType = EmitionTypes.TEST, Times.Exactly(1));
            dataObject.VerifySet(o => o.ReturnType = EmitionTypes.XML, Times.Exactly(1));
            Assert.AreEqual("hello World", invoke.ToString());
            Assert.AreEqual("hello World", dataObject.Object.ServiceName);
            Assert.AreEqual("*", dataObject.Object.TestName);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void SetEmitionType_GivenServiceNameEndsWithtests_ShouldSetDataObjectIsTestExecution()
        {
            //---------------Set up test pack-------------------
            var setEmitionTypeMethod = typeof(AbstractWebRequestHandler).GetMethod("SetEmitionType", BindingFlags.NonPublic | BindingFlags.Static);
            const string ServiceName = "hello World.tests";
            NameValueCollection collection = new NameValueCollection();

            var dataObject = new Mock<IDSFDataObject>();
            dataObject.SetupProperty(o => o.ReturnType);
            dataObject.SetupProperty(o => o.IsServiceTestExecution);
            dataObject.SetupProperty(o => o.ServiceName);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(setEmitionTypeMethod);
            //---------------Execute Test ----------------------
            var invoke = setEmitionTypeMethod.Invoke(null, new object[] { ServiceName, collection, dataObject.Object });
            //---------------Test Result -----------------------
            dataObject.VerifySet(o => o.ReturnType = EmitionTypes.TEST, Times.Exactly(1));
            dataObject.VerifySet(o => o.IsServiceTestExecution = true, Times.Exactly(1));
            dataObject.VerifySet(o => o.ReturnType = EmitionTypes.XML, Times.Exactly(1));
            Assert.AreEqual("hello World", invoke.ToString());
            Assert.AreEqual("hello World", dataObject.Object.ServiceName);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void SetTestResourceIds_GivenRequestForAllTests_ShouldSetDataObjectTestResourceIds()
        {
            /*//---------------Set up test pack-------------------
            //http://rsaklfnkosinath:3142/secure/Hello%20World.debug?Name=&wid=540beccb-b4f5-4b34-bc37-aa24b26370e2
            //SetTestResourceIds(WebRequestTO webRequest, IDSFDataObject dataObject)
            var setEmitionTypeMethod = typeof(AbstractWebRequestHandler).GetMethod("SetTestResourceIds", BindingFlags.NonPublic | BindingFlags.Static);
            const string ServiceName = "hello World./tests";
            var webRequestTO = new WebRequestTO()
            {

                Variables = new NameValueCollection() { { "isPublic", "true" } },
                WebServerUrl = "http://rsaklfnkosinath:3142/secure/Hello%20World.debug?Name=&wid=540beccb-b4f5-4b34-bc37-aa24b26370e2"

            };


            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------*/
        }
    }

    #region AbstractWebRequestHandlerMock

    internal class AbstractWebRequestHandlerMock : AbstractWebRequestHandler
    {
        public AbstractWebRequestHandlerMock(IDSFDataObject dataObject, IAuthorizationService service, IResourceCatalog catalog, ITestCatalog testCatalog)
            : base(catalog, testCatalog, dataObject, service)
        {

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

    #endregion
}

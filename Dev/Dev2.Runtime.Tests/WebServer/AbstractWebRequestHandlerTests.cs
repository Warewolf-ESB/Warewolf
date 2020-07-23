#pragma warning disable
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
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Security.Principal;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.Common.ExtMethods;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Enums;
using Dev2.Common.Interfaces.Runtime.Services;
using Dev2.Data;
using Dev2.DynamicServices;
using Dev2.Interfaces;
using Dev2.Runtime;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.WebServer;
using Dev2.Runtime.WebServer.Handlers;
using Dev2.Runtime.WebServer.Responses;
using Dev2.Runtime.WebServer.TransferObjects;
using Dev2.Services.Security;
using Dev2.Web;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Warewolf.Data;
using Warewolf.Services;
using Warewolf.Storage.Interfaces;
using StringExtension = Dev2.Common.ExtMethods.StringExtension;

namespace Dev2.Tests.Runtime.WebServer
{
    [TestClass]
    [TestCategory("Runtime WebServer")]
    public class AbstractWebRequestHandlerTests
    {
        NameValueCollection LocalBoundVariables => new NameValueCollection
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
        public TestContext TestContext { get; set; }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(AbstractWebRequestHandler))]
        public void AbstractWebRequestHandler_CreateForm_GivenValidArgsContainsIsDebug_ShouldSetDataObjectAsDebug()
        {
            //---------------Set up test pack-------------------
            var principal = new Mock<IPrincipal>();
            GetExecutingUser(principal);

            var authorizationService = new Mock<IAuthorizationService>();
            authorizationService.Setup(service => service.IsAuthorized(It.IsAny<AuthorizationContext>(), It.IsAny<string>())).Returns(true);
            var dataObject = new Mock<IDSFDataObject>();
            dataObject.SetupAllProperties();
            var env = new Mock<IExecutionEnvironment>();
            env.SetupAllProperties();
            dataObject.SetupGet(o => o.Environment).Returns(env.Object);
            dataObject.SetupGet(o => o.RawPayload).Returns(new StringBuilder("<raw>SomeData</raw>"));
            dataObject.Setup(p => p.ExecutingUser).Returns(principal.Object);
            
            var resource = new Mock<IResource>();
            var resourceId = Guid.NewGuid();
            resource.SetupGet(resource1 => resource1.ResourceID).Returns(resourceId);
            var resourceCatalog = new Mock<IResourceCatalog>();
            resourceCatalog.Setup(o => o.GetResource(It.IsAny<Guid>(), It.IsAny<string>())).Returns(resource.Object);
            var testCatalog = new Mock<ITestCatalog>();
            var mockCoverageCatalog = new Mock<ITestCoverageCatalog>();
            var wRepo = new Mock<IWorkspaceRepository>();
            wRepo.SetupGet(repository => repository.ServerWorkspace).Returns(new Workspace(Guid.Empty));
            wRepo.SetupGet(repository => repository.ServerWorkspace).Returns(new Workspace(Guid.Empty));
            var handlerMock = new AbstractWebRequestHandlerMock(new TestAbstractWebRequestDataObjectFactory(dataObject.Object), authorizationService.Object, resourceCatalog.Object, testCatalog.Object, mockCoverageCatalog.Object, wRepo.Object);
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

        static void GetExecutingUser(Mock<IPrincipal> principal)
        {
            var identity = new Mock<IIdentity>();
            identity.Setup(p => p.Name).Returns("User1");
            principal.Setup(p => p.Identity).Returns(identity.Object);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(AbstractWebRequestHandler))]
        public void AbstractWebRequestHandler_CreateForm_GivenValidArgsContainsIsServiceTes_ShouldSetDataObjectAsTest()
        {
            //---------------Set up test pack-------------------
            var principal = new Mock<IPrincipal>();
            GetExecutingUser(principal);
            var authorizationService = new Mock<IAuthorizationService>();
            authorizationService.Setup(service => service.IsAuthorized(It.IsAny<AuthorizationContext>(), It.IsAny<string>())).Returns(true);
            var dataObject = new Mock<IDSFDataObject>();
            dataObject.SetupAllProperties();
            var env = new Mock<IExecutionEnvironment>();
            env.SetupAllProperties();
            dataObject.SetupGet(o => o.Environment).Returns(env.Object);
            dataObject.SetupGet(o => o.IsServiceTestExecution).Returns(true);
            dataObject.SetupGet(o => o.RawPayload).Returns(new StringBuilder("<raw>SomeData</raw>"));
            dataObject.Setup(p => p.ExecutingUser).Returns(principal.Object);
            dataObject.Setup(p => p.ExecutingUser).Returns(principal.Object);
            var resourceCatalog = new Mock<IResourceCatalog>();
            var resource = new Mock<IResource>();
            var resourceId = Guid.NewGuid();
            resource.SetupGet(resource1 => resource1.ResourceID).Returns(resourceId);
            resource.SetupGet(a => a.DataList).Returns(new StringBuilder("<DataList></DataList>"));
            resourceCatalog.Setup(o => o.GetResource(It.IsAny<Guid>(), It.IsAny<string>())).Returns(resource.Object);
            var testCatalog = new Mock<ITestCatalog>();
            var mockCoverageCatalog = new Mock<ITestCoverageCatalog>();
            var wRepo = new Mock<IWorkspaceRepository>();
            wRepo.SetupGet(repository => repository.ServerWorkspace).Returns(new Workspace(Guid.Empty));
            var handlerMock = new AbstractWebRequestHandlerMock(new TestAbstractWebRequestDataObjectFactory(dataObject.Object), authorizationService.Object, resourceCatalog.Object, testCatalog.Object, mockCoverageCatalog.Object, wRepo.Object);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var webRequestTO = new WebRequestTO()
            {
                Variables = new NameValueCollection()
                {
                    {"IsDebug","false"},

                },
                WebServerUrl = "http://rsaklfnkosinath:3142/secure/Home/HelloWorld/.tests"
            };
            var responseWriter = handlerMock.CreateFromMock(webRequestTO, "Hello World", string.Empty, new NameValueCollection(), principal.Object);
            //---------------Test Result -----------------------
            Assert.IsNotNull(responseWriter);

        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(AbstractWebRequestHandler))]
        public void AbstractWebRequestHandler_CreateForm_GivenValidArgsContainsIsServiceSwagger_ShouldSetDataObjectAsSwagger()
        {
            //---------------Set up test pack-------------------
            var principal = new Mock<IPrincipal>();
            GetExecutingUser(principal);
            var authorizationService = new Mock<IAuthorizationService>();
            authorizationService.Setup(service => service.IsAuthorized(It.IsAny<AuthorizationContext>(), It.IsAny<string>())).Returns(true);
            var dataObject = new Mock<IDSFDataObject>();
            dataObject.SetupAllProperties();
            var env = new Mock<IExecutionEnvironment>();
            env.SetupAllProperties();
            dataObject.SetupGet(o => o.Environment).Returns(env.Object);
            dataObject.SetupGet(o => o.IsServiceTestExecution).Returns(false);
            dataObject.SetupGet(o => o.RawPayload).Returns(new StringBuilder("<raw>SomeData</raw>"));
            dataObject.Setup(p => p.ExecutingUser).Returns(principal.Object);
            var resourceCatalog = new Mock<IResourceCatalog>();
            var resource = new Mock<IResource>();
            var resourceId = Guid.NewGuid();
            resource.SetupGet(resource1 => resource1.ResourceID).Returns(resourceId);
            resource.SetupGet(a => a.DataList).Returns(new StringBuilder("<DataList></DataList>"));
            resourceCatalog.Setup(o => o.GetResource(It.IsAny<Guid>(), It.IsAny<string>())).Returns(resource.Object);
            var testCatalog = new Mock<ITestCatalog>();
            var mockCoverageCatalog = new Mock<ITestCoverageCatalog>();
            var wRepo = new Mock<IWorkspaceRepository>();
            wRepo.SetupGet(repository => repository.ServerWorkspace).Returns(new Workspace(Guid.Empty));
            var handlerMock = new AbstractWebRequestHandlerMock(new TestAbstractWebRequestDataObjectFactory(dataObject.Object), authorizationService.Object, resourceCatalog.Object, testCatalog.Object, mockCoverageCatalog.Object, wRepo.Object);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var webRequestTO = new WebRequestTO()
            {
                ServiceName = "",
                Variables = new NameValueCollection()
                {

                },
                WebServerUrl = "http://rsaklfnkosinath:3142/secure/Home/HelloWorld.api"
            };
            var responseWriter = handlerMock.CreateFromMock(webRequestTO, "Hello World.api", string.Empty, new NameValueCollection(), principal.Object);
            //---------------Test Result -----------------------
            Assert.IsNotNull(responseWriter);

            dataObject.VerifySet(o => o.ReturnType = EmitionTypes.SWAGGER, Times.Once);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(AbstractWebRequestHandler))]
        public void AbstractWebRequestHandler_CreateForm_GivenValidArgsContainsIsServiceJson_ShouldSetDataObjectAsJson()
        {
            //---------------Set up test pack-------------------
            var principal = new Mock<IPrincipal>();
            GetExecutingUser(principal);
            var authorizationService = new Mock<IAuthorizationService>();
            authorizationService.Setup(service => service.IsAuthorized(It.IsAny<AuthorizationContext>(), It.IsAny<string>())).Returns(true);
            var dataObject = new Mock<IDSFDataObject>();
            dataObject.SetupAllProperties();
            var env = new Mock<IExecutionEnvironment>();
            env.SetupAllProperties();
            dataObject.SetupGet(o => o.Environment).Returns(env.Object);
            dataObject.SetupGet(o => o.ExecutionID).Returns(new Guid());
            dataObject.SetupGet(o => o.IsServiceTestExecution).Returns(false);
            dataObject.SetupGet(o => o.RawPayload).Returns(new StringBuilder("<raw>SomeData</raw>"));
            dataObject.Setup(p => p.ExecutingUser).Returns(principal.Object);
            var resourceCatalog = new Mock<IResourceCatalog>();
            var resource = new Mock<IResource>();
            var resourceId = Guid.NewGuid();
            resource.SetupGet(resource1 => resource1.ResourceID).Returns(resourceId);
            resource.SetupGet(a => a.DataList).Returns(new StringBuilder("<DataList></DataList>"));
            resourceCatalog.Setup(o => o.GetResource(It.IsAny<Guid>(), It.IsAny<string>())).Returns(resource.Object);
            var testCatalog = new Mock<ITestCatalog>();
            var mockCoverageCatalog = new Mock<ITestCoverageCatalog>();
            var wRepo = new Mock<IWorkspaceRepository>();
            wRepo.SetupGet(repository => repository.ServerWorkspace).Returns(new Workspace(Guid.Empty));
            var handlerMock = new AbstractWebRequestHandlerMock(new TestAbstractWebRequestDataObjectFactory(dataObject.Object), authorizationService.Object, resourceCatalog.Object, testCatalog.Object, mockCoverageCatalog.Object, wRepo.Object);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var webRequestTo = new WebRequestTO()
            {
                ServiceName = "",
                Variables = new NameValueCollection()
                {

                },
                WebServerUrl = "http://rsaklfnkosinath:3142/secure/Home/HelloWorld.Json"
            };
            var responseWriter = handlerMock.CreateFromMock(webRequestTo, "Hello World.Json", string.Empty, new NameValueCollection(), principal.Object);
            //---------------Test Result -----------------------
            Assert.IsNotNull(responseWriter);

            dataObject.VerifySet(o => o.ReturnType = EmitionTypes.JSON, Times.Once);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(AbstractWebRequestHandler))]
        public void AbstractWebRequestHandler_CreateForm_GivenValidArgsContainsIsServiceXMl_ShouldSetDataObjectAsXml()
        {
            //---------------Set up test pack-------------------
            var principal = new Mock<IPrincipal>();
            GetExecutingUser(principal);
            var authorizationService = new Mock<IAuthorizationService>();
            authorizationService.Setup(service => service.IsAuthorized(It.IsAny<AuthorizationContext>(), It.IsAny<string>())).Returns(true);
            var dataObject = new Mock<IDSFDataObject>();
            dataObject.SetupAllProperties();
            var env = new Mock<IExecutionEnvironment>();
            env.SetupAllProperties();
            dataObject.SetupGet(o => o.Environment).Returns(env.Object);
            dataObject.SetupGet(o => o.IsServiceTestExecution).Returns(false);
            dataObject.SetupGet(o => o.RawPayload).Returns(new StringBuilder("<raw>SomeData</raw>"));
            dataObject.Setup(p => p.ExecutingUser).Returns(principal.Object);
            var resourceCatalog = new Mock<IResourceCatalog>();
            var resource = new Mock<IResource>();
            var resourceId = Guid.NewGuid();
            resource.SetupGet(resource1 => resource1.ResourceID).Returns(resourceId);
            resource.SetupGet(a => a.DataList).Returns(new StringBuilder("<DataList><Message>Hello World.</Message></DataList>"));
            resourceCatalog.Setup(o => o.GetResource(It.IsAny<Guid>(), It.IsAny<string>())).Returns(resource.Object);
            var testCatalog = new Mock<ITestCatalog>();
            var mockCoverageCatalog = new Mock<ITestCoverageCatalog>();
            var wRepo = new Mock<IWorkspaceRepository>();
            wRepo.SetupGet(repository => repository.ServerWorkspace).Returns(new Workspace(Guid.Empty));
            var handlerMock = new AbstractWebRequestHandlerMock(new TestAbstractWebRequestDataObjectFactory(dataObject.Object), authorizationService.Object, resourceCatalog.Object, testCatalog.Object, mockCoverageCatalog.Object, wRepo.Object);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var webRequestTO = new WebRequestTO()
            {
                ServiceName = "",
                Variables = new NameValueCollection()
                {

                },
                WebServerUrl = "http://rsaklfnkosinath:3142/secure/Home/HelloWorld.XML"
            };
            var responseWriter = handlerMock.CreateFromMock(webRequestTO, "Hello World.XML", string.Empty, new NameValueCollection(), principal.Object);
            //---------------Test Result -----------------------
            Assert.IsNotNull(responseWriter);
            dataObject.VerifySet(o => o.ReturnType = EmitionTypes.XML, Times.Exactly(2));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(AbstractWebRequestHandler))]
        public void AbstractWebRequestHandler_CreateForm_GivenValidArgsContainsIsServiceXMlWithErrors_ShouldCheckReturnTypeOfError()
        {
            //---------------Set up test pack-------------------
            var principal = new Mock<IPrincipal>();
            GetExecutingUser(principal);
            var authorizationService = new Mock<IAuthorizationService>();
            authorizationService.Setup(service => service.IsAuthorized(It.IsAny<AuthorizationContext>(), It.IsAny<string>())).Returns(true);
            var dataObject = new Mock<IDSFDataObject>();
            dataObject.SetupAllProperties();
            var env = new Mock<IExecutionEnvironment>();
            env.SetupAllProperties();
            env.Setup(environment => environment.HasErrors()).Returns(true);
            dataObject.SetupGet(o => o.Environment).Returns(env.Object);
            dataObject.SetupGet(o => o.IsServiceTestExecution).Returns(false);
            dataObject.SetupGet(o => o.RawPayload).Returns(new StringBuilder("<raw>SomeData</raw>"));
            dataObject.Setup(p => p.ExecutingUser).Returns(principal.Object);
            var resourceCatalog = new Mock<IResourceCatalog>();
            var resource = new Mock<IResource>();
            var resourceId = Guid.NewGuid();
            resource.SetupGet(resource1 => resource1.ResourceID).Returns(resourceId);
            resource.SetupGet(a => a.DataList).Returns(new StringBuilder("<DataList><Message>Hello World.</Message></DataList>"));
            resourceCatalog.Setup(o => o.GetResource(It.IsAny<Guid>(), It.IsAny<string>())).Returns(resource.Object);
            var testCatalog = new Mock<ITestCatalog>();
            var mockCoverageCatalog = new Mock<ITestCoverageCatalog>();
            var wRepo = new Mock<IWorkspaceRepository>();
            wRepo.SetupGet(repository => repository.ServerWorkspace).Returns(new Workspace(Guid.Empty));
            var handlerMock = new AbstractWebRequestHandlerMock(new TestAbstractWebRequestDataObjectFactory(dataObject.Object), authorizationService.Object, resourceCatalog.Object, testCatalog.Object, mockCoverageCatalog.Object, wRepo.Object);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var webRequestTO = new WebRequestTO()
            {
                ServiceName = "",
                Variables = new NameValueCollection()
                {

                },
                WebServerUrl = "http://rsaklfnkosinath:3142/secure/Home/HelloWorld.XML"
            };
            var responseWriter = handlerMock.CreateFromMock(webRequestTO, "Hello World.XML", string.Empty, new NameValueCollection(), principal.Object);
            //---------------Test Result -----------------------
            Assert.IsNotNull(responseWriter);
            dataObject.VerifyGet(o => o.ReturnType);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(AbstractWebRequestHandler))]
        public void AbstractWebRequestHandler_CreateForm_GivenValidArgsContainsIsServiceJsonWithErrors_ShouldCheckReturnTypeOfError()
        {
            //---------------Set up test pack-------------------
            var principal = new Mock<IPrincipal>();
            GetExecutingUser(principal);
            var authorizationService = new Mock<IAuthorizationService>();
            authorizationService.Setup(service => service.IsAuthorized(It.IsAny<AuthorizationContext>(), It.IsAny<string>())).Returns(true);
            var dataObject = new Mock<IDSFDataObject>();
            dataObject.SetupAllProperties();
            var env = new Mock<IExecutionEnvironment>();
            env.SetupAllProperties();
            env.Setup(environment => environment.HasErrors()).Returns(true);
            dataObject.SetupGet(o => o.Environment).Returns(env.Object);
            dataObject.SetupGet(o => o.IsServiceTestExecution).Returns(false);
            dataObject.SetupGet(o => o.RawPayload).Returns(new StringBuilder("<raw>SomeData</raw>"));
            dataObject.Setup(p => p.ExecutingUser).Returns(principal.Object);
            var resourceCatalog = new Mock<IResourceCatalog>();
            var resource = new Mock<IResource>();
            var resourceId = Guid.NewGuid();
            resource.SetupGet(resource1 => resource1.ResourceID).Returns(resourceId);
            resource.SetupGet(a => a.DataList).Returns(new StringBuilder("<DataList><Message>Hello World.</Message></DataList>"));
            resourceCatalog.Setup(o => o.GetResource(It.IsAny<Guid>(), It.IsAny<string>())).Returns(resource.Object);
            var testCatalog = new Mock<ITestCatalog>();
            var mockCoverageCatalog = new Mock<ITestCoverageCatalog>();
            var wRepo = new Mock<IWorkspaceRepository>();
            wRepo.SetupGet(repository => repository.ServerWorkspace).Returns(new Workspace(Guid.Empty));
            var handlerMock = new AbstractWebRequestHandlerMock(new TestAbstractWebRequestDataObjectFactory(dataObject.Object), authorizationService.Object, resourceCatalog.Object, testCatalog.Object, mockCoverageCatalog.Object, wRepo.Object);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var webRequestTO = new WebRequestTO()
            {
                ServiceName = "",
                Variables = new NameValueCollection()
                {

                },
                WebServerUrl = "http://rsaklfnkosinath:3142/secure/Home/HelloWorld.JSON"
            };
            var responseWriter = handlerMock.CreateFromMock(webRequestTO, "Hello World.JSON", string.Empty, new NameValueCollection(), principal.Object);
            //---------------Test Result -----------------------
            Assert.IsNotNull(responseWriter);
            dataObject.VerifyGet(o => o.ReturnType);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(AbstractWebRequestHandler))]
        public void AbstractWebRequestHandler_CreateForm_GivenEmitionTypeTESTAndIsRunAllTestsRequestTrue_ShouldFetchTests()
        {
            //---------------Set up test pack-------------------
            var principal = new Mock<IPrincipal>();
            GetExecutingUser(principal);
            var authorizationService = new Mock<IAuthorizationService>();
            authorizationService.Setup(service => service.IsAuthorized(principal.Object,It.IsAny<AuthorizationContext>(), It.IsAny<string>())).Returns(true);
            
            var env = new Mock<IExecutionEnvironment>();
            env.SetupAllProperties();
            var dataObject = new DsfDataObject(string.Empty, Guid.Empty)
            {
                Environment = env.Object,
                RawPayload = new StringBuilder("<raw>SomeData</raw>"),
                ReturnType = EmitionTypes.TEST,
                TestName = "*",
                ExecutingUser = principal.Object
            };

            var resourceId = Guid.NewGuid();
            var resourceName = @"Hello World";
            
            var resourceIds = new[] {resourceId};
            dataObject.TestsResourceIds = resourceIds;
            
            var mockResource = new Mock<IResource>();
            var mockWarewolfResource = mockResource.As<IWarewolfResource>();
            mockResource.Setup(o => o.ResourceID).Returns(resourceId);
            mockResource.Setup(o => o.ResourceName).Returns(resourceName);
            mockWarewolfResource.Setup(o => o.ResourceID).Returns(resourceId);
            mockWarewolfResource.Setup(o => o.ResourceName).Returns(resourceName);

            mockResource.Setup(o => o.GetResourcePath(It.IsAny<Guid>())).Returns(resourceName);
            
            var resourceCatalog = new Mock<IResourceCatalog>();
            resourceCatalog.Setup(o => o.GetResource(Guid.Empty, resourceName)).Returns(mockResource.Object);
            var resources = new List<IResource> {mockResource.Object};
            resourceCatalog.Setup(o => o.GetResources(Guid.Empty)).Returns(resources);
            
            var serviceTestModelTO = new Mock<IServiceTestModelTO>();
            serviceTestModelTO.Setup(to => to.Enabled).Returns(true);
            serviceTestModelTO.Setup(to => to.TestName).Returns("Test1");
            serviceTestModelTO.Setup(to => to.ResourceId).Returns(resourceId);
            
            var tests = new List<IServiceTestModelTO> {serviceTestModelTO.Object};
            var testCatalog = new Mock<ITestCatalog>();
            var mockCoverageCatalog = new Mock<ITestCoverageCatalog>();
            testCatalog.Setup(catalog => catalog.Fetch(It.IsAny<Guid>())).Returns(tests);
            var wRepo = new Mock<IWorkspaceRepository>();
            wRepo.SetupGet(repository => repository.ServerWorkspace).Returns(new Workspace(Guid.Empty));
            var handlerMock = new AbstractWebRequestHandlerMock(new TestAbstractWebRequestDataObjectFactory(dataObject), authorizationService.Object, resourceCatalog.Object, testCatalog.Object, mockCoverageCatalog.Object, wRepo.Object);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var webRequestTO = new WebRequestTO
            {
                Variables = new NameValueCollection
                {
                    {"IsDebug","true"}
                },
                WebServerUrl = ""
            };
            
            var responseWriter = handlerMock.CreateFromMock(webRequestTO, resourceName, null, new NameValueCollection(), principal.Object);
            //---------------Test Result -----------------------
            Assert.IsNotNull(responseWriter);

            string content = "";
            var request = new HttpRequestMessage(HttpMethod.Get, "")
            {
                Content = new StringContent(content, Encoding.UTF8)
                {
                    Headers = { ContentType = new MediaTypeHeaderValue("text/plain") }
                },
            };
            var boundVars = new NameValueCollection
            {
                { "name", "hello" }
            };
            var context = new WebServerContext(request, boundVars);
            
            responseWriter.Write(context);
            var text = context.ResponseMessage.Content.ReadAsStringAsync().Result;
            var obj = JsonConvert.DeserializeObject<TestResults>(text);
            Assert.IsNotNull(obj, $"failed to deserialize {nameof(TestResults)}");
            Assert.AreEqual(1, obj.Results.Count);
            testCatalog.Verify(o => o.Fetch(It.IsAny<Guid>()), Times.Once);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(AbstractWebRequestHandler))]
        public void AbstractWebRequestHandler_CreateForm_GivenEmitionTypeTESTAndIsRunAllTestsRequestTrue_ShouldFetchTests1()
        {
            //---------------Set up test pack-------------------
            var principal = new Mock<IPrincipal>();
            GetExecutingUser(principal);
            var authorizationService = new Mock<IAuthorizationService>();
            authorizationService.Setup(service => service.IsAuthorized(principal.Object, It.IsAny<AuthorizationContext>(), It.IsAny<string>())).Returns(true);
            var dataObject = new Mock<IDSFDataObject>();
            dataObject.SetupAllProperties();
            var env = new Mock<IExecutionEnvironment>();
            env.SetupAllProperties();
            dataObject.SetupGet(o => o.Environment).Returns(env.Object);
            dataObject.SetupGet(o => o.RawPayload).Returns(new StringBuilder("<raw>SomeData</raw>"));
            dataObject.SetupGet(o => o.ReturnType).Returns(EmitionTypes.TEST);
            dataObject.SetupGet(o => o.TestName).Returns("");
            dataObject.Setup(p => p.ExecutingUser).Returns(principal.Object);
            dataObject.Setup(o => o.Clone()).Returns(dataObject.Object);
            var resource = new Mock<IResource>();
            var resourceId = Guid.NewGuid();
            resource.SetupGet(resource1 => resource1.ResourceID).Returns(resourceId);
            resource.Setup(resource1 => resource1.GetResourcePath(It.IsAny<Guid>())).Returns(@"Home\HelloWorld");
            var resourceCatalog = new Mock<IResourceCatalog>();
            resourceCatalog.Setup(catalog => catalog.GetResources(It.IsAny<Guid>()))
                .Returns(new List<IResource>()
                {
                   resource.Object
                });
            resourceCatalog.Setup(o => o.GetResource(It.IsAny<Guid>(), It.IsAny<string>())).Returns(resource.Object);
            var testCatalog = new Mock<ITestCatalog>();
            var mockCoverageCatalog = new Mock<ITestCoverageCatalog>();
            var serviceTestModelTO = new Mock<IServiceTestModelTO>();
            serviceTestModelTO.Setup(to => to.Enabled).Returns(true);
            serviceTestModelTO.Setup(to => to.TestName).Returns("Test1");
            var tests = new List<IServiceTestModelTO>
            {
                serviceTestModelTO.Object
            };
            testCatalog.Setup(catalog => catalog.Fetch(Guid.Empty)).Returns(tests);
            var wRepo = new Mock<IWorkspaceRepository>();
            wRepo.SetupGet(repository => repository.ServerWorkspace).Returns(new Workspace(Guid.Empty));
            var handlerMock = new AbstractWebRequestHandlerMock(new TestAbstractWebRequestDataObjectFactory(dataObject.Object), authorizationService.Object, resourceCatalog.Object, testCatalog.Object, mockCoverageCatalog.Object, wRepo.Object);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var webRequestTO = new WebRequestTO()
            {
                Variables = new NameValueCollection()
                {
                    {"IsDebug","true"}
                },
                WebServerUrl = ""
            };
            var responseWriter = handlerMock.CreateFromMock(webRequestTO, "Hello World", null, new NameValueCollection(), principal.Object);
            //---------------Test Result -----------------------
            Assert.IsNotNull(responseWriter);
            testCatalog.Verify(o => o.Fetch(Guid.Empty), Times.Never);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(AbstractWebRequestHandler))]
        public void AbstractWebRequestHandler_BindRequestVariablesToDataObjectGivenHasBookMarkShouldSetDataObjectBookmark()
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
            var mockCoverageCatalog = new Mock<ITestCoverageCatalog>();
            var wRepo = new Mock<IWorkspaceRepository>();
            wRepo.SetupGet(repository => repository.ServerWorkspace).Returns(new Workspace(Guid.Empty));
            var handlerMock = new AbstractWebRequestHandlerMock(new TestAbstractWebRequestDataObjectFactory(dataObject.Object), authorizationService.Object, resourceCatalog.Object, testCatalog.Object, mockCoverageCatalog.Object, wRepo.Object);
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
        [TestCategory(nameof(AbstractWebRequestHandler))]
        public void AbstractWebRequestHandler_GetPostDataGivenUrlWithGetJsonDataShouldReturnPostData()
        {
            //---------------Set up test pack-------------------
            var communicationContext = new Mock<ICommunicationContext>();
            var payLoad = this.SerializeToJsonString(new DefaultSerializationBinder());
            var uriString = $"https://warewolf.atlassian.net/secure/RapidBoard.jspa?{payLoad}";
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
            var resource = new Mock<IResource>();
            var resourceId = Guid.NewGuid();
            resource.SetupGet(resource1 => resource1.ResourceID).Returns(resourceId);
            resource.Setup(resource1 => resource1.GetResourcePath(It.IsAny<Guid>())).Returns(@"Home\HelloWorld");
            var resourceCatalog = new Mock<IResourceCatalog>();
            resourceCatalog.Setup(catalog => catalog.GetResources(It.IsAny<Guid>()))
                .Returns(new List<IResource>()
                {
                   resource.Object
                });
            var testCatalog = new Mock<ITestCatalog>();
            var mockCoverageCatalog = new Mock<ITestCoverageCatalog>();
            var wRepo = new Mock<IWorkspaceRepository>();
            wRepo.SetupGet(repository => repository.ServerWorkspace).Returns(new Workspace(Guid.Empty));
            var handlerMock = new AbstractWebRequestHandlerMock(new TestAbstractWebRequestDataObjectFactory(dataObject.Object), authorizationService.Object, resourceCatalog.Object, testCatalog.Object, mockCoverageCatalog.Object, wRepo.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(handlerMock);
            //---------------Execute Test ----------------------
            var postDataMock = handlerMock.GetPostDataMock(communicationContext.Object);
            //---------------Test Result -----------------------
            Assert.AreEqual(payLoad, postDataMock);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(AbstractWebRequestHandler))]
        public void AbstractWebRequestHandler_GetPostDataGivenUrlWithJsonPostDataShouldReturnPostData()
        {
            //---------------Set up test pack-------------------
            var communicationContext = new Mock<ICommunicationContext>();
            var payLoad = this.SerializeToJsonString(new DefaultSerializationBinder());
            var uriString = $"https://warewolf.atlassian.net/secure/RapidBoard.jspa?{payLoad}";
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
            var mockCoverageCatalog = new Mock<ITestCoverageCatalog>();
            var wRepo = new Mock<IWorkspaceRepository>();
            wRepo.SetupGet(repository => repository.ServerWorkspace).Returns(new Workspace(Guid.Empty));
            var handlerMock = new AbstractWebRequestHandlerMock(new TestAbstractWebRequestDataObjectFactory(dataObject.Object), authorizationService.Object, resourceCatalog.Object, testCatalog.Object, mockCoverageCatalog.Object, wRepo.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(handlerMock);
            //---------------Execute Test ----------------------
            var postDataMock = handlerMock.GetPostDataMock(communicationContext.Object);
            //---------------Test Result -----------------------
            Assert.AreEqual(payLoad, postDataMock);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(AbstractWebRequestHandler))]
        public void AbstractWebRequestHandler_GetPostData_GivenPostDataInContext_ShouldReturnEmpty()
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
            var mockCoverageCatalog = new Mock<ITestCoverageCatalog>();
            var wRepo = new Mock<IWorkspaceRepository>();
            wRepo.SetupGet(repository => repository.ServerWorkspace).Returns(new Workspace(Guid.Empty));
            var handlerMock = new AbstractWebRequestHandlerMock(new TestAbstractWebRequestDataObjectFactory(dataObject.Object), authorizationService.Object, resourceCatalog.Object, testCatalog.Object, mockCoverageCatalog.Object, wRepo.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(handlerMock);
            //---------------Execute Test ----------------------
            var postDataMock = handlerMock.GetPostDataMock(communicationContext.Object);
            //---------------Test Result -----------------------
            Assert.AreEqual("", postDataMock);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(AbstractWebRequestHandler))]
        public void AbstractWebRequestHandler_GetPostData_GivenPostJsonDataInContextThrowsException_ShouldSwallowException()
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
            communicationContext.Setup(context => context.Request.QueryString).Throws(new Exception());
            var dataObject = new Mock<IDSFDataObject>();
            var authorizationService = new Mock<IAuthorizationService>();
            var resourceCatalog = new Mock<IResourceCatalog>();
            var testCatalog = new Mock<ITestCatalog>();
            var mockCoverageCatalog = new Mock<ITestCoverageCatalog>();
            var wRepo = new Mock<IWorkspaceRepository>();
            wRepo.SetupGet(repository => repository.ServerWorkspace).Returns(new Workspace(Guid.Empty));
            var handlerMock = new AbstractWebRequestHandlerMock(new TestAbstractWebRequestDataObjectFactory(dataObject.Object), authorizationService.Object, resourceCatalog.Object, testCatalog.Object, mockCoverageCatalog.Object, wRepo.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(handlerMock);
            //---------------Execute Test ----------------------

            //---------------Test Result -----------------------
            handlerMock.GetPostDataMock(communicationContext.Object);

        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(AbstractWebRequestHandler))]
        public void AbstractWebRequestHandler_GetPostData_GivenPostJsonDataInContext_ShouldReturnJsonData()
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
            var mockCoverageCatalog = new Mock<ITestCoverageCatalog>();
            var wRepo = new Mock<IWorkspaceRepository>();
            wRepo.SetupGet(repository => repository.ServerWorkspace).Returns(new Workspace(Guid.Empty));
            var handlerMock = new AbstractWebRequestHandlerMock(new TestAbstractWebRequestDataObjectFactory(dataObject.Object), authorizationService.Object, resourceCatalog.Object, testCatalog.Object, mockCoverageCatalog.Object, wRepo.Object);
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
        [TestCategory(nameof(AbstractWebRequestHandler))]
        public void AbstractWebRequestHandler_GetPostData_GivenJsonDataInContextAndUnknownWebMethod_ShouldReturnEmpty()
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
            var mockCoverageCatalog = new Mock<ITestCoverageCatalog>();
            var wRepo = new Mock<IWorkspaceRepository>();
            wRepo.SetupGet(repository => repository.ServerWorkspace).Returns(new Workspace(Guid.Empty));
            var handlerMock = new AbstractWebRequestHandlerMock(new TestAbstractWebRequestDataObjectFactory(dataObject.Object), authorizationService.Object, resourceCatalog.Object, testCatalog.Object, mockCoverageCatalog.Object, wRepo.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(handlerMock);
            //---------------Execute Test ----------------------
            var postDataMock = handlerMock.GetPostDataMock(communicationContext.Object);
            //---------------Test Result -----------------------
            Assert.AreEqual(string.Empty, postDataMock);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(AbstractWebRequestHandler))]
        public void AbstractWebRequestHandler_GetPostData_GivenPostXmlDataInContext_ShouldReturnXmlData()
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
            var mockCoverageCatalog = new Mock<ITestCoverageCatalog>();
            var wRepo = new Mock<IWorkspaceRepository>();
            wRepo.SetupGet(repository => repository.ServerWorkspace).Returns(new Workspace(Guid.Empty));
            var handlerMock = new AbstractWebRequestHandlerMock(new TestAbstractWebRequestDataObjectFactory(dataObject.Object), authorizationService.Object, resourceCatalog.Object, testCatalog.Object, mockCoverageCatalog.Object, wRepo.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(handlerMock);
            //---------------Execute Test ----------------------
            var postDataMock = handlerMock.GetPostDataMock(communicationContext.Object);
            //---------------Test Result -----------------------
            var isXml = StringExtension.IsXml(postDataMock);
            Assert.IsTrue(isXml);
            Assert.AreEqual(xmlData, postDataMock);
        }

        static string ConvertJsonToXml(string data)
        {
            var xml =
                XDocument.Load(JsonReaderWriterFactory.CreateJsonReader(Encoding.ASCII.GetBytes(data),
                    new XmlDictionaryReaderQuotas()));
            var xmlData = xml.ToString();
            return xmlData;
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(AbstractWebRequestHandler))]
        public void AbstractWebRequestHandler_GetPostData_GivenGetJsonDataInContext_ShouldReturnJsonData()
        {
            //---------------Set up test pack-------------------
            var communicationContext = new Mock<ICommunicationContext>();
            var data = this.SerializeToJsonString(new DefaultSerializationBinder());
            var uriString = $"https://warewolf.atlassian.net/secure/RapidBoard.jspa?&{data}";
            communicationContext.SetupGet(context => context.Request.Uri).Returns(new Uri(uriString));
            communicationContext.Setup(context => context.Request.Method).Returns("GET");
            communicationContext.Setup(context => context.Request.ContentEncoding).Returns(Encoding.Default);

            communicationContext.Setup(context => context.Request.BoundVariables).Returns(new NameValueCollection());
            communicationContext.Setup(context => context.Request.QueryString).Returns(new NameValueCollection());
            var dataObject = new Mock<IDSFDataObject>();
            var authorizationService = new Mock<IAuthorizationService>();
            var resourceCatalog = new Mock<IResourceCatalog>();
            var testCatalog = new Mock<ITestCatalog>();
            var mockCoverageCatalog = new Mock<ITestCoverageCatalog>();
            var wRepo = new Mock<IWorkspaceRepository>();
            wRepo.SetupGet(repository => repository.ServerWorkspace).Returns(new Workspace(Guid.Empty));
            var handlerMock = new AbstractWebRequestHandlerMock(new TestAbstractWebRequestDataObjectFactory(dataObject.Object), authorizationService.Object, resourceCatalog.Object, testCatalog.Object, mockCoverageCatalog.Object, wRepo.Object);
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
        [TestCategory(nameof(AbstractWebRequestHandler))]
        public void AbstractWebRequestHandler_GetPostData_GivenGetXmlDataInContext_ShouldReturnXmlData()
        {
            //---------------Set up test pack-------------------
            var communicationContext = new Mock<ICommunicationContext>();
            var data = this.SerializeToJsonString(new DefaultSerializationBinder());
            var xmlData = ConvertJsonToXml(data);
            var uriString = $"https://warewolf.atlassian.net/secure/RapidBoard.jspa?&{xmlData}";
            communicationContext.SetupGet(context => context.Request.Uri).Returns(new Uri(uriString));
            communicationContext.Setup(context => context.Request.Method).Returns("GET");
            communicationContext.Setup(context => context.Request.ContentEncoding).Returns(Encoding.Default);

            communicationContext.Setup(context => context.Request.BoundVariables).Returns(new NameValueCollection());
            communicationContext.Setup(context => context.Request.QueryString).Returns(new NameValueCollection());
            var dataObject = new Mock<IDSFDataObject>();
            var authorizationService = new Mock<IAuthorizationService>();
            var resourceCatalog = new Mock<IResourceCatalog>();
            var testCatalog = new Mock<ITestCatalog>();
            var mockCoverageCatalog = new Mock<ITestCoverageCatalog>();
            var wRepo = new Mock<IWorkspaceRepository>();
            wRepo.SetupGet(repository => repository.ServerWorkspace).Returns(new Workspace(Guid.Empty));
            var handlerMock = new AbstractWebRequestHandlerMock(new TestAbstractWebRequestDataObjectFactory(dataObject.Object), authorizationService.Object, resourceCatalog.Object, testCatalog.Object, mockCoverageCatalog.Object, wRepo.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(handlerMock);
            //---------------Execute Test ----------------------
            var postDataMock = handlerMock.GetPostDataMock(communicationContext.Object);
            //---------------Test Result -----------------------
            var isXml = StringExtension.IsXml(postDataMock);
            Assert.IsTrue(isXml);
            Assert.AreEqual(xmlData, postDataMock);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(AbstractWebRequestHandler))]
        public void AbstractWebRequestHandler_GetPostData_GivenGetDataListXmlDataInContext_ShouldReturnXmlData()
        {
            //---------------Set up test pack-------------------
            var communicationContext = new Mock<ICommunicationContext>();
            var data = this.SerializeToJsonString(new DefaultSerializationBinder());
            var myXmlNode = JsonConvert.DeserializeXmlNode(data, "DataList");
            var xmlData = myXmlNode.InnerXml;
            var uriString = $"https://warewolf.atlassian.net/secure/RapidBoard.jspa?&{xmlData}";
            communicationContext.SetupGet(context => context.Request.Uri).Returns(new Uri(uriString));
            communicationContext.Setup(context => context.Request.Method).Returns("GET");
            communicationContext.Setup(context => context.Request.ContentEncoding).Returns(Encoding.Default);

            communicationContext.Setup(context => context.Request.BoundVariables).Returns(new NameValueCollection());
            communicationContext.Setup(context => context.Request.QueryString).Returns(new NameValueCollection());
            var dataObject = new Mock<IDSFDataObject>();
            var authorizationService = new Mock<IAuthorizationService>();
            var resourceCatalog = new Mock<IResourceCatalog>();
            var testCatalog = new Mock<ITestCatalog>();
            var mockCoverageCatalog = new Mock<ITestCoverageCatalog>();
            var wRepo = new Mock<IWorkspaceRepository>();
            wRepo.SetupGet(repository => repository.ServerWorkspace).Returns(new Workspace(Guid.Empty));
            var handlerMock = new AbstractWebRequestHandlerMock(new TestAbstractWebRequestDataObjectFactory(dataObject.Object), authorizationService.Object, resourceCatalog.Object, testCatalog.Object, mockCoverageCatalog.Object, wRepo.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(handlerMock);
            //---------------Execute Test ----------------------
            var postDataMock = handlerMock.GetPostDataMock(communicationContext.Object);
            //---------------Test Result -----------------------
            var isXml = StringExtension.IsXml(postDataMock);
            Assert.IsTrue(isXml);
            Assert.AreEqual(xmlData, postDataMock);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(AbstractWebRequestHandler))]
        public void AbstractWebRequestHandler_LocationGivenReturnsCorrectly()
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
            var mockCoverageCatalog = new Mock<ITestCoverageCatalog>();
            var wRepo = new Mock<IWorkspaceRepository>();
            wRepo.SetupGet(repository => repository.ServerWorkspace).Returns(new Workspace(Guid.Empty));
            var handlerMock = new AbstractWebRequestHandlerMock(new TestAbstractWebRequestDataObjectFactory(dataObject.Object), authorizationService.Object, resourceCatalog.Object, testCatalog.Object, mockCoverageCatalog.Object, wRepo.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(handlerMock);
            //---------------Execute Test ----------------------
            var location = handlerMock.Location;
            //---------------Test Result -----------------------
            var locationCurrent = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string currentLocation = Path.GetDirectoryName(Path.GetDirectoryName(TestContext.TestRunDirectory));
            Assert.IsTrue(location == locationCurrent || location == currentLocation, location + " does not equal " + locationCurrent + " or " + currentLocation);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        [TestCategory(nameof(AbstractWebRequestHandler))]
        public void AbstractWebRequestHandler_RemoteInove_GivenServerInvoke_ShouldSetRemoteIdOnTheDataObjectAndRemoteInvokeTo_True()
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
            var mockCoverageCatalog = new Mock<ITestCoverageCatalog>();
            //------------Setup for test-------------------------
            var wRepo = new Mock<IWorkspaceRepository>();
            wRepo.SetupGet(repository => repository.ServerWorkspace).Returns(new Workspace(Guid.Empty));
            var handlerMock = new AbstractWebRequestHandlerMock(new TestAbstractWebRequestDataObjectFactory(dataObject.Object), authorizationService.Object, resourceCatalog.Object, testCatalog.Object, mockCoverageCatalog.Object, wRepo.Object);
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
        [TestCategory(nameof(AbstractWebRequestHandler))]
        public void AbstractWebRequestHandler_RemoteInvoke_GivenRemoteDebugInvoke_ShouldSetRemoteIdOnTheDataObjectAndRemoteInvokeTo_True()
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
            var mockCoverageCatalog = new Mock<ITestCoverageCatalog>();
            //------------Setup for test-------------------------
            var wRepo = new Mock<IWorkspaceRepository>();
            wRepo.SetupGet(repository => repository.ServerWorkspace).Returns(new Workspace(Guid.Empty));
            var handlerMock = new AbstractWebRequestHandlerMock(new TestAbstractWebRequestDataObjectFactory(dataObject.Object), authorizationService.Object, resourceCatalog.Object, testCatalog.Object, mockCoverageCatalog.Object, wRepo.Object);
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
        [TestCategory(nameof(AbstractWebRequestHandler))]
        public void AbstractWebRequestHandler_GetBookmark_GivenBoundVariables_ShouldReturnBookmark()
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
        [TestCategory(nameof(AbstractWebRequestHandler))]
        public void AbstractWebRequestHandler_GetClassName_GivenBoundVariables_ShouldReturnName()
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
        [TestCategory(nameof(AbstractWebRequestHandler))]
        public void AbstractWebRequestHandler_GetMethodName_GivenBoundVariables_ShouldReturnMethodName()
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
        [TestCategory(nameof(AbstractWebRequestHandler))]
        public void AbstractWebRequestHandler_GetPath_GivenBoundVariables_ShouldReturnPath()
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
        [TestCategory(nameof(AbstractWebRequestHandler))]
        public void AbstractWebRequestHandler_GetWebsite_GivenBoundVariables_ShouldReturnWebsite()
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
        [TestCategory(nameof(AbstractWebRequestHandler))]
        public void AbstractWebRequestHandler_GetInstanceID_GivenBoundVariables_ShouldReturnInstanceId()
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
        [TestCategory(nameof(AbstractWebRequestHandler))]
        public void AbstractWebRequestHandler_GetServiceName_GivenBoundVariables_ShouldReturnServiceName()
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
        [TestCategory(nameof(AbstractWebRequestHandler))]
        public void AbstractWebRequestHandler_GetWorkspaceID_GivenQueryString_ShouldReturnId()
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
        [TestCategory(nameof(AbstractWebRequestHandler))]
        public void AbstractWebRequestHandler_GetDataListID_GivenQueryString_ShouldReturnDatalsi()
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
        [TestCategory(nameof(AbstractWebRequestHandler))]
        public void AbstractWebRequestHandler_BuildTestResultJSONForWebRequest_GivenTestResultPassed_ShouldSetMessage()
        {
            //------------Setup for test-------------------------
            IServiceTestModelTO to = new ServiceTestModelTO();
            to.Result = new TestRunResult
            {
                RunTestResult = RunResult.TestPassed
            };
            var privateObject = new PrivateType(typeof(ServiceTestModelJObjectResultBuilder));
            //------------Execute Test---------------------------            
            var result = privateObject.InvokeStatic("BuildTestResultJSONForWebRequest", to);
            //------------Assert Results-------------------------
            Assert.IsTrue(result.ToString().Contains("\"Result\": \"Passed\""));
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        [TestCategory(nameof(AbstractWebRequestHandler))]
        public void AbstractWebRequestHandler_BuildTestResultJSONForWebRequest_GivenTestResultFailed_ShouldSetMessage()
        {
            //------------Setup for test-------------------------
            IServiceTestModelTO to = new ServiceTestModelTO();
            to.Result = new TestRunResult
            {
                RunTestResult = RunResult.TestFailed,
                Message = ""
            };
            var privateObject = new PrivateType(typeof(ServiceTestModelJObjectResultBuilder));
            //------------Execute Test---------------------------            
            var result = privateObject.InvokeStatic("BuildTestResultJSONForWebRequest", to);
            //------------Assert Results-------------------------
            Assert.IsTrue(result.ToString().Contains("\"Result\": \"Failed\""));
        }
        [TestMethod]
        [Owner("Sanele Mthembu")]
        [TestCategory(nameof(AbstractWebRequestHandler))]
        public void AbstractWebRequestHandler_BuildTestResultJSONForWebRequest_GivenTestResultInvalid_ShouldSetMessage()
        {
            //------------Setup for test-------------------------
            IServiceTestModelTO to = new ServiceTestModelTO();
            to.Result = new TestRunResult
            {
                RunTestResult = RunResult.TestInvalid,
                Message = ""
            };
            var privateObject = new PrivateType(typeof(ServiceTestModelJObjectResultBuilder));
            //------------Execute Test---------------------------            
            var result = privateObject.InvokeStatic("BuildTestResultJSONForWebRequest", to);
            //------------Assert Results-------------------------
            Assert.IsTrue(result.ToString().Contains("\"Result\": \"Invalid\""));
        }
        [TestMethod]
        [Owner("Sanele Mthembu")]
        [TestCategory(nameof(AbstractWebRequestHandler))]
        public void AbstractWebRequestHandler_BuildTestResultJSONForWebRequest_GivenTestResultTestResourceDeleted_ShouldSetMessage()
        {
            //------------Setup for test-------------------------
            IServiceTestModelTO to = new ServiceTestModelTO();
            to.Result = new TestRunResult
            {
                RunTestResult = RunResult.TestResourceDeleted,
                Message = ""
            };
            var privateObject = new PrivateType(typeof(ServiceTestModelJObjectResultBuilder));
            //------------Execute Test---------------------------            
            var result = privateObject.InvokeStatic("BuildTestResultJSONForWebRequest", to);
            //------------Assert Results-------------------------
            Assert.IsTrue(result.ToString().Contains("\"Result\": \"ResourceDelete\""));
        }
        [TestMethod]
        [Owner("Sanele Mthembu")]
        [TestCategory(nameof(AbstractWebRequestHandler))]
        public void AbstractWebRequestHandler_BuildTestResultJSONForWebRequest_GivenTestResultTestResourcePathUpdated_ShouldSetMessage()
        {
            //------------Setup for test-------------------------
            IServiceTestModelTO to = new ServiceTestModelTO();
            to.Result = new TestRunResult
            {
                RunTestResult = RunResult.TestResourcePathUpdated,
                Message = ""
            };
            var privateObject = new PrivateType(typeof(ServiceTestModelJObjectResultBuilder));
            //------------Execute Test---------------------------            
            var result = privateObject.InvokeStatic("BuildTestResultJSONForWebRequest", to);
            //------------Assert Results-------------------------
            Assert.IsTrue(result.ToString().Contains("\"Result\": \"ResourcpathUpdated\""));
        }
        [TestMethod]
        [Owner("Sanele Mthembu")]
        [TestCategory(nameof(AbstractWebRequestHandler))]
        public void AbstractWebRequestHandler_BuildTestResultJSONForWebRequest_GivenTestResultTestPending_ShouldSetMessage()
        {
            //------------Setup for test-------------------------
            IServiceTestModelTO to = new ServiceTestModelTO();
            to.Result = new TestRunResult
            {
                RunTestResult = RunResult.TestPending,
                Message = ""
            };
            var privateObject = new PrivateType(typeof(ServiceTestModelJObjectResultBuilder));
            //------------Execute Test---------------------------            
            var result = privateObject.InvokeStatic("BuildTestResultJSONForWebRequest", to);
            //------------Assert Results-------------------------
            Assert.IsTrue(result.ToString().Contains("\"Result\": \"Pending\""));
        }


        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(AbstractWebRequestHandler))]
        public void AbstractWebRequestHandler_BuildTestResultTRXForWebRequest_GivenTestResultPassed_ShouldSetMessage()
        {
            //------------Setup for test-------------------------
            var toList = new List<IServiceTestModelTO>();
            IServiceTestModelTO to = new ServiceTestModelTO();
            to.TestName = "Test 1";
            to.Result = new TestRunResult
            {
                RunTestResult = RunResult.TestPassed
            };
            toList.Add(to);
            var privateObject = new PrivateType(typeof(ServiceTestModelTRXResultBuilder));
            //------------Execute Test---------------------------            
            var result = privateObject.InvokeStatic("BuildTestResultTRX", "Hello World", toList);
            //------------Assert Results-------------------------
            Assert.IsTrue(result.ToString().Contains("outcome=\"Passed\""));
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(AbstractWebRequestHandler))]
        public void AbstractWebRequestHandler_BuildTestResultTRXForWebRequest_GivenTestResultFailed_ShouldSetMessage()
        {
            //------------Setup for test-------------------------
            var toList = new List<IServiceTestModelTO>();
            IServiceTestModelTO to = new ServiceTestModelTO();
            to.TestName = "Test 1";
            to.Result = new TestRunResult
            {
                RunTestResult = RunResult.TestFailed,
                Message = ""
            };
            toList.Add(to);
            var privateObject = new PrivateType(typeof(ServiceTestModelTRXResultBuilder));
            //------------Execute Test---------------------------            
            var result = privateObject.InvokeStatic("BuildTestResultTRX", "Hello World", toList);
            //------------Assert Results-------------------------
            Assert.IsTrue(result.ToString().Contains("outcome=\"Failed\""));
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(AbstractWebRequestHandler))]
        public void AbstractWebRequestHandler_BuildTestResultTRXForWebRequest_GivenTestResultInvalid_ShouldSetMessage()
        {
            //------------Setup for test-------------------------
            var toList = new List<IServiceTestModelTO>();
            IServiceTestModelTO to = new ServiceTestModelTO();
            to.TestName = "Test 1";
            to.Result = new TestRunResult
            {
                RunTestResult = RunResult.TestInvalid,
                Message = ""
            };
            toList.Add(to);
            var privateObject = new PrivateType(typeof(ServiceTestModelTRXResultBuilder));
            //------------Execute Test---------------------------            
            var result = privateObject.InvokeStatic("BuildTestResultTRX", "Hello World", toList);
            //------------Assert Results-------------------------
            Assert.IsTrue(result.ToString().Contains("outcome=\"Invalid\""));
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(AbstractWebRequestHandler))]
        public void AbstractWebRequestHandler_BuildTestResultTRXForWebRequest_GivenTestResultTestResourceDeleted_ShouldSetMessage()
        {
            //------------Setup for test-------------------------
            var toList = new List<IServiceTestModelTO>();
            IServiceTestModelTO to = new ServiceTestModelTO();
            to.TestName = "Test 1";
            to.Result = new TestRunResult
            {
                RunTestResult = RunResult.TestResourceDeleted,
                Message = ""
            };
            toList.Add(to);
            var privateObject = new PrivateType(typeof(ServiceTestModelTRXResultBuilder));
            //------------Execute Test---------------------------            
            var result = privateObject.InvokeStatic("BuildTestResultTRX", "Hello World", toList);
            //------------Assert Results-------------------------
            Assert.IsTrue(result.ToString().Contains("outcome=\"ResourceDeleted\""));
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(AbstractWebRequestHandler))]
        public void AbstractWebRequestHandler_BuildTestResultTRXForWebRequest_GivenTestResultTestResourcePathUpdated_ShouldSetMessage()
        {
            //------------Setup for test-------------------------
            var toList = new List<IServiceTestModelTO>();
            IServiceTestModelTO to = new ServiceTestModelTO();
            to.TestName = "Test 1";
            to.Result = new TestRunResult
            {
                RunTestResult = RunResult.TestResourcePathUpdated,
                Message = ""
            };
            toList.Add(to);
            var privateObject = new PrivateType(typeof(ServiceTestModelTRXResultBuilder));
            //------------Execute Test---------------------------            
            var result = privateObject.InvokeStatic("BuildTestResultTRX", "Hello World", toList);
            //------------Assert Results-------------------------
            Assert.IsTrue(result.ToString().Contains("outcome=\"ResourcePathUpdated\""));
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(AbstractWebRequestHandler))]
        public void AbstractWebRequestHandler_BuildTestResultTRXForWebRequest_GivenTestResultTestPending_ShouldSetMessage()
        {
            //------------Setup for test-------------------------
            var toList = new List<IServiceTestModelTO>();
            IServiceTestModelTO to = new ServiceTestModelTO();
            to.TestName = "Test 1";
            to.Result = new TestRunResult
            {
                RunTestResult = RunResult.TestPending,
                Message = ""
            };
            toList.Add(to);
            var privateObject = new PrivateType(typeof(ServiceTestModelTRXResultBuilder));
            //------------Execute Test---------------------------            
            var result = privateObject.InvokeStatic("BuildTestResultTRX", "Hello World", toList);
            //------------Assert Results-------------------------
            Assert.IsTrue(result.ToString().Contains("outcome=\"Pending\""));
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        [TestCategory(nameof(AbstractWebRequestHandler))]
        public void AbstractWebRequestHandler_ExtractKeyValuePairs_GivenKeyvaluePairs_ShouldCloneKeyValuePair()
        {
            var boundVariables = new NameValueCollection();

            AbstractWebRequestHandler.SubmittedData.ExtractKeyValuePairs(LocalBoundVariables, boundVariables);
            
            //The WID is skipped
            Assert.AreEqual(LocalBoundVariables.Count - 1, boundVariables.Count);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        [TestCategory(nameof(AbstractWebRequestHandler))]
        public void AbstractWebRequestHandler_CleanupXml_GivenXml_ShouldAppendXmlCorrectly()
        {
            //------------Setup for test-------------------------
            var privateObject = new PrivateType(typeof(AbstractWebRequestHandler));
            const string BaseStr = "www.examlple.com?home=<Datalist>DatalistPayload</Datalist>";
            //------------Execute Test---------------------------            
            var value = AbstractWebRequestHandler.SubmittedData.CleanupXml(BaseStr);
            //------------Assert Results-------------------------\
            var isNullOrEmpty = string.IsNullOrEmpty(value);
            Assert.IsFalse(isNullOrEmpty);
            var startsWith = value.StartsWith("www.examlple.com?home=~XML~");
            Assert.IsTrue(startsWith);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(AbstractWebRequestHandler))]
        public void AbstractWebRequestHandler_ExtractKeyValuePairForGetMethod_GivenEmptyPayload_ShouldUseContextQueryString()
        {
            //------------Setup for test-------------------------
            var privateObject = new PrivateType(typeof(AbstractWebRequestHandler));
            var mock = new Mock<ICommunicationContext>();
            mock.Setup(communicationContext => communicationContext.Request.QueryString)
                .Returns(new NameValueCollection());
            var context = mock.Object;

            //------------Execute Test---------------------------            
            AbstractWebRequestHandler.SubmittedData.ExtractKeyValuePairForGetMethod(context, "");

            //------------Assert Results-------------------------
            mock.VerifyGet(communicationContext => communicationContext.Request.QueryString, Times.Once);


        }


        [TestMethod]
        [Owner("Sanele Mthembu")]
        [TestCategory(nameof(AbstractWebRequestHandler))]
        public void AbstractWebRequestHandler_SetContentType_GivenJsonType_ShouldSetDataObjectReturnType()
        {
            var principal = new Mock<IPrincipal>();
            GetExecutingUser(principal);
            var authorizationService = new Mock<IAuthorizationService>();
            authorizationService.Setup(service => service.IsAuthorized(It.IsAny<AuthorizationContext>(), It.IsAny<string>())).Returns(true);
            var dataObject = new Mock<IDSFDataObject>();
            dataObject.SetupAllProperties();
            var env = new Mock<IExecutionEnvironment>();
            env.SetupAllProperties();
            dataObject.SetupGet(o => o.Environment).Returns(env.Object);
            dataObject.SetupGet(o => o.RawPayload).Returns(new StringBuilder("<raw>SomeData</raw>"));
            dataObject.Setup(p => p.ExecutingUser).Returns(principal.Object);
            var resourceCatalog = new Mock<IResourceCatalog>();
            var resourceId = Guid.NewGuid();
            var resource = new Mock<IResource>();
            resource.SetupGet(resource1 => resource1.ResourceID).Returns(resourceId);
            resourceCatalog.Setup(o => o.GetResource(It.IsAny<Guid>(), It.IsAny<string>())).Returns(resource.Object);
            var testCatalog = new Mock<ITestCatalog>();
            var mockCoverageCatalog = new Mock<ITestCoverageCatalog>();
            var wRepo = new Mock<IWorkspaceRepository>();
            wRepo.SetupGet(repository => repository.ServerWorkspace).Returns(new Workspace(Guid.Empty));
            var handlerMock = new AbstractWebRequestHandlerMock(new TestAbstractWebRequestDataObjectFactory(dataObject.Object), authorizationService.Object, resourceCatalog.Object, testCatalog.Object, mockCoverageCatalog.Object, wRepo.Object);
            //---------------Assert Precondition----------------
            var headers = new Mock<NameValueCollection>();
            headers.Setup(collection => collection.Get("Content-Type")).Returns("application/json");
            var webRequestTO = new WebRequestTO
            {
                ServiceName = ""
            };
            handlerMock.CreateFromMock(webRequestTO, "Hello World", Guid.Empty.ToString(), headers.Object, principal.Object);
            //---------------Execute Test ----------------------
            dataObject.Object.SetContentType(headers.Object);
            //------------Assert Results-------------------------
            Assert.AreEqual(EmitionTypes.JSON, dataObject.Object.ReturnType);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        [TestCategory(nameof(AbstractWebRequestHandler))]
        public void AbstractWebRequestHandler_SetContentType_GivenXMLType_ShouldSetDataObjectReturnType()
        {
            var principal = new Mock<IPrincipal>();
            GetExecutingUser(principal);
            var authorizationService = new Mock<IAuthorizationService>();
            authorizationService.Setup(service => service.IsAuthorized(It.IsAny<AuthorizationContext>(), It.IsAny<string>())).Returns(true);
            var dataObject = new Mock<IDSFDataObject>();
            dataObject.SetupAllProperties();
            var env = new Mock<IExecutionEnvironment>();
            env.SetupAllProperties();
            dataObject.SetupGet(o => o.Environment).Returns(env.Object);
            dataObject.SetupGet(o => o.RawPayload).Returns(new StringBuilder("<raw>SomeData</raw>"));
            dataObject.Setup(p => p.ExecutingUser).Returns(principal.Object);
            var resourceCatalog = new Mock<IResourceCatalog>();
            var resourceId = Guid.NewGuid();
            var resource = new Mock<IResource>();
            resource.SetupGet(resource1 => resource1.ResourceID).Returns(resourceId);
            resourceCatalog.Setup(o => o.GetResource(It.IsAny<Guid>(), It.IsAny<string>())).Returns(resource.Object);
            var testCatalog = new Mock<ITestCatalog>();
            var mockCoverageCatalog = new Mock<ITestCoverageCatalog>();
            var wRepo = new Mock<IWorkspaceRepository>();
            wRepo.SetupGet(repository => repository.ServerWorkspace).Returns(new Workspace(Guid.Empty));
            var handlerMock = new AbstractWebRequestHandlerMock(new TestAbstractWebRequestDataObjectFactory(dataObject.Object), authorizationService.Object, resourceCatalog.Object, testCatalog.Object, mockCoverageCatalog.Object, wRepo.Object);
            //---------------Assert Precondition----------------
            var headers = new Mock<NameValueCollection>();
            headers.Setup(collection => collection.Get("Content-Type")).Returns("application/xml");
            var webRequestTO = new WebRequestTO()
            {
                ServiceName = ""
            };
            handlerMock.CreateFromMock(webRequestTO, "Hello World", Guid.Empty.ToString(), headers.Object, principal.Object);
            //---------------Execute Test ----------------------            
            dataObject.Object.SetContentType(headers.Object);
            //------------Assert Results-------------------------
            Assert.AreEqual(EmitionTypes.XML, dataObject.Object.ReturnType);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(AbstractWebRequestHandler))]
        public void AbstractWebRequestHandler_SetEmitionType_GivenHeaderContentTypeJson_ShouldSetDataObjectContentTypeJson()
        {
            //---------------Set up test pack-------------------
            const string ServiceName = "hello World";
            var collection = new NameValueCollection
            {
                {"Content-Type", "Json"}
            };
            var dataObject = new Mock<IDSFDataObject>();
            dataObject.SetupProperty(o => o.ReturnType);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var invoke = dataObject.Object.SetEmissionType(null, ServiceName, collection);
            //---------------Test Result -----------------------
            dataObject.VerifySet(o => o.ReturnType = EmitionTypes.JSON, Times.Exactly(1));
            Assert.AreEqual(ServiceName, invoke);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(AbstractWebRequestHandler))]
        public void AbstractWebRequestHandler_SetEmitionType_GivenHeaderContentTypeXml_ShouldSetDataObjectContentTypeXml()
        {
            //---------------Set up test pack-------------------
            const string ServiceName = "hello World";
            var collection = new NameValueCollection
            {
                {"Content-Type", "xml"}
            };
            var dataObject = new Mock<IDSFDataObject>();
            dataObject.SetupProperty(o => o.ReturnType);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var emitionType = dataObject.Object.SetEmissionType(null, ServiceName, collection);
            //---------------Test Result -----------------------
            dataObject.VerifySet(o => o.ReturnType = EmitionTypes.XML, Times.Exactly(1));
            Assert.AreEqual(ServiceName, emitionType);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(AbstractWebRequestHandler))]
        public void AbstractWebRequestHandler_SetEmitionType_GivenNoHeaders_ShouldSetDataObjectContentTypeXml()
        {
            //---------------Set up test pack-------------------
            const string ServiceName = "hello World";
            var dataObject = new Mock<IDSFDataObject>();
            dataObject.SetupProperty(o => o.ReturnType);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var emitionType = dataObject.Object.SetEmissionType(null, ServiceName, null);
            //---------------Test Result -----------------------
            dataObject.VerifySet(o => o.ReturnType = EmitionTypes.XML, Times.Exactly(1));
            Assert.AreEqual(ServiceName, emitionType);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(AbstractWebRequestHandler))]
        public void AbstractWebRequestHandler_SetEmitionType_GivenServiceNameEndsWithapi_ShouldSetDataObjectContentTypeSwagger()
        {
            //---------------Set up test pack-------------------
            const string ServiceName = "hello World.api";
            var collection = new NameValueCollection();

            var dataObject = new Mock<IDSFDataObject>();
            dataObject.SetupProperty(o => o.ReturnType);
            dataObject.SetupProperty(o => o.ServiceName);
            dataObject.Setup(o => o.OriginalServiceName).Returns(ServiceName);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var emitionType = dataObject.Object.SetEmissionType(null, ServiceName, collection);
            //---------------Test Result -----------------------
            dataObject.VerifySet(o => o.ReturnType = EmitionTypes.SWAGGER, Times.Exactly(1));
            dataObject.VerifySet(o => o.ReturnType = EmitionTypes.XML, Times.Exactly(1));
            Assert.AreEqual("hello World", emitionType);
            Assert.AreEqual("hello World", dataObject.Object.ServiceName);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(AbstractWebRequestHandler))]
        public void AbstractWebRequestHandler_SetEmitionType_GivenServiceNameEndsWithtests_ShouldSetDataObjectContentTypeTests()
        {
            //---------------Set up test pack-------------------
            const string ServiceName = "hello World.tests";
            var collection = new NameValueCollection();

            var dataObject = new Mock<IDSFDataObject>();
            dataObject.SetupProperty(o => o.ReturnType);
            dataObject.SetupProperty(o => o.ServiceName);
            dataObject.Setup(o => o.OriginalServiceName).Returns(ServiceName);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var emitionType = dataObject.Object.SetEmissionType(null, ServiceName, collection);
            //---------------Test Result -----------------------
            dataObject.VerifySet(o => o.ReturnType = EmitionTypes.TEST, Times.Exactly(1));
            dataObject.VerifySet(o => o.ReturnType = EmitionTypes.XML, Times.Exactly(1));
            Assert.AreEqual("hello World", emitionType);
            Assert.AreEqual("hello World", dataObject.Object.ServiceName);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(AbstractWebRequestHandler))]
        public void AbstractWebRequestHandler_SetEmitionType_GivenServiceNameEndsWithteststrx_ShouldSetDataObjectContentTypeTRX()
        {
            //---------------Set up test pack-------------------
            const string ServiceName = "hello World.tests";
            var collection = new NameValueCollection();

            var dataObject = new Mock<IDSFDataObject>();
            dataObject.SetupProperty(o => o.ReturnType);
            dataObject.SetupProperty(o => o.ServiceName);
            dataObject.Setup(o => o.OriginalServiceName).Returns(ServiceName);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var emitionType = dataObject.Object.SetEmissionType(null, ServiceName, collection);
            //---------------Test Result -----------------------
            dataObject.VerifySet(o => o.ReturnType = EmitionTypes.TEST, Times.Exactly(1));
            Assert.AreEqual("hello World", emitionType);
            Assert.AreEqual("hello World", dataObject.Object.ServiceName);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(AbstractWebRequestHandler))]
        public void AbstractWebRequestHandler_SetEmitionType_GivenServiceNameEndsWithJson_ShouldSetDataObjectContentTypeJson()
        {
            //---------------Set up test pack-------------------
            const string ServiceName = "hello World.JSON";
            var collection = new NameValueCollection();

            var dataObject = new Mock<IDSFDataObject>();
            dataObject.SetupProperty(o => o.ReturnType);
            dataObject.SetupProperty(o => o.ServiceName);
            dataObject.Setup(o => o.OriginalServiceName).Returns(ServiceName);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var emitionType = dataObject.Object.SetEmissionType(null, ServiceName, collection);
            //---------------Test Result -----------------------
            dataObject.VerifySet(o => o.ReturnType = EmitionTypes.JSON, Times.Exactly(1));
            dataObject.VerifySet(o => o.ReturnType = EmitionTypes.XML, Times.Exactly(1));
            Assert.AreEqual("hello World", emitionType);
            Assert.AreEqual("hello World", dataObject.Object.ServiceName);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(AbstractWebRequestHandler))]
        public void AbstractWebRequestHandler_SetEmitionType_GivenTestsInFolder_ShouldSetDataObjectContentTypeTests()
        {
            //---------------Set up test pack-------------------
            const string ServiceName = "hello World.tests/";
            var collection = new NameValueCollection();

            var dataObject = new Mock<IDSFDataObject>();
            dataObject.SetupProperty(o => o.ReturnType);
            dataObject.SetupProperty(o => o.ServiceName);
            dataObject.SetupProperty(o => o.TestName);
            dataObject.Setup(o => o.OriginalServiceName).Returns(ServiceName);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var emitionType = dataObject.Object.SetEmissionType(null, ServiceName, collection);
            //---------------Test Result -----------------------
            dataObject.VerifySet(o => o.ReturnType = EmitionTypes.TEST, Times.Exactly(1));
            dataObject.VerifySet(o => o.ReturnType = EmitionTypes.XML, Times.Exactly(1));
            Assert.AreEqual("hello World", emitionType);
            Assert.AreEqual("hello World", dataObject.Object.ServiceName);
            Assert.AreEqual("*", dataObject.Object.TestName);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(AbstractWebRequestHandler))]
        public void AbstractWebRequestHandler_SetEmitionType_GivenServiceNameEndsWithtests_ShouldSetDataObjectIsTestExecution()
        {
            //---------------Set up test pack-------------------
            const string ServiceName = "hello World.tests";
            var collection = new NameValueCollection();

            var dataObject = new Mock<IDSFDataObject>();
            dataObject.SetupProperty(o => o.ReturnType);
            dataObject.SetupProperty(o => o.IsServiceTestExecution);
            dataObject.SetupProperty(o => o.ServiceName);
            dataObject.Setup(o => o.OriginalServiceName).Returns(ServiceName);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var emitionType = dataObject.Object.SetEmissionType(null, ServiceName, collection);
            //---------------Test Result -----------------------
            dataObject.VerifySet(o => o.ReturnType = EmitionTypes.TEST, Times.Exactly(1));
            dataObject.VerifySet(o => o.IsServiceTestExecution = true, Times.Exactly(1));
            dataObject.VerifySet(o => o.ReturnType = EmitionTypes.XML, Times.Exactly(1));
            Assert.AreEqual("hello World", emitionType);
            Assert.AreEqual("hello World", dataObject.Object.ServiceName);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(AbstractWebRequestHandler))]
        public void AbstractWebRequestHandler_SetTestResourceIds_GivenRequestForAllTestsInAFolder_ShouldSetDataObjectTestResourceIds()
        {
            //---------------Set up test pack-------------------
            //http://rsaklfnkosinath:3142/secure/Hello%20World.debug?Name=&wid=540beccb-b4f5-4b34-bc37-aa24b26370e2
            var webRequestTO = new WebRequestTO()
            {
                Variables = new NameValueCollection() { { "isPublic", "true" } },
                WebServerUrl = "http://rsaklfnkosinath:3142/public/Home/HelloWorld/.tests"
            };
            var resource = new Mock<IWarewolfWorkflow>();
            var resourceId = Guid.NewGuid();
            resource.SetupGet(resource1 => resource1.ResourceID).Returns(resourceId);
            //resource.Setup(resource1 => resource1.GetResourcePath(It.IsAny<Guid>())).Returns(@"Home\HelloWorld");
            var mockResourceCatalog = new Mock<IContextualResourceCatalog>();
            mockResourceCatalog.Setup(o => o.GetExecutableResources(It.IsAny<string>()))
                .Returns(new List<IWarewolfResource>()
                {
                   resource.Object
                });
            var dataObject = new Mock<IDSFDataObject>();
            dataObject.SetupProperty(o => o.ResourceID);
            dataObject.Setup(o => o.TestName).Returns("*");
            dataObject.SetupProperty(o => o.TestsResourceIds);
            dataObject.Setup(o => o.ReturnType).Returns(EmitionTypes.TEST);
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            dataObject.Object.SetTestResourceIds(mockResourceCatalog.Object, webRequestTO, "*", resource.Object);
            //---------------Test Result -----------------------
            dataObject.VerifySet(o => o.ResourceID = Guid.Empty, Times.Exactly(1));
            dataObject.VerifySet(o => o.TestsResourceIds = It.IsAny<Guid[]>(), Times.Exactly(1));
            var contains = dataObject.Object.TestsResourceIds.ToList().Contains(resourceId);
            Assert.IsTrue(contains);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(AbstractWebRequestHandler))]
        public void AbstractWebRequestHandler_SetTestResourceIds_GivenRequestForAllTests_ShouldSetDataObjectTestResourceIds()
        {
            //---------------Set up test pack-------------------
            var webRequestTO = new WebRequestTO()
            {
                Variables = new NameValueCollection() { { "isPublic", "true" } },
                WebServerUrl = "http://rsaklfnkosinath:3142/public/.tests"
            };
            var resourceCatalog = new Mock<IContextualResourceCatalog>();
            resourceCatalog.Setup(catalog => catalog.GetExecutableResources(It.IsAny<string>()))
                .Returns(new List<IWarewolfResource>());
            var dataObject = new Mock<IDSFDataObject>();
            dataObject.SetupProperty(o => o.ResourceID);
            dataObject.Setup(o => o.TestName).Returns("*");
            dataObject.SetupProperty(o => o.TestsResourceIds);
            dataObject.Setup(o => o.ReturnType).Returns(EmitionTypes.TEST);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            dataObject.Object.SetTestResourceIds(resourceCatalog.Object, webRequestTO, "*", null);
            //---------------Test Result -----------------------
            dataObject.VerifySet(o => o.ResourceID = Guid.Empty, Times.Exactly(1));
            dataObject.VerifySet(o => o.TestsResourceIds = It.IsAny<Guid[]>(), Times.Exactly(1));
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(AbstractWebRequestHandler))]
        public void AbstractWebRequestHandler_SetHeaders_ShouldSetDataObject_CustomTransactionID_ExecutionID()
        {
            var principal = new Mock<IPrincipal>();
            GetExecutingUser(principal);
            var authorizationService = new Mock<IAuthorizationService>();
            authorizationService.Setup(service => service.IsAuthorized(It.IsAny<AuthorizationContext>(), It.IsAny<string>())).Returns(true);
            var dataObject = new Mock<IDSFDataObject>();
            dataObject.SetupAllProperties();
            var env = new Mock<IExecutionEnvironment>();
            env.SetupAllProperties();
            dataObject.SetupGet(o => o.Environment).Returns(env.Object);
            dataObject.SetupGet(o => o.CustomTransactionID).Returns("");
            dataObject.SetupGet(o => o.RawPayload).Returns(new StringBuilder("<raw>SomeData</raw>"));
            dataObject.Setup(p => p.ExecutingUser).Returns(principal.Object);
            var resourceCatalog = new Mock<IResourceCatalog>();
            var resourceId = Guid.NewGuid();
            var resource = new Mock<IResource>();
            resource.SetupGet(resource1 => resource1.ResourceID).Returns(resourceId);
            resourceCatalog.Setup(o => o.GetResource(It.IsAny<Guid>(), It.IsAny<string>())).Returns(resource.Object);
            var testCatalog = new Mock<ITestCatalog>();
            var mockCoverageCatalog = new Mock<ITestCoverageCatalog>();
            var wRepo = new Mock<IWorkspaceRepository>();
            wRepo.SetupGet(repository => repository.ServerWorkspace).Returns(new Workspace(Guid.Empty));
            var handlerMock = new AbstractWebRequestHandlerMock(new TestAbstractWebRequestDataObjectFactory(dataObject.Object), authorizationService.Object, resourceCatalog.Object, testCatalog.Object, mockCoverageCatalog.Object, wRepo.Object);
            //---------------Assert Precondition----------------
            var testingCustomTransactionID = "testingCustomTransactionID";
            var executionId = Guid.NewGuid();
            var headers = new Mock<NameValueCollection>();
            headers.Setup(collection => collection.Get("Warewolf-Custom-Transaction-Id")).Returns(testingCustomTransactionID);
            headers.Setup(collection => collection.Get("Warewolf-Execution-Id")).Returns(executionId.ToString());
            var webRequestTO = new WebRequestTO()
            {
                ServiceName = ""
            };
            handlerMock.CreateFromMock(webRequestTO, "Hello World", Guid.Empty.ToString(), headers.Object, principal.Object);
            //---------------Execute Test ----------------------            
            dataObject.Object.SetHeaders(headers.Object);
            //------------Assert Results-------------------------
            Assert.AreEqual(testingCustomTransactionID, dataObject.Object.CustomTransactionID);
            Assert.AreEqual(executionId.ToString(), dataObject.Object.ExecutionID.ToString());
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(AbstractWebRequestHandler))]
        public void AbstractWebRequestHandler_SetHeaders_CustomTransactionID_ExecutionID_DoesNotExistOnHeader()
        {
            var principal = new Mock<IPrincipal>();
            GetExecutingUser(principal);
            var authorizationService = new Mock<IAuthorizationService>();
            authorizationService.Setup(service => service.IsAuthorized(It.IsAny<AuthorizationContext>(), It.IsAny<string>())).Returns(true);
            var dataObject = new Mock<IDSFDataObject>();
            dataObject.SetupAllProperties();
            var env = new Mock<IExecutionEnvironment>();
            env.SetupAllProperties();
            dataObject.SetupGet(o => o.Environment).Returns(env.Object);
            dataObject.SetupGet(o => o.RawPayload).Returns(new StringBuilder("<raw>SomeData</raw>"));
            dataObject.Setup(p => p.ExecutingUser).Returns(principal.Object);
            var resourceCatalog = new Mock<IResourceCatalog>();
            var resourceId = Guid.NewGuid();
            var resource = new Mock<IResource>();
            resource.SetupGet(resource1 => resource1.ResourceID).Returns(resourceId);
            resourceCatalog.Setup(o => o.GetResource(It.IsAny<Guid>(), It.IsAny<string>())).Returns(resource.Object);
            var testCatalog = new Mock<ITestCatalog>();
            var mockCoverageCatalog = new Mock<ITestCoverageCatalog>();
            var wRepo = new Mock<IWorkspaceRepository>();
            wRepo.SetupGet(repository => repository.ServerWorkspace).Returns(new Workspace(Guid.Empty));
            var handlerMock = new AbstractWebRequestHandlerMock(new TestAbstractWebRequestDataObjectFactory(dataObject.Object), authorizationService.Object, resourceCatalog.Object, testCatalog.Object, mockCoverageCatalog.Object, wRepo.Object);
            //---------------Assert Precondition----------------
            var headers = new Mock<NameValueCollection>();
            headers.Setup(collection => collection.Get("Content-Type")).Returns("application/json");
            var webRequestTO = new WebRequestTO()
            {
                ServiceName = ""
            };
            handlerMock.CreateFromMock(webRequestTO, "Hello World", Guid.Empty.ToString(), headers.Object, principal.Object);
            //---------------Execute Test ----------------------            
            dataObject.Object.SetHeaders(headers.Object);
            //------------Assert Results-------------------------
            Assert.AreEqual(null, dataObject.Object.CustomTransactionID);
            Assert.IsNotNull(dataObject.Object.ExecutionID); //New guid is created is nothing is set in the header
        }
    }

    class TestAbstractWebRequestDataObjectFactory : AbstractWebRequestHandler.IDataObjectFactory
    {
        readonly IDSFDataObject _dataObject;
        public TestAbstractWebRequestDataObjectFactory(IDSFDataObject dataObject)
        {
            _dataObject = dataObject;
        }
        public IDSFDataObject New(Guid workspaceGuid, IPrincipal user, string serviceName, WebRequestTO webRequest) => _dataObject;
    }

    class AbstractWebRequestHandlerMock : AbstractWebRequestHandler
    {
        public AbstractWebRequestHandlerMock(AbstractWebRequestHandler.IDataObjectFactory dataObjectFactory, IAuthorizationService service, IResourceCatalog catalog, ITestCatalog testCatalog, ITestCoverageCatalog testCoverageCatalog, IWorkspaceRepository repository)
            : base(catalog, testCatalog, testCoverageCatalog, repository, service, dataObjectFactory)
        {
        }

        public AbstractWebRequestHandlerMock()
        {
        }

        // protected static IResponseWriter CreateForm(WebRequestTO webRequest, string serviceName, string workspaceId, NameValueCollection headers, IPrincipal user = null)
        public IResponseWriter CreateFromMock(WebRequestTO webRequest, string serviceName, string workspaceId, NameValueCollection headers, IPrincipal user = null)
        {
            return CreateForm(webRequest, serviceName, workspaceId, headers, user);
        }

        //protected static string GetPostData(ICommunicationContext ctx)
        public string GetPostDataMock(ICommunicationContext ctx)
        {
            return SubmittedData.GetPostData(ctx);
        }

        //BindRequestVariablesToDataObject(WebRequestTO request, ref IDSFDataObject dataObject)
        public void BindRequestVariablesToDataObjectMock(WebRequestTO request, ref IDSFDataObject dataObject)
        {
            request.BindRequestVariablesToDataObject(ref dataObject);
        }

        public override void ProcessRequest(ICommunicationContext ctx)
        {
            throw new NotImplementedException();
        }

        public void RemoteInvokeMock(NameValueCollection headers, IDSFDataObject dataObject)
        {
            dataObject.SetupForRemoteInvoke(headers);
        }

        public string GetServiceNameMock(ICommunicationContext ctx)
        {
            return ctx.GetServiceName();
        }

        public string GetWorkspaceIDMock(ICommunicationContext ctx)
        {
            return ctx.GetWorkspaceID();
        }

        public string GetDataListIDMock(ICommunicationContext ctx)
        {
            return ctx.GetDataListID();
        }

        public string GetBookmarkMock(ICommunicationContext ctx)
        {
            return ctx.GetBookmark();
        }

        public string GetInstanceIDMock(ICommunicationContext ctx)
        {
            return ctx.GetInstanceID();
        }

        public string GetWebsiteMock(ICommunicationContext ctx)
        {
            return ctx.GetWebsite();
        }

        public string GetPathMock(ICommunicationContext ctx)
        {
            return ctx.GetPath();
        }

        public string GetClassNameMock(ICommunicationContext ctx)
        {
            return ctx.GetClassName();
        }

        public string GetMethodNameMock(ICommunicationContext ctx)
        {
            return ctx.GetMethodName();
        }
    }
}

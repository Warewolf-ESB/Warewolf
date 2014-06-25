using System;
using System.Text;
using Caliburn.Micro;
using Dev2.Core.Tests.Environments;
using Dev2.Services.Events;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;
using Dev2.Util;
using Dev2.Webs;
using Dev2.Webs.Callbacks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

// ReSharper disable InconsistentNaming
namespace Dev2.Core.Tests.Webs
{
    /// <summary>
    /// Summary description for RootWebSiteTest
    /// </summary> 
    [TestClass]
    public class RootWebSiteTest
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        // ReSharper disable InconsistentNaming

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("RootWebSite_ShowDialog")]
        public void RootWebSite_ShowDialog_WhenEditingMalformedWebService_ExpectSourceIDPassedThrough()
        {

            //------------Setup for test--------------------------
            CompositionInitializer.DefaultInitialize();

            RootWebSite.IsTestMode = true;

            Mock<IContextualResourceModel> model = new Mock<IContextualResourceModel>();
            Mock<IEnvironmentModel> env = new Mock<IEnvironmentModel>();
            Mock<IEnvironmentConnection> con = new Mock<IEnvironmentConnection>();

            AppSettings.LocalHost = "http://localhost:3142";

            Guid id;
            Guid.TryParse("4270372d-c6c1-457d-9f08-e36d69b71147", out id);
            const string xaml = @"<Action Name=""foobar"" Type=""InvokeWebService"" SourceID=""1afe38e9-a6f5-403d-9e52-06dd7ae11198"" SourceName=""dummy"" SourceMethod="""" RequestUrl="""" RequestMethod=""Post"" JsonPath="""">";

            con.Setup(c => c.WorkspaceID).Returns(Guid.Empty);
            con.Setup(c => c.DisplayName).Returns("Foobar");
            con.Setup(c => c.AppServerUri).Returns(new Uri("http://127.0.0.1:3142"));

            env.Setup(e => e.Connection).Returns(con.Object);

            model.Setup(m => m.Category).Returns("WebServiceTest");
            model.Setup(m => m.ID).Returns(id);
            model.Setup(m => m.WorkflowXaml).Returns(new StringBuilder(xaml));
            model.Setup(m => m.ServerResourceType).Returns("WebService");
            model.Setup(m => m.Environment).Returns(env.Object);

            //------------Execute Test---------------------------
            RootWebSite.ShowDialog(model.Object);
            var result = RootWebSite.TestModeRelativeUri;

            //------------Assert Results-------------------------
            const string expected = "services/webservice?wid=00000000-0000-0000-0000-000000000000&rid=4270372d-c6c1-457d-9f08-e36d69b71147&envir=Foobar+(http%3a%2f%2f127%252E0%252E0%252E1%3a3142%2f)&path=&sourceID=1afe38e9-a6f5-403d-9e52-06dd7ae11198&category=WebServiceTest";

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("RootWebSite_ShowDialog")]
        public void RootWebSite_ShowDialog_WhenEditingWebService_ExpectSourceIDPassedThrough()
        {

            //------------Setup for test--------------------------
            CompositionInitializer.DefaultInitialize();

            RootWebSite.IsTestMode = true;

            Mock<IContextualResourceModel> model = new Mock<IContextualResourceModel>();
            Mock<IEnvironmentModel> env = new Mock<IEnvironmentModel>();
            Mock<IEnvironmentConnection> con = new Mock<IEnvironmentConnection>();

            AppSettings.LocalHost = "http://localhost:3142";

            Guid id;
            Guid.TryParse("4270372d-c6c1-457d-9f08-e36d69b71147", out id);
            const string xaml = @"<Action Name=""foobar"" Type=""InvokeWebService"" SourceID=""1afe38e9-a6f5-403d-9e52-06dd7ae11198"" SourceName=""dummy"" SourceMethod="""" RequestUrl="""" RequestMethod=""Post"" JsonPath=""""></Action>";

            con.Setup(c => c.WorkspaceID).Returns(Guid.Empty);
            con.Setup(c => c.DisplayName).Returns("Foobar");
            con.Setup(c => c.AppServerUri).Returns(new Uri("http://127.0.0.1:3142"));

            env.Setup(e => e.Connection).Returns(con.Object);

            model.Setup(m => m.Category).Returns("WebServiceTest");
            model.Setup(m => m.ID).Returns(id);
            model.Setup(m => m.WorkflowXaml).Returns(new StringBuilder(xaml));
            model.Setup(m => m.ServerResourceType).Returns("WebService");
            model.Setup(m => m.Environment).Returns(env.Object);

            //------------Execute Test---------------------------
            RootWebSite.ShowDialog(model.Object);
            var result = RootWebSite.TestModeRelativeUri;

            //------------Assert Results-------------------------
            const string expected = "services/webservice?wid=00000000-0000-0000-0000-000000000000&rid=4270372d-c6c1-457d-9f08-e36d69b71147&envir=Foobar+(http%3a%2f%2f127%252E0%252E0%252E1%3a3142%2f)&path=&sourceID=1afe38e9-a6f5-403d-9e52-06dd7ae11198&category=WebServiceTest";

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("RootWebSite_ShowDialog")]
        public void RootWebSite_ShowDialog_WhenEditingWorkflowService_WorkflowServiceEmptyGuid_ExpectSaveDialog()
        {

            //------------Setup for test--------------------------
            CompositionInitializer.DefaultInitialize();

            RootWebSite.IsTestMode = true;

            Mock<IContextualResourceModel> model = new Mock<IContextualResourceModel>();
            Mock<IEnvironmentModel> env = new Mock<IEnvironmentModel>();
            Mock<IEnvironmentConnection> con = new Mock<IEnvironmentConnection>();

            AppSettings.LocalHost = "http://localhost:3142";

            Guid id;
            Guid.TryParse("4270372d-c6c1-457d-9f08-e36d69b71147", out id);

            con.Setup(c => c.WorkspaceID).Returns(Guid.Empty);
            con.Setup(c => c.DisplayName).Returns("Foobar");
            con.Setup(c => c.AppServerUri).Returns(new Uri("http://127.0.0.1:3142"));

            env.Setup(e => e.Connection).Returns(con.Object);

            model.Setup(m => m.Category).Returns("Test");
            model.Setup(m => m.ID).Returns(Guid.Empty);
            model.Setup(m => m.WorkflowXaml).Returns((StringBuilder)null);
            model.Setup(m => m.ServerResourceType).Returns("WorkflowService");
            model.Setup(m => m.DisplayName).Returns("WorkflowService");
            model.Setup(m => m.Environment).Returns(env.Object);

            //------------Execute Test---------------------------
            RootWebSite.ShowDialog(model.Object);
            var result = RootWebSite.TestModeRelativeUri;

            //------------Assert Results-------------------------
            const string expected = "dialogs/savedialog?wid=00000000-0000-0000-0000-000000000000&rid=&type=WorkflowService&title=New+Workflow&envir=Foobar+(http%3a%2f%2f127%252E0%252E0%252E1%3a3142%2f)&category=Test";

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("RootWebSite_ShowDialog")]
        public void RootWebSite_ShowDialog_WhenEditingWebServiceWithUrlParameters_ExpectSourceIDPassedThrough()
        {

            //------------Setup for test--------------------------
            CompositionInitializer.DefaultInitialize();

            RootWebSite.IsTestMode = true;

            Mock<IContextualResourceModel> model = new Mock<IContextualResourceModel>();
            Mock<IEnvironmentModel> env = new Mock<IEnvironmentModel>();
            Mock<IEnvironmentConnection> con = new Mock<IEnvironmentConnection>();

            AppSettings.LocalHost = "http://localhost:3142";

            Guid id;
            Guid.TryParse("4270372d-c6c1-457d-9f08-e36d69b71147", out id);
            const string xaml = @"<Action Name=""foobar"" Type=""InvokeWebService"" SourceID=""1afe38e9-a6f5-403d-9e52-06dd7ae11198"" SourceName=""dummy"" SourceMethod="""" RequestUrl=""?method=[[method]]&marketid=[[marketid]]"" RequestMethod=""Post"" JsonPath=""""></Action>";

            con.Setup(c => c.WorkspaceID).Returns(Guid.Empty);
            con.Setup(c => c.DisplayName).Returns("Foobar");
            con.Setup(c => c.AppServerUri).Returns(new Uri("http://127.0.0.1:3142"));

            env.Setup(e => e.Connection).Returns(con.Object);

            model.Setup(m => m.Category).Returns("WebServiceTest");
            model.Setup(m => m.ID).Returns(id);
            model.Setup(m => m.WorkflowXaml).Returns(new StringBuilder(xaml));
            model.Setup(m => m.ServerResourceType).Returns("WebService");
            model.Setup(m => m.Environment).Returns(env.Object);

            //------------Execute Test---------------------------
            RootWebSite.ShowDialog(model.Object);
            var result = RootWebSite.TestModeRelativeUri;

            //------------Assert Results-------------------------
            const string expected = "services/webservice?wid=00000000-0000-0000-0000-000000000000&rid=4270372d-c6c1-457d-9f08-e36d69b71147&envir=Foobar+(http%3a%2f%2f127%252E0%252E0%252E1%3a3142%2f)&path=&sourceID=1afe38e9-a6f5-403d-9e52-06dd7ae11198&category=WebServiceTest";

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("RootWebsite_ShowSaveDialog")]
        public void RootWebsite_ShowSaveDialog_AddToTabManagerIsFalse_IsFalseOnTheCallbackHandler()
        {
            //------------Setup for test--------------------------
            var resourceModel = SetupResourceModel();
            //------------Execute Test---------------------------
            RootWebSite.ShowNewWorkflowSaveDialog(resourceModel.Object, addToTabManager: false);
            //------------Assert Results-------------------------
            var saveCallBackHandler = RootWebSite.CallBackHandler as SaveNewWorkflowCallbackHandler;
            Assert.IsNotNull(saveCallBackHandler);
            Assert.IsFalse(saveCallBackHandler.AddToTabManager);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("RootWebsite_ShowSaveDialog")]
        public void RootWebsite_ShowSaveDialog_AddToTabManagerIsTrue_IsTrueOnTheCallbackHandler()
        {
            //------------Setup for test--------------------------
            var resourceModel = SetupResourceModel();
            //------------Execute Test---------------------------
            RootWebSite.ShowNewWorkflowSaveDialog(resourceModel.Object);
            //------------Assert Results-------------------------
            var saveCallBackHandler = RootWebSite.CallBackHandler as SaveNewWorkflowCallbackHandler;
            Assert.IsNotNull(saveCallBackHandler);
            Assert.IsTrue(saveCallBackHandler.AddToTabManager);
        }

        static Mock<IContextualResourceModel> SetupResourceModel()
        {
            CompositionInitializer.DefaultInitialize();
            RootWebSite.IsTestMode = true;
            Mock<IEnvironmentModel> environment = new Mock<IEnvironmentModel>();
            environment.SetupGet(r => r.Name).Returns("localhost");
            Mock<IEnvironmentConnection> connection = new Mock<IEnvironmentConnection>();
            connection.SetupGet(e => e.AppServerUri).Returns(new Uri("http://www.azure.com"));
            environment.SetupGet(r => r.Connection).Returns(connection.Object);
            var testEnvRepo = new TestEnvironmentRespository(environment.Object);
            // ReSharper disable ObjectCreationAsStatement
            new EnvironmentRepository(testEnvRepo);
            // ReSharper restore ObjectCreationAsStatement
            EventPublishers.Aggregator = new Mock<IEventAggregator>().Object;
            var resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.SetupGet(m => m.Environment).Returns(environment.Object);
            resourceModel.SetupGet(m => m.Category).Returns("MyFolder");
            return resourceModel;
        }

        // ReSharper restore InconsistentNaming
    }
}

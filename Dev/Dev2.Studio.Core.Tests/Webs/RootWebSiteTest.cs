using System;
using System.Text;
using Caliburn.Micro;
using Dev2.Common.Interfaces.Data;
using Dev2.Core.Tests.Environments;
using Dev2.Services.Events;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
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
        public void RootWebSite_ShowDialog_WhenEditingDbService_ExpectSourceIDPassedThrough()
        {

            //------------Setup for test--------------------------

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

            model.Setup(m => m.Category).Returns("DbServiceTest");
            model.Setup(m => m.ID).Returns(id);
            model.Setup(m => m.WorkflowXaml).Returns(new StringBuilder(xaml));
            model.Setup(m => m.ServerResourceType).Returns("DbService");
            model.Setup(m => m.Environment).Returns(env.Object);

            //------------Execute Test---------------------------
            RootWebSite.ShowDialog(model.Object);
            var result = RootWebSite.TestModeRelativeUri;

            //------------Assert Results-------------------------
            const string expected = "services/dbservice?wid=00000000-0000-0000-0000-000000000000&rid=4270372d-c6c1-457d-9f08-e36d69b71147&envir=Foobar+(http%3a%2f%2f127%252E0%252E0%252E1%3a3142%2f)&path=&sourceID=1afe38e9-a6f5-403d-9e52-06dd7ae11198&category=DbServiceTest";

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("RootWebSite_ShowDialog")]
        public void RootWebSite_ShowDialog_WhenEditingPluginService_ExpectSourceIDPassedThrough()
        {

            //------------Setup for test--------------------------

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

            model.Setup(m => m.Category).Returns("PluginServiceTest");
            model.Setup(m => m.ID).Returns(id);
            model.Setup(m => m.WorkflowXaml).Returns(new StringBuilder(xaml));
            model.Setup(m => m.ServerResourceType).Returns("PluginService");
            model.Setup(m => m.Environment).Returns(env.Object);

            //------------Execute Test---------------------------
            RootWebSite.ShowDialog(model.Object);
            var result = RootWebSite.TestModeRelativeUri;

            //------------Assert Results-------------------------
            const string expected = "services/pluginservice?wid=00000000-0000-0000-0000-000000000000&rid=4270372d-c6c1-457d-9f08-e36d69b71147&envir=Foobar+(http%3a%2f%2f127%252E0%252E0%252E1%3a3142%2f)&path=&sourceID=1afe38e9-a6f5-403d-9e52-06dd7ae11198&category=PluginServiceTest";

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("RootWebSite_ShowDialog")]
        public void RootWebSite_ShowDialog_WhenEditingPluginSource_ExpectSourceIDPassedThrough()
        {

            //------------Setup for test--------------------------

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

            model.Setup(m => m.Category).Returns("PluginSourceTest");
            model.Setup(m => m.ResourceName).Returns("PluginSourceTest");
            model.Setup(m => m.ID).Returns(id);
            model.Setup(m => m.WorkflowXaml).Returns(new StringBuilder(xaml));
            model.Setup(m => m.ServerResourceType).Returns("PluginSource");
            model.Setup(m => m.Environment).Returns(env.Object);

            //------------Execute Test---------------------------
            RootWebSite.ShowDialog(model.Object);
            var result = RootWebSite.TestModeRelativeUri;

            //------------Assert Results-------------------------
            const string expected = "sources/pluginsource?wid=00000000-0000-0000-0000-000000000000&rid=4270372d-c6c1-457d-9f08-e36d69b71147&envir=Foobar+(http%3a%2f%2f127%252E0%252E0%252E1%3a3142%2f)&path=&sourceID=4270372d-c6c1-457d-9f08-e36d69b71147&category=PluginSourceTest";

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("RootWebSite_ShowDialog")]
        public void RootWebSite_ShowDialog_WhenNewPluginSource_ExpectSourceIDPassedThrough()
        {

            //------------Setup for test--------------------------

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

            model.Setup(m => m.Category).Returns("PluginSourceTest");
            model.Setup(m => m.ID).Returns(Guid.Empty);
            model.Setup(m => m.WorkflowXaml).Returns(new StringBuilder(xaml));
            model.Setup(m => m.ServerResourceType).Returns("PluginSource");
            model.Setup(m => m.Environment).Returns(env.Object);

            //------------Execute Test---------------------------
            RootWebSite.ShowDialog(model.Object);
            var result = RootWebSite.TestModeRelativeUri;

            //------------Assert Results-------------------------
            const string expected = "sources/pluginsource?wid=00000000-0000-0000-0000-000000000000&rid=00000000-0000-0000-0000-000000000000&envir=Foobar+(http%3a%2f%2f127%252E0%252E0%252E1%3a3142%2f)&path=&sourceID=00000000-0000-0000-0000-000000000000&category=PluginSourceTest";

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("RootWebSite_ShowDialog")]
        public void RootWebSite_ShowDialog_WhenEditingDbSource_ExpectSourceIDPassedThrough()
        {

            //------------Setup for test--------------------------

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

            model.Setup(m => m.Category).Returns("DbSourceTest");
            model.Setup(m => m.ResourceName).Returns("DbSourceTest");
            model.Setup(m => m.ID).Returns(id);
            model.Setup(m => m.WorkflowXaml).Returns(new StringBuilder(xaml));
            model.Setup(m => m.ServerResourceType).Returns("DbSource");
            model.Setup(m => m.Environment).Returns(env.Object);

            //------------Execute Test---------------------------
            RootWebSite.ShowDialog(model.Object);
            var result = RootWebSite.TestModeRelativeUri;

            //------------Assert Results-------------------------
            const string expected = "sources/dbsource?wid=00000000-0000-0000-0000-000000000000&rid=4270372d-c6c1-457d-9f08-e36d69b71147&envir=Foobar+(http%3a%2f%2f127%252E0%252E0%252E1%3a3142%2f)&path=&sourceID=4270372d-c6c1-457d-9f08-e36d69b71147&category=DbSourceTest";

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("RootWebSite_ShowDialog")]
        public void RootWebSite_ShowDialog_WhenNewDbSource_ExpectSourceIDPassedThrough()
        {

            //------------Setup for test--------------------------

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

            model.Setup(m => m.Category).Returns("DbSourceTest");
            model.Setup(m => m.ID).Returns(Guid.Empty);
            model.Setup(m => m.WorkflowXaml).Returns(new StringBuilder(xaml));
            model.Setup(m => m.ServerResourceType).Returns("DbSource");
            model.Setup(m => m.Environment).Returns(env.Object);

            //------------Execute Test---------------------------
            RootWebSite.ShowDialog(model.Object);
            var result = RootWebSite.TestModeRelativeUri;

            //------------Assert Results-------------------------
            const string expected = "sources/dbsource?wid=00000000-0000-0000-0000-000000000000&rid=00000000-0000-0000-0000-000000000000&envir=Foobar+(http%3a%2f%2f127%252E0%252E0%252E1%3a3142%2f)&path=&sourceID=00000000-0000-0000-0000-000000000000&category=DbSourceTest";

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("RootWebSite_ShowDialog")]
        public void RootWebSite_ShowDialog_WhenEditingWebSource_ExpectSourceIDPassedThrough()
        {

            //------------Setup for test--------------------------

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

            model.Setup(m => m.Category).Returns("WebSourceTest");
            model.Setup(m => m.ResourceName).Returns("WebSourceTest");
            model.Setup(m => m.ID).Returns(id);
            model.Setup(m => m.WorkflowXaml).Returns(new StringBuilder(xaml));
            model.Setup(m => m.ServerResourceType).Returns("WebSource");
            model.Setup(m => m.Environment).Returns(env.Object);

            //------------Execute Test---------------------------
            RootWebSite.ShowDialog(model.Object);
            var result = RootWebSite.TestModeRelativeUri;

            //------------Assert Results-------------------------
            const string expected = "sources/websource?wid=00000000-0000-0000-0000-000000000000&rid=4270372d-c6c1-457d-9f08-e36d69b71147&envir=Foobar+(http%3a%2f%2f127%252E0%252E0%252E1%3a3142%2f)&path=&sourceID=4270372d-c6c1-457d-9f08-e36d69b71147&category=WebSourceTest";

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("RootWebSite_ShowDialog")]
        public void RootWebSite_ShowDialog_WhenNewWebSource_ExpectSourceIDPassedThrough()
        {

            //------------Setup for test--------------------------

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

            model.Setup(m => m.Category).Returns("WebSourceTest");
            model.Setup(m => m.Category).Returns("WebSourceTest");
            model.Setup(m => m.ID).Returns(Guid.Empty);
            model.Setup(m => m.WorkflowXaml).Returns(new StringBuilder(xaml));
            model.Setup(m => m.ServerResourceType).Returns("WebSource");
            model.Setup(m => m.Environment).Returns(env.Object);

            //------------Execute Test---------------------------
            RootWebSite.ShowDialog(model.Object);
            var result = RootWebSite.TestModeRelativeUri;

            //------------Assert Results-------------------------
            const string expected = "sources/websource?wid=00000000-0000-0000-0000-000000000000&rid=00000000-0000-0000-0000-000000000000&envir=Foobar+(http%3a%2f%2f127%252E0%252E0%252E1%3a3142%2f)&path=&sourceID=00000000-0000-0000-0000-000000000000&category=WebSourceTest";

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("RootWebSite_ShowDialog")]
        public void RootWebSite_ShowDialog_WhenEditingEmailSource_ExpectSourceIDPassedThrough()
        {

            //------------Setup for test--------------------------

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

            model.Setup(m => m.Category).Returns("EmailSourceTest");
            model.Setup(m => m.ResourceName).Returns("EmailSourceTest");
            model.Setup(m => m.ID).Returns(id);
            model.Setup(m => m.WorkflowXaml).Returns(new StringBuilder(xaml));
            model.Setup(m => m.ServerResourceType).Returns("EmailSource");
            model.Setup(m => m.Environment).Returns(env.Object);

            //------------Execute Test---------------------------
            RootWebSite.ShowDialog(model.Object);
            var result = RootWebSite.TestModeRelativeUri;

            //------------Assert Results-------------------------
            const string expected = "sources/emailsource?wid=00000000-0000-0000-0000-000000000000&rid=4270372d-c6c1-457d-9f08-e36d69b71147&envir=Foobar+(http%3a%2f%2f127%252E0%252E0%252E1%3a3142%2f)&path=&sourceID=4270372d-c6c1-457d-9f08-e36d69b71147&category=EmailSourceTest";

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("RootWebSite_ShowDialog")]
        public void RootWebSite_ShowDialog_WhenNewEmailSource_ExpectSourceIDPassedThrough()
        {

            //------------Setup for test--------------------------

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

            model.Setup(m => m.Category).Returns("EmailSourceTest");
            model.Setup(m => m.Category).Returns("EmailSourceTest");
            model.Setup(m => m.ID).Returns(Guid.Empty);
            model.Setup(m => m.WorkflowXaml).Returns(new StringBuilder(xaml));
            model.Setup(m => m.ServerResourceType).Returns("EmailSource");
            model.Setup(m => m.Environment).Returns(env.Object);

            //------------Execute Test---------------------------
            RootWebSite.ShowDialog(model.Object);
            var result = RootWebSite.TestModeRelativeUri;

            //------------Assert Results-------------------------
            const string expected = "sources/emailsource?wid=00000000-0000-0000-0000-000000000000&rid=00000000-0000-0000-0000-000000000000&envir=Foobar+(http%3a%2f%2f127%252E0%252E0%252E1%3a3142%2f)&path=&sourceID=00000000-0000-0000-0000-000000000000&category=EmailSourceTest";

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("RootWebSite_ShowDialog")]
        public void RootWebSite_ShowDialog_WhenEditingServerSource_ExpectSourceIDPassedThrough()
        {

            //------------Setup for test--------------------------

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

            model.Setup(m => m.Category).Returns("ServerSourceTest");
            model.Setup(m => m.ResourceName).Returns("ServerSourceTest");
            model.Setup(m => m.ID).Returns(id);
            model.Setup(m => m.WorkflowXaml).Returns(new StringBuilder(xaml));
            model.Setup(m => m.ServerResourceType).Returns("ServerSource");
            model.Setup(m => m.Environment).Returns(env.Object);

            //------------Execute Test---------------------------
            RootWebSite.ShowDialog(model.Object);
            var result = RootWebSite.TestModeRelativeUri;

            //------------Assert Results-------------------------
            const string expected = "sources/server?wid=00000000-0000-0000-0000-000000000000&rid=4270372d-c6c1-457d-9f08-e36d69b71147&envir=Foobar+(http%3a%2f%2f127%252E0%252E0%252E1%3a3142%2f)&path=&sourceID=1afe38e9-a6f5-403d-9e52-06dd7ae11198&category=ServerSourceTest";

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("RootWebSite_ShowDialog")]
        public void RootWebSite_ShowDialog_WhenEditingServer_ExpectSourceIDPassedThrough()
        {

            //------------Setup for test--------------------------

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

            model.Setup(m => m.Category).Returns("ServerTest");
            model.Setup(m => m.ResourceName).Returns("ServerTest");
            model.Setup(m => m.ID).Returns(id);
            model.Setup(m => m.WorkflowXaml).Returns(new StringBuilder(xaml));
            model.Setup(m => m.ServerResourceType).Returns("Server");
            model.Setup(m => m.Environment).Returns(env.Object);

            //------------Execute Test---------------------------
            RootWebSite.ShowDialog(model.Object);
            var result = RootWebSite.TestModeRelativeUri;

            //------------Assert Results-------------------------
            const string expected = "sources/server?wid=00000000-0000-0000-0000-000000000000&rid=4270372d-c6c1-457d-9f08-e36d69b71147&envir=Foobar+(http%3a%2f%2f127%252E0%252E0%252E1%3a3142%2f)&path=&sourceID=1afe38e9-a6f5-403d-9e52-06dd7ae11198&category=ServerTest";

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("RootWebSite_ShowDialog")]
        public void RootWebSite_ShowDialog_WhenNewServerSource_ExpectSourceIDPassedThrough()
        {

            //------------Setup for test--------------------------

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

            model.Setup(m => m.Category).Returns("ServerSourceTest");
            model.Setup(m => m.ID).Returns(Guid.Empty);
            model.Setup(m => m.WorkflowXaml).Returns(new StringBuilder(xaml));
            model.Setup(m => m.ServerResourceType).Returns("ServerSource");
            model.Setup(m => m.Environment).Returns(env.Object);

            //------------Execute Test---------------------------
            RootWebSite.ShowDialog(model.Object);
            var result = RootWebSite.TestModeRelativeUri;

            //------------Assert Results-------------------------
            const string expected = "sources/server?wid=00000000-0000-0000-0000-000000000000&rid=00000000-0000-0000-0000-000000000000&envir=Foobar+(http%3a%2f%2f127%252E0%252E0%252E1%3a3142%2f)&path=&sourceID=1afe38e9-a6f5-403d-9e52-06dd7ae11198&category=ServerSourceTest";

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("RootWebSite_ShowDialog")]
        public void RootWebSite_ShowDialog_WhenNewServer_ExpectSourceIDPassedThrough()
        {

            //------------Setup for test--------------------------

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

            model.Setup(m => m.Category).Returns("ServerTest");
            model.Setup(m => m.ID).Returns(Guid.Empty);
            model.Setup(m => m.WorkflowXaml).Returns(new StringBuilder(xaml));
            model.Setup(m => m.ServerResourceType).Returns("Server");
            model.Setup(m => m.Environment).Returns(env.Object);

            //------------Execute Test---------------------------
            RootWebSite.ShowDialog(model.Object);
            var result = RootWebSite.TestModeRelativeUri;

            //------------Assert Results-------------------------
            const string expected = "sources/server?wid=00000000-0000-0000-0000-000000000000&rid=00000000-0000-0000-0000-000000000000&envir=Foobar+(http%3a%2f%2f127%252E0%252E0%252E1%3a3142%2f)&path=&sourceID=1afe38e9-a6f5-403d-9e52-06dd7ae11198&category=ServerTest";

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("RootWebSite_ShowDialog")]
        public void RootWebSite_ShowDialog_WhenEditingWorkflowService_WorkflowServiceEmptyGuid_ExpectSaveDialog()
        {

            //------------Setup for test--------------------------

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
        [Owner("Hagashen Naidu")]
        [TestCategory("RootWebSite_ShowDialog")]
        public void RootWebSite_ShowDialog_WhenEditingWorkflowService_WithCategoryNotRoot_ExpectSaveDialog()
        {

            //------------Setup for test--------------------------

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

            model.Setup(m => m.Category).Returns("Test\\TestWorkflow");
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
        [Owner("Hagashen Naidu")]
        [TestCategory("RootWebSite_ShowDialog")]
        public void RootWebSite_ShowDialog_WhenEditingWebServiceWithCategoryNotRoot_ExpectSourceIDPassedThrough()
        {

            //------------Setup for test--------------------------

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

            model.Setup(m => m.Category).Returns("WebService\\WebServiceTest");
            model.Setup(m => m.ID).Returns(id);
            model.Setup(m => m.WorkflowXaml).Returns(new StringBuilder(xaml));
            model.Setup(m => m.ServerResourceType).Returns("WebService");
            model.Setup(m => m.Environment).Returns(env.Object);

            //------------Execute Test---------------------------
            RootWebSite.ShowDialog(model.Object);
            var result = RootWebSite.TestModeRelativeUri;

            //------------Assert Results-------------------------
            const string expected = "services/webservice?wid=00000000-0000-0000-0000-000000000000&rid=4270372d-c6c1-457d-9f08-e36d69b71147&envir=Foobar+(http%3a%2f%2f127%252E0%252E0%252E1%3a3142%2f)&path=&sourceID=1afe38e9-a6f5-403d-9e52-06dd7ae11198&category=WebService";

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("RootWebSite_ShowDialog")]
        public void RootWebSite_ShowDialog_WhenInvalidResourceType_ExpectFalse()
        {

            //------------Setup for test--------------------------

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
            model.Setup(m => m.ServerResourceType).Returns("Folder");
            model.Setup(m => m.Environment).Returns(env.Object);

            //------------Execute Test---------------------------
            var showDialog = RootWebSite.ShowDialog(model.Object);
            var result = RootWebSite.TestModeRelativeUri;

            //------------Assert Results-------------------------
            const string expected = "services/webservice?wid=00000000-0000-0000-0000-000000000000&rid=4270372d-c6c1-457d-9f08-e36d69b71147&envir=Foobar+(http%3a%2f%2f127%252E0%252E0%252E1%3a3142%2f)&path=&sourceID=1afe38e9-a6f5-403d-9e52-06dd7ae11198&category=WebServiceTest";
            Assert.AreNotEqual(expected, result);
            Assert.IsFalse(showDialog);
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


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("RootWebsite_ShowSaveDialog")]
        public void RootWebsite_ShowSaveDialog_ChangesActiveEnvironmentToResourceModelEnvironment()
        {
            //------------Setup for test--------------------------
            var resourceModel = SetupResourceModel();
            var mockEnvironmentModel = new Mock<IEnvironmentModel>();
            EnvironmentRepository.Instance.ActiveEnvironment = mockEnvironmentModel.Object;
            //------------Assert Precondition--------------------
            Assert.AreNotSame(resourceModel.Object.Environment, EnvironmentRepository.Instance.ActiveEnvironment);
            //------------Execute Test---------------------------
            RootWebSite.ShowNewWorkflowSaveDialog(resourceModel.Object);
            //------------Assert Results-------------------------
            Assert.AreSame(resourceModel.Object.Environment, EnvironmentRepository.Instance.ActiveEnvironment);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("RootWebsite_ShowSaveDialog")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RootWebsite_ShowSaveDialog_NullResource_Exception()
        {
            //------------Setup for test--------------------------
            var resourceModel = SetupResourceModel();
            var mockEnvironmentModel = new Mock<IEnvironmentModel>();
            EnvironmentRepository.Instance.ActiveEnvironment = mockEnvironmentModel.Object;
            //------------Assert Precondition--------------------
            Assert.AreNotSame(resourceModel.Object.Environment, EnvironmentRepository.Instance.ActiveEnvironment);
            //------------Execute Test---------------------------
            RootWebSite.ShowNewWorkflowSaveDialog(null);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("RootWebsite_ShowSaveDialog")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RootWebsite_ShowSaveDialog_NullEnvironmentModel_Exception()
        {
            //------------Setup for test--------------------------
            var resourceModel = SetupResourceModel();
            resourceModel.Setup(model => model.Environment).Returns((IEnvironmentModel)null);
            //------------Execute Test---------------------------
            RootWebSite.ShowNewWorkflowSaveDialog(resourceModel.Object);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("RootWebSite_ShowFileChooser")]
        public void RootWebSite_ShowFileChooser_Environment_HasCorrectUri()
        {
            //------------Setup for test--------------------------
            RootWebSite.IsTestMode = true;
            Mock<IEnvironmentModel> environment = new Mock<IEnvironmentModel>();
            environment.Setup(model => model.Name).Returns("localhost");
            var mockConnection = new Mock<IEnvironmentConnection>();
            mockConnection.Setup(connection => connection.DisplayName).Returns("localhost");
            mockConnection.Setup(connection => connection.AppServerUri).Returns(new Uri("http://localhost:3142"));
            environment.Setup(model => model.Connection).Returns(mockConnection.Object);
            var testEnvRepo = new TestEnvironmentRespository(environment.Object);
            // ReSharper disable ObjectCreationAsStatement
            new EnvironmentRepository(testEnvRepo);
            // ReSharper restore ObjectCreationAsStatement
            //------------Execute Test---------------------------
            RootWebSite.ShowFileChooser(environment.Object, new FileChooserMessage());
            //------------Assert Results-------------------------
            var result = RootWebSite.TestModeRelativeUri;
            Assert.AreEqual("dialogs/filechooser?envir=localhost+(http%3a%2f%2flocalhost%3a3142%2f)", result);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("RootWebSite_ShowDialog")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RootWebSite_ShowDialog_NullEnvironment_Exception()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            RootWebSite.ShowDialog(null, ResourceType.WorkflowService, "", "");
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("RootWebSite_ShowDialog")]
        public void RootWebSite_ShowDialog_NullEnvironmentConnection_ConnectCalled()
        {
            //------------Setup for test--------------------------
            var mockEnvironmentModel = new Mock<IEnvironmentModel>();
            mockEnvironmentModel.Setup(model => model.IsConnected).Returns(false);
            mockEnvironmentModel.Setup(model => model.Connect()).Verifiable();
            //------------Execute Test---------------------------
            var showDialog = RootWebSite.ShowDialog(mockEnvironmentModel.Object, ResourceType.WorkflowService, "", "");
            //------------Assert Results-------------------------
            Assert.IsFalse(showDialog);
            mockEnvironmentModel.Verify(model => model.Connect(), Times.Once());

        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("RootWebSite_ShowSwitchDragDialog")]
        public void RootWebSite_ShowSwitchDragDialog_Environment_ValidUri()
        {
            //------------Setup for test--------------------------
            RootWebSite.IsTestMode = true;
            var mockEnvironmentModel = new Mock<IEnvironmentModel>();
            mockEnvironmentModel.Setup(model => model.IsConnected).Returns(false);
            mockEnvironmentModel.Setup(model => model.Connect()).Verifiable();
            //------------Execute Test---------------------------
            var callbackHandler = RootWebSite.ShowSwitchDragDialog(mockEnvironmentModel.Object, "");
            //------------Assert Results-------------------------
            Assert.IsNotNull(callbackHandler);
            Assert.AreEqual("switch/drag", RootWebSite.TestModeRelativeUri);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("RootWebSite_ShowSwitchDragDialog")]
        public void RootWebSite_ShowSwitchDropDialog_Environment_ValidUri()
        {
            //------------Setup for test--------------------------
            RootWebSite.IsTestMode = true;
            var mockEnvironmentModel = new Mock<IEnvironmentModel>();
            mockEnvironmentModel.Setup(model => model.IsConnected).Returns(false);
            mockEnvironmentModel.Setup(model => model.Connect()).Verifiable();
            //------------Execute Test---------------------------
            var callbackHandler = RootWebSite.ShowSwitchDropDialog(mockEnvironmentModel.Object, "");
            //------------Assert Results-------------------------
            Assert.IsNotNull(callbackHandler);
            Assert.AreEqual("switch/drop", RootWebSite.TestModeRelativeUri);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("RootWebSite_ShowSwitchDragDialog")]
        public void RootWebSite_ShowDecisionDialog_Environment_ValidUri()
        {
            //------------Setup for test--------------------------
            RootWebSite.IsTestMode = true;
            var mockEnvironmentModel = new Mock<IEnvironmentModel>();
            mockEnvironmentModel.Setup(model => model.IsConnected).Returns(false);
            mockEnvironmentModel.Setup(model => model.Connect()).Verifiable();
            //------------Execute Test---------------------------
            var callbackHandler = RootWebSite.ShowDecisionDialog(mockEnvironmentModel.Object, "");
            //------------Assert Results-------------------------
            Assert.IsNotNull(callbackHandler);
            Assert.AreEqual("decisions/wizard", RootWebSite.TestModeRelativeUri);
        }

        static Mock<IContextualResourceModel> SetupResourceModel()
        {
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

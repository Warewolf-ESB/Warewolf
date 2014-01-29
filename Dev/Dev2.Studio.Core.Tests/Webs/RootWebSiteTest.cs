using System;
using System.Text;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Webs;
using Dev2.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

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
            const string expected = "services/webservice?wid=00000000-0000-0000-0000-000000000000&rid=4270372d-c6c1-457d-9f08-e36d69b71147&envir=Foobar+(http%3a%2f%2f127%252E0%252E0%252E1%3a3142%2f)&path=&sourceID=1afe38e9-a6f5-403d-9e52-06dd7ae11198";

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
            const string expected = "services/webservice?wid=00000000-0000-0000-0000-000000000000&rid=4270372d-c6c1-457d-9f08-e36d69b71147&envir=Foobar+(http%3a%2f%2f127%252E0%252E0%252E1%3a3142%2f)&path=&sourceID=1afe38e9-a6f5-403d-9e52-06dd7ae11198";

            Assert.AreEqual(expected, result);
        }
    }
}

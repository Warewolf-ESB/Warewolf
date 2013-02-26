using System;
using System.Xml.Linq;
using Caliburn.Micro;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Core.Tests
{
    /// <summary>
    ///This is a result class for EnvironmentModelTest and is intended
    ///to contain all EnvironmentModelTest Unit Tests
    ///</summary>
    [TestClass()]
    public class EnvironmentModelTest
    {
        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the result context which provides
        ///information about and functionality for the current result run.
        ///</summary>
        public TestContext TestContext
        {
            get { return testContextInstance; }
            set { testContextInstance = value; }
        }

        #region Connect Tests

        //5559 Check test
        //[TestMethod]
        //public void TestConnectDefault_Expected_DefaultEnvironmentConnectCalled() {

        //    Mock<IEnvironmentModel> mockEnvironment = new Mock<IEnvironmentModel>();
        //    mockEnvironment.Setup(connect => connect.Connect()).Verifiable();

        //    var env = new EnvironmentModel();
        //    env.DsfAddress = new Uri("http://localhost:77/dsf");
        //    env.Name = "result";
        //    env.WebServerPort = 1234;


        //    Mock<IEnvironmentConnection> envConnection = CreateFakeEnvironmentConnection();
        //    env.EnvironmentConnection = envConnection.Object;
        //    env.Connect();

        //    envConnection.Verify(c => c.Connect(), Times.Once());
        //}

        #endregion Connect Tests

        #region ToSourceDefinition

        [TestMethod]
        public void ToSourceDefinitionExpectedCategoryIsNotServers()
        {
            // BUG: 8786 - TWR - 2013.02.20
            var eventAggregator = new Mock<IEventAggregator>();
            var securityContext = new Mock<IFrameworkSecurityContext>();
            var environmentConnection = new Mock<IEnvironmentConnection>();
            environmentConnection.Setup(c => c.DisplayName).Returns(() => "TestEnv");

            var envModel = new EnvironmentModel(eventAggregator.Object, securityContext.Object, environmentConnection.Object)
            {
                ID = Guid.NewGuid(),
                DsfAddress = new Uri("http://localhost:1234/dsf"),
                WebServerPort = 77,
            };
            var sourceDef = envModel.ToSourceDefinition();
            var sourceXml = XElement.Parse(sourceDef);
            var category = sourceXml.ElementSafe("Category").ToUpper();
            Assert.AreNotEqual("SERVERS", category);
        }

        #endregion

    }
}

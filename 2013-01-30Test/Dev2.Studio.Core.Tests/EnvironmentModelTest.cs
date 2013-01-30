using Dev2.Studio.Core.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Moq;
using Unlimited.Framework;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Reflection;
using Dev2.Studio;
using Dev2.Composition;
using System.Collections.Generic;
using System.ComponentModel.Composition.Primitives;

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
    }
}

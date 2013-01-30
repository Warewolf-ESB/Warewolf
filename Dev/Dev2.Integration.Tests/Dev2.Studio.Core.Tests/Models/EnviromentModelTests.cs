using Dev2.Composition;
using Dev2.Integration.Tests.MEF;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Dev2.Integration.Tests.Dev2.Studio.Core.Tests.Models
{
    [TestClass]
    public class EnviromentModelTests
    {
        #region Connect Tests

        [TestMethod]
        public void EnvironmentModel_ConnectToAvailableServersAuxiliryChannelOnLocalhost_Expected_ConnectionSuccesful()
        {
            ImportService.CurrentContext = CompositionInitializer.DefaultInitialize();

            IEnvironmentModel environment = new EnvironmentModel();
            ImportService.SatisfyImports(environment);
            //Explicitly set the environment connection so that it uses a random username
            environment.EnvironmentConnection = new EnvironmentConnection(Guid.NewGuid().ToString(), "cake");
            environment.Name = "conn";
            environment.DsfAddress = new Uri(ServerSettings.DsfAddress);

            IEnvironmentModel auxEnvironment = new EnvironmentModel();
            ImportService.SatisfyImports(auxEnvironment);
            auxEnvironment.Name = "auxconn";
            auxEnvironment.DsfAddress = new Uri(ServerSettings.DsfAddress);

            environment.Connect();
            Assert.IsTrue(environment.IsConnected);

            auxEnvironment.Connect(environment);
            Assert.IsTrue(auxEnvironment.IsConnected);

            auxEnvironment.Disconnect();
            environment.Disconnect();
        }

        [TestMethod]
        public void EnvironmentModel_ConnectToAvailableServersAuxiliryChannelOnPCName_Expected_ConnectionSuccesful()
        {
            ImportService.CurrentContext = CompositionInitializer.DefaultInitialize();

            IEnvironmentModel environment = new EnvironmentModel();
            ImportService.SatisfyImports(environment);
            //Explicitly set the environment connection so that it uses a random username
            environment.EnvironmentConnection = new EnvironmentConnection(Guid.NewGuid().ToString(), "cake");
            environment.Name = "conn";
            environment.DsfAddress = new Uri(string.Format(ServerSettings.DsfAddressFormat, Environment.MachineName));

            IEnvironmentModel auxEnvironment = new EnvironmentModel();
            ImportService.SatisfyImports(auxEnvironment);
            auxEnvironment.Name = "auxconn";
            auxEnvironment.DsfAddress = new Uri(string.Format(ServerSettings.DsfAddressFormat, Environment.MachineName));

            environment.Connect();
            Assert.IsTrue(environment.IsConnected);

            auxEnvironment.Connect(environment);
            Assert.IsTrue(auxEnvironment.IsConnected);

            auxEnvironment.Disconnect();
            environment.Disconnect();
        }

        #endregion Connect Tests
    }
}

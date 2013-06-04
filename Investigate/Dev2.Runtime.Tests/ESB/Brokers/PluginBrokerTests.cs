using System;
using Dev2.Common;
using Dev2.Runtime.ServiceModel.Esb.Brokers;
using HgCo.WindowsLive.SkyDrive;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Runtime.ESB.Brokers
{
    // BUG 9500 - 2013.05.31 - TWR : refactored to this class
    [TestClass]
    public class PluginBrokerTests
    {
        #region ValidatePlugin

        [TestMethod]
        public void PluginBrokerValidatePluginWithBlockedDllExpectedValidationSucceeds()
        {
            //Initialize
            var broker = new PluginBroker();
            string error;
            var newVar = new SkyDriveServiceClient(); //must intantiate to pull actual dll file through >.<

            //Execute
            var result = broker.ValidatePlugin(EnvironmentVariables.ApplicationPath + "\\HgCo.WindowsLive.SkyDriveServiceClient.dll", out error);

            //Assert
            Assert.IsTrue(result, "Plugin validation failed for blocked Dll file");
        }

        #endregion


    }
}

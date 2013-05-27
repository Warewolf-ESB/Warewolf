using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.Data.ServiceModel;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Runtime.ServiceModel.Esb.Brokers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

// ReSharper disable CheckNamespace
namespace Dev2.Tests.Runtime.ServiceModel
// ReSharper restore CheckNamespace
{
    [TestClass]
    public class PluginSourceTests
    {
        #region ToString Tests

        [TestMethod]
        public void ToStringFullySetupObjectExpectedJsonSerializedObjectReturnedAsString()
        {
            PluginSource testPluginSource = SetupDefaultPluginSource();
            string actualPluginSourceToString = testPluginSource.ToString();
            string expected = JsonConvert.SerializeObject(testPluginSource);
            Assert.AreEqual(expected, actualPluginSourceToString);
        }

        [TestMethod]
        public void ToStringEmptyObjectExpected()
        {
            var testPluginSource = new PluginSource();
            string actualSerializedPluginSource = testPluginSource.ToString();
            string expected = JsonConvert.SerializeObject(testPluginSource);
            Assert.AreEqual(expected, actualSerializedPluginSource);
        }

        #endregion ToString Tests

        #region ToXml Tests

        [TestMethod]
        public void ToXmlAllPropertiesSetupExpectedXElementContainingAllObjectInformation()
        {

            PluginSource testPluginSource = SetupDefaultPluginSource();
            XElement expectedXml = testPluginSource.ToXml();

            IEnumerable<XAttribute> attrib = expectedXml.Attributes();
            IEnumerator<XAttribute> attribEnum = attrib.GetEnumerator();
            while(attribEnum.MoveNext())
            {
                if(attribEnum.Current.Name == "Name")
                {
                    Assert.AreEqual("TestResourceIMadeUp", attribEnum.Current.Value);
                    break;
                }
            }
        }

        [TestMethod]
        public void ToXmlEmptyObjectExpectedXElementContainingNoInformationRegardingSource()
        {
            var testPluginSource = new PluginSource();
            XElement expectedXml = testPluginSource.ToXml();

            IEnumerable<XAttribute> attrib = expectedXml.Attributes();
            IEnumerator<XAttribute> attribEnum = attrib.GetEnumerator();
            while(attribEnum.MoveNext())
            {
                if(attribEnum.Current.Name == "Name")
                {
                    Assert.AreEqual(string.Empty, attribEnum.Current.Value);
                    break;
                }
            }
        }

        #endregion ToXml Tests

        #region Validate Plugin

        [TestMethod]
        public void ValidatePluginWithBlockedDllExpectedValidationSucceeds()
        {
            //Initialize
            var broker = new PluginBroker();
            string error;
            var newVar = new HgCo.WindowsLive.SkyDrive.SkyDriveServiceClient();//must intantiate to pull actual dll file through >.<

            //Execute
            var result = broker.ValidatePlugin(EnvironmentVariables.ApplicationPath + "\\HgCo.WindowsLive.SkyDriveServiceClient.dll", out error);

            //Assert
            Assert.IsTrue(result, "Plugin validation failed for blocked Dll file");
        }

        #endregion

        #region Private Test Methods

        private PluginSource SetupDefaultPluginSource()
        {
            var testPluginSource = new PluginSource
            {
                AssemblyLocation = "Plugins\\someDllIMadeUpToTest.dll",
                AssemblyName = "dev2.test.namespacefortesting",
                ResourceID = Guid.NewGuid(),
                ResourceName = "TestResourceIMadeUp",
                ResourcePath = @"host\Server",
                ResourceType = ResourceType.PluginSource
            };

            return testPluginSource;
        }

        #endregion Private Test Methods
    }
}

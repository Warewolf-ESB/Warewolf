using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;
using Dev2.Common.Interfaces.Data;
using Dev2.Data.ServiceModel;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

// ReSharper disable CheckNamespace
namespace Dev2.Tests.Runtime.ServiceModel
// ReSharper restore CheckNamespace
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class DbSourceTests
    {
        #region ToString Tests

        [TestMethod]
        public void ToStringFullySetupObjectExpectedJsonSerializedObjectReturnedAsString()
        {
            DbSource testDbSource = SetupDefaultDbSource();
            string actualDbSourceToString = testDbSource.ToString();
            string expected = JsonConvert.SerializeObject(testDbSource);
            Assert.AreEqual(expected, actualDbSourceToString);
        }

        [TestMethod]
        public void ToStringEmptyObjectExpected()
        {
            var testDbSource = new DbSource();
            string actualSerializedDbSource = testDbSource.ToString();
            string expected = JsonConvert.SerializeObject(testDbSource);
            Assert.AreEqual(expected, actualSerializedDbSource);
        }

        #endregion ToString Tests

        #region ToXml Tests

        [TestMethod]
        public void ToXmlAllPropertiesSetupExpectedXElementContainingAllObjectInformation()
        {
            var testDbSource = SetupDefaultDbSource();
            var expectedXml = testDbSource.ToXml();
            var workflowXamlDefintion = expectedXml.Element("XamlDefinition");
            var attrib = expectedXml.Attributes();
            var attribEnum = attrib.GetEnumerator();
            while(attribEnum.MoveNext())
            {
                if(attribEnum.Current.Name == "Name")
                {
                    Assert.AreEqual("TestResourceIMadeUp", attribEnum.Current.Value);
                    break;
                }
            }
            Assert.IsNull(workflowXamlDefintion);
        }

        [TestMethod]
        public void ToXmlEmptyObjectExpectedXElementContainingNoInformationRegardingSource()
        {
            var testDbSource = new DbSource();
            XElement expectedXml = testDbSource.ToXml();

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

        #region Private Test Methods

        private DbSource SetupDefaultDbSource()
        {
            var testDbSource = new DbSource
            {
                Server = "someServerIMadeUpToTest",
                Port = 420,
                AuthenticationType = AuthenticationType.Windows,
                UserID = @"Domain\User",
                Password = "secret",
                DatabaseName = "someDatabaseNameIMadeUpToTest",
                ResourceID = Guid.NewGuid(),
                ResourceName = "TestResourceIMadeUp",
                ResourcePath = @"host\Server",
                ResourceType = ResourceType.DbSource
            };

            return testDbSource;
        }

        #endregion Private Test Methods
    }
}

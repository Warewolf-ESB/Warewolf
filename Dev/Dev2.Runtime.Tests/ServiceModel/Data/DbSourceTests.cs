using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Dev2.Common.ServiceModel;
using Dev2.Runtime.ServiceModel;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

// ReSharper disable CheckNamespace
namespace Dev2.Tests.Runtime.ServiceModel
// ReSharper restore CheckNamespace
{
    [TestClass]
    public class DbSourceTests
    {
        #region Test

        [TestMethod]
        public void TestWithInvalidUriFormatExpectedReturnsInvalidResult()
        {
            var conn = new DbSource
            {
                ResourceType = ResourceType.DbSource,
                Server = "http://www.google.co.za"
            };
            var dbSources = new DbSources();
            var result = dbSources.Test(JsonConvert.SerializeObject(conn), Guid.Empty, Guid.Empty);
            Assert.AreEqual(false, result.IsValid);

        }

        //[TestMethod]
        //public void Test_With_ValidUriFormatForInvalidHost_Expected_ReturnsInvalidResult()
        //{
        //    var conn = new DbSource
        //    {
        //        ResourceType = ResourceType.DbSource,
        //        //Server = "http://localhost:77/dsf"
        //        Server = "http://localhost:77/dsf"
        //    };
        //    var DbSources = new DbSources();
        //    var result = DbSources.Test(JsonConvert.SerializeObject(conn), Guid.Empty, Guid.Empty);
        //    Assert.AreEqual(false, result.IsValid);
        //}

        [TestMethod]
        public void TestWithValidUriFormatExpectedReturnsValidResult()
        {
            var conn = new DbSource
            {
                ResourceType = ResourceType.DbSource,
                Server = "http://192.168.13.42:788/dsf"
            };
            var dbSources = new DbSourcesMock { DatabaseValidationHitCount = 0 };
            var result = dbSources.Test(JsonConvert.SerializeObject(conn), Guid.Empty, Guid.Empty);
            Assert.AreEqual(1, dbSources.DatabaseValidationHitCount);
            Assert.AreEqual(true, result.IsValid);
        }

        #endregion

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

            DbSource testDbSource = SetupDefaultDbSource();
            XElement expectedXml = testDbSource.ToXml();

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

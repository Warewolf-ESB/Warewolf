using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Dev2.Data.ServiceModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Dev2.Tests.Runtime.ServiceModel.Data
{

    [TestClass]
    public class DropBoxSourceTests
    {
        #region ToString Tests

        [TestMethod]
        public void ToStringFullySetupObjectExpectedJsonSerializedObjectReturnedAsString()
        {
            DropBoxSource testDropBoxSource = SetupDefaultDropBoxSource();
            string actualDropBoxSourceToString = testDropBoxSource.ToString();
            string expected = JsonConvert.SerializeObject(testDropBoxSource);
            Assert.AreEqual(expected, actualDropBoxSourceToString);
        }

        [TestMethod]
        public void ToStringEmptyObjectExpected()
        {
            var testDropBoxSource = new DropBoxSource();
            string actualSerializedDropBoxSource = testDropBoxSource.ToString();
            string expected = JsonConvert.SerializeObject(testDropBoxSource);
            Assert.AreEqual(expected, actualSerializedDropBoxSource);
        }

        #endregion ToString Tests

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("DropBoxSource_AppKey")]
        public void DropBoxSource_AppKey_CannotBeEmpty()
        {
            //------------Setup for test--------------------------
            var dbSource = new DropBoxSource
            {
                AppKey = "",
                AccessToken = ""
            };
            //------------Execute Test---------------------------
            var appKey = dbSource.AppKey;
            //------------Assert Results-------------------------
            StringAssert.Contains(appKey, "");
        }

        #region ToXml Tests

        [TestMethod]
        public void ToXmlAllPropertiesSetupExpectedXElementContainingAllObjectInformation()
        {
            var testDropBoxSource = SetupDefaultDropBoxSource();
            var expectedXml = testDropBoxSource.ToXml();
            var workflowXamlDefintion = expectedXml.Element("XamlDefinition");
            var attrib = expectedXml.Attributes();
            var attribEnum = attrib.GetEnumerator();
            while (attribEnum.MoveNext())
            {
                if (attribEnum.Current.Name == "Name")
                {
                    Assert.AreEqual("TestResource", attribEnum.Current.Value);
                    break;
                }
            }
            Assert.IsNull(workflowXamlDefintion);
        }

        [TestMethod]
        public void ToXmlEmptyObjectExpectedXElementContainingNoInformationRegardingSource()
        {
            var testDropBoxSource = new DropBoxSource();
            XElement expectedXml = testDropBoxSource.ToXml();

            IEnumerable<XAttribute> attrib = expectedXml.Attributes();
            IEnumerator<XAttribute> attribEnum = attrib.GetEnumerator();
            while (attribEnum.MoveNext())
            {
                if (attribEnum.Current.Name == "Name")
                {
                    Assert.AreEqual(string.Empty, attribEnum.Current.Value);
                    break;
                }
            }
        }

        #endregion ToXml Tests

        #region Private Test Methods

        private DropBoxSource SetupDefaultDropBoxSource()
        {
            var testDropBoxSource = new DropBoxSource
            {
                ResourceID = Guid.NewGuid(),
                AppKey = "",
                AccessToken = "",
                ResourceName = "TestResource",
                ResourceType = "DropBoxSource"
            };

            return testDropBoxSource;
        }

        #endregion Private Test Methods
    }
}

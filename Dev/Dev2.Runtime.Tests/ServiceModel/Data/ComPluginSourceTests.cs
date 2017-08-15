using System;
using System.Xml.Linq;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Dev2.Tests.Runtime.ServiceModel
{
    [TestClass]
    public class ComPluginSourceTests
    {
  
        #region CTOR

        [TestMethod]
        public void ComPluginSourceContructorWithDefaultExpectedInitializesProperties()
        {
            var source = new ComPluginSource();
            Assert.AreEqual(Guid.Empty, source.ResourceID);
            Assert.AreEqual("ComPluginSource", source.ResourceType);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ComPluginSourceContructorWithNullXmlExpectedThrowsArgumentNullException()
        {
            var source = new ComPluginSource(null);
        }

        [TestMethod]
        public void ComPluginSourceContructorWithInvalidXmlExpectedDoesNotThrowExceptionAndInitializesProperties()
        {
            var xml = new XElement("root");
            var source = new ComPluginSource(xml);
            Assert.AreNotEqual(Guid.Empty, source.ResourceID);
            Assert.IsTrue(source.IsUpgraded);
            Assert.AreEqual("ComPluginSource", source.ResourceType);
        }

        #endregion

        #region ToXml

        [TestMethod]
        public void ComPluginSourceToXmlExpectedSerializesProperties()
        {
            var expected = new ComPluginSource
            {
                ClsId = "Plugins\\someDllIMadeUpToTest.dll",
                Is32Bit = false,
            };

            var xml = expected.ToXml();

            var actual = new ComPluginSource(xml);

            Assert.AreEqual(expected.ResourceType, actual.ResourceType);
            Assert.AreEqual(expected.ClsId, actual.ClsId);
            Assert.AreEqual(expected.Is32Bit, actual.Is32Bit);
        }

        [TestMethod]
        public void ComPluginSourceToXmlWithNullPropertiesExpectedSerializesPropertiesAsEmpty()
        {
            var expected = new ComPluginSource
            {
                ClsId = null,
                Is32Bit = false,
            };

            var xml = expected.ToXml();

            var actual = new ComPluginSource(xml);

            Assert.AreEqual(expected.ResourceType, actual.ResourceType);
            Assert.AreEqual("", actual.ClsId);
            Assert.AreEqual(false, actual.Is32Bit);
        }

        #endregion
    }
}
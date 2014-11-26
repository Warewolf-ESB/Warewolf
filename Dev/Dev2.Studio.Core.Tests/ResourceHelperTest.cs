
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Core.Tests
{

    // Sashen - 18-10-2012 - This class needs to be excluded
    /// <summary>
    ///This is a test class for ResourceHelperTest and is intended
    ///to contain all ResourceHelperTest Unit Tests
    ///</summary>
    [TestClass]
    public class ResourceHelperTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary> 
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///A test for GetWebPageElementNames were the XmlConfig is malformed
        ///</summary>
        [TestMethod]
        public void GetWebPageElementNames_Malformed_XmlConfig()
        {
            string xmlConfig = StringResourcesTest.Webpage_Malformed_XmlConfig;
            string actual;
            actual = ResourceHelper.GetWebPageElementNames(xmlConfig);
            Assert.AreEqual(actual, string.Empty);
        }

        /// <summary>
        ///A test for GetWebPageElementNames were the XmlConfig is normal
        ///</summary>
        [TestMethod]
        public void GetWebPageElementNames_PositiveTest()
        {
            string xmlConfig = StringResourcesTest.WebPartWizards_DuplicateNameCheck;
            string actual;
            actual = ResourceHelper.GetWebPageElementNames(xmlConfig);
            Assert.AreEqual(StringResourcesTest.GetWebPageElementNames_PositiveTestResult, actual);
        }

        /// <summary>
        ///A test for GetWebPageElementNames were the XmlConfig is blank
        ///</summary>
        [TestMethod]
        public void GetWebPageElementNames_BlankXmlConfig()
        {
            string actual;
            actual = ResourceHelper.GetWebPageElementNames("");
            Assert.AreEqual(string.Empty, actual);
        }

        // This seems to stop the Test Agent process for some reason, it may be related to threading 

        /// <summary>
        ///A test for MergeXmlConfig were the XmlConfig is normal
        ///</summary>
        [TestMethod]
        public void MergeXmlConfig_PositiveTest()
        {
            string xmlConfig = StringResourcesTest.NameRegion_xmlCofig;
            string elementList;
            elementList = ResourceHelper.GetWebPageElementNames(StringResourcesTest.WebPartWizards_DuplicateNameCheck);
            string actual = ResourceHelper.MergeXmlConfig(xmlConfig, elementList);
            Assert.AreEqual(StringResourcesTest.MergeXmlConfig_test_result, actual);
        }


        /// <summary>
        ///A test for MergeXmlConfig were the XmlConfig is blank
        ///</summary>
        [TestMethod]
        public void MergeXmlConfig_BlankXmlConfig()
        {
            string xmlConfig = StringResourcesTest.NameRegion_xmlCofig;
            string elementList;
            elementList = ResourceHelper.GetWebPageElementNames(StringResourcesTest.WebPartWizards_DuplicateNameCheck);
            string actual = ResourceHelper.MergeXmlConfig("", elementList);
            Assert.AreEqual(string.Empty, actual);
        }

        /// <summary>
        ///A test for MergeXmlConfig were the XmlConfig has no Dev2XmlResult tag
        ///</summary>
        [TestMethod]
        public void MergeXmlConfig_NoRootTag()
        {
            string xmlConfig = StringResourcesTest.NameRegion_xmlCofig;
            string elementList;
            elementList = ResourceHelper.GetWebPageElementNames(StringResourcesTest.WebPartWizards_DuplicateNameCheck);
            string actual = ResourceHelper.MergeXmlConfig(@"
  <XmlData />
  <Dev2ResumeData>
    <Dev2WebServer>http://127.0.0.1:1234</Dev2WebServer>
    <namedRegionBoundElement>Dev2elementNameButton</namedRegionBoundElement>
    <Async />
  </Dev2ResumeData>
", elementList);
            Assert.AreEqual(string.Empty, actual);
        }

        /// <summary>
        ///A test for MergeXmlConfig were the XmlConfig has two Dev2XmlResult tag
        ///</summary>
        [TestMethod]
        public void MergeXmlConfig_DoubleRootTag()
        {
            string xmlConfig = StringResourcesTest.NameRegion_xmlCofig;
            string elementList;
            elementList = ResourceHelper.GetWebPageElementNames(StringResourcesTest.WebPartWizards_DuplicateNameCheck);
            string actual = ResourceHelper.MergeXmlConfig(@"<Dev2XMLResult><Dev2XMLResult>
  <XmlData />
  <Dev2ResumeData>
    <Dev2WebServer>http://127.0.0.1:1234</Dev2WebServer>
    <namedRegionBoundElement>Dev2elementNameButton</namedRegionBoundElement>
    <Async />
  </Dev2ResumeData>
</Dev2XMLResult></Dev2XMLResult>", elementList);
            Assert.IsFalse(string.IsNullOrEmpty(actual));
        }

        [TestMethod]
        public void ResourceHelperReturnsEmptyDisplayNameForNullResource()
        {
            //Test
            var name = ResourceHelper.GetDisplayName(null);

            //Assert
            Assert.AreEqual(String.Empty, name);
        }

        [TestMethod]
        public void ResourceHelperReturnsResourceDisplayNameForNullEnvironment()
        {
            //Setup
            var model = new Mock<IContextualResourceModel>();
            model.Setup(m => m.ResourceName).Returns("Test");
            model.Setup(m => m.IsWorkflowSaved).Returns(true);

            //Test
            var name = ResourceHelper.GetDisplayName(model.Object);

            //Assert
            Assert.AreEqual("Test", name);
        }

        [TestMethod]
        public void ResourceHelper_UnitTest_WhenEnvironmentNullResourceIsWorkflowSavedFalse_ExpectResourceDisplayNameWithStar()
        {
            //Setup
            var model = new Mock<IContextualResourceModel>();
            model.Setup(m => m.ResourceName).Returns("Test");
            model.Setup(m => m.IsWorkflowSaved).Returns(false);

            //Test
            var name = ResourceHelper.GetDisplayName(model.Object);

            //Assert
            Assert.AreEqual("Test *", name);
        }

        [TestMethod]
        public void ResourceHelperReturnsResourceDisplayNameForLocalHost()
        {
            //Setup
            var env = new Mock<IEnvironmentModel>();
            env.Setup(e => e.IsLocalHost).Returns(true);

            var model = new Mock<IContextualResourceModel>();
            model.Setup(m => m.ResourceName).Returns("Test");
            model.Setup(m => m.Environment).Returns(env.Object);
            model.Setup(m => m.IsWorkflowSaved).Returns(true);
            //Test
            var name = ResourceHelper.GetDisplayName(model.Object);

            //Assert
            Assert.AreEqual("Test", name);
        }

        [TestMethod]
        [Description("Returned name has * when resource IsWorkflowSaved is false")]
        public void ResourceHelper_WhenLocalhostResourceIsWorkflowSavedFalse_ExpectResourceDisplayNameForLocalHostWithStar()
        {
            //Setup
            var env = new Mock<IEnvironmentModel>();
            env.Setup(e => e.IsLocalHost).Returns(true);

            var model = new Mock<IContextualResourceModel>();
            model.Setup(m => m.ResourceName).Returns("Test");
            model.Setup(m => m.Environment).Returns(env.Object);
            model.Setup(m => m.IsWorkflowSaved).Returns(false);

            //Test
            var name = ResourceHelper.GetDisplayName(model.Object);

            //Assert
            Assert.AreEqual("Test *", name);
        }

        [TestMethod]
        public void ResourceHelperReturnsResourceAndEnvironmentDisplayNameForNonLocalEnvironments()
        {
            //Setup
            var env = new Mock<IEnvironmentModel>();
            env.Setup(e => e.IsLocalHost).Returns(false);
            env.Setup(e => e.Name).Returns("HostName");

            var model = new Mock<IContextualResourceModel>();
            model.Setup(m => m.ResourceName).Returns("Test");
            model.Setup(m => m.Environment).Returns(env.Object);
            model.Setup(m => m.IsWorkflowSaved).Returns(true);
            //Test
            var name = ResourceHelper.GetDisplayName(model.Object);

            //Assert
            Assert.AreEqual("Test - HostName", name);
        }

        [TestMethod]
        [Description("Resource IsWorkflowSaved should show * in text")]
        [Owner("Huggs")]
        public void ResourceHelper_UnitTest_WhenResourceIsWorkflowSavedFalseAndEnvironmentDisplayNameForNonLocalEnvironments_ReturnsStarInText()
        {
            //Setup
            var env = new Mock<IEnvironmentModel>();
            env.Setup(e => e.IsLocalHost).Returns(false);
            env.Setup(e => e.Name).Returns("HostName");

            var model = new Mock<IContextualResourceModel>();
            model.Setup(m => m.ResourceName).Returns("Test");
            model.Setup(m => m.Environment).Returns(env.Object);
            model.Setup(m => m.IsWorkflowSaved).Returns(false);
            //Test
            var name = ResourceHelper.GetDisplayName(model.Object);

            //Assert
            Assert.AreEqual("Test - HostName *", name);
        }
    }
}

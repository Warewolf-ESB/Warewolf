
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


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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Dev2.Services.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Core.Tests.Services
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class UserConfigurationServiceTests
    {
        #region Static Class Init

        static string _testDir;

        [ClassInitialize]
        public static void MyClassInit(TestContext context)
        {
            _testDir = context.DeploymentDirectory;
        }

        #endregion

        [TestMethod]
        [TestCategory("UserConfigurationService_Instance")]
        [Description("UserConfigurationService instance must not be null and a singleton.")]
        [Owner("Trevor Williams-Ros")]
        public void UserConfigurationService_UnitTest_Instance_IsNotNull()
        {
            var instance = UserConfigurationService.Instance;
            Assert.IsNotNull(instance, "UserConfigurationService instance was null.");
        }

        [TestMethod]
        [TestCategory("UserConfigurationService_Instance")]
        [Description("UserConfigurationService instance must not be null and a singleton.")]
        [Owner("Trevor Williams-Ros")]
        public void UserConfigurationService_UnitTest_Instance_IsSingleton()
        {
            var instance = UserConfigurationService.Instance;
            var instance2 = UserConfigurationService.Instance;
            Assert.AreSame(instance, instance2, "UserConfigurationService instance was not a singleton.");
        }

        [TestMethod]
        [TestCategory("UserConfigurationService_Constructor")]
        [Description("UserConfigurationService constructor must have a non-null file path.")]
        [Owner("Trevor Williams-Ros")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void UserConfigurationService_UnitTest_ConstructorWithNullFilePath_ThrowsArgumentNullException()
        {
            var service = new UserConfigurationService(null);
        }

        [TestMethod]
        [TestCategory("UserConfigurationService_Constructor")]
        [Description("UserConfigurationService constructor must initialize properties.")]
        [Owner("Trevor Williams-Ros")]
        public void UserConfigurationService_UnitTest_Constructor_InitializesProperties()
        {
            var filePath = Path.Combine(_testDir, Guid.NewGuid() + ".config");
            var service = new UserConfigurationService(filePath);

            Assert.IsNotNull(service.Help, "Help property was not initialized by constructor.");
            Assert.IsNotNull(service.Help.IsCollapsed, "Help IsCollapsed property was not initialized by constructor.");
        }

      
        [TestMethod]
        [TestCategory("UserConfigurationService_Persistence")]
        [Description("UserConfigurationService must read/write it's properties to/from disk correctly.")]
        [Owner("Trevor Williams-Ros")]
        public void UserConfigurationService_UnitTest_PersistenceDispose_FileWritten()
        {
            var filePath = Path.Combine(_testDir, Guid.NewGuid() + ".config");
            var service = new UserConfigurationService(filePath);

            var exists = File.Exists(filePath);
            Assert.IsFalse(exists, "UserConfigurationService created file before test.");

            service.Dispose();
            exists = File.Exists(filePath);
            Assert.IsTrue(exists, "UserConfigurationService did not write file when disposed.");
        }

        [TestMethod]
        [TestCategory("UserConfigurationService_HelpIsCollapsed")]
        [Description("UserConfigurationService Help.IsCollapsed must return the value stored for the type in the dictionary.")]
        [Owner("Trevor Williams-Ros")]
        public void UserConfigurationService_UnitTest_HelpIsCollapsedWithType_ReturnsStoredValue()
        {
            var filePath = Path.Combine(_testDir, Guid.NewGuid() + ".config");

            var service = new UserConfigurationService(filePath);
            Assert.IsNotNull(service.Help, "Help property was not initialized by constructor.");
            Assert.IsNotNull(service.Help.IsCollapsed, "Help IsCollapsed property was not initialized by constructor.");

            const bool Expected = true;

            var type = typeof(DsfActivity);
            service.Help.IsCollapsed[type] = Expected;

            var actual = service.Help.IsCollapsed[type];

            Assert.AreEqual(Expected, actual, "Help.IsCollapsed did not return the value stored for the type in the dictionary.");
        }

        [TestMethod]
        [TestCategory("UserConfigurationService_HelpIsCollapsed")]
        [Description("UserConfigurationService Help.IsCollapsed must return false for a type not in the dictionary.")]
        [Owner("Trevor Williams-Ros")]
        public void UserConfigurationService_UnitTest_HelpIsCollapsedWithoutType_ReturnsFalse()
        {
            var filePath = Path.Combine(_testDir, Guid.NewGuid() + ".config");


            var service = new UserConfigurationService(filePath);

            var type = typeof(DsfActivity);
            var actual = service.Help.IsCollapsed[type];

            Assert.IsFalse(actual, "Help.IsCollapsed did not return false for a type not in the dictionary.");
        }
    }
}

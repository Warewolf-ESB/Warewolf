/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common;
using Dev2.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace Dev2.Core.Tests.Utils
{
    [TestClass]
    public class HelperUtilsTests
    {
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("HelperUtils")]
        public void HelperUtils_GetStudioLogSettingsConfigFile()
        {
            var settingsConfigFile = HelperUtils.GetStudioLogSettingsConfigFile();
            Assert.IsTrue(settingsConfigFile.Contains("Settings.config"));
        }
       
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("HelperUtils")]
        public void HelperUtils_SanitizePath()
        {            
            var path = "C:\\\\ProgramData\\Warewolf\\Server Log";
            var resourceName = "warewolf-Server.log";
            var resCat = HelperUtils.SanitizePath(path, resourceName);
            Assert.AreEqual("C:\\ProgramData\\Warewolf\\Server Log\\warewolf-Server.log", resCat);
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("HelperUtils")]
        public void HelperUtils_SanitizePath_StartsWith_Backslash_Root()
        {
            var path = "root\\\\Warewolf\\Server Log";
            var resourceName = "warewolf-Server.log";
            var resCat = HelperUtils.SanitizePath(path, resourceName);
            Assert.AreEqual("Warewolf\\Server Log\\warewolf-Server.log", resCat);
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("HelperUtils")]
        public void HelperUtils_SanitizePath_StartsWith_Backslash()
        {
            var path = "\\\\Warewolf\\Server Log";
            var resourceName = "warewolf-Server.log";
            var resCat = HelperUtils.SanitizePath(path, resourceName);
            Assert.AreEqual("\\Warewolf\\Server Log\\warewolf-Server.log", resCat);
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("HelperUtils")]
        public void HelperUtils_SanitizePath_Equals_Root()
        {
            var path = "root";
            var resourceName = "warewolf-Server.log";
            var resCat = HelperUtils.SanitizePath(path, resourceName);
            Assert.AreEqual("warewolf-Server.log", resCat);
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("HelperUtils")]
        public void HelperUtils_SanitizePath_Empty_ReturnNothing()
        {
            var resourceName = "warewolf-Server.log";
            var resCat = HelperUtils.SanitizePath("", resourceName);
            Assert.AreEqual("", resCat);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("HelperUtils")]
        public void HelperUtils_GetServerLogSettingsConfigFile()
        {
            var serverLogSettingsConfigFile = HelperUtils.GetServerLogSettingsConfigFile();
            Assert.IsTrue(serverLogSettingsConfigFile.Contains("warewolf-Server.log"));
        }
    }
}

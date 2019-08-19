/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Common.Wrappers;
using Dev2.Runtime.Triggers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Dev2.Tests.Runtime.Triggers
{
    [TestClass]
    public class TriggersCatalogTests
    {
        public static IDirectory DirectoryWrapperInstance()
        {
            return new DirectoryWrapper();
        }

        [TestInitialize]
        public void CleanupTestDirectory()
        {
            if (Directory.Exists(EnvironmentVariables.TriggersPath))
            {
                DirectoryWrapperInstance().CleanUp(EnvironmentVariables.TriggersPath);
            }
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(TriggersCatalog))]
        public void TriggersCatalog_Constructor_TestPathDoesNotExist_ShouldCreateIt()
        {
            //------------Setup for test--------------------------
            //------------Assert Preconditions-------------------
            Assert.IsFalse(Directory.Exists(EnvironmentVariables.TriggersPath));
            //------------Execute Test---------------------------
            new TriggersCatalog();
            //------------Assert Results-------------------------
            Assert.IsTrue(Directory.Exists(EnvironmentVariables.TriggersPath));
        }
    }
}

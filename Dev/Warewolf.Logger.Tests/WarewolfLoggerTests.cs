/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.Logger.Tests
{
    [TestClass]
    public class WarewolfLoggerTests
    {
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(LoggerContext))]
        public void LoggerContext_Contructor_Verbose_IsTrue()
        {
            var args = new Args
            {
                Verbose = true
            };
            var loggerContext = new LoggerContext(args,null);
            Assert.IsNotNull(loggerContext);
            Assert.IsTrue(loggerContext.Verbose);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(LoggerContext))]
        public void LoggerContext_Contructor_Verbose_IsFalse()
        {
            var args = new Args
            {
                Verbose = false
            };
            var loggerContext = new LoggerContext(args,null);
            Assert.IsNotNull(loggerContext);
            Assert.IsFalse(loggerContext.Verbose);
        }
        
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(LoggerContext))]
        public void LoggerContext_DataSource_LoggerConfig_IsNotNull()
        {
            var args = new Args
            {
                Verbose = false
            };
            var loggerContext = new LoggerContext(args,null);
            Assert.IsNotNull(loggerContext.Source);
            Assert.IsNotNull(loggerContext.LoggerConfig);
        }
    }
}

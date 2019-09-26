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
using Moq;
using System;
using Warewolf.Interfaces.Auditing;
using Warewolf.Logging;

namespace Warewolf.Logger.Tests
{
    [TestClass]
    public class LogServerTests
    {

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory(nameof(LogServer))]
        public void LogServer_Constructor_WhenValidParameters_ShouldNotThrowException()
        {
            //--------------------------------Arrange-------------------------------
            //--------------------------------Act-----------------------------------
            var logServer = new LogServer(new Mock<IWebSocketServerFactory>().Object, new Mock<IWriter>().Object, new Mock<ILoggerContext>().Object);
            //--------------------------------Assert--------------------------------
            Assert.IsNotNull(logServer);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory(nameof(LogServer))]
        [ExpectedException(typeof(ArgumentNullException))]
        public void LogServer_Constructor_WhenNullWebSocketFactory_ShouldNotThrowException()
        {
            //--------------------------------Arrange-------------------------------
            //--------------------------------Act-----------------------------------
            var logServer = new LogServer(null, new Mock<IWriter>().Object, new Mock<ILoggerContext>().Object);
            //--------------------------------Assert--------------------------------
            Assert.IsNull(logServer);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory(nameof(LogServer))]
        [ExpectedException(typeof(ArgumentNullException))]
        public void LogServer_Constructor_WhenNullWriter_ShouldNotThrowException()
        {
            //--------------------------------Arrange-------------------------------
            //--------------------------------Act-----------------------------------
            var logServer = new LogServer(new Mock<IWebSocketServerFactory>().Object, null, new Mock<ILoggerContext>().Object);
            //--------------------------------Assert--------------------------------
            Assert.IsNull(logServer);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory(nameof(LogServer))]
        [ExpectedException(typeof(ArgumentNullException))]
        public void LogServer_Constructor_WhenNullLoggerContext_ShouldNotThrowException()
        {
            //--------------------------------Arrange-------------------------------
            //--------------------------------Act-----------------------------------
            var logServer = new LogServer(new Mock<IWebSocketServerFactory>().Object, new Mock<IWriter>().Object, null);
            //--------------------------------Assert--------------------------------
            Assert.IsNull(logServer);
        }
    }
}

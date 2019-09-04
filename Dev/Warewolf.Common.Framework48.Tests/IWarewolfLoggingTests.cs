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

namespace Warewolf.Common.Framework48.Tests
{
    [TestClass]
    public class IWarewolfLoggingTests
    {
        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(IWarewolfLogging))]
        public void IWarewolfLogging_NewConnection_IsSuccessfull()
        {
            //--------------------------Arrange--------------------------
            var mockWarewolfLogging = new Mock<IWarewolfLogging>();
            var mockWarewolfLogger = new Mock<IWarewolfLogger>();

            //--------------------------Act------------------------------
            mockWarewolfLogger.Setup(o => o.IsSuccessful).Returns(true);
            mockWarewolfLogging.Setup(o => o.CreateLogger()).Returns(mockWarewolfLogger.Object);

            //--------------------------Assert---------------------------
            Assert.IsTrue(mockWarewolfLogger.Object.IsSuccessful);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(IWarewolfLogging))]
        public void IWarewolfLogging_File_PathConfiguration_IsSuccessfull()
        {
            //--------------------------Arrange--------------------------
            var mockWarewolfLogging = new Mock<IWarewolfLogging>();
            var mockWarewolfLogger = new Mock<IWarewolfLogger>();
            var mockWarewolfLoggerConfig = new Mock<IWarewolfLoggerConfig>();

            mockWarewolfLogger.Setup(o => o.IsSuccessful).Returns(true);
            mockWarewolfLogger.Setup(o => o.WriteTo.File(It.IsAny<string>()));
            mockWarewolfLogging.Setup(o => o.CreateLogger()).Returns(mockWarewolfLogger.Object);

            //--------------------------Act------------------------------
            var logConfig = mockWarewolfLoggerConfig.Object;
            logConfig.File(fileName: "testfile.txt");

            //--------------------------Assert---------------------------
            Assert.IsTrue(mockWarewolfLogger.Object.IsSuccessful);
            mockWarewolfLoggerConfig.Verify(o => o.File(It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(IWarewolfLogging))]
        public void IWarewolfLogging_File_PathConfiguration_With_OutputTemplate_IsSuccessfull()
        {
            //--------------------------Arrange--------------------------
            var mockWarewolfLogging = new Mock<IWarewolfLogging>();
            var mockWarewolfLogger = new Mock<IWarewolfLogger>();
            var mockWarewolfLogConfig = new Mock<IWarewolfLoggerConfig>();

            mockWarewolfLogger.Setup(o => o.IsSuccessful).Returns(true);
            mockWarewolfLogging.Setup(o => o.CreateLogger()).Returns(mockWarewolfLogger.Object);

            //--------------------------Act------------------------------
            var logConfig = mockWarewolfLogConfig.Object;
            logConfig.File(fileName: "testfile.txt", outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}");

            //--------------------------Assert---------------------------
            Assert.IsTrue(mockWarewolfLogger.Object.IsSuccessful);
            mockWarewolfLogConfig.Verify(o => o.File(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }
    }
}

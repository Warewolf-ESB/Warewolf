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
using Serilog;

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
            var mockLogger = new Mock<IWarewolfLogging>();
            var mockLogConnection = new Mock<ILogConnection>();

            //--------------------------Act------------------------------
            mockLogConnection.Setup(o => o.IsSuccessful).Returns(true);
            mockLogger.Setup(o => o.NewConnection()).Returns(mockLogConnection.Object);

            //--------------------------Assert---------------------------
            Assert.IsTrue(mockLogConnection.Object.IsSuccessful);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(IWarewolfLogging))]
        public void IWarewolfLogging_File_PathConfiguration_IsSuccessfull()
        {
            //--------------------------Arrange--------------------------
            var mockLogger = new Mock<IWarewolfLogging>();
            var mockLogConnection = new Mock<ILogConnection>();
            var mockWarewolfLogConfig = new Mock<IWarewolfLogConfig>();

            mockLogConnection.Setup(o => o.IsSuccessful).Returns(true);
            mockLogConnection.Setup(o => o.WriteTo.File(It.IsAny<string>()));
            mockLogger.Setup(o => o.NewConnection()).Returns(mockLogConnection.Object);

            //--------------------------Act------------------------------
            var logConfig = mockWarewolfLogConfig.Object;
            logConfig.File("file");

            //--------------------------Assert---------------------------
            Assert.IsTrue(mockLogConnection.Object.IsSuccessful);
            mockWarewolfLogConfig.Verify(o => o.File(It.IsAny<string>()), Times.Once);
        }
    }
}

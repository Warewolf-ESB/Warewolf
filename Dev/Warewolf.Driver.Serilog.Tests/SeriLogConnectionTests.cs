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
using System;
using Warewolf.Driver.Serilog;
using Warewolf.Streams;

namespace Warewolf.Driver.Serilog.Tests
{
    [TestClass]
    public class SeriLogConnectionTests
    {
        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(SeriLogConnection))]
        [ExpectedException(typeof(NotSupportedException), "SeriLog not supported in this mode")]
        public void SeriLogConnection_StartConsuming_Should_Throw_NotSupportedException()
        {
            //---------------------------------Arrange-------------------------------
            var mockSeriLogConfig = new Mock<ISeriLogConfig>();

            mockSeriLogConfig.Setup(o => o.Logger).Returns(new Mock<ILogger>().Object);

            //---------------------------------Act-----------------------------------
            using (var seriLogConnection = new SeriLogConnection(mockSeriLogConfig.Object))
            {
                seriLogConnection.StartConsuming(mockSeriLogConfig.Object, new Mock<IConsumer>().Object);
            };
            //---------------------------------Assert--------------------------------
        }
    }
}

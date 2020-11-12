/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using HangfireServer;

namespace Warewolf.HangfireServer.Tests
{
    [TestClass]
    public class HangfireContextTests
    {
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(HangfireContext))]
        public void HangfireContext_Verbose_Expect_False()
        {
            var mockArgs = new Mock<IArgs>();
            mockArgs.Setup(o => o.ShowConsole).Returns(false);
            var hangfireContext = new HangfireContext(mockArgs.Object);

            Assert.IsFalse(hangfireContext.Verbose);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(HangfireContext))]
        public void HangfireContext_Verbose_Expect_True()
        {
            var mockArgs = new Mock<IArgs>();
            mockArgs.Setup(o => o.ShowConsole).Returns(true);
            var hangfireContext = new HangfireContext(mockArgs.Object);

            Assert.IsTrue(hangfireContext.Verbose);
        }
    }
}
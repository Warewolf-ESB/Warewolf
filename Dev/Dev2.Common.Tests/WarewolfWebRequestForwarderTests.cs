/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Dev2.Common.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Common.Tests
{
    [TestClass]
    public class WarewolfWebRequestForwarderTests
    {
        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(IWarewolfWebRequestForwarder))]
        public void WarewolfWebRequestForwarder_ProcessMessage_Success()
        {
            //-----------------------------Arrange------------------------------
            var testWebRequestForwarder = new TestWarewolfWebRequestForwarder();
            //-----------------------------Act----------------------------------
            testWebRequestForwarder.ProcessMessage();
            //-----------------------------Assert-------------------------------
            Assert.IsTrue(testWebRequestForwarder.IsMessageProcessed);
        }

        private class TestWarewolfWebRequestForwarder : IWarewolfWebRequestForwarder
        {
            public bool IsMessageProcessed { get; private set; }

            public void ProcessMessage()
            {
                IsMessageProcessed = true;
            }
        }
    }
}

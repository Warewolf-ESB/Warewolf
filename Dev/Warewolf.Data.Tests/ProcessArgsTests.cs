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
using System.Linq;

namespace Warewolf.Data.Tests
{
    [TestClass]
    public class ProcessArgsTests
    {
        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ProcessArgs))]
        public void ProcessArgs_SetProperties_Success()
        {
            //-----------------------------Arrange-----------------------------
            var hostName = "rsaklfsvrdev.dev2.local";
            var workflowUrl = "http://t000420:1002/public/Hello%20World.json";
            var valueKey = "testValueKey";
            var queue = "testQueue";
            var userName = "testUserName";
            var password = "testPassword";

            var args = $"-q {queue} -w {workflowUrl} -v {valueKey} -h {hostName} -u {userName} -p {password}".Split(" ").ToArray();
            //-----------------------------Act---------------------------------
            var processArgs = new ProcessArgs(args: args);
            //-----------------------------Assert------------------------------
            Assert.AreEqual(hostName, processArgs.HostName);
            Assert.AreEqual(workflowUrl, processArgs.WorkflowUrl);
            Assert.AreEqual(valueKey, processArgs.ValueKey);
            Assert.AreEqual(queue, processArgs.QueueName);
            Assert.AreEqual(userName, processArgs.UserName);
            Assert.AreEqual(password, processArgs.Password);
        }
    }
}

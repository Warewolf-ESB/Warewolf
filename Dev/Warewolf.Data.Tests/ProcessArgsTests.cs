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
        [TestCategory(nameof(CommandLine))]
        public void CommandLine_ParseArguments_SetProperties_Success()
        {
            //-----------------------------Arrange-----------------------------
            var filename = "rsaklfsvrdev.dev2.local";

            var args = $"-f {filename}".Split(" ").ToArray();
            //-----------------------------Act---------------------------------
            var processArgs = CommandLine.ParseArguments(args: args);
            //-----------------------------Assert------------------------------
            Assert.AreEqual(filename, processArgs.TriggerId);
        }
    }
}

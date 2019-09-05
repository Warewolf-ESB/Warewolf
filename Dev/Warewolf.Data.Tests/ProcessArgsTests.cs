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
using System;
using System.Linq;

namespace Warewolf.Data.Tests
{
    [TestClass]
    public class ProcessArgsTests
    {
        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(CommandLineParser))]
        public void CommandLine_ParseArguments_SetProperties_Success()
        {
            //-----------------------------Arrange-----------------------------
            var id = Guid.NewGuid().ToString();

            var args = $"-c {id} -v".Split(" ").ToArray();
            //-----------------------------Act---------------------------------
            var processArgs = CommandLineParser.ParseArguments<Args>(args: args);
            //-----------------------------Assert------------------------------
            Assert.AreEqual(id, processArgs.TriggerId);
            Assert.AreEqual(true, processArgs.ShowConsole);
        }
    }
}

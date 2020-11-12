/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Linq;
using CommandLine;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HangfireServer;

[assembly: Parallelize(Workers = 0, Scope = ExecutionScope.MethodLevel)]
namespace Warewolf.HangfireServer.Tests
{
    [TestClass]
    public class HangfireArgsTests
    {
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(HangfireServer))]
        public void HangfireServer_Args_ParseSuccess()
        {
            var processArgs = new Args();

            //-----------------------------Arrange-----------------------------
            var args = $"-v".Split(' ').ToArray();

            //-----------------------------Act---------------------------------
            var result = CommandLine.Parser.Default.ParseArguments<Args>(args);
            result.MapResult(
                options => {
                    processArgs = options;
                    return 0;
                },
                _ => 1);

            //-----------------------------Assert------------------------------
            Assert.AreEqual(true, processArgs.ShowConsole);
        }
    }
}
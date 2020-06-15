/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using CommandLine;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QueueWorker;
using System;
using System.Linq;

[assembly: Parallelize(Workers = 0, Scope = ExecutionScope.MethodLevel)]
namespace Warewolf.QueueWorker.Tests
{
    [TestClass]
    public class QueueWorkerArgsTests
    {
        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(QueueWorker))]
        public void QueueWorker_Args_ParseSuccess()
        {
            var processArgs = new Args();

            //-----------------------------Arrange-----------------------------
            var id = Guid.NewGuid().ToString();

            var args = $"-c {id} -v".Split(' ').ToArray();

            //-----------------------------Act---------------------------------
            var result = CommandLine.Parser.Default.ParseArguments<Args>(args);
            result.MapResult(
                options => {
                    processArgs = options;
                    return 0;
                },
                _ => 1);

            //-----------------------------Assert------------------------------
            Assert.AreEqual(id, processArgs.TriggerId);
            Assert.AreEqual(true, processArgs.ShowConsole);
        }
    }
}

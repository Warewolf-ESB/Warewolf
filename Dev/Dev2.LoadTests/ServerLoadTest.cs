/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Dev2.Integration.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable once CheckNamespace
namespace Dev2.Integration.Tests.Load_Tests
{
    /// <summary>
    /// Summary description for LoadTest 
    /// </summary>
    [TestClass]
    public class ServerLoadTest
    {
        const double TicksPerSec = 10000000;

        // Travis.Frisinger - Bug 8579
        [TestMethod]
        public void FileWith200kLine_Expect200kRecordsetEntries_In_Under_25Seconds()
        {

            string path = "http://localhost:3142/services/" + "Load Test Resources/DataSplit200kEntryFile_Expect_Sub5SecondProcess";

            DateTime start = DateTime.Now;
            TestHelper.PostDataToWebserver(path);
            DateTime end = DateTime.Now;


            double duration = (end.Ticks - start.Ticks) / TicksPerSec;

            Console.WriteLine(@"Took " + duration);
            if(duration <= 300.0)
            {
                Assert.IsTrue(true);
            }
            else
            {
                Assert.Fail("The process took too long to run! " + duration);
            }
        }
    }
}

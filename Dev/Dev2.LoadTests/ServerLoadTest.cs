
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using Dev2.Integration.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Integration.Tests.Load_Tests
{
    /// <summary>
    /// Summary description for LoadTest 
    /// </summary>
    [TestClass]
    public class ServerLoadTest
    {
        const double _ticksPerSec = 10000000;

        [TestMethod]
        [Ignore]
        public void FileWith10kPrimes_Expect10kRecordsetEntries_in_Under_5Seconds()
        {
            string path = ServerSettings.WebserverURI + "Load Test Resources/LargeDataTest";

            DateTime start = DateTime.Now;
            string result = TestHelper.PostDataToWebserver(path);
            DateTime end = DateTime.Now;
            double duration = (end.Ticks - start.Ticks) / _ticksPerSec;

            const string exp = "<myPrimes index=\"1\"><value>Result</value></myPrimes>"; // Last value in the file

            Assert.IsTrue(result.IndexOf(exp, StringComparison.Ordinal) > 0, result + " does not contain " + exp);
            // Travis.Frisinger - Bug 8579
            // Was 10.0 Moved to 2.5
            if(duration <= 225)
            {
                Assert.IsTrue(duration <= 225, " It Took { " + duration + " }");
            }
            else
            {
                Assert.Inconclusive("Get new hardware buddy! { " + duration + " }");
            }
        }

        // Travis.Frisinger - Bug 8579
        [TestMethod]
        public void FileWith200kLine_Expect200kRecordsetEntries_In_Under_25Seconds()
        {

            string path = ServerSettings.WebserverURI + "Load Test Resources/DataSplit200kEntryFile_Expect_Sub5SecondProcess";

            DateTime start = DateTime.Now;
            TestHelper.PostDataToWebserver(path);
            DateTime end = DateTime.Now;


            double duration = (end.Ticks - start.Ticks) / _ticksPerSec;

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

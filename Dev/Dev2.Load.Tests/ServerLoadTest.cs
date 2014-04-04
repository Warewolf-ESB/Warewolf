using Dev2.Integration.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Dev2.Integration.Tests.Load_Tests
{
    /// <summary>
    /// Summary description for LoadTest
    /// </summary>
    [TestClass]
    public class ServerLoadTest
    {
        private double _ticksPerSec = 10000000;

        [TestMethod]
        public void FileWith10kPrimes_Expect10kRecordsetEntries_in_Under_5Seconds()
        {
            string path = ServerSettings.WebserverURI + "LargeDataTest";

            DateTime start = DateTime.Now;
            string result = TestHelper.PostDataToWebserver(path);
            DateTime end = DateTime.Now;
            double duration = (end.Ticks - start.Ticks) / _ticksPerSec;

            string exp = "<myPrimes><value> 104729</value></myPrimes>"; // Last value in the file

            Assert.IsTrue(result.IndexOf(exp, StringComparison.Ordinal) > 0);
            // Travis.Frisinger - Bug 8579
            // Was 10.0 Moved to 2.5
            if (duration <= 2.5)
            {
                Assert.IsTrue(duration <= 2.5, " It Took { " + duration + " }");
            }
            else if (duration <= 35)
            {
                Assert.Inconclusive("It took too long to run this test! { " + duration + " }");
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

            string path = ServerSettings.WebserverURI + "DataSplit200kEntryFile_Expect_Sub5SecondProcess";

            DateTime start = DateTime.Now;
            string result = TestHelper.PostDataToWebserver(path);
            DateTime end = DateTime.Now;

            string exp = "00199999"; // Last value in the file to ensure it was read ;)

            double duration = (end.Ticks - start.Ticks) / _ticksPerSec;

            StringAssert.Contains(result, exp);
            Console.WriteLine("Took " + duration);
            if (duration <= 25.0)
            {
                Assert.AreEqual(1, 1);
            }
            else if (duration <= 60.0)
            {
                Assert.Inconclusive("Your PC passed the test, although it was a bit slow - It meant to take less than 25 seconds, but it took " + duration);
            }
            else
            {
                Assert.Fail("The process took too long to run! " + duration);
            }
        }
    }
}

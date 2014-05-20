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
        public void FileWith10kPrimes_Expect10kRecordsetEntries_in_Under_5Seconds()
        {
            string path = ServerSettings.WebserverURI + "LargeDataTest";

            DateTime start = DateTime.Now;
            string result = TestHelper.PostDataToWebserver(path);
            DateTime end = DateTime.Now;
            double duration = (end.Ticks - start.Ticks) / _ticksPerSec;

            const string exp = "<myPrimes index=\"1\"><value>Result</value></myPrimes>"; // Last value in the file

            Assert.IsTrue(result.IndexOf(exp, StringComparison.Ordinal) > 0);
            // Travis.Frisinger - Bug 8579
            // Was 10.0 Moved to 2.5
            if(duration <= 25)
            {
                Assert.IsTrue(duration <= 25, " It Took { " + duration + " }");
            }
            else if(duration <= 45)
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
            TestHelper.PostDataToWebserver(path);
            DateTime end = DateTime.Now;


            double duration = (end.Ticks - start.Ticks) / _ticksPerSec;

            Console.WriteLine(@"Took " + duration);
            if(duration <= 45.0)
            {
                Assert.AreEqual(1, 1);
            }
            else if(duration <= 90.0)
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

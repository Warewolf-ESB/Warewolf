using System;
using Dev2.Data.Binary_Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Data.Tests.Persistence
{
    /// <summary>
    /// Summary description for BinaryDataListStorageTest
    /// </summary>
    [TestClass]
    public class BinaryDataListStorageTest
    {
        public BinaryDataListStorageTest()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        [TestMethod]
        public void CanStorageDisposeInAResonableAmountOfTime()
        {
            BinaryDataListStorage bdls = new BinaryDataListStorage("MySweetNamespace", Guid.NewGuid());


            // build insert value ;)
            BinaryDataListRow row = new BinaryDataListRow(5);

            row.UpdateValue(Guid.NewGuid().ToString(), 0);
            row.UpdateValue(Guid.NewGuid().ToString(), 1);
            row.UpdateValue(Guid.NewGuid().ToString(), 2);
            row.UpdateValue(Guid.NewGuid().ToString(), 3);
            row.UpdateValue(Guid.NewGuid().ToString(), 4);

            DateTime start = DateTime.Now;

            // Insert information
            for (int i = 1; i <= 100000; i++)
            {
                bdls.Add(i, row);
            }

            DateTime end = DateTime.Now;

            double dif = ((double)end.Ticks - (double)start.Ticks) / (double)TimeSpan.TicksPerSecond;

            bdls.Dispose();

            Console.WriteLine("Duration : " + dif);

            Assert.IsTrue(dif <= 3.5, "100k rows took too long to insert into storage, should be about 1.5 seconds { " + dif + " }");

            Assert.AreEqual(1, bdls.Keys.Count, "Not all items disposed from row storage");
        }
    }
}

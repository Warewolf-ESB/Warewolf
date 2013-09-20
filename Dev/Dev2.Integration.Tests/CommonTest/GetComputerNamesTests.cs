using Dev2.Common.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Data.Tests.Persistence
{
    /// <summary>
    /// Summary description for AvlTreeTest
    /// </summary>
    [TestClass][Ignore]//Ashley: round 2 hunting the evil test
    public class GetComputerNamesTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void GetComputerNamesListExpectListOfComputerNames()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            GetComputerNames.GetComputerNamesList();
            //------------Assert Results-------------------------
            Assert.IsNotNull(GetComputerNames.ComputerNames);
            Assert.IsTrue(GetComputerNames.ComputerNames.Count >= 1);
        }

    }
}
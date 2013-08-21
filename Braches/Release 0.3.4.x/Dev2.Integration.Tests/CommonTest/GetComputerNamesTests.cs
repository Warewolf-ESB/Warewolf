using Dev2.Common.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Data.Tests.Persistence
{
    /// <summary>
    /// Summary description for AvlTreeTest
    /// </summary>
    [TestClass]
    public class GetComputerNamesTests
    {
        public GetComputerNamesTests()
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
        public void GetComputerNamesListExpectListOfComputerNames()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            GetComputerNames.GetComputerNamesList();
            //------------Assert Results-------------------------
            Assert.IsNotNull(GetComputerNames.ComputerNames);
            Assert.IsTrue(GetComputerNames.ComputerNames.Count >= 1);
        }

        [TestMethod]
        public void ComputerNamesWhereGetComputerNamesListNotCalledExpectListIsStillRetrieved()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            Assert.IsNotNull(GetComputerNames.ComputerNames);
            Assert.IsTrue(GetComputerNames.ComputerNames.Count >= 1);
        }
    }
}
using Dev2.Integration.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Dev2.Integration.Tests.Dev2.Activities.Tests
{
    /// <summary>
    /// Summary description for DsfDataSplitActivityWFTests
    /// </summary>
    [TestClass][Ignore]//Ashley: One of these tests may be causing the server to hang in a background thread, preventing windows 7 build server from performing any more builds
    public class DsfDataMergeActivityWFTests
    {
        string WebserverURI = ServerSettings.WebserverURI;
        public DsfDataMergeActivityWFTests()
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
        public void DataMergeRecordsetsUsingStarAndCharMerge()
        {
            string PostData = String.Format("{0}{1}", WebserverURI, "DataMergeRecordsetsUsingStarAndCharMerge");
            string expected = @"<res>Wallis's surname name is Buchan
Barney's surname name is Buchan
Trevor's surname name is Williams-Ros
Travis's surname name is Frisinger
Jurie's surname name is Smit
Brendon's surname name is Page
Massimo's surname name is Guerrera
Ashley's surname name is Lewis
Sashen's surname name is Naidoo
Michael's surname name is Cullen
</res>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            StringAssert.Contains(ResponseData, expected);
        }


        [TestMethod]
        public void DataMergeWithScalarsAndTabMerge()
        {
            string PostData = String.Format("{0}{1}", WebserverURI, "DataMergeWithScalarsAndTabMerge");
            string expected = @"<res>Dev2	0317641234</res>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            StringAssert.Contains(ResponseData, expected);
        }       
    }
}

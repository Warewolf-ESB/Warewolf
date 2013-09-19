using Dev2.Integration.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Dev2.Integration.Tests.Dev2.Activities.Tests
{
    /// <summary>
    /// Summary description for DsfDataSplitActivityWFTests
    /// </summary>
    [TestClass][Ignore]//Ashley: One of these tests may be causing the server to hang in a background thread, preventing windows 7 build server from performing any more builds
    public class DsfIndexActivityWFTests
    {
        string WebserverURI = ServerSettings.WebserverURI;
        public DsfIndexActivityWFTests()
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
        public void IndexToolWithTwoRecordsetsWithStars()
        {
            string PostData = String.Format("{0}{1}", WebserverURI, "IndexToolWithTwoRecordsetsWithStars");
            string expected = @"<DataList><EmailResults><result>The email to Barney.buchan@dev2.co.za was successful</result></EmailResults><EmailResults><result>The email to Wallis.Buchan@dev2.co.za failed</result></EmailResults><EmailResults><result>The email to Travis.Frisinger@dev2.co.za failed</result></EmailResults><EmailResults><result>The email to Trevor.Williams-Ros@dev2.co.za was successful</result></EmailResults><EmailResults><result>The email to Massimo.Guerrera@dev2.co.za was successful</result></EmailResults><ResultsCollection><option>successful</option></ResultsCollection><ResultsCollection><option>failed</option></ResultsCollection><IndexResult><inres>43</inres></IndexResult><IndexResult><inres>-1</inres></IndexResult><IndexResult><inres>-1</inres></IndexResult><IndexResult><inres>49</inres></IndexResult><IndexResult><inres>46</inres></IndexResult><IndexResult><inres>-1</inres></IndexResult><IndexResult><inres>39</inres></IndexResult><IndexResult><inres>42</inres></IndexResult><IndexResult><inres>-1</inres></IndexResult><IndexResult><inres>-1</inres></IndexResult>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            StringAssert.Contains(ResponseData, expected);
        }


        [TestMethod]
        public void IndexToolWithScalarAndAllOccurances()
        {
            string PostData = String.Format("{0}{1}", WebserverURI, "IndexToolWithScalarAndAllOccurances");
            string expected = @"<DataList><Scalar>Massimo Guerrera</Scalar><result>2,16</result>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            StringAssert.Contains(ResponseData, expected);
        }
    }
}

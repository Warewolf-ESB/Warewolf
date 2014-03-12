using System;
using Dev2.Integration.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Integration.Tests.Dev2.Activities.Tests
{
    /// <summary>
    /// Summary description for DsfDataSplitActivityWFTests
    /// </summary>
    [TestClass]
    public class DsfIndexActivityWFTests
    {
        readonly string WebserverURI = ServerSettings.WebserverURI;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void IndexToolWithTwoRecordsetsWithStars()
        {
            string PostData = String.Format("{0}{1}", WebserverURI, "IndexToolWithTwoRecordsetsWithStars");
            const string expected = @"<EmailResults rowID=""1""><result>The email to Barney.buchan@dev2.co.za was successful</result></EmailResults><EmailResults rowID=""2""><result>The email to Wallis.Buchan@dev2.co.za failed</result></EmailResults><EmailResults rowID=""3""><result>The email to Travis.Frisinger@dev2.co.za failed</result></EmailResults><EmailResults rowID=""4""><result>The email to Trevor.Williams-Ros@dev2.co.za was successful</result></EmailResults><EmailResults rowID=""5""><result>The email to Massimo.Guerrera@dev2.co.za was successful</result></EmailResults><ResultsCollection rowID=""1""><option>successful</option></ResultsCollection><ResultsCollection rowID=""2""><option>failed</option></ResultsCollection><IndexResult rowID=""1""><inres>43</inres></IndexResult><IndexResult rowID=""2""><inres>-1</inres></IndexResult><IndexResult rowID=""3""><inres>-1</inres></IndexResult><IndexResult rowID=""4""><inres>49</inres></IndexResult><IndexResult rowID=""5""><inres>46</inres></IndexResult><IndexResult rowID=""6""><inres>-1</inres></IndexResult><IndexResult rowID=""7""><inres>39</inres></IndexResult><IndexResult rowID=""8""><inres>42</inres></IndexResult><IndexResult rowID=""9""><inres>-1</inres></IndexResult><IndexResult rowID=""10""><inres>-1</inres></IndexResult>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            StringAssert.Contains(ResponseData, expected);
        }


        [TestMethod]
        public void IndexToolWithScalarAndAllOccurances()
        {
            string PostData = String.Format("{0}{1}", WebserverURI, "IndexToolWithScalarAndAllOccurances");
            const string expected = @"<DataList><Scalar>Massimo Guerrera</Scalar><result>2,16</result>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            StringAssert.Contains(ResponseData, expected);
        }
    }
}

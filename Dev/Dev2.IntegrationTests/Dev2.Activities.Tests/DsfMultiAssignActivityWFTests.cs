
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
using System.Text.RegularExpressions;
using Dev2.Integration.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable InconsistentNaming
namespace Dev2.Integration.Tests.Dev2.Activities.Tests
{
    /// <summary>
    /// Summary description for DsfMultiAssignActivityWFTests
    /// </summary>
    [TestClass]
    public class DsfMultiAssignActivityWFTests
    {
        readonly string WebserverURI = ServerSettings.WebserverURI;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region RecordSet Tests

        [TestMethod]
        public void MutiAssignUsingStarIntegrationTest()
        {
            string PostData = String.Format("{0}{1}", WebserverURI, "Acceptance Testing Resources/MutiAssignWithStarTestWorkFlow");
            string expected = "<testScalar>testScalarData</testScalar>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);


            Regex regex = new Regex(@">\s*<");

            expected = regex.Replace(expected, "><");
            string expected2 = regex.Replace(expected, "><");
            string expected3 = regex.Replace(expected, "><");
            ResponseData = regex.Replace(ResponseData, "><");

            StringAssert.Contains(ResponseData, expected);
            StringAssert.Contains(ResponseData, expected2);
            StringAssert.Contains(ResponseData, expected3);
        }

        // Test created by: Michael
        [TestMethod]
        public void MultiAssignUsingIndexIntegrationTest()
        {
            string PostData = String.Format("{0}{1}", WebserverURI, "Acceptance Testing Resources/MultiAssignUsingIndexIntegrationTest");
            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            const string expected = @"<recSet index=""1""><Name>1</Name><Surname>2</Surname></recSet><recSet index=""2""><Name>3</Name><Surname>4</Surname></recSet>";
            StringAssert.Contains(ResponseData, expected);
        }

        // Test created by: Michael
        // Broken - Bug: 7836

        [TestMethod]
        public void MultiAssignUsingBlankIntegrationTest()
        {
            string PostData = String.Format("{0}{1}", WebserverURI, "Acceptance Testing Resources/MultiAssignUsingBlankIntegrationTest");
            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            const string expected1 = @"<someRec index=""1""><Name>NAME1</Name><Surname>SURNAME1</Surname></someRec>";
            const string expected2 = @"<someRec index=""2""><Name>Name2</Name><Surname>Surname2</Surname></someRec>";
            const string expected3 = @"<someRec index=""3""><Name>name3</Name><Surname>SURNAME3</Surname></someRec>";
            const string expected4 = @"<someRec index=""4""><Name>Name4</Name><Surname></Surname></someRec>";
            const string expected5 = @"<someRec index=""5""><Name>name5</Name><Surname>SURNAME5</Surname></someRec>";
            const string expected6 = @"<someRec index=""6""><Name></Name><Surname>Surname6</Surname></someRec>";
            const string expected7 = @"<someRec index=""7""><Name></Name><Surname>Surname7</Surname></someRec>";

            StringAssert.Contains(ResponseData, expected1);
            StringAssert.Contains(ResponseData, expected2);
            StringAssert.Contains(ResponseData, expected3);
            StringAssert.Contains(ResponseData, expected4);
            StringAssert.Contains(ResponseData, expected5);
            StringAssert.Contains(ResponseData, expected6);
            StringAssert.Contains(ResponseData, expected7);
        }

        #endregion RecordSet Tests

        #region Recursive Nature Tests

        [TestMethod]
        public void MutiAssignUsingRecursiveEvalutationIntergrationTest()
        {
            string PostData = String.Format("{0}{1}", WebserverURI, "Acceptance Testing Resources/MutiAssignRecursiveEvaluationTestWorkflow");
            const string expected = @"<testScalar>hello2</testScalar><recset1 index=""1""><rec>testScalarData</rec><field>world1</field></recset1><recset1 index=""2""><rec>hello1</rec><field>world2</field></recset1><recset1 index=""3""><rec>hello2</rec><field>world3</field></recset1><recset1 index=""4""><rec>hello3</rec><field>world4</field></recset1><recset1 index=""5""><rec>hello4</rec><field></field></recset1><recset2 index=""1""><field2>world1</field2></recset2><recset2 index=""2""><field2>world2</field2></recset2><recset2 index=""3""><field2>world3</field2></recset2><recset2 index=""4""><field2>world4</field2></recset2><recsetName>recset1</recsetName><recsetFieldName>rec</recsetFieldName><recsetIndex>3</recsetIndex><five>se</five><six>ven</six><temp>7</temp><seven>7</seven><eight></eight>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            StringAssert.Contains(ResponseData, expected);
        }

        #endregion Recursive Nature Tests

        #region Assign After Delete

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        public void MutiAssign_ARecordUsingFixedIndex_IndexMustHaveBeenDeleted_WillAssingNothingForTheIndex()
        {
            string PostData = String.Format("{0}{1}", WebserverURI, "Acceptance Testing Resources/Assign Debug With Empty");

            Guid id = Guid.NewGuid();
            TestHelper.PostDataToWebserverAsRemoteAgent(PostData, id);

            var debugItems = TestHelper.FetchRemoteDebugItems(ServerSettings.WebserverURI, id);

            Assert.AreEqual(4, debugItems.Count);
            Assert.AreEqual("[[rec(2).row]]", debugItems[2].Outputs[0].ResultsList[1].Variable);
            Assert.AreEqual("NOT EMPTY", debugItems[2].Outputs[0].ResultsList[1].Value);
        }
        #endregion

        #region Calculation Mode Tests


        [TestMethod]
        public void MultiAssign_Calculate_NoCalculate_Comparison_Expected()
        {
            string postData = String.Format("{0}{1}?{2}", ServerSettings.WebserverURI, "Acceptance Testing Resources/MultiAssignCalculateNoCalculateComparisonTest", "Input=10");
            string responseData = TestHelper.PostDataToWebserver(postData);


            int index = responseData.IndexOf("<CalcResult>", StringComparison.Ordinal);
            string actualCalcResult = null;
            string actualNoCalcResult = null;

            if(index != -1)
            {
                int next = responseData.IndexOf("</CalcResult>", index + 1, StringComparison.Ordinal);

                if(next != -1)
                {
                    actualCalcResult = responseData.Substring(index + 12, next - (index + 12));
                }
            }

            index = responseData.IndexOf("<NoCalcResult>", StringComparison.Ordinal);

            if(index != -1)
            {
                int next = responseData.IndexOf("</NoCalcResult>", index + 1, StringComparison.Ordinal);

                if(next != -1)
                {
                    actualNoCalcResult = responseData.Substring(index + 14, next - (index + 14));
                }
            }

            Assert.AreEqual(actualCalcResult, "40");
            Assert.AreEqual(actualNoCalcResult, "sum(30,10)");
        }

        #endregion Calculation Mode Testss
    }
}


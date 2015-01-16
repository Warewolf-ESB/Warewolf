
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

namespace Dev2.Integration.Tests.Dev2.Activities.Tests
{
    /// <summary>
    /// Summary description for DsfBaseConvertActivityWFTests
    /// </summary>
    [TestClass]
    // ReSharper disable InconsistentNaming
    public class DsfBaseConvertActivityWFTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void BaseConvertRecsetWithStar()
        {
            string postData = String.Format("{0}{1}", ServerSettings.WebserverURI, "Acceptance Testing Resources/BaseConvertRecsetWithStar");
            string responseData = TestHelper.PostDataToWebserver(postData);

            const string Expected1 = "<recSet index=\"1\"><Val2>tHiS iS a TeSt RS1</Val2><Val>tHiS iS a TeSt RS1</Val></recSet>";
            const string Expected2 = "<recSet index=\"2\"><Val2>ThIs Is A tEsT RS2</Val2><Val>ThIs Is A tEsT RS2</Val></recSet>";

            StringAssert.Contains(responseData, Expected1);
            StringAssert.Contains(responseData, Expected2);
        }


        [TestMethod]
        public void BaseConvertRecsetWithIndex()
        {
            string PostData = String.Format("{0}{1}", ServerSettings.WebserverURI, "Acceptance Testing Resources/BaseConvertRecsetWithIndex");
            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            const string expected = "<recSet index=\"1\"><Name>RecSet1Name</Name><Surname>RecSet1Surname</Surname></recSet><recSet index=\"2\"><Name>RecSet2Name</Name><Surname>RecSet2Surname</Surname></recSet>";

            StringAssert.Contains(ResponseData, expected);
        }

        [TestMethod]
        public void Test_BaseConvert_Recset_With_Star()
        {
            string PostData = String.Format("{0}{1}", ServerSettings.WebserverURI, "Acceptance Testing Resources/BaseConvertIntegrationTest");
            string expected = @"<TextToTextData>Text data to convert</TextToTextData>    <TextToHexData>0x54657874206461746120746f20636f6e76657274</TextToHexData>    <TextToBase64Data>VGV4dCBkYXRhIHRvIGNvbnZlcnQ=</TextToBase64Data>    <BinaryToTextData>Text data to convert</BinaryToTextData>    <TextToBinaryData>0101010001100101011110000111010000100000011001000110000101110100011000010010000001110100011011110010000001100011011011110110111001110110011001010111001001110100</TextToBinaryData>    <HexToTextData>Text data to convert</HexToTextData>    <Base64ToTextData>Text data to convert</Base64ToTextData>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            Regex regex = new Regex(@">\s*<");

            expected = regex.Replace(expected, "><");
            ResponseData = regex.Replace(ResponseData, "><");

            StringAssert.Contains(ResponseData, expected);
        }


        [TestMethod]
        public void BaseConvertRecsetWithNoIndex_Expected_RecordSetInternallyAppendedPerOperation()
        {
            string PostData = String.Format("{0}{1}", ServerSettings.WebserverURI, "Acceptance Testing Resources/BaseConvertRecsetWithNoIndex");
            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            const string expected = "<recSet index=\"1\"><Name>RecSet1Name</Name><Surname>RecSet1Surname</Surname></recSet><recSet index=\"2\"><Name>RecSet2Name</Name><Surname>RecSet2Surname</Surname></recSet><recSet index=\"3\"><Name>0101001001100101011000110101001101100101011101000011001001001110011000010110110101100101</Name><Surname>UmVjU2V0MlN1cm5hbWU=</Surname></recSet><recSet index=\"4\"><Name>RecSet2Name</Name><Surname>RecSet2Surname</Surname></recSet>";

            StringAssert.Contains(ResponseData, expected);
        }

    }

    // ReSharper restore InconsistentNaming
}

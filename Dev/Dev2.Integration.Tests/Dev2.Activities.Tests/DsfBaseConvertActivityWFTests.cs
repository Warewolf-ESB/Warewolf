using Dev2.Integration.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text.RegularExpressions;

namespace Dev2.Integration.Tests.Dev2.Activities.Tests
{
    /// <summary>
    /// Summary description for DsfBaseConvertActivityWFTests
    /// </summary>
    [TestClass]
    public class DsfBaseConvertActivityWFTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void BaseConvertRecsetWithStar()
        {
            string PostData = String.Format("{0}{1}", ServerSettings.WebserverURI, "BaseConvertRecsetWithStar");
            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            string expected1 = "<recSet><Val2>tHiS iS a TeSt RS1</Val2><Val>tHiS iS a TeSt RS1</Val></recSet>";
            string expected2 = "<recSet><Val2>ThIs Is A tEsT RS2</Val2><Val>ThIs Is A tEsT RS2</Val></recSet>";

            StringAssert.Contains(ResponseData, expected1);
            StringAssert.Contains(ResponseData, expected2);
        }


        [TestMethod]
        public void BaseConvertRecsetWithIndex()
        {
            string PostData = String.Format("{0}{1}", ServerSettings.WebserverURI, "BaseConvertRecsetWithIndex");
            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            string expected = "<recSet><Name>RecSet1Name</Name><Surname>RecSet1Surname</Surname></recSet><recSet><Name>RecSet2Name</Name><Surname>RecSet2Surname</Surname></recSet>";

            StringAssert.Contains(ResponseData, expected);
        }

        [TestMethod]
        public void Test_BaseConvert_Recset_With_Star()
        {
            string PostData = String.Format("{0}{1}", ServerSettings.WebserverURI, "BaseConvertIntegrationTest");
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
            string PostData = String.Format("{0}{1}", ServerSettings.WebserverURI, "BaseConvertRecsetWithNoIndex");
            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            const string expected = "<recSet><Name>RecSet1Name</Name><Surname>RecSet1Surname</Surname></recSet><recSet><Name>RecSet2Name</Name><Surname>RecSet2Surname</Surname></recSet><recSet><Name></Name><Surname>UmVjU2V0MlN1cm5hbWU=</Surname></recSet><recSet><Name></Name><Surname>RecSet2Surname</Surname></recSet>";

            StringAssert.Contains(ResponseData, expected);
        }

    }
}

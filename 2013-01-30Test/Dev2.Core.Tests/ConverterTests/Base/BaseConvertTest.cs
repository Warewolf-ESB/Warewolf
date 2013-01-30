using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dev2.Converters;

namespace Unlimited.UnitTest.Framework.ConverterTests.Base
{
    /// <summary>
    /// PBI : 1204 - Base Convert Test
    /// </summary>
    [TestClass]
    public class BaseConvertTest
    {

        private static Dev2BaseConversionFactory fac = new Dev2BaseConversionFactory();

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
        public void Factory_Can_Create_Converter_Expected_HexConverter()
        {
            IBaseConverter converter = fac.CreateConverter(enDev2BaseConvertType.Hex);

            Assert.AreEqual(enDev2BaseConvertType.Hex, converter.HandlesType());
        }


        #region Text Test
        [TestMethod]
        public void Broker_Can_Convert_Formats_Expected_Text_to_Text()
        {
            IBaseConverter from = fac.CreateConverter(enDev2BaseConvertType.Text);
            IBaseConverter to = fac.CreateConverter(enDev2BaseConvertType.Text);
            IBaseConversionBroker broker = fac.CreateBroker(from, to);

            string payload = "this is a line of text, how does that make you feel";

            string result = broker.Convert(payload);
            string expected = "this is a line of text, how does that make you feel";

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void Broker_Can_Convert_Formats_Expected_Text_to_Hex()
        {
            IBaseConverter from = fac.CreateConverter(enDev2BaseConvertType.Text);
            IBaseConverter to = fac.CreateConverter(enDev2BaseConvertType.Hex);
            IBaseConversionBroker broker = fac.CreateBroker(from, to);

            string payload = "this is a line of text, how does that make you feel";

            string result = broker.Convert(payload);
            string expected = "0x746869732069732061206c696e65206f6620746578742c20686f7720646f65732074686174206d616b6520796f75206665656c";

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void Broker_Can_Convert_Formats_Expected_Text_to_Base64()
        {
            IBaseConverter to = fac.CreateConverter(enDev2BaseConvertType.Base64);
            IBaseConverter from = fac.CreateConverter(enDev2BaseConvertType.Text);
            IBaseConversionBroker broker = fac.CreateBroker(from, to);

            string payload = "this is a line of text, how does that make you feel";

            string result = broker.Convert(payload);
            string expected = "dGhpcyBpcyBhIGxpbmUgb2YgdGV4dCwgaG93IGRvZXMgdGhhdCBtYWtlIHlvdSBmZWVs";

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void Broker_Can_Convert_Formats_Expected_Text_to_Binary()
        {
            IBaseConverter to = fac.CreateConverter(enDev2BaseConvertType.Binary);
            IBaseConverter from = fac.CreateConverter(enDev2BaseConvertType.Text);
            IBaseConversionBroker broker = fac.CreateBroker(from, to);

            string payload = "this is a line of text, how does that make you feel";
            //string payload = "za";

            string result = broker.Convert(payload);
            string expected = "011101000110100001101001011100110010000001101001011100110010000001100001001000000110110001101001011011100110010100100000011011110110011000100000011101000110010101111000011101000010110000100000011010000110111101110111001000000110010001101111011001010111001100100000011101000110100001100001011101000010000001101101011000010110101101100101001000000111100101101111011101010010000001100110011001010110010101101100";
            //string expected = "0111101001100001";

            Assert.AreEqual(expected, result);
        }
        #endregion

        #region hex test
        [TestMethod]
        public void Broker_Can_Convert_Formats_Expected_Hex_to_Hex()
        {
            IBaseConverter to = fac.CreateConverter(enDev2BaseConvertType.Hex);
            IBaseConverter from = fac.CreateConverter(enDev2BaseConvertType.Hex);
            IBaseConversionBroker broker = fac.CreateBroker(from, to);

            string payload = "0x746869732069732061206c696e65206f6620746578742c20686f7720646f65732074686174206d616b6520796f75206665656c";

            string result = broker.Convert(payload);
            string expected = "0x746869732069732061206c696e65206f6620746578742c20686f7720646f65732074686174206d616b6520796f75206665656c";

            // test input
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void Broker_Can_Convert_Formats_Expected_Hex_to_Text()
        {
            IBaseConverter to = fac.CreateConverter(enDev2BaseConvertType.Text);
            IBaseConverter from = fac.CreateConverter(enDev2BaseConvertType.Hex);
            IBaseConversionBroker broker = fac.CreateBroker(from, to);

            string payload = "0x746869732069732061206c696e65206f6620746578742c20686f7720646f65732074686174206d616b6520796f75206665656c";

            string result = broker.Convert(payload);
            string expected = "this is a line of text, how does that make you feel";

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void Broker_Can_Convert_Formats_No_Leading0x_Expected_Hex_to_Text()
        {
            IBaseConverter to = fac.CreateConverter(enDev2BaseConvertType.Text);
            IBaseConverter from = fac.CreateConverter(enDev2BaseConvertType.Hex);
            IBaseConversionBroker broker = fac.CreateBroker(from, to);

            string payload = "746869732069732061206c696e65206f6620746578742c20686f7720646f65732074686174206d616b6520796f75206665656c";

            string result = broker.Convert(payload);
            string expected = "this is a line of text, how does that make you feel";

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void Broker_Can_Convert_Formats_Expected_Hex_to_Binary()
        {
            IBaseConverter to = fac.CreateConverter(enDev2BaseConvertType.Binary);
            IBaseConverter from = fac.CreateConverter(enDev2BaseConvertType.Hex);
            IBaseConversionBroker broker = fac.CreateBroker(from, to);

            string payload = "746869732069732061206c696e65206f6620746578742c20686f7720646f65732074686174206d616b6520796f75206665656c";

            string result = broker.Convert(payload);
            string expected = "011101000110100001101001011100110010000001101001011100110010000001100001001000000110110001101001011011100110010100100000011011110110011000100000011101000110010101111000011101000010110000100000011010000110111101110111001000000110010001101111011001010111001100100000011101000110100001100001011101000010000001101101011000010110101101100101001000000111100101101111011101010010000001100110011001010110010101101100";

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void Broker_Can_Convert_Formats_Expected_Hex_to_Base64()
        {
            IBaseConverter to = fac.CreateConverter(enDev2BaseConvertType.Base64);
            IBaseConverter from = fac.CreateConverter(enDev2BaseConvertType.Hex);
            IBaseConversionBroker broker = fac.CreateBroker(from, to);

            string payload = "0x746869732069732061206c696e65206f6620746578742c20686f7720646f65732074686174206d616b6520796f75206665656c";

            string result = broker.Convert(payload);
            string expected = "dGhpcyBpcyBhIGxpbmUgb2YgdGV4dCwgaG93IGRvZXMgdGhhdCBtYWtlIHlvdSBmZWVs";

            Assert.AreEqual(expected, result);
        }
        #endregion

        #region Base64 Test
        [TestMethod]
        public void Broker_Can_Convert_Formats_Expected_Base64_to_Base64()
        {
            IBaseConverter to = fac.CreateConverter(enDev2BaseConvertType.Base64);
            IBaseConverter from = fac.CreateConverter(enDev2BaseConvertType.Base64);
            IBaseConversionBroker broker = fac.CreateBroker(from, to);

            string payload = "dGhpcyBpcyBhIGxpbmUgb2YgdGV4dCwgaG93IGRvZXMgdGhhdCBtYWtlIHlvdSBmZWVs";

            string result = broker.Convert(payload);
            string expected = "dGhpcyBpcyBhIGxpbmUgb2YgdGV4dCwgaG93IGRvZXMgdGhhdCBtYWtlIHlvdSBmZWVs";

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void Broker_Can_Convert_Formats_Expected_Base64_to_Text()
        {
            IBaseConverter to = fac.CreateConverter(enDev2BaseConvertType.Text);
            IBaseConverter from = fac.CreateConverter(enDev2BaseConvertType.Base64);
            IBaseConversionBroker broker = fac.CreateBroker(from, to);

            string payload = "dGhpcyBpcyBhIGxpbmUgb2YgdGV4dCwgaG93IGRvZXMgdGhhdCBtYWtlIHlvdSBmZWVs";

            string result = broker.Convert(payload);
            string expected = "this is a line of text, how does that make you feel";

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void Broker_Can_Convert_Formats_Expected_Base64_to_Hex()
        {
            IBaseConverter to = fac.CreateConverter(enDev2BaseConvertType.Hex);
            IBaseConverter from = fac.CreateConverter(enDev2BaseConvertType.Base64);
            IBaseConversionBroker broker = fac.CreateBroker(from, to);

            string payload = "dGhpcyBpcyBhIGxpbmUgb2YgdGV4dCwgaG93IGRvZXMgdGhhdCBtYWtlIHlvdSBmZWVs";

            string result = broker.Convert(payload);
            string expected = "0x746869732069732061206c696e65206f6620746578742c20686f7720646f65732074686174206d616b6520796f75206665656c";

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void Broker_Can_Convert_Formats_Expected_Base64_to_Binary()
        {
            IBaseConverter to = fac.CreateConverter(enDev2BaseConvertType.Binary);
            IBaseConverter from = fac.CreateConverter(enDev2BaseConvertType.Base64);
            IBaseConversionBroker broker = fac.CreateBroker(from, to);

            string payload = "dGhpcyBpcyBhIGxpbmUgb2YgdGV4dCwgaG93IGRvZXMgdGhhdCBtYWtlIHlvdSBmZWVs";

            string result = broker.Convert(payload);
            string expected = "011101000110100001101001011100110010000001101001011100110010000001100001001000000110110001101001011011100110010100100000011011110110011000100000011101000110010101111000011101000010110000100000011010000110111101110111001000000110010001101111011001010111001100100000011101000110100001100001011101000010000001101101011000010110101101100101001000000111100101101111011101010010000001100110011001010110010101101100";

            Assert.AreEqual(expected, result);
        }

        #endregion

        #region Binary Test
        [TestMethod]
        public void Broker_Can_Convert_Formats_Expected_Binary_to_Binary()
        {
            IBaseConverter to = fac.CreateConverter(enDev2BaseConvertType.Binary);
            IBaseConverter from = fac.CreateConverter(enDev2BaseConvertType.Binary);
            IBaseConversionBroker broker = fac.CreateBroker(from, to);

            string payload = "011101000110100001101001011100110010000001101001011100110010000001100001001000000110110001101001011011100110010100100000011011110110011000100000011101000110010101111000011101000010110000100000011010000110111101110111001000000110010001101111011001010111001100100000011101000110100001100001011101000010000001101101011000010110101101100101001000000111100101101111011101010010000001100110011001010110010101101100";

            string result = broker.Convert(payload);
            string expected = "011101000110100001101001011100110010000001101001011100110010000001100001001000000110110001101001011011100110010100100000011011110110011000100000011101000110010101111000011101000010110000100000011010000110111101110111001000000110010001101111011001010111001100100000011101000110100001100001011101000010000001101101011000010110101101100101001000000111100101101111011101010010000001100110011001010110010101101100";

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void Broker_Can_Convert_Formats_Expected_Binary_to_Text()
        {
            IBaseConverter to = fac.CreateConverter(enDev2BaseConvertType.Text);
            IBaseConverter from = fac.CreateConverter(enDev2BaseConvertType.Binary);
            IBaseConversionBroker broker = fac.CreateBroker(from, to);

            string payload = "011101000110100001101001011100110010000001101001011100110010000001100001001000000110110001101001011011100110010100100000011011110110011000100000011101000110010101111000011101000010110000100000011010000110111101110111001000000110010001101111011001010111001100100000011101000110100001100001011101000010000001101101011000010110101101100101001000000111100101101111011101010010000001100110011001010110010101101100";

            string result = broker.Convert(payload);
            string expected = "this is a line of text, how does that make you feel";

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void Broker_Can_Convert_Formats_Expected_Binary_to_Hex()
        {
            IBaseConverter to = fac.CreateConverter(enDev2BaseConvertType.Hex);
            IBaseConverter from = fac.CreateConverter(enDev2BaseConvertType.Binary);
            IBaseConversionBroker broker = fac.CreateBroker(from, to);

            string payload = "011101000110100001101001011100110010000001101001011100110010000001100001001000000110110001101001011011100110010100100000011011110110011000100000011101000110010101111000011101000010110000100000011010000110111101110111001000000110010001101111011001010111001100100000011101000110100001100001011101000010000001101101011000010110101101100101001000000111100101101111011101010010000001100110011001010110010101101100";

            string result = broker.Convert(payload);
            string expected = "0x746869732069732061206c696e65206f6620746578742c20686f7720646f65732074686174206d616b6520796f75206665656c";

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void Broker_Can_Convert_Formats_Expected_Binary_to_Base64()
        {
            IBaseConverter to = fac.CreateConverter(enDev2BaseConvertType.Base64);
            IBaseConverter from = fac.CreateConverter(enDev2BaseConvertType.Binary);
            IBaseConversionBroker broker = fac.CreateBroker(from, to);

            string payload = "011101000110100001101001011100110010000001101001011100110010000001100001001000000110110001101001011011100110010100100000011011110110011000100000011101000110010101111000011101000010110000100000011010000110111101110111001000000110010001101111011001010111001100100000011101000110100001100001011101000010000001101101011000010110101101100101001000000111100101101111011101010010000001100110011001010110010101101100";

            string result = broker.Convert(payload);
            string expected = "dGhpcyBpcyBhIGxpbmUgb2YgdGV4dCwgaG93IGRvZXMgdGhhdCBtYWtlIHlvdSBmZWVs";

            Assert.AreEqual(expected, result);
        }

        #endregion

        #region negative test
        [TestMethod]
        [ExpectedException(typeof(BaseTypeException))]
        public void Format_MisMatch_Binary_Expect_Exception()
        {
            IBaseConverter to = fac.CreateConverter(enDev2BaseConvertType.Hex);
            IBaseConverter from = fac.CreateConverter(enDev2BaseConvertType.Binary);
            IBaseConversionBroker broker = fac.CreateBroker(from, to);

            string payload = "011101000110100001101001011100110010000001101001011100110010000001100001001000000110110001101001011011100110010100100000011011110110011000100000011101000110010101111000011101000010110000100000011010000110111101110111001000000110010001101111011001010111001100100000011101000110100001100001011101000010000001101101011000010110101101100101001000000111100101101111011101010010000001100110011001010110010101101102";

            string result = broker.Convert(payload);
        }

        [TestMethod]
        [ExpectedException(typeof(BaseTypeException))]
        public void Format_MisMatch_Base64_Expect_Exception()
        {
            IBaseConverter to = fac.CreateConverter(enDev2BaseConvertType.Hex);
            IBaseConverter from = fac.CreateConverter(enDev2BaseConvertType.Base64);
            IBaseConversionBroker broker = fac.CreateBroker(from, to);

            string payload = "dGhpcyBpcyBhIGxpbmUgb2YgdGV4dCwgaG93IGRvZXMgdGhhdCBtYWtlIHlvdSBmZWVsqzdfs";

            string result = broker.Convert(payload);
        }

        [TestMethod]
        [ExpectedException(typeof(BaseTypeException))]
        public void Format_MisMatch_Hex_Expect_Exception()
        {
            IBaseConverter to = fac.CreateConverter(enDev2BaseConvertType.Text);
            IBaseConverter from = fac.CreateConverter(enDev2BaseConvertType.Hex);
            IBaseConversionBroker broker = fac.CreateBroker(from, to);

            string payload = "0x746869732069732061206c696e65206f6620746578742c20686f7720646f65732074686174206d616b6520796f75206665656";

            string result = broker.Convert(payload);
        }

        [TestMethod]
        [ExpectedException(typeof(BaseTypeException))]
        public void Format_MisMatch_Text_Expect_Exception()
        {
            IBaseConverter to = fac.CreateConverter(enDev2BaseConvertType.Hex);
            IBaseConverter from = fac.CreateConverter(enDev2BaseConvertType.Text);
            IBaseConversionBroker broker = fac.CreateBroker(from, to);

            string payload = "0x746869732069732061206c696e65206f6620746578742c20686f7720646f65732074686174206d616b6520796f75206665656c";

            string result = broker.Convert(payload);
        }
        #endregion
    }
}

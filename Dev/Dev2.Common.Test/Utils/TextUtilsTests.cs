using Dev2.Common.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Common.Tests.Utils
{
    // ReSharper disable InconsistentNaming
    [TestClass]
    public class TextUtilsTests
    {
        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("TextUtils_ReplaceWorkflowNewLinesWithEnvironmentNewLines")]

        public void TextUtils_ReplaceWorkflowNewLinesWithEnvironmentNewLines_ReplaceWackNWithWackRWackN_ReplaceOccurs()
        {
            //------------Setup for test--------------------------
            string expected = "warewolf\r\n is\\n awesome\r\n";
            string stringToReplace = "warewolf\n is\\n awesome\r\n";
            //------------Execute Test---------------------------
            string actual = TextUtils.ReplaceWorkflowNewLinesWithEnvironmentNewLines(stringToReplace);
            //------------Assert Results-------------------------
            Assert.AreEqual(expected, actual, "The replacement didn't work correctly");
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("TextUtils_ReplaceWorkflowNewLinesWithEnvironmentNewLines")]
        public void TextUtils_ReplaceWorkflowNewLinesWithEnvironmentNewLines_WackNFirstChars_ReplaceOccurs()
        {
            //------------Setup for test--------------------------
            string expected = "\r\nwarewolf\r\n is\\n awesome\r\n";
            string stringToReplace = "\nwarewolf\n is\\n awesome\r\n";
            //------------Execute Test---------------------------
            string actual = TextUtils.ReplaceWorkflowNewLinesWithEnvironmentNewLines(stringToReplace);
            //------------Assert Results-------------------------
            Assert.AreEqual(expected, actual, "The replacement didn't work correctly");
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("TextUtils_ReplaceWorkflowNewLinesWithEnvironmentNewLines")]
        public void TextUtils_ReplaceWorkflowNewLinesWithEnvironmentNewLines_WackNSecondChars_ReplaceOccurs()
        {
            //------------Setup for test--------------------------
            string expected = "t\r\nwarewolf\r\n is\\n awesome\r\n";
            string stringToReplace = "t\nwarewolf\n is\\n awesome\r\n";
            //------------Execute Test---------------------------
            string actual = TextUtils.ReplaceWorkflowNewLinesWithEnvironmentNewLines(stringToReplace);
            //------------Assert Results-------------------------
            Assert.AreEqual(expected, actual, "The replacement didn't work correctly");
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("TextUtils_ReplaceWorkflowNewLinesWithEnvironmentNewLines")]
        public void TextUtils_ReplaceWorkflowNewLinesWithEnvironmentNewLines_WackNLastChars_ReplaceOccurs()
        {
            //------------Setup for test--------------------------
            string expected = "t\r\nwarewolf\r\n is\\n awesome\r\n";
            string stringToReplace = "t\nwarewolf\n is\\n awesome\n";
            //------------Execute Test---------------------------
            string actual = TextUtils.ReplaceWorkflowNewLinesWithEnvironmentNewLines(stringToReplace);
            //------------Assert Results-------------------------
            Assert.AreEqual(expected, actual, "The replacement didn't work correctly");
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("TextUtils_ReplaceWorkflowNewLinesWithEnvironmentNewLines")]
        public void TextUtils_ReplaceWorkflowNewLinesWithEnvironmentNewLines_WackWackNFirstChars_ReplaceOccurs()
        {
            //------------Setup for test--------------------------
            string expected = "\\nwarewolf\r\n is\\n awesome\r\ntest";
            string stringToReplace = "\\nwarewolf\n is\\n awesome\ntest";
            //------------Execute Test---------------------------
            string actual = TextUtils.ReplaceWorkflowNewLinesWithEnvironmentNewLines(stringToReplace);
            //------------Assert Results-------------------------
            Assert.AreEqual(expected, actual, "The replacement didn't work correctly");
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("TextUtils_ReplaceWorkflowNewLinesWithEnvironmentNewLines")]
        public void TextUtils_ReplaceWorkflowNewLinesWithEnvironmentNewLines_WackkWackNFirstChars_ReplaceOccurs()
        {
            //------------Setup for test--------------------------
            string expected = "\\4\\\r\n\r\n\\\\\\\r\n\r\n\\n\r\n";
            string stringToReplace = "\\4\\\n\n\\\\\\\n\n\\n\n";
            //------------Execute Test---------------------------
            string actual = TextUtils.ReplaceWorkflowNewLinesWithEnvironmentNewLines(stringToReplace);
            //------------Assert Results-------------------------
            Assert.AreEqual(expected, actual, "The replacement didn't work correctly");
        }
    }
}

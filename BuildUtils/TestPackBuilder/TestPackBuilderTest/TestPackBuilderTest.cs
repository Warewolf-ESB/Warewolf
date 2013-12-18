using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestPackBuilder;

namespace TestPackBuilderTest
{
    [TestClass]
    public class TestPackBuilderTest
    {
        private string FetchTestDir(bool isFaulty = false)
        {
            if (isFaulty)
            {
                return "C:\\foo"+Guid.NewGuid();
            }

            var tmpPath = Path.GetTempPath();
            var returnPath = Path.Combine(tmpPath, Guid.NewGuid().ToString());

            Directory.CreateDirectory(returnPath);

            return returnPath;
        }

        [TestMethod]
        public void CanLocateTestMethodNames()
        {
            var obj = new TestScanner();

            var dir = FetchTestDir();
            const string ext = ".cs";
            const string annotation = "[TestMethod]";

            const string expected = @"<Methods><TestMethod>TestMethod</TestMethod><TestMethod>TestMethod2</TestMethod><TestMethod>TestMethod3</TestMethod></Methods>";

            var result = obj.ScanDirectory(dir, ext, annotation);

            StringAssert.Contains(result, expected);
        }

        [TestMethod]
        public void ReturnsErrorMessageWhenDirectoryDoesNotExist()
        {
            var obj = new TestScanner();

            var dir = FetchTestDir(true);
            const string ext = ".cs";
            const string annotation = "[TestMethod]";

            const string expected = @"Error : Directory Does Not Exist";

            var result = obj.ScanDirectory(dir, ext, annotation);

            StringAssert.Contains(result, expected);
        }

        [TestMethod]
        public void ReturnsErrorMessageWhenDirectoryNull()
        {
            var obj = new TestScanner();

            const string ext = ".cs";
            const string annotation = "[TestMethod]";

            const string expected = @"Error : Directory Does Not Exist";

            var result = obj.ScanDirectory(null, ext, annotation);

            StringAssert.Contains(result, expected);
        }

        [TestMethod]
        public void ReturnsErrorMessageWhenFileExtentionNull()
        {
            var obj = new TestScanner();

            var dir = FetchTestDir();
            const string annotation = "[TestMethod]";

            const string expected = @"Error : No File Extention To Scan For";

            var result = obj.ScanDirectory(dir, null, annotation);

            StringAssert.Contains(result, expected);
        }

        [TestMethod]
        public void ReturnsErrorMessageWhenFileExtentionEmpty()
        {
            var obj = new TestScanner();

            var dir = FetchTestDir();
            const string annotation = "[TestMethod]";

            const string expected = @"Error : No File Extention To Scan For";

            var result = obj.ScanDirectory(dir, string.Empty, annotation);

            StringAssert.Contains(result, expected);
        }

        [TestMethod]
        public void ReturnsErrorMessageWhenNoFilesForExtention()
        {
            var obj = new TestScanner();

            const string ext = ".bar";
            var dir = FetchTestDir();
            const string annotation = "[TestMethod]";

            const string expected = @"<Methods></Methods>";

            var result = obj.ScanDirectory(dir, ext, annotation);

            StringAssert.Contains(result, expected);
        }

        [TestMethod]
        public void ReturnsErrorMessageWhenExtentionContainsStar()
        {
            var obj = new TestScanner();

            const string ext = "*.cs";
            var dir = FetchTestDir();
            const string annotation = "[TestMethod]";

            const string expected = @"<Methods><TestMethod>TestMethod</TestMethod><TestMethod>TestMethod2</TestMethod><TestMethod>TestMethod3</TestMethod></Methods>";

            var result = obj.ScanDirectory(dir, ext, annotation);

            StringAssert.Contains(result, expected);
        }

        [TestMethod]
        public void ReturnsErrorMessageWhenExtentionContainsNoDot()
        {
            var obj = new TestScanner();

            const string ext = "cs";
            var dir = FetchTestDir();
            const string annotation = "[TestMethod]";

            const string expected = @"<Methods><TestMethod>TestMethod</TestMethod><TestMethod>TestMethod2</TestMethod><TestMethod>TestMethod3</TestMethod></Methods>";

            var result = obj.ScanDirectory(dir, ext, annotation);

            StringAssert.Contains(result, expected);
        }
    }
}

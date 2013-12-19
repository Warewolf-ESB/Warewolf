using System;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestPackBuilder;

namespace TestPackBuilderTest
{
    [TestClass]
    public class TestPackBuilderTest
    {
        private string FetchTestDir(bool isFaulty = false, bool isRecursive = false)
        {
            if (isFaulty)
            {
                return "C:\\foo"+Guid.NewGuid();
            }

            var tmpPath = Path.GetTempPath();
            var returnPath = Path.Combine(tmpPath, Guid.NewGuid().ToString());

            Directory.CreateDirectory(returnPath);

            // now populate it ;)
            PopulateDirectory(returnPath);

            if (isRecursive)
            {
                /* now build up 2 dirs */

                // 1 - with a test file
                var subDir = Path.Combine(returnPath, Guid.NewGuid().ToString());
                Directory.CreateDirectory(subDir);
                PopulateDirectory(subDir, "Method0");

                // 1 - with another directory with a test file ;)
                var subSubDir = Path.Combine(subDir, Guid.NewGuid().ToString());
                Directory.CreateDirectory(subSubDir);
                PopulateDirectory(subSubDir, "Method00");
                
            }

            return returnPath;
        }

        private void PopulateDirectory(string dir, string adjustNamesValue = "")
        {
            const string fileName = "SampleTestFile.cs";
            var resourceName = string.Format("TestPackBuilderTest.Artifacts.{0}", fileName);
            
            var assembly = Assembly.GetExecutingAssembly();
            using(var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream != null)
                {
                    var len = (int)stream.Length;
                    var bytes = new byte[len];
                    stream.Read(bytes, 0, len);
                    var loc = Path.Combine(dir, fileName);
                    var str = Encoding.UTF8.GetString(bytes);

                    if (!string.IsNullOrEmpty(adjustNamesValue))
                    {
                        str = str.Replace("MyTest", "MyTest_"+adjustNamesValue);
                    }

                    File.WriteAllText(loc, str);
                }
            }
        }

        #region ScanDirectory Test

        [TestMethod]
        public void CanLocateTestMethodNames()
        {
            var obj = new TestScanner();

            var dir = FetchTestDir();
            const string ext = ".cs";
            const string annotation = "[TestMethod]";

            const string expected = @"MyTest,MyTest2,MyTest3,";

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

            const string expected = @"";

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

            const string expected = @"MyTest,MyTest2,MyTest3,";

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

            const string expected = @"MyTest,MyTest2,MyTest3,";

            var result = obj.ScanDirectory(dir, ext, annotation);

            StringAssert.Contains(result, expected);
        }

        #endregion

        #region RecursivelyScanDirectory Test

        [TestMethod]
        public void CanScanDirectoryRecursively()
        {
            var obj = new TestScanner();

            const string ext = "cs";
            var dir = FetchTestDir(false, true);
            const string annotation = "[TestMethod]";
            const string expected = @"MyTest,MyTest2,MyTest3,MyTest_Method0,MyTest_Method02,MyTest_Method03,MyTest_Method00,MyTest_Method002,MyTest_Method003";

            var result = obj.RecursivelyScanDirectory(dir, ext, annotation);

            StringAssert.Contains(result, expected);
        }

        #endregion
    }
}

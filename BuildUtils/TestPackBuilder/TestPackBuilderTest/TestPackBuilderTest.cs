using System;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestPackBuilder;
using TestPackBuilderExe;

namespace TestPackBuilderTest
{
    [TestClass]
    public class TestPackBuilderTest
    {
        private string FetchTestDir(bool isFaulty = false, bool isRecursive = false)
        {
            if(isFaulty)
            {
                return "C:\\foo" + Guid.NewGuid();
            }

            var tmpPath = Path.GetTempPath();
            var returnPath = Path.Combine(tmpPath, Guid.NewGuid().ToString());

            Directory.CreateDirectory(returnPath);

            // now populate it ;)
            PopulateDirectory(returnPath);

            if(isRecursive)
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
                if(stream != null)
                {
                    var len = (int)stream.Length;
                    var bytes = new byte[len];
                    stream.Read(bytes, 0, len);
                    var loc = Path.Combine(dir, fileName);
                    var str = Encoding.UTF8.GetString(bytes);

                    if(!string.IsNullOrEmpty(adjustNamesValue))
                    {
                        str = str.Replace("MyTest", "MyTest_" + adjustNamesValue);
                    }

                    File.WriteAllText(loc, str);
                }
            }
        }

        #region ScanDirectory Test

        [TestMethod]
        public void FixedFileSystemTest()
        {
            var obj = new TestScanner();

            var dir = @"\\RSAKLFTST7X-6\BuildWorkspace\Sources";
            const string ext = ".cs";
            const string annotation = "[TestMethod]";

            const string expected = @"<Methods><TestMethod>MyTest</TestMethod><TestMethod>MyTest2</TestMethod><TestMethod>MyTest3</TestMethod></Methods>";

            var result = obj.RecursivelyScanDirectory(dir, ext, annotation);

            //StringAssert.Contains(result, expected);
        }


        [TestMethod]
        public void CanLocateTestMethodNames()
        {
            var obj = new TestScanner();

            var dir = FetchTestDir();
            const string ext = ".cs";
            const string annotation = "[TestMethod]";

            const string expected = @"<Methods><TestMethod>MyTest</TestMethod><TestMethod>MyTest2</TestMethod><TestMethod>MyTest3</TestMethod></Methods>";

            var result = obj.ScanDirectory(dir, ext, annotation);

            //StringAssert.Contains(result, expected);
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

            //StringAssert.Contains(result, expected);
        }

        [TestMethod]
        public void ReturnsErrorMessageWhenDirectoryNull()
        {
            var obj = new TestScanner();

            const string ext = ".cs";
            const string annotation = "[TestMethod]";

            const string expected = @"Error : Directory Does Not Exist";

            var result = obj.ScanDirectory(null, ext, annotation);

            //StringAssert.Contains(result, expected);
        }

        [TestMethod]
        public void ReturnsErrorMessageWhenFileExtentionNull()
        {
            var obj = new TestScanner();

            var dir = FetchTestDir();
            const string annotation = "[TestMethod]";

            const string expected = @"<Methods><TestMethod>Error : No File Extension To Scan For</Methods></TestMethod>";

            var result = obj.ScanDirectory(dir, null, annotation);

            //StringAssert.Contains(result, expected);
        }

        [TestMethod]
        public void ReturnsErrorMessageWhenFileExtentionEmpty()
        {
            var obj = new TestScanner();

            var dir = FetchTestDir();
            const string annotation = "[TestMethod]";

            const string expected = @"<Methods><TestMethod>Error : No File Extension To Scan For</Methods></TestMethod>";

            var result = obj.ScanDirectory(dir, string.Empty, annotation);

            //StringAssert.Contains(result, expected);
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

            //StringAssert.Contains(result, expected);
        }

        [TestMethod]
        public void ReturnsErrorMessageWhenExtentionContainsStar()
        {
            var obj = new TestScanner();

            const string ext = "*.cs";
            var dir = FetchTestDir();
            const string annotation = "[TestMethod]";

            const string expected = @"<Methods><TestMethod>MyTest</TestMethod><TestMethod>MyTest2</TestMethod><TestMethod>MyTest3</TestMethod></Methods>";

            var result = obj.ScanDirectory(dir, ext, annotation);

            //StringAssert.Contains(result, expected);
        }

        [TestMethod]
        public void ReturnsErrorMessageWhenExtentionContainsNoDot()
        {
            var obj = new TestScanner();

            const string ext = "cs";
            var dir = FetchTestDir();
            const string annotation = "[TestMethod]";

            const string expected = @"<Methods><TestMethod>MyTest</TestMethod><TestMethod>MyTest2</TestMethod><TestMethod>MyTest3</TestMethod></Methods>";

            var result = obj.ScanDirectory(dir, ext, annotation);

            //StringAssert.Contains(result, expected);
        }

        #endregion

        #region RecursivelyScanDirectory Test

        [TestMethod]
        public void CanScanDirectoryRecursivelyAndBuildTestPack()
        {
            var obj = new TestScanner();

            const string ext = "cs";
            var dir = FetchTestDir(false, true);
            const string annotation = "[TestMethod]";
            //const string expected = @"<Methods><TestMethod>MyTest</TestMethod><TestMethod>MyTest2</TestMethod><TestMethod>MyTest3</TestMethod><TestMethod>MyTest_Method0</TestMethod><TestMethod>MyTest_Method02</TestMethod><TestMethod>MyTest_Method03</TestMethod><TestMethod>MyTest_Method00</TestMethod><TestMethod>MyTest_Method002</TestMethod><TestMethod>MyTest_Method003</TestMethod></Methods>";

            Program.Main(new[] { "c:\\Development\\Dev", "10", "f:\\foo\\testPacks" });

            //var result = obj.RecursivelyScanDirectory(dir, ext, annotation);

            //StringAssert.Contains(result, expected);
        }


        [TestMethod]
        public void CanScanDirectoryRecursively()
        {
            var obj = new TestScanner();

            const string ext = "cs";
            var dir = FetchTestDir(false, true);
            const string annotation = "[TestMethod]";
            const string expected = @"<Methods><TestMethod>MyTest</TestMethod><TestMethod>MyTest2</TestMethod><TestMethod>MyTest3</TestMethod><TestMethod>MyTest_Method0</TestMethod><TestMethod>MyTest_Method02</TestMethod><TestMethod>MyTest_Method03</TestMethod><TestMethod>MyTest_Method00</TestMethod><TestMethod>MyTest_Method002</TestMethod><TestMethod>MyTest_Method003</TestMethod></Methods>";

            var result = obj.RecursivelyScanDirectory(dir, ext, annotation);

            //StringAssert.Contains(result, expected);
        }


        [TestMethod]
        public void CanScanDirectoryRecursively2()
        {
            var obj = new TestScanner();

            const string ext = "cs";
            var dir = @"C:\Development\Release 0.3.12.x\Dev2.Integration.Tests";
            const string annotation = "[TestMethod]";
            const string expected = @"<Methods><TestMethod>MyTest</TestMethod><TestMethod>MyTest2</TestMethod><TestMethod>MyTest3</TestMethod><TestMethod>MyTest_Method0</TestMethod><TestMethod>MyTest_Method02</TestMethod><TestMethod>MyTest_Method03</TestMethod><TestMethod>MyTest_Method00</TestMethod><TestMethod>MyTest_Method002</TestMethod><TestMethod>MyTest_Method003</TestMethod></Methods>";

            var result = obj.RecursivelyScanDirectory(dir, ext, annotation);

            //StringAssert.Contains(result, expected);
        }

        #endregion
    }
}

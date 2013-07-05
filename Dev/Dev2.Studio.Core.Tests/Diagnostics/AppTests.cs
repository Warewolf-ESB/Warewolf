using System;
using System.IO;
using Dev2.Studio;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Core.Tests.Diagnostics
{
    [TestClass]
    public class AppTests
    {
        private static readonly string RootPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        private const string NewPath = @"Warewolf\TempDirectoryTests\";
        private static readonly string FullNewPath = Path.Combine(RootPath, NewPath);
        private const string OldPath = @"Dev2\TempDirectoryTests\";
        private static readonly string FullOldPath = Path.Combine(RootPath, OldPath);

        [ClassCleanup]
        public static void ClassCleanup()
        {
            FindAndRemoveTestFiles();
        }

        [ClassInitialize]
        public static void ClassInit(TestContext testContext)
        {
            FindAndRemoveTestFiles();
        }

        static void FindAndRemoveTestFiles()
        {
            if (Directory.Exists(FullOldPath))
            {
                Directory.Delete(FullOldPath, true);
            }
            if (Directory.Exists(FullNewPath))
            {
                Directory.Delete(FullNewPath, true);
            }
        }

        [TestMethod]
        public void DirectoryCopyExpectedAllFilesCopied()
        {
            //Initialization
            if(!Directory.Exists(FullOldPath))
            {
                Directory.CreateDirectory(FullOldPath);
            }
            if(!File.Exists(FullOldPath + "\\a file in the old temp dir"))
            {
                var stream = File.Create(FullOldPath + "\\a file in the old temp dir");
                stream.Close();
            }
            if(!Directory.Exists(FullOldPath + "\\temp"))
            {
                Directory.CreateDirectory(FullOldPath + "\\temp");
            }
            if (!File.Exists(FullOldPath + "\\temp\\a file in asub dir in the old temp folder"))
            {
                var stream = File.Create(FullOldPath + "\\temp\\a file in asub dir in the old temp folder");
                stream.Close();
            }

            //Execute
            App.DirectoryCopy(FullOldPath, FullNewPath);

            //Assert
            Assert.IsTrue(File.Exists(FullNewPath + "\\a file in the old temp dir"), "File not migrated from old temp folder");
            Assert.IsTrue(File.Exists(FullNewPath + "\\temp\\a file in asub dir in the old temp folder"), "File in a sub directory of the old temp folder not migrated to new temp folder");
        }
    }
}

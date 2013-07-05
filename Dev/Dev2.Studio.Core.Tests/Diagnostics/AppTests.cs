using System;
using System.IO;
using Dev2.Studio;
using Dev2.Studio.Core.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Core.Tests.Diagnostics
{
    [TestClass]
    public class AppTests
    {
        private static string NewPath;
        private static string OldPath;
        static TestContext Context;

        [ClassInitialize]
        public static void ClassInit(TestContext testContext)
        {
            FindAndRemoveTestFiles();
            Context = testContext;
            NewPath = Context.TestDir + @"\Warewolf\";
            OldPath = Context.TestDir + @"\Dev2\";
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            FindAndRemoveTestFiles();
        }

        static void FindAndRemoveTestFiles()
        {
            if (Directory.Exists(NewPath))
            {
                Directory.Delete(OldPath, true);
            }
            if (Directory.Exists(NewPath))
            {
                Directory.Delete(NewPath, true);
            }
        }

        [TestMethod]
        public void DirectoryCopyExpectedAllFilesCopied()
        {
            //Initialization
            if(!Directory.Exists(OldPath))
            {
                Directory.CreateDirectory(OldPath);
            }
            if(!File.Exists(OldPath + "\\a file in the old temp dir"))
            {
                var stream = File.Create(OldPath + "\\a file in the old temp dir");
                stream.Close();
            }
            if(!Directory.Exists(OldPath + "\\temp"))
            {
                Directory.CreateDirectory(OldPath + "\\temp");
            }
            if (!File.Exists(OldPath + "\\temp\\a file in asub dir in the old temp folder"))
            {
                var stream = File.Create(OldPath + "\\temp\\a file in asub dir in the old temp folder");
                stream.Close();
            }

            //Execute
            FileHelper.MigrateTempData(Context.TestDir);

            //Assert
            Assert.IsTrue(File.Exists(NewPath + "\\a file in the old temp dir"), "File not migrated from old temp folder");
            Assert.IsTrue(File.Exists(NewPath + "\\temp\\a file in asub dir in the old temp folder"), "File in a sub directory of the old temp folder not migrated to new temp folder");
        }
    }
}

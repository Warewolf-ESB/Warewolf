using System;
using Dev2.Providers.Logs;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable InconsistentNaming
namespace Dev2.Infrastructure.Tests.Logs
{
    [TestClass]
    public class CustomTraceListnerTests
    {
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("CustomTraceListner_WithEmptyFileName")]
        public void CustomTraceListener_Constructor_WithNullFileName_ShouldUseDefaultFileName()
        {
            //------------Setup for test--------------------------
            new CustomTextWriter();
            //------------Execute Test---------------------------
            string loggingFileName = CustomTextWriter.LoggingFileName;
            //------------Assert Results-------------------------
            StringAssert.Contains(loggingFileName, "Warewolf Studio.log");
   
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("CustomTraceListner_WithEmptyFileName")]
        public void CustomTraceListener_Constructor_WithEmptyFileName_ShouldUseDefaultFileName()
        {
            //------------Setup for test--------------------------
            new CustomTextWriter();
            //------------Execute Test---------------------------
            string loggingFileName = CustomTextWriter.LoggingFileName;
            //------------Assert Results-------------------------
            StringAssert.Contains(loggingFileName, "Warewolf Studio.log");

        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("CustomTraceListner_WithEmptyFileName")]
        public void CustomTraceListener_StaticLoggingFileNameAccessed_WithEmptyFileName_ShouldUseDefaultFileName()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            string loggingFileName = CustomTextWriter.LoggingFileName;
            //------------Assert Results-------------------------
            StringAssert.Contains(loggingFileName, "Warewolf Studio.log");
        }



        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("CustomTraceListner_WarewolfAppPath")]
        public void CustomTraceListner_WarewolfAppPath_ShouldContainLocalAppDataPathAndWarewolf()
        {
            //------------Setup for test--------------------------
            var localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            //------------Execute Test---------------------------
            var warewolfAppPath = CustomTextWriter.WarewolfAppPath;
            //------------Assert Results-------------------------
            StringAssert.Contains(warewolfAppPath, localAppDataPath);
            StringAssert.Contains(warewolfAppPath, "Warewolf");
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("CustomTraceListner_WarewolfAppPath")]
        public void CustomTraceListner_StudioLogPath_ShouldContainLocalAppDataPathAndWarewolfAndStudioLogs()
        {
            //------------Setup for test--------------------------
            var localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            //------------Execute Test---------------------------
            var warewolfAppPath = CustomTextWriter.StudioLogPath;
            //------------Assert Results-------------------------
            StringAssert.Contains(warewolfAppPath, localAppDataPath);
            StringAssert.Contains(warewolfAppPath, "Warewolf");
            StringAssert.Contains(warewolfAppPath, "Studio Logs");
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("CustomTraceListner_WithEmptyFileName")]
        public void CustomTraceListner_StaticLoggingFileNameAccessed_ShouldHaveFullPathAndDefaultFileName()
        {
            //------------Setup for test--------------------------
            var localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            //------------Execute Test---------------------------
            string loggingFileName = CustomTextWriter.LoggingFileName;
            //------------Assert Results-------------------------
            StringAssert.Contains(loggingFileName, localAppDataPath);
            StringAssert.Contains(loggingFileName, "Warewolf");
            StringAssert.Contains(loggingFileName, "Studio Logs");
            StringAssert.Contains(loggingFileName, "Warewolf Studio.log");
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("CustomTraceListner_WithEmptyFileName")]
        public void CustomTraceListner_StaticLoggingFileNameAccessed_WithFileName_ShouldHaveFullPathAndGivenFileName()
        {
            //------------Setup for test--------------------------
            const string fileName = "Warewolf Studio.log";
            new CustomTextWriter();
            var localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            //------------Execute Test---------------------------
            string loggingFileName = CustomTextWriter.LoggingFileName;
            //------------Assert Results-------------------------
            StringAssert.Contains(loggingFileName, localAppDataPath);
            StringAssert.Contains(loggingFileName, "Warewolf");
            StringAssert.Contains(loggingFileName, "Studio Logs");
            StringAssert.Contains(loggingFileName, fileName);
 
        }

    }
}

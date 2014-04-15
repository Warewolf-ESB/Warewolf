using System;
using System.IO;
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
            var customTextWriter = new CustomTextWriter(null);
            //------------Execute Test---------------------------
            string loggingFileName = CustomTextWriter.LoggingFileName;
            //------------Assert Results-------------------------
            StringAssert.Contains(loggingFileName, "Warewolf Studio.log");
            customTextWriter.CloseTraceWriter();
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("CustomTraceListner_WithEmptyFileName")]
        public void CustomTraceListener_Constructor_WithEmptyFileName_ShouldUseDefaultFileName()
        {
            //------------Setup for test--------------------------
            var customTextWriter = new CustomTextWriter("");
            //------------Execute Test---------------------------
            string loggingFileName = CustomTextWriter.LoggingFileName;
            //------------Assert Results-------------------------
            StringAssert.Contains(loggingFileName, "Warewolf Studio.log");
            customTextWriter.CloseTraceWriter();
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
        [TestCategory("CustomTraceListner_WithEmptyFileName")]
        public void CustomTraceListener_Constructor_WithFileName_ShouldUseFileName()
        {
            //------------Setup for test--------------------------
            const string fileName = "MyLogFile.txt";
            var customTextWriter = new CustomTextWriter(fileName);
            //------------Execute Test---------------------------
            string loggingFileName = CustomTextWriter.LoggingFileName;
            //------------Assert Results-------------------------
            StringAssert.Contains(loggingFileName, fileName);
            customTextWriter.CloseTraceWriter();
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
            const string fileName = "mylogfile.txt";
            var customTextWriter = new CustomTextWriter(fileName);
            var localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            //------------Execute Test---------------------------
            string loggingFileName = CustomTextWriter.LoggingFileName;
            //------------Assert Results-------------------------
            StringAssert.Contains(loggingFileName, localAppDataPath);
            StringAssert.Contains(loggingFileName, "Warewolf");
            StringAssert.Contains(loggingFileName, "Studio Logs");
            StringAssert.Contains(loggingFileName, fileName);
            customTextWriter.CloseTraceWriter();
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("CustomTraceListner_Write")]
        public void CustomTraceListner_Write_NotRollover_ShouldWriteTextWithNoNewLine()
        {
            //------------Setup for test--------------------------
            var customTraceListner = new CustomTextWriter(Guid.NewGuid().ToString());
            const string message = "Some text written";
            string loggingFileName = CustomTextWriter.LoggingFileName;
            //------------Execute Test---------------------------
            customTraceListner.Write(message);
            customTraceListner.CloseTraceWriter();
            //------------Assert Results-------------------------
            var writtenText = File.ReadAllText(loggingFileName);
            StringAssert.Contains(writtenText, message);
            bool hasNewLine = writtenText.Contains(Environment.NewLine);
            Assert.IsFalse(hasNewLine);
            File.Delete(loggingFileName);

        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("CustomTraceListner_Write")]
        public void CustomTraceListner_WriteLine_NotRollover_ShouldWriteTextWithNewLine()
        {
            //------------Setup for test--------------------------
            var customTraceListner = new CustomTextWriter(Guid.NewGuid().ToString());
            const string message = "Some text written";
            string loggingFileName = CustomTextWriter.LoggingFileName;
            //------------Execute Test---------------------------
            customTraceListner.WriteLine(message);
            customTraceListner.CloseTraceWriter();
            //------------Assert Results-------------------------
            var writtenText = File.ReadAllText(loggingFileName);
            StringAssert.Contains(writtenText, message);
            bool hasNewLine = writtenText.Contains(Environment.NewLine);
            Assert.IsTrue(hasNewLine);
            File.Delete(loggingFileName);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("CustomTraceListner_Write")]
        public void CustomTraceListner_WriteLine_Rollover_ShouldWriteRemainderTextToNewFile()
        {
            //------------Setup for test--------------------------
            var customTraceListner = new CustomTextWriter(Guid.NewGuid().ToString());
            const int numberOfChars = 1048576;
            var message = new string('A', numberOfChars);
            string loggingFileName = CustomTextWriter.LoggingFileName;
            customTraceListner.WriteLine(message);
            //------------Execute Test---------------------------
            customTraceListner.WriteLine("BB");
            customTraceListner.CloseTraceWriter();
            //------------Assert Results-------------------------
            var writtenText = File.ReadAllText(loggingFileName);
            bool hasMessage = writtenText.Contains(message);
            Assert.IsFalse(hasMessage);
            bool hasNewText = writtenText.Contains("BB");
            Assert.IsTrue(hasNewText);
            bool hasNewLine = writtenText.Contains(Environment.NewLine);
            Assert.IsTrue(hasNewLine);
            File.Delete(loggingFileName);
        }
    }
}

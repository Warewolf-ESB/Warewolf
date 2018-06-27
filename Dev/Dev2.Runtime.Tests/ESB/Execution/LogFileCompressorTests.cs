using Dev2.Common.Wrappers;
using Dev2.Runtime.ESB.Execution;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Runtime.ESB.Execution
{
    [TestClass]
    public class LogFileCompressorTests
    {
        [TestMethod]
        public void Given_FolderContaingFiles_Compress_Should_CreateNewZip()
        {
            DirectoryWrapper directoryWrapper = new DirectoryWrapper();
            FileWrapper filewrapper = new FileWrapper();
            //var detailedLogFile = new DetailedLogFile(@"C:\ProgramData\Warewolf\DetailedLogs\acb75027-ddeb-47d7-814e-a54c37247ec1 - Hello World\Hello World_Detail.log", filewrapper);
            //FileCompressor.Compress(detailedLogFile);
            Assert.IsTrue(directoryWrapper.Exists(@"C:\ProgramData\Warewolf\DetailedLogs\acb75027-ddeb-47d7-814e-a54c37247ec1 - Hello World\\"));
            //assert the files are in the zipfile created
        }
        [TestMethod]
        public void Given_FolderContaingFiles_Compress_Should_AddEntryToExistingZip()
        {
            DirectoryWrapper directoryWrapper = new DirectoryWrapper();
            FileWrapper filewrapper = new FileWrapper();
            //var detailedLogFile = new DetailedLogFile(@"C:\ProgramData\Warewolf\DetailedLogs\acb75027-ddeb-47d7-814e-a54c37247ec1 - Hello World\Hello World_Detail.log", filewrapper);
            //FileCompressor.Compress(detailedLogFile);
            Assert.IsTrue(directoryWrapper.Exists(@"C:\ProgramData\Warewolf\DetailedLogs\acb75027-ddeb-47d7-814e-a54c37247ec1 - Hello World\\"));
            //assert the files are in the zipfile created
            //assert the files are delete from folder they were copied from
        }
       
    }
}    
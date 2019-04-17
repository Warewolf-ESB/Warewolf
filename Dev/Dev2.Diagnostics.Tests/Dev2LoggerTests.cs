using Dev2.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Dev2.Diagnostics.Test
{
    [TestClass]
    public class Dev2LoggerTests
    {
        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GetLogMaxSize_Scenerio_Result()
        {
            //------------Setup for test-------------------------
            var value = Dev2Logger.GetLogMaxSize();
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.IsNotNull(value);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GetFileLogLevel_Scenerio_Result()
        {
            //------------Setup for test-------------------------
            var logMaxSize = Dev2Logger.GetFileLogLevel();
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.IsNotNull(logMaxSize);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GetEventLogLevel_Scenerio_Result()
        {
            //------------Setup for test-------------------------
            var value = Dev2Logger.GetEventLogLevel();
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.IsNotNull(value);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void GetMappingElement_Scenerio_Result()
        {
            //------------Setup for test-------------------------
            var pr = new PrivateType(typeof(Dev2Logger));
            var value = pr.InvokeStatic("GetMappingElement", "Level0", "ERROR");
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.IsNotNull(value);
            Assert.IsNotNull(value);
            Assert.AreEqual(value.ToString(), @"<mapping>
  <level value=""Level0"" />
  <eventLogEntryType value=""ERROR"" />
</mapping>");
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [DeploymentItem("Settings.config")]
        public void UpdateFileLoggerToProgramData_UpdateFromAsyncRollingFileAppender_ToParallelForwardingAppender()
        {
            //------------Setup for test-------------------------
            Assert.IsTrue(File.ReadAllText("Settings.config").Contains("Log4Net.Async.AsyncRollingFileAppender"));
            Assert.IsFalse(File.ReadAllText("Settings.config").Contains("Log4Net.Async.ParallelForwardingAppender"));
            //------------Execute Test---------------------------
            Dev2Logger.UpdateFileLoggerToProgramData("Settings.config");
            //------------Assert Results-------------------------
            Assert.IsFalse(File.ReadAllText("Settings.config").Contains("Log4Net.Async.AsyncRollingFileAppender"));
            Assert.IsTrue(File.ReadAllText("Settings.config").Contains("Log4Net.Async.ParallelForwardingAppender"));
            Assert.IsTrue(File.ReadAllText("Settings.config").Contains(@"
  <appender name=""LogFileAppender"" type=""Log4Net.Async.ParallelForwardingAppender,Log4Net.Async"">
    <appender-ref ref=""rollingFile"" />
    <bufferSize value=""200"" />
  </appender>
  "));
        }
    };
}
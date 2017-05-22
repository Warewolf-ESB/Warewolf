using System;
using Dev2.Common;
using log4net;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Diagnostics.Test
{
    [TestClass]
    public class Dev2LoggerTests
    {
        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Info_Given_Id_And_Exception_Scenerio_Result()
        {
            //------------Setup for test-------------------------
            object message = "Log this";
            var execId = new Random().Next(35).ToString();
            Exception exception = new Exception("");
            Dev2Logger.Info(message, exception, execId);
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            var id = GlobalContext.Properties["eid"];
            Assert.AreEqual(id, execId);
        }
        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Warn_Given_Id_And_Exception_Scenerio_Result()
        {
            //------------Setup for test-------------------------
            object message = "Log this";
            var execId = new Random().Next(35).ToString();
            Exception exception = new Exception("");
            Dev2Logger.Warn(message, exception, execId);
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            var id = GlobalContext.Properties["eid"];

            Assert.AreEqual(id, execId);
        }
        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Error_Given_Id_And_Exception_Scenerio_Result()
        {
            //------------Setup for test-------------------------
            object message = "Log this";
            var execId = new Random().Next(35).ToString();
            Exception exception = new Exception("");
            Dev2Logger.Error(message, exception, execId);
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            var id = GlobalContext.Properties["eid"];

            Assert.AreEqual(id, execId);
        }
        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Fatal_Given_Id_And_Exception_Scenerio_Result()
        {
            //------------Setup for test-------------------------
            object message = "Log this";
            var execId = new Random().Next(35).ToString();
            Exception exception = new Exception("");
            Dev2Logger.Fatal(message, exception, execId);
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            var id = GlobalContext.Properties["eid"];

            Assert.AreEqual(id, execId);
        }
        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Debug_Given_Id_And_Exception_Scenerio_Result()
        {
            //------------Setup for test-------------------------
            object message = "Log this";
            var execId = new Random().Next(35).ToString();
            Exception exception = new Exception("");
            Dev2Logger.Debug(message, exception, execId);
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            var id = GlobalContext.Properties["eid"];

            Assert.AreEqual(id, execId);
        }
        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Info_Scenerio_Result()
        {
            //------------Setup for test-------------------------
            object message = "Log this";
            var execId = new Random().Next(35).ToString();
            Dev2Logger.Info(message, execId);
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            var id = GlobalContext.Properties["eid"];

            Assert.AreEqual(id, execId);
        }
        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Warn_Scenerio_Result()
        {
            //------------Setup for test-------------------------
            object message = "Log this";
            var execId = new Random().Next(35).ToString();
            Dev2Logger.Warn(message, execId);
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            var id = GlobalContext.Properties["eid"];

            Assert.AreEqual(id, execId);
        }
        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Error_Scenerio_Result()
        {
            //------------Setup for test-------------------------
            object message = "Log this";
            var execId = new Random().Next(35).ToString();
            Dev2Logger.Error(message, execId);
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            var id = GlobalContext.Properties["eid"];

            Assert.AreEqual(id, execId);
        }
        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Fatal_Scenerio_Result()
        {
            //------------Setup for test-------------------------
            object message = "Log this";
            var execId = new Random().Next(35).ToString();
            Dev2Logger.Fatal(message, execId);
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            var id = GlobalContext.Properties["eid"];

            Assert.AreEqual(id, execId);
        }
        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Debug_Scenerio_Result()
        {
            //------------Setup for test-------------------------
            object message = "Log this";
            var execId = new Random().Next(35).ToString();
            Dev2Logger.Debug(message, execId);
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            var id = GlobalContext.Properties["eid"];

            Assert.AreEqual(id, execId);
        }
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
            Assert.AreEqual(value.ToString(), @"<mapping>
  <level value=""Level0"" />
  <eventLogEntryType value=""ERROR"" />
</mapping>");
        }
    }
}
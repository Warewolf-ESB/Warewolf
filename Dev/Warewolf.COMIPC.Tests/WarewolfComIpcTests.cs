using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WarewolfCOMIPC.Client;
using Dev2.Tests.Runtime.ESB.ComPlugin;
using System.Reflection;
using System.IO;
using System.Diagnostics;

// ReSharper disable InconsistentNaming

namespace WarewolfCOMIPC.Test
{
    [TestClass]
    public class WarewolfComIpcTests
    {
        [ClassInitialize]
        public static void Add_Component_To_Registry(TestContext tstctx)
        {
            var runtimeTestsAssembly = Assembly.Load("Dev2.Runtime.Tests");
            var resourceName = "Dev2.Tests.Runtime.ESB.ComPlugin.SystemWOW6432NodeCLSIDadodbConnection.reg";
            var RegistryFilePath = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.FullName + @"\SystemWOW6432NodeCLSIDadodbConnection.reg";
            using (Stream stream = runtimeTestsAssembly.GetManifestResourceStream(resourceName))
            {
                using (var fileStream = File.Create(RegistryFilePath))
                {
                    stream.CopyTo(fileStream);
                }
            }
            Process regeditProcess = Process.Start("regedit.exe", "/s " + RegistryFilePath);
            regeditProcess.WaitForExit();
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("WarewolfCOMIPCClient_Execute")]
        [DeploymentItem("WarewolfCOMIPC.exe")]
        [DeploymentItem("Dev2.Runtime.Tests.dll")]
        public void WarewolfCOMIPCClient_Execute_GetType_ShouldReturnType()
        {
            //------------Setup for test--------------------------
            var clsid = new Guid(ComPluginRuntimeHandlerTest.adodbConnectionClassId);

            //------------Execute Test---------------------------           
            var execute = (KeyValuePair<bool, string>)IpcClient.GetIPCExecutor().Invoke(clsid, "", Execute.GetType,  new ParameterInfoTO[] { });

            //------------Assert Results-------------------------
            Assert.IsNotNull(execute.Value);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [DeploymentItem("WarewolfCOMIPC.exe")]
        [DeploymentItem("Dev2.Runtime.Tests.dll")]
        public void GetMethods_GivenConnection_ShouldReturnMethodList()
        {
            //---------------Set up test pack-------------------
            var classId = new Guid(ComPluginRuntimeHandlerTest.adodbConnectionClassId);

            //---------------Execute Test ----------------------
            var execute = IpcClient.GetIPCExecutor().Invoke(classId, "", Execute.GetMethods, new ParameterInfoTO[] { });
            var enumerable = execute as List<MethodInfoTO>;

            //---------------Test Result -----------------------
            Assert.IsNotNull(enumerable);
            Assert.AreEqual(30, enumerable.Count);
        }
        
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [DeploymentItem("Dev2.Runtime.Tests.dll")]
        public void ExecuteSpecifiedMethod_GivenConnection_ReturnSuccess()
        {
            //---------------Set up test pack-------------------
            var classId = new Guid(ComPluginRuntimeHandlerTest.adodbConnectionClassId);
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
             KeyValuePair<bool, string> execute = (KeyValuePair<bool, string>)IpcClient.GetIPCExecutor().Invoke(classId, "Open", Execute.ExecuteSpecifiedMethod, new ParameterInfoTO[] { });

            //---------------Test Result -----------------------
            
            Assert.IsNotNull(execute.Value);
        }
    }
}

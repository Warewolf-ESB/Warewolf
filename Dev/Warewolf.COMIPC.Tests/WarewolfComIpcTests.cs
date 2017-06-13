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
        public void Add_Component_To_Registry()
        {
            var assemblyDirectory = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.FullName;
            var runtimeTestsAssembly = Assembly.Load(assemblyDirectory + @"\Dev2.Runtime.Tests.dll");
            var resourceName = "Dev2.Tests.Runtime.ESB.ComPlugin.SystemWOW6432NodeCLSIDadodbConnection.reg";
            var RegistryFilePath = assemblyDirectory + @"\SystemWOW6432NodeCLSIDadodbConnection.reg";
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
        public void WarewolfCOMIPCClient_Execute_GetType_ShouldReturnType()
        {
            //------------Setup for test--------------------------
            var clsid = new Guid(ComPluginRuntimeHandlerTest.adodbConnectionClassId);

            //------------Execute Test---------------------------           
            var execute = IpcClient.GetIPCExecutor().Invoke(clsid, "", Execute.GetType,  new ParameterInfoTO[] { });

            //------------Assert Results-------------------------
            Assert.IsNotNull(execute);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [Ignore]//Verfiy that the ID is actually registered
        public void GetMethods_GivenPersonLib_PersonController_ShouldReturnMethodList()
        {
            //---------------Set up test pack-------------------
            var classId = new Guid("2AC49130-C532-4154-B0DC-E930370D36EA");
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            {
                var execute = IpcClient.GetIPCExecutor().Invoke(classId, "", Execute.GetMethods,  new ParameterInfoTO[] { });
                var enumerable = execute as List<MethodInfoTO>;
                Assert.IsNotNull(enumerable);
                //---------------Test Result -----------------------
                Assert.AreNotEqual(10, enumerable.Count);
            }

        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GetMethods_GivenConnection_ShouldReturnMethodList()
        {
            //---------------Set up test pack-------------------
            var classId = new Guid(ComPluginRuntimeHandlerTest.adodbConnectionClassId);
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            {
                var execute = IpcClient.GetIPCExecutor().Invoke(classId, "", Execute.GetMethods, new ParameterInfoTO[] { });
                var enumerable = execute as List<MethodInfoTO>;
                Assert.IsNotNull(enumerable);
                //---------------Test Result -----------------------
                Assert.AreNotEqual(30, enumerable.Count);
            }

        }
        
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ExecuteSpecifiedMethod_GivenConnection_ReturnSuccess()
        {
            //---------------Set up test pack-------------------
            var classId = new Guid(ComPluginRuntimeHandlerTest.adodbConnectionClassId);
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var execute = IpcClient.GetIPCExecutor().Invoke(classId, "Open", Execute.ExecuteSpecifiedMethod, new ParameterInfoTO[] { });

            //---------------Test Result -----------------------
            var actual = execute as string;
            Assert.IsNotNull(actual);
        }
    }
}

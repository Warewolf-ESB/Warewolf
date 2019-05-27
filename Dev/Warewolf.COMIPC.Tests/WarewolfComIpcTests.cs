/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WarewolfCOMIPC.Client;
using Dev2.Tests.Runtime.ESB.ComPlugin;
using System.Reflection;
using System.IO;
using System.Diagnostics;



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
            var regeditProcess = Process.Start("regedit.exe", "/s " + RegistryFilePath);
            regeditProcess.WaitForExit();
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("WarewolfCOMIPCClient_Execute")]
        [DeploymentItem("Warewolf.COMIPC.exe")]
        [DeploymentItem("Dev2.Runtime.Tests.dll")]
        public void WarewolfCOMIPCClient_Execute_GetType_ShouldReturnType()
        {
            //------------Setup for test--------------------------
            var clsid = new Guid(ComPluginRuntimeHandlerTest.adodbConnectionClassId);

            //------------Execute Test---------------------------           
            var execute = (KeyValuePair<bool, string>)IpcClient.GetIpcExecutor().Invoke(clsid, "", Execute.GetType,  new ParameterInfoTO[] { });

            //------------Assert Results-------------------------
            Assert.IsNotNull(execute.Value);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [DeploymentItem("Warewolf.COMIPC.exe")]
        [DeploymentItem("Dev2.Runtime.Tests.dll")]
        public void GetMethods_GivenConnection_ShouldReturnMethodList()
        {
            //---------------Set up test pack-------------------
            var classId = new Guid(ComPluginRuntimeHandlerTest.adodbConnectionClassId);

            //---------------Execute Test ----------------------
            var execute = IpcClient.GetIpcExecutor().Invoke(classId, "", Execute.GetMethods, new ParameterInfoTO[] { });
            var enumerable = execute as List<MethodInfoTO>;

            //---------------Test Result -----------------------
            Assert.IsNotNull(enumerable);
            Assert.AreEqual(30, enumerable.Count);
        }
        
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [DeploymentItem("Dev2.Runtime.Tests.dll")]
        [DeploymentItem("Warewolf.COMIPC.exe"),DeploymentItem("Warewolf.COMIPC.pdb")]
        public void ExecuteSpecifiedMethod_GivenConnection_ReturnSuccess()
        {
            //---------------Set up test pack-------------------
            var classId = new Guid(ComPluginRuntimeHandlerTest.adodbConnectionClassId);
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
             var execute = (KeyValuePair<bool, string>)IpcClient.GetIpcExecutor().Invoke(classId, "Open", Execute.ExecuteSpecifiedMethod, new ParameterInfoTO[] { });

            //---------------Test Result -----------------------

            Assert.IsNotNull(execute.Value);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WarewolfCOMIPC))]
        [DeploymentItem("Dev2.Runtime.Tests.dll")]
        [DeploymentItem("Warewolf.COMIPC.exe"), DeploymentItem("Warewolf.COMIPC.pdb")]
        public void ExecuteSpecifiedMethod_Instance_IsNotNull()
        {
            //---------------Arrange----------------------------
            var classId = new Guid(ComPluginRuntimeHandlerTest.adodbConnectionClassId);
            //---------------Act--------------------------------
            var execute = IpcClient.Instance; 
            //---------------Assert-----------------------------
            Assert.IsNotNull(execute);
        }
    }
}

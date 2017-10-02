/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Dev2.Common.ExtMethods;
using Dev2.Runtime;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Runtime.ServiceModel.Esb.Brokers.ComPlugin;
using DummyNamespaceForTest;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using TestingDotnetDllCascading;
using WarewolfCOMIPC.Client;
using System.Diagnostics;

namespace Dev2.Tests.Runtime.ESB.ComPlugin
{

    /// <summary>
    /// Summary description for ComPluginRuntimeHandlerTest
    /// </summary>
    [TestClass]

    public class ComPluginRuntimeHandlerTest
    {
        public const string adodbConnectionClassId = "00000514-0000-0010-8000-00AA006D2EA4";

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        
        public TestContext TestContext { get; set; }

        [ClassInitialize]
        public static void Add_Component_To_Registry(TestContext tstctx)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var assemblyDirectory = new FileInfo(assembly.Location).Directory.FullName;
            var resourceName = "Dev2.Tests.Runtime.ESB.ComPlugin.SystemWOW6432NodeCLSIDadodbConnection.reg";
            var RegistryFilePath = assemblyDirectory + @"\SystemWOW6432NodeCLSIDadodbConnection.reg";
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                using (var fileStream = File.Create(RegistryFilePath))
                {
                    stream.CopyTo(fileStream);
                }
            }
            Process regeditProcess = Process.Start("regedit.exe", "/s " + RegistryFilePath);
            regeditProcess.WaitForExit();
        }

        #region FetchNamespaceListObject

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("ComPluginRuntimeHandler_ListMethods")]
        public void ComPluginRuntimeHandler_ListMethods_WhenValidDll_ExpectListMethods_32bit()
        {
            //------------Setup for test--------------------------
            var source = CreateComPluginSource();
            source.Is32Bit = true;
            //------------Execute Test---------------------------
            var mock = new Mock<INamedPipeClientStreamWrapper>();
            var memoryStream = new MemoryStream();
            var list = new List<string>() { "Home" };
            memoryStream.WriteByte(Encoding.ASCII.GetBytes(list.SerializeToJsonString(new KnownTypesBinder()))[0]);
            mock.Setup(wrapper => wrapper.GetInternalStream()).Returns(memoryStream);
            var isolated = new ComPluginRuntimeHandler(mock.Object);
            var serviceMethodList = isolated.ListMethods(adodbConnectionClassId, true);
            //------------Assert Results-------------------------
            Assert.AreEqual(0, serviceMethodList.Count);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("ComPluginRuntimeHandler_ListMethods")]
        public void ComPluginRuntimeHandler_ListMethods_WhenValidDll_ExpectListMethods_64Bit()
        {
            //------------Setup for test--------------------------
            var source = CreateComPluginSource();
            source.Is32Bit = false;
            //------------Execute Test---------------------------
            var mock = new Mock<INamedPipeClientStreamWrapper>();
            var memoryStream = new MemoryStream();
            var list = new List<string>() { "Home" };
            memoryStream.WriteByte(Encoding.ASCII.GetBytes(list.SerializeToJsonString(new KnownTypesBinder()))[0]);
            mock.Setup(wrapper => wrapper.GetInternalStream()).Returns(memoryStream);
            var isolated = new ComPluginRuntimeHandler(mock.Object);
            var result = isolated.ListMethods(adodbConnectionClassId, false);
            //------------Assert Results-------------------------
            CollectionAssert.AllItemsAreUnique(result);
            CollectionAssert.AllItemsAreNotNull(result);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("ComPluginRuntimeHandler_FetchNamespaceListObject")]
        public void ComPluginRuntimeHandler_FetchNamespaceListObject_WhenValidDll_ExpectNamespaces()
        {
            //------------Setup for test--------------------------
            var source = CreateComPluginSource();
            source.Is32Bit = true;
            //------------Execute Test---------------------------
            var mock = new Mock<INamedPipeClientStreamWrapper>();
            var memoryStream = new MemoryStream();
            var list = new List<string>() { "Home" };
            memoryStream.WriteByte(Encoding.ASCII.GetBytes(list.SerializeToJsonString(new KnownTypesBinder()))[0]);
            mock.Setup(wrapper => wrapper.GetInternalStream()).Returns(memoryStream);
            var isolated = new ComPluginRuntimeHandler(mock.Object);
            var result = isolated.FetchNamespaceListObject(source);
            //------------Assert Results-------------------------
            Assert.AreEqual(1, result.Count);
            var assemblyLocation = result[0].AssemblyLocation;
            Assert.IsTrue(string.IsNullOrEmpty(assemblyLocation));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void BuildValuedTypeParams_GivenValid_ShouldPassThrough()
        {
            //---------------Set up test pack-------------------
            var type = typeof(ComPluginRuntimeHandler);
            var methodInfo = type.GetMethod("BuildValuedTypeParams", BindingFlags.Static | BindingFlags.NonPublic);
            ComPluginInvokeArgs args = new ComPluginInvokeArgs
            {
                ClsId = adodbConnectionClassId,
                Is32Bit = false,
                Method = "ToString",
                Parameters = new List<MethodParameter>()
                {
                    new MethodParameter()
                    {
                        Value = "hi",
                        TypeName = typeof(string).FullName
                    }
                }
            };
            //---------------Execute Test ----------------------
            var enumerable = methodInfo.Invoke("BuildValuedTypeParams", new object[] { args }) as IEnumerable<object>;
            //---------------Test Result -----------------------
            Assert.AreEqual(1,enumerable?.Count());
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void BuildValuedTypeParams_GivenValidObjectparam_ShouldPassThrough()
        {
            //---------------Set up test pack-------------------
            var type = typeof(ComPluginRuntimeHandler);
            var methodInfo = type.GetMethod("BuildValuedTypeParams", BindingFlags.Static | BindingFlags.NonPublic);
            ComPluginInvokeArgs args = new ComPluginInvokeArgs
            {
                ClsId = adodbConnectionClassId,
                Is32Bit = false,
                Method = "ToString",
                Parameters = new List<MethodParameter>()
                {
                    new MethodParameter()
                    {
                        Value = new Human().SerializeToJsonString(new DefaultSerializationBinder())
                        ,
                        TypeName = typeof(Human).FullName
                    }
                }
            };
            //---------------Execute Test ----------------------
            var enumerable = methodInfo.Invoke("BuildValuedTypeParams", new object[] { args }) as IEnumerable<object>;
            //---------------Test Result -----------------------
            Assert.AreEqual(1,enumerable?.Count());
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("ComPluginRuntimeHandler_FetchNamespaceListObject")]
        [ExpectedException(typeof(NullReferenceException))]
        public void ComPluginRuntimeHandler_FetchNamespaceListObject_WhenNullDll_ExpectException()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            using (Isolated<ComPluginRuntimeHandler> isolated = new Isolated<ComPluginRuntimeHandler>())
            {
                isolated.Value.FetchNamespaceListObject(null);
                //------------Assert Results-------------------------
            }
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("ComPluginRuntimeHandler_FetchNamespaceListObject")]
        [ExpectedException(typeof(NullReferenceException))]
        public void ComPluginRuntimeHandler_FetchNamespaceListObject_WhenNullLocationAndInvalidSourceID_ExpectException()
        {
            //------------Setup for test--------------------------
            using (var isolated = new Isolated<ComPluginRuntimeHandler>())
            {
                isolated.Value.FetchNamespaceListObject(new ComPluginSource());
            }

        }

        #endregion

        #region ValidatePlugin

        #endregion

        #region ListNamespaces

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("ComPluginRuntimeHandler_ListNamespaces")]
        public void ComPluginRuntimeHandler_ListNamespaces_WhenValidClassID_ExpectNamespaces()
        {
            //------------Setup for test--------------------------
            var source = CreateComPluginSource();
            //------------Execute Test---------------------------
            using (var isolated = new Isolated<ComPluginRuntimeHandler>())
            {
                var result = isolated.Value.FetchNamespaceListObject(source);
                Assert.IsNotNull(result);
            }
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("ComPluginRuntimeHandler_ListNamespaces")]
        [ExpectedException(typeof(NullReferenceException))]
        public void ComPluginRuntimeHandler_ListNamespaces_WhenNullLocation_ExpectException()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            using (var isolated = new Isolated<ComPluginRuntimeHandler>())
            {
                isolated.Value.FetchNamespaceListObject(null);
            }
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("ComPluginRuntimeHandler_ListMethods")]
        public void ComPluginRuntimeHandler_ListMethods_WhenInvalidLocation_ExpectNoResults()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            using (var isolated = new Isolated<ComPluginRuntimeHandler>())
            {
                var result = isolated.Value.ListMethods(string.Empty, false);
                Assert.AreEqual(0, result.Count);
            }
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("ComPluginRuntimeHandler_ListMethods")]
        [DeploymentItem("WarewolfCOMIPC.exe")]
        public void ComPluginRuntimeHandler_ListMethods_WhenValidLocation_ExpectResults()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            using (var isolated = new Isolated<ComPluginRuntimeHandler>())
            {
                var result = isolated.Value.ListMethods(adodbConnectionClassId, true);
                CollectionAssert.AllItemsAreUnique(result);
                Assert.AreNotEqual(0, result.Count);
            }
        }

        #endregion

        #region Run and test

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("ComPluginRuntimeHandler_Run")]
        public void ComPluginRuntimeHandler_Run_WhenInvalidMethod_ExpectNoReturn()
        {
            //------------Setup for test--------------------------
            var svc = CreatePluginService();
            CreateComPluginSource();
            //------------Execute Test-------------------------- -
            var isolated = new ComPluginRuntimeHandler();

            var args = new ComPluginInvokeArgs { ClsId = adodbConnectionClassId, Fullname = svc.Namespace, Method = "InvalidName", Parameters = svc.Method.Parameters };
            var run = isolated.Run(args);
            Assert.IsNull(run);

        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("ComPluginRuntimeHandler_Test")]
        public void ComPluginRuntimeHandler_Test_WhenInvalidMethod_ExpectNoReturn()
        {
            //------------Setup for test--------------------------
            var svc = CreatePluginService();
            CreateComPluginSource();
            //------------Execute Test-------------------------- -
            var isolated = new ComPluginRuntimeHandler();
            var args = new ComPluginInvokeArgs { ClsId = adodbConnectionClassId, Fullname = svc.Namespace, Method = "InvalidName", Parameters = svc.Method.Parameters };
            var run = isolated.Test(args, out string outString);
            Assert.IsNotNull(run);
            Assert.IsNull(outString);

        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("ComPluginRuntimeHandler_Test")]
        public void ComPluginRuntimeHandler_Test_WhenValidMethod_ExpectReturn()
        {
            //------------Setup for test--------------------------
            var svc = CreatePluginService();
            CreateComPluginSource();
            var mock = new Mock<INamedPipeClientStreamWrapper>();
            var memoryStream = new MemoryStream();
            var list = new List<string>() { "Home" };
            memoryStream.WriteByte(Encoding.ASCII.GetBytes(list.SerializeToJsonString(new KnownTypesBinder()))[0]);
            mock.Setup(wrapper => wrapper.GetInternalStream()).Returns(memoryStream);
            //------------Execute Test-------------------------- -
            var isolated = new ComPluginRuntimeHandler(mock.Object);
            var args = new ComPluginInvokeArgs { ClsId = adodbConnectionClassId, Fullname = svc.Namespace, Method = "ToString", Parameters = svc.Method.Parameters, Is32Bit = true };
            var run = isolated.Test(args, out string outString);
            Assert.IsNotNull(run);
            Assert.IsNotNull(outString);

        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("ComPluginRuntimeHandler_Run")]
        [ExpectedException(typeof(NullReferenceException))]
        public void ComPluginRuntimeHandler_Run_WhenNullParameters_ExpectException()
        {
            //------------Setup for test--------------------------
            var svc = CreatePluginService();
            var source = CreateComPluginSource();
            //------------Execute Test---------------------------
            using (var isolated = new Isolated<ComPluginRuntimeHandler>())
            {
                var args = new ComPluginInvokeArgs { ClsId = source.ClsId, AssemblyName = "Foo", Fullname = svc.Namespace, Method = svc.Method.Name, Parameters = null };
                isolated.Value.Run(args);
            }
        }



        #endregion



        #region Helper Methods

        static ComPluginSource CreateComPluginSource(bool invalidResourceID = false)
        {

            var resourceID = Guid.Empty;
            if (!invalidResourceID)
            {
                resourceID = Guid.NewGuid();
            }

            return new ComPluginSource
            {
                ResourceID = resourceID,
                ResourceName = "Dummy",
                ResourceType = "ComPluginSource",
                ClsId = adodbConnectionClassId
            };
        }

        private static ComPluginService CreatePluginService()
        {
            return CreatePluginService(new ServiceMethod
            {
                Name = "DummyMethod"
            });
        }

        private static ComPluginService CreatePluginService(ServiceMethod method)
        {
            var type = typeof(DummyClassForPluginTest);

            var source = CreateComPluginSource();
            var service = new ComPluginService
            {
                ResourceID = Guid.NewGuid(),
                ResourceName = "DummyPluginService",
                ResourceType = "PluginService",
                Namespace = type.FullName,
                Method = method,
                Source = source,
            };
            return service;
        }

        #endregion
    }
}

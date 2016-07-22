/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Runtime.ServiceModel.Esb.Brokers.ComPlugin;
using Dev2.Runtime.ServiceModel.Esb.Brokers.Plugin;
using DummyNamespaceForTest;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Dev2.Tests.Runtime.ESB.ComPlugin
{
    /// <summary>
    /// Summary description for PluginServiceExecutionFactory
    /// </summary>
    [TestClass]
    public class ComPluginServiceExecutionFactoryTest
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }


        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("PluginServiceExecutionFactory_GetNamespaces")]
        public void PluginRuntimeHandler_GetNamespaces_WhenValidDll_ExpectNamespaces()
        {
            //------------Setup for test--------------------------
            var source = CreatePluginSource();
            //------------Execute Test---------------------------
            using (Isolated<ComPluginRuntimeHandler> isolated = new Isolated<ComPluginRuntimeHandler>())
            {
                var result = ComPluginServiceExecutionFactory.GetNamespaces(source);
                //------------Assert Results-------------------------
                Assert.IsTrue(result.Count > 0);
            }
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("PluginServiceExecutionFactory_GetNamespaces")]
        [ExpectedException(typeof(NullReferenceException))]
        public void PluginRuntimeHandler_GetNamespaces_WhenNullDll_ExpectException()
        {
            //------------Execute Test---------------------------
            ComPluginServiceExecutionFactory.GetNamespaces(null);
        }

        
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("PluginServiceExecutionFactory_GetMethods")]
        public void PluginRuntimeHandler_GetMethods_WhenValidDll_ExpectValidResults()
        {
            //------------Setup for test--------------------------
            var source = CreatePluginSource();
            var service = CreatePluginService();
            //------------Execute Test---------------------------
            using (Isolated<ComPluginRuntimeHandler> isolated = new Isolated<ComPluginRuntimeHandler>())
            {
                var result = ComPluginServiceExecutionFactory.GetMethods(source.ClsId);
                //------------Assert Results-------------------------
                Assert.IsTrue(result.Count > 0);
            }            
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("PluginServiceExecutionFactory_InvokePlugin")]
        public void PluginRuntimeHandler_InvokePlugin_WhenValidDll_ExpectValidResults()
        {
           /* //------------Setup for test--------------------------
            var source = CreatePluginSource();
            var svc = CreatePluginService();
                     
            

            //------------Execute Test---------------------------
            using (Isolated<PluginRuntimeHandler> isolated = new Isolated<PluginRuntimeHandler>())
            {
                ComPluginInvokeArgs args = new ComPluginInvokeArgs { ClsId = source.ClsId, AssemblyName = "Foo", Fullname = svc.Namespace, Method = svc.Method.Name, Parameters = svc.Method.Parameters };
                var result = ComPluginServiceExecutionFactory.InvokeComPlugin(args);
                var castResult = JsonConvert.DeserializeObject(result.ToString()) as dynamic;
                //------------Assert Results-------------------------
                if (castResult != null)
                {
                    StringAssert.Contains(castResult.Name.ToString(), "test data");
                }
                else
                {
                    Assert.Fail("Failed Conversion for Assert");
                }
            }  */
            
        }

        #region Helper Methods

        static ComPluginSource CreatePluginSource(bool invalidResourceID = false)
        {
            Guid resourceID = Guid.Empty;
            if(!invalidResourceID)
            {
                resourceID = Guid.NewGuid();
            }

            return new ComPluginSource
            {
                ClsId = Guid.NewGuid().ToString(),
                ResourceID = resourceID,
                ResourceName = "Dummy",
                ResourceType = "ComPluginSource",
                ResourcePath = "Test",
                ProgId = Guid.NewGuid().ToString()
            };
        }

        public static ComPluginService CreatePluginService()
        {
            return CreatePluginService(new ServiceMethod
            {
                Name = "DummyMethod"
            });
        }

        public static ComPluginService CreatePluginService(ServiceMethod method)
        {
            var type = typeof(DummyClassForPluginTest);

            var source = CreatePluginSource();
            var service = new ComPluginService
            {
                ResourceID = Guid.NewGuid(),
                ResourceName = "DummyPluginService",
                ResourceType = "PluginService",
                ResourcePath = "Tests",
                Namespace = type.FullName,
                Method = method,
                Source = source
            };
            return service;
        }

        #endregion
    }
}

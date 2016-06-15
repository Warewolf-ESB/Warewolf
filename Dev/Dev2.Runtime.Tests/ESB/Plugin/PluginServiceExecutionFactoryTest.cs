
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
using Dev2.Runtime.ServiceModel.Esb.Brokers.Plugin;
using DummyNamespaceForTest;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Runtime.ESB.Plugin
{
    /// <summary>
    /// Summary description for PluginServiceExecutionFactory
    /// </summary>
    [TestClass]
    public class PluginServiceExecutionFactoryTest
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }


        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("PluginServiceExecutionFactory_GetNamespaces")]
        public void PluginRuntimeHandler_GetNamespaces_WhenValidDll_ExpectNamespaces()
        {
            //------------Setup for test--------------------------
            var source = CreatePluginSource();
            //------------Execute Test---------------------------
            using (Isolated<PluginRuntimeHandler> isolated = new Isolated<PluginRuntimeHandler>())
            {
                var result = PluginServiceExecutionFactory.GetNamespaces(source);
                //------------Assert Results-------------------------
                Assert.IsTrue(result.Count > 0);
            }
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("PluginServiceExecutionFactory_GetNamespaces")]
        [ExpectedException(typeof(NullReferenceException))]
        public void PluginRuntimeHandler_GetNamespaces_WhenNullDll_ExpectException()
        {
            //------------Execute Test---------------------------
            PluginServiceExecutionFactory.GetNamespaces(null);
        }

        
        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("PluginServiceExecutionFactory_GetMethods")]
        public void PluginRuntimeHandler_GetMethods_WhenValidDll_ExpectValidResults()
        {
            //------------Setup for test--------------------------
            var source = CreatePluginSource();
            var service = CreatePluginService();
            //------------Execute Test---------------------------
            using (Isolated<PluginRuntimeHandler> isolated = new Isolated<PluginRuntimeHandler>())
            {
                var result = PluginServiceExecutionFactory.GetMethods(source.AssemblyLocation, source.AssemblyName, service.Namespace);
                //------------Assert Results-------------------------
                Assert.IsTrue(result.Count > 0);
            }            
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("PluginServiceExecutionFactory_InvokePlugin")]
        public void PluginRuntimeHandler_InvokePlugin_WhenValidDll_ExpectValidResults()
        {
            //------------Setup for test--------------------------
            var source = CreatePluginSource();
            var svc = CreatePluginService();
                     
            

            //------------Execute Test---------------------------
            using (Isolated<PluginRuntimeHandler> isolated = new Isolated<PluginRuntimeHandler>())
            {
                PluginInvokeArgs args = new PluginInvokeArgs { AssemblyLocation = source.AssemblyLocation, AssemblyName = "Foo", Fullname = svc.Namespace, Method = svc.Method.Name, Parameters = svc.Method.Parameters };
                var result = PluginServiceExecutionFactory.InvokePlugin(args);
                var castResult = result as DummyClassForPluginTest;
                //------------Assert Results-------------------------
                if (castResult != null)
                {
                    StringAssert.Contains(castResult.Name, "test data");
                }
                else
                {
                    Assert.Fail("Failed Conversion for Assert");
                }
            }  
            
        }

        #region Helper Methods

        static PluginSource CreatePluginSource(bool nullLocation = false, bool invalidResourceID = false)
        {
            var type = typeof(DummyClassForPluginTest);
            var assembly = type.Assembly;

            string loc = null;
            if(!nullLocation)
            {
                loc = assembly.Location;
            }

            Guid resourceID = Guid.Empty;
            if(!invalidResourceID)
            {
                resourceID = Guid.NewGuid();
            }

            return new PluginSource
            {
                AssemblyLocation = loc,
                ResourceID = resourceID,
                ResourceName = "Dummy",
                ResourceType = "PluginSource",
                ResourcePath = "Test",
            };
        }

        public static PluginService CreatePluginService()
        {
            return CreatePluginService(new ServiceMethod
            {
                Name = "DummyMethod"
            });
        }

        public static PluginService CreatePluginService(ServiceMethod method)
        {
            var type = typeof(DummyClassForPluginTest);

            var source = CreatePluginSource();
            var service = new PluginService
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

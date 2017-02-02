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
using Dev2.Runtime;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Runtime.ServiceModel.Esb.Brokers.ComPlugin;
using DummyNamespaceForTest;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable InconsistentNaming

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



        /*[TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ListMethods_GivenAdodbConnection_ShouldContainOpen()
        {
            //---------------Set up test pack-------------------
            const string adodbConGuid = "00000514-0000-0010-8000-00AA006D2EA4";
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var ns = ComPluginServiceExecutionFactory.GetNamespaces(new ComPluginSource { ClsId = adodbConGuid });
            Assert.IsNotNull(ns);
            
            var result = ComPluginServiceExecutionFactory.GetMethods(adodbConGuid,true);
            //------------Assert Results-------------------------
            var openMethod = result.First(method => method.Name.ToUpper() == "open".ToUpper());
            //---------------Test Result -----------------------
            Assert.IsNotNull(openMethod);
        }
        */
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
                Is32Bit =false
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
                Namespace = type.FullName,
                Method = method,
                Source = source
            };
            return service;
        }

        #endregion
    }
}

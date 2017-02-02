/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Runtime.ESB.ComPlugin
{


    /// <summary>
    /// Summary description for ComPluginRuntimeHandlerTest
    /// </summary>
    [TestClass]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class ComComPluginRuntimeHandlerTest
    {

        //[TestMethod]
        //[Owner("Nkosinathi Sangweni")]
        //[TestCategory("ComPluginRuntimeHandler_ListMethods")]
        //public void ComPluginRuntimeHandler_ListMethods_WhenInvalidLocation_ExpectNoResults()
        //{
        //    //------------Setup for test--------------------------
        //    //------------Execute Test---------------------------
        //    using (var isolated = new Isolated<ComPluginRuntimeHandler>())
        //    {
        //        var result = isolated.Value.ListMethods("D267A886-0BAD-4457-BD3A-B800D3C671D0", false);
        //        Assert.AreEqual(10, result.Count);
        //    }
        //}

    //        private const string adodbConnectionClassId = "00000514-0000-0010-8000-00AA006D2EA4";
    //        /// <summary>
    //        ///Gets or sets the test context which provides
    //        ///information about and functionality for the current test run.
    //        ///</summary>
    //        // ReSharper disable once UnusedMember.Global
    //        public TestContext TestContext { get; set; }
    //
    //        #region FetchNamespaceListObject
    //
    //        [TestMethod]
    //        [Owner("Nkosinathi Sangweni")]
    //        [TestCategory("ComPluginRuntimeHandler_FetchNamespaceListObject")]
    //        public void ComPluginRuntimeHandler_FetchNamespaceListObject_WhenValidDll_ExpectNamespaces()
    //        {
    //            //------------Setup for test--------------------------
    //            var source = CreateComPluginSource();
    //            //------------Execute Test---------------------------
    //            using (var isolated = new Isolated<ComPluginRuntimeHandler>())
    //            {
    //                var result = isolated.Value.FetchNamespaceListObject(source);
    //                //------------Assert Results-------------------------
    //                Assert.IsTrue(result.Count > 0);
    //            }
    //        }
    //
    //        [TestMethod]
    //        [Owner("Nkosinathi Sangweni")]
    //        [TestCategory("ComPluginRuntimeHandler_FetchNamespaceListObject")]
    //        [ExpectedException(typeof(NullReferenceException))]
    //        public void ComPluginRuntimeHandler_FetchNamespaceListObject_WhenNullDll_ExpectException()
    //        {
    //            //------------Setup for test--------------------------
    //            //------------Execute Test---------------------------
    //            using (Isolated<ComPluginRuntimeHandler> isolated = new Isolated<ComPluginRuntimeHandler>())
    //            {
    //                isolated.Value.FetchNamespaceListObject(null);
    //                //------------Assert Results-------------------------
    //            }
    //        }
    //
    //        [TestMethod]
    //        [Owner("Nkosinathi Sangweni")]
    //        [TestCategory("ComPluginRuntimeHandler_FetchNamespaceListObject")]
    //        [ExpectedException(typeof(NullReferenceException))]
    //        public void ComPluginRuntimeHandler_FetchNamespaceListObject_WhenNullLocationAndInvalidSourceID_ExpectException()
    //        {
    //            //------------Setup for test--------------------------
    //            using (var isolated = new Isolated<ComPluginRuntimeHandler>())
    //            {
    //                isolated.Value.FetchNamespaceListObject(new ComPluginSource());
    //            }
    //
    //        }
    //
    //        #endregion
    //
    //        #region ValidatePlugin
    //
    //        #endregion
    //
    //        #region ListNamespaces
    //
    //        [TestMethod]
    //        [Owner("Nkosinathi Sangweni")]
    //        [TestCategory("ComPluginRuntimeHandler_ListNamespaces")]
    //        public void ComPluginRuntimeHandler_ListNamespaces_WhenValidClassID_ExpectNamespaces()
    //        {
    //            //------------Setup for test--------------------------
    //            var source = CreateComPluginSource();
    //            //------------Execute Test---------------------------
    //            using (var isolated = new Isolated<ComPluginRuntimeHandler>())
    //            {
    //                var result = isolated.Value.ListNamespaces(source.ClsId, false);
    //                Assert.IsNotNull(result);
    //            }
    //        }
    //
    //        [TestMethod]
    //        [Owner("Nkosinathi Sangweni")]
    //        [TestCategory("ComPluginRuntimeHandler_ListNamespaces")]
    //        [ExpectedException(typeof(NullReferenceException))]
    //        public void ComPluginRuntimeHandler_ListNamespaces_WhenNullLocation_ExpectException()
    //        {
    //            //------------Setup for test--------------------------
    //            //------------Execute Test---------------------------
    //            using (var isolated = new Isolated<ComPluginRuntimeHandler>())
    //            {
    //                isolated.Value.ListNamespaces(null,false);
    //            }
    //        }
    //
    //        [TestMethod]
    //        [Owner("Nkosinathi Sangweni")]
    //        [TestCategory("ComPluginRuntimeHandler_ListMethods")]
    //        public void ComPluginRuntimeHandler_ListMethods_WhenInvalidLocation_ExpectNoResults()
    //        {
    //            //------------Setup for test--------------------------
    //            //------------Execute Test---------------------------
    //            using (var isolated = new Isolated<ComPluginRuntimeHandler>())
    //            {
    //                var result = isolated.Value.ListMethods(string.Empty,false);
    //                Assert.AreEqual(0, result.Count);
    //            }
    //        }
    //
    //        [TestMethod]
    //        [Owner("Nkosinathi Sangweni")]
    //        [TestCategory("ComPluginRuntimeHandler_ListMethods")]
    //        public void ComPluginRuntimeHandler_ListMethods_WhenValidLocation_ExpectResults()
    //        {
    //            //------------Setup for test--------------------------
    //            //------------Execute Test---------------------------
    //            using (var isolated = new Isolated<ComPluginRuntimeHandler>())
    //            {
    //                var result = isolated.Value.ListMethods(adodbConnectionClassId,true);
    //                Assert.AreEqual(55, result.Count);
    //            }
    //        }
    //
    //        #endregion
    //
    //        #region Run
    //
    //        [TestMethod]
    //        [Owner("Nkosinathi Sangweni")]
    //        [TestCategory("ComPluginRuntimeHandler_Run")]
    //        public void ComPluginRuntimeHandler_Run_WhenInvalidMethod_ExpectException()
    //        {
    //            //------------Setup for test--------------------------
    //            var svc = CreatePluginService();
    //            CreateComPluginSource();
    //            //------------Execute Test---------------------------
    //            using (var isolated = new Isolated<ComPluginRuntimeHandler>())
    //            {
    //                var args = new ComPluginInvokeArgs { ClsId = adodbConnectionClassId, Fullname = svc.Namespace, Method = "InvalidName", Parameters = svc.Method.Parameters };
    //                var run = isolated.Value.Run(args);
    //                Assert.IsNotNull(run);
    //            }
    //        }
    //
    //        [TestMethod]
    //        [Owner("Nkosinathi Sangweni")]
    //        [TestCategory("ComPluginRuntimeHandler_Run")]
    //        [ExpectedException(typeof(NullReferenceException))]
    //        public void ComPluginRuntimeHandler_Run_WhenNullParameters_ExpectException()
    //        {
    //            //------------Setup for test--------------------------
    //            var svc = CreatePluginService();
    //            var source = CreateComPluginSource();
    //            //------------Execute Test---------------------------
    //            using (var isolated = new Isolated<ComPluginRuntimeHandler>())
    //            {
    //                var args = new ComPluginInvokeArgs { ClsId = source.ClsId, AssemblyName = "Foo", Fullname = svc.Namespace, Method = svc.Method.Name, Parameters = null };
    //                isolated.Value.Run(args);
    //            }
    //        }
    //
    //        #endregion
    //
    //
    //
    //        #region Helper Methods
    //
    //        static ComPluginSource CreateComPluginSource(bool invalidResourceID = false)
    //        {
    //
    //            var resourceID = Guid.Empty;
    //            if (!invalidResourceID)
    //            {
    //                resourceID = Guid.NewGuid();
    //            }
    //
    //            return new ComPluginSource
    //            {
    //                ResourceID = resourceID,
    //                ResourceName = "Dummy",
    //                ResourceType = "ComPluginSource",
    //                ResourcePath = "Test",
    //                ClsId = adodbConnectionClassId
    //            };
    //        }
    //
    //        private static ComPluginService CreatePluginService()
    //        {
    //            return CreatePluginService(new ServiceMethod
    //            {
    //                Name = "DummyMethod"
    //            });
    //        }
    //
    //        private static ComPluginService CreatePluginService(ServiceMethod method)
    //        {
    //            var type = typeof(DummyClassForPluginTest);
    //
    //            var source = CreateComPluginSource();
    //            var service = new ComPluginService
    //            {
    //                ResourceID = Guid.NewGuid(),
    //                ResourceName = "DummyPluginService",
    //                ResourceType = "PluginService",
    //                ResourcePath = "Tests",
    //                Namespace = type.FullName,
    //                Method = method,
    //                Source = source,
    //
    //            };
    //            return service;
    //        }
    //
    //        #endregion
}
}

using System;
using System.Collections.Generic;
using System.Reflection;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Studio.Core;
using Dev2.Common.Interfaces.Studio.Core.Controller;
using Dev2.Common.Interfaces.Toolbox;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Studio.ServerProxyLayer;
using Warewolf.UnittestingUtils;

namespace ServerProxyLayerTests
{
    [TestClass]
    public class PluginProxyTests
    {
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("PluginProxy_Ctor")]
        // ReSharper disable InconsistentNaming
        public void PluginProxy_Ctor_NullArguments_ExpectError()

        {
            //------------Setup for test--------------------------
            var cf = new Mock<ICommunicationControllerFactory>();
            var cn = new Mock<IEnvironmentConnection>();
            NullArgumentConstructorHelper.AssertNullConstructor(new object[]{cf.Object,cn.Object}, typeof(PluginProxy));

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("PluginProxy_Ctor")]
        public void PluginProxy_Ctor_ValidArgs_ExpectError()
        {
            //------------Setup for test--------------------------
            var cf = new Mock<ICommunicationControllerFactory>();
            var cn = new Mock<IEnvironmentConnection>();
            // ReSharper disable ObjectCreationAsStatement
            new PluginProxy(cf.Object, cn.Object);
            // ReSharper restore ObjectCreationAsStatement


        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("PluginProxy_DeleteTool")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PluginProxy_DeleteTool_NullArgs_ExpectError()
        {
            //------------Setup for test--------------------------
            var cf = new Mock<ICommunicationControllerFactory>();
            var cn = new Mock<IEnvironmentConnection>();
            PluginProxy proxy = new PluginProxy(cf.Object, cn.Object);
            proxy.DeleteTool(null);

        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("PluginProxy_DeleteTool")]
        public void PluginProxy_DeleteTool_ValidArgs_ExpectPassThrough()
        {
            //------------Setup for test--------------------------
            var cf = new Mock<ICommunicationControllerFactory>();
            var comms = new Mock<ICommunicationController>();
             var cn = new Mock<IEnvironmentConnection>();
             var result = new Mock<IEsbRequestResult<string>>();
             result.Setup(a => a.HasErrors).Returns(false);
            cf.Setup(a => a.CreateController("DeleteToolService")).Returns(comms.Object);
            // ReSharper disable MaximumChainedReferences
            comms.Setup(a => a.ExecuteCommand<IEsbRequestResult<string>>(cn.Object, Guid.Empty))
                 .Returns(result.Object);
            // ReSharper restore MaximumChainedReferences
           
            PluginProxy proxy = new PluginProxy(cf.Object, cn.Object);
            proxy.DeleteTool(new Mock<IToolDescriptor>().Object);
            cf.Verify(a => a.CreateController("DeleteToolService"));

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("PluginProxy_DeleteTool")]
        [ExpectedException(typeof(TargetInvocationException))]
        public void PluginProxy_DeleteTool_ValidArgs_ExpectPassThroughAndExceptionThrown()
        {
            //------------Setup for test--------------------------
            var cf = new Mock<ICommunicationControllerFactory>();
            var comms = new Mock<ICommunicationController>();
            var cn = new Mock<IEnvironmentConnection>();
            var result = new Mock<IEsbRequestResult<string>>();
            result.Setup(a => a.HasErrors).Returns(true);
            result.Setup(a => a.Error).Returns(new TargetInvocationException(null));
            cf.Setup(a => a.CreateController("DeleteToolService")).Returns(comms.Object);
            // ReSharper disable MaximumChainedReferences
            comms.Setup(a => a.ExecuteCommand<IEsbRequestResult<string>>(cn.Object, Guid.Empty))
                 .Returns(result.Object);
            // ReSharper restore MaximumChainedReferences

            PluginProxy proxy = new PluginProxy(cf.Object, cn.Object);
            proxy.DeleteTool(new Mock<IToolDescriptor>().Object);
            cf.Verify(a => a.CreateController("DeleteToolService"));
            comms.Verify(a => a.ExecuteCommand<IEsbRequestResult<string>>(cn.Object, Guid.Empty));
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("PluginProxy_LoadToolService")]
        [ExpectedException(typeof(TargetInvocationException))]
        public void PluginProxy_LoadTool_ValidArgs_ExpectPassThroughAndExceptionThrown()
        {
            //------------Setup for test--------------------------
            var cf = new Mock<ICommunicationControllerFactory>();
            var comms = new Mock<ICommunicationController>();
            var cn = new Mock<IEnvironmentConnection>();
            var result = new Mock<IEsbRequestResult<string>>();
            var toolDescriptor = new Mock<IToolDescriptor>();
            result.Setup(a => a.HasErrors).Returns(true);
            result.Setup(a => a.Error).Returns(new TargetInvocationException(null));
            cf.Setup(a => a.CreateController("LoadToolService")).Returns(comms.Object);
            // ReSharper disable MaximumChainedReferences
            comms.Setup(a => a.ExecuteCommand<IEsbRequestResult<string>>(cn.Object, Guid.Empty))
                 .Returns(result.Object);
            // ReSharper restore MaximumChainedReferences

            PluginProxy proxy = new PluginProxy(cf.Object, cn.Object);
            proxy.LoadTool(toolDescriptor.Object, new byte[0]);
            cf.Verify(a => a.CreateController("LoadToolService"));
            comms.Verify(a => a.ExecuteCommand<IEsbRequestResult<string>>(cn.Object, Guid.Empty));

        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("PluginProxy_LoadToolService")]
        public void PluginProxy_LoadTool_ValidArgs_ExpectPassThroughAndNoExceptionThrown()
        {
            //------------Setup for test--------------------------
            var cf = new Mock<ICommunicationControllerFactory>();
            var comms = new Mock<ICommunicationController>();
            var cn = new Mock<IEnvironmentConnection>();
            var result = new Mock<IEsbRequestResult<string>>();
            var toolDescriptor = new Mock<IToolDescriptor>();
            result.Setup(a => a.HasErrors).Returns(false);
            result.Setup(a => a.Error).Returns(new TargetInvocationException(null));
            cf.Setup(a => a.CreateController("LoadToolService")).Returns(comms.Object);
            // ReSharper disable MaximumChainedReferences
            comms.Setup(a => a.ExecuteCommand<IEsbRequestResult<string>>(cn.Object, Guid.Empty))
                 .Returns(result.Object);
            // ReSharper restore MaximumChainedReferences

            PluginProxy proxy = new PluginProxy(cf.Object, cn.Object);
            proxy.LoadTool(toolDescriptor.Object, new byte[0]);
            cf.Verify(a => a.CreateController("LoadToolService"));
            comms.Verify(a => a.ExecuteCommand<IEsbRequestResult<string>>(cn.Object, Guid.Empty));

        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("PluginProxy_LoadToolService")]
        public void PluginProxy_GetTooldependencies_ValidArgs_ExpectPassThroughAndNoExceptionThrown()
        {
            //------------Setup for test--------------------------
            var cf = new Mock<ICommunicationControllerFactory>();
            var comms = new Mock<ICommunicationController>();
            var cn = new Mock<IEnvironmentConnection>();
            var result = new Mock<IEsbRequestResult<IList<IExplorerItemModel>>>();
            var toolDescriptor = new Mock<IToolDescriptor>();
            var res = new List<IExplorerItemModel>();
            result.Setup(a => a.HasErrors).Returns(false);
            result.Setup(a => a.Error).Returns(new TargetInvocationException(null));
            result.Setup(a => a.Value).Returns(res);
            cf.Setup(a => a.CreateController("GetToolDependenciesService")).Returns(comms.Object);
            // ReSharper disable MaximumChainedReferences
            comms.Setup(a => a.ExecuteCommand<IEsbRequestResult<IList<IExplorerItemModel>>>(cn.Object, Guid.Empty))
                 .Returns(result.Object);
            // ReSharper restore MaximumChainedReferences

            PluginProxy proxy = new PluginProxy(cf.Object, cn.Object);
            var output = proxy.GetToolDependencies(toolDescriptor.Object);
            cf.Verify(a => a.CreateController("GetToolDependenciesService"));
            comms.Verify(a => a.ExecuteCommand<IEsbRequestResult<IList<IExplorerItemModel>>>(cn.Object, Guid.Empty));
            Assert.AreEqual(res,output);

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("PluginProxy_LoadToolService")]
        [ExpectedException(typeof(TargetInvocationException))]
        public void PluginProxy_GetTooldependencies_ValidArgs_ExpectPassThroughAndExceptionThrown()
        {
            //------------Setup for test--------------------------
            var cf = new Mock<ICommunicationControllerFactory>();
            var comms = new Mock<ICommunicationController>();
            var cn = new Mock<IEnvironmentConnection>();
            var result = new Mock<IEsbRequestResult<IList<IExplorerItemModel>>>();
            var toolDescriptor = new Mock<IToolDescriptor>();
            var res = new List<IExplorerItemModel>();
            result.Setup(a => a.HasErrors).Returns(true);
            result.Setup(a => a.Error).Returns(new TargetInvocationException(null));
            result.Setup(a => a.Value).Returns(res);
            cf.Setup(a => a.CreateController("GetToolDependenciesService")).Returns(comms.Object);
            // ReSharper disable MaximumChainedReferences
            comms.Setup(a => a.ExecuteCommand<IEsbRequestResult<IList<IExplorerItemModel>>>(cn.Object, Guid.Empty))
                 .Returns(result.Object);
            // ReSharper restore MaximumChainedReferences

            PluginProxy proxy = new PluginProxy(cf.Object, cn.Object);
            var output = proxy.GetToolDependencies(toolDescriptor.Object);
            cf.Verify(a => a.CreateController("GetToolDependenciesService"));
            comms.Verify(a => a.ExecuteCommand<IEsbRequestResult<IList<IExplorerItemModel>>>(cn.Object, Guid.Empty));
            Assert.AreEqual(res, output);

        }
        // ReSharper restore InconsistentNaming
    }
}

using System;
using Dev2.Common;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Common.Interfaces.Studio.Core;
using Dev2.Common.Interfaces.Studio.Core.Controller;
using Dev2.Common.Interfaces.Studio.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.UnittestingUtils;

namespace Warewolf.Studio.AntiCorruptionLayer.Tests
{
    [TestClass]
    public class StudioServerProxyTests
    {
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("StudioServerProxy_Ctor")]
        public void StudioServerProxy_Ctor_Null_VerifyExceptions()
        {
            //------------Setup for test--------------------------
                      NullArgumentConstructorHelper.AssertNullConstructor(new object[]{new Mock<ICommunicationControllerFactory>().Object,new Mock<IEnvironmentConnection>().Object},typeof(StudioServerProxy) );
            //------------Execute Test---------------------------
             
            //------------Assert Results-------------------------
        }
        
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("StudioServerProxy_Ctor")]
        public void StudioServerProxy_Ctor_ValidArgss()
        {
            //------------Setup for test--------------------------
            var studioServerProxy = new StudioServerProxy(new Mock<ICommunicationControllerFactory>().Object, new Mock<IEnvironmentConnection>().Object);
            //------------Execute Test---------------------------
           Assert.IsNotNull(studioServerProxy.QueryManagerProxy);
           Assert.IsNotNull(studioServerProxy.UpdateManagerProxy);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("StudioServerProxy_Rename")]
        public void StudioServerProxyr_Rename_PassThrough()
        {
            //------------Setup for test--------------------------
            var fact = new Mock<ICommunicationControllerFactory>();
            var comms = new Mock<ICommunicationController>();
            var id = Guid.NewGuid();
            var conn = new Mock<IEnvironmentConnection>();
            fact.Setup(a=>a.CreateController("RenameItemService")).Returns(comms.Object);
            var objtoRename = new Mock<IExplorerItemViewModel>();
            objtoRename.Setup(a => a.ResourceId).Returns(id);
            var studioServerProxy = new StudioServerProxy(fact.Object,conn.Object );
            //------------Execute Test---------------------------
            studioServerProxy.Rename(objtoRename.Object, "bob");
            //------------Assert Results-------------------------
            comms.Verify(a => a.AddPayloadArgument("itemToRename",id.ToString()));
            comms.Verify(a=>a.AddPayloadArgument("newName","bob"));
            comms.Verify(a => a.ExecuteCommand<IExplorerRepositoryResult>(conn.Object, GlobalConstants.ServerWorkspaceID));
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("StudioServerProxy_Move")]
        public void StudioServerProxy_Move_PassThrough()
        {
            //------------Setup for test--------------------------
            var fact = new Mock<ICommunicationControllerFactory>();
            var comms = new Mock<ICommunicationController>();
            var id = Guid.NewGuid();
            var id2 = Guid.NewGuid();
            var conn = new Mock<IEnvironmentConnection>();
            fact.Setup(a => a.CreateController("MoveItemService")).Returns(comms.Object);
            var objtoMove = new Mock<IExplorerItemViewModel>();
            var objtoMoveInto = new Mock<IExplorerItemViewModel>();
            objtoMove.Setup(a => a.ResourceId).Returns(id);
            objtoMoveInto.Setup(a => a.ResourceId).Returns(id2);
            var studioServerProxy = new StudioServerProxy(fact.Object, conn.Object);
            //------------Execute Test---------------------------
            studioServerProxy.Move(objtoMove.Object, objtoMoveInto.Object);
            //------------Assert Results-------------------------
            comms.Verify(a => a.AddPayloadArgument("itemToMove", id.ToString()));
            comms.Verify(a => a.AddPayloadArgument("newPath", id2.ToString()));
            comms.Verify(a => a.ExecuteCommand<IExplorerRepositoryResult>(conn.Object, GlobalConstants.ServerWorkspaceID));
        }
    }
}

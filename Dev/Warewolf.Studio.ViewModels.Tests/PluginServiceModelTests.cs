using System;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.UnittestingUtils;

namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass]
    public class PluginServiceModelTests
    {
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("PluginServiceModel_Ctor")]
        public void PluginServiceModel_Ctor_AssertNulls_CheckNoNullsCanBePassedIn()
        {
            //------------Setup for test--------------------------
           // var pluginServiceModel = new PluginServiceModel(new Mock<IStudioUpdateManager>().Object,new Mock<IQueryManager>().Object, new Mock<IShellViewModel>().Object,"");
            NullArgumentConstructorHelper.AssertNullConstructor(new object[] { new Mock<IStudioUpdateManager>().Object, new Mock<IQueryManager>().Object, new Mock<IShellViewModel>().Object, "" },typeof(PluginServiceModel));
            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("PluginServiceModel_RetrieveSources")]
        public void PluginServiceModel_RetrieveSources_PassThrough()
        {
            //------------Setup for test--------------------------
            var qm = new Mock<IQueryManager>();
             var pluginServiceModel = new PluginServiceModel(new Mock<IStudioUpdateManager>().Object,qm.Object, new Mock<IShellViewModel>().Object,"");
          //------------Execute Test---------------------------

             pluginServiceModel.RetrieveSources();
            //------------Assert Results-------------------------
            qm.Verify(a=>a.FetchPluginSources(),Times.Once());
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("PluginServiceModel_RetrieveSources")]
        public void PluginServiceModel_GetActions_PassThrough()
        {
            //------------Setup for test--------------------------
            var qm = new Mock<IQueryManager>();
            var src = new Mock<IPluginSource>();
            var pluginServiceModel = new PluginServiceModel(new Mock<IStudioUpdateManager>().Object, qm.Object, new Mock<IShellViewModel>().Object, "");
            //------------Execute Test---------------------------

            pluginServiceModel.GetActions(src.Object,new Mock<INamespaceItem>().Object);
            //------------Assert Results-------------------------
            qm.Verify(a => a.PluginActions(src.Object,new Mock<INamespaceItem>().Object), Times.Once());
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("PluginServiceModel_CreateSource")]
        public void PluginServiceModel_CreateSource_PassesThroughToShell()
        {
            //------------Setup for test--------------------------
            var qm = new Mock<IQueryManager>();
            var src = new Mock<IPluginSource>();
            var shell = new Mock<IShellViewModel>();
            var pluginServiceModel = new PluginServiceModel(new Mock<IStudioUpdateManager>().Object, qm.Object, shell.Object, "");
            //------------Execute Test---------------------------

            pluginServiceModel.CreateNewSource();
            //------------Assert Results-------------------------
            shell.Verify(a => a.NewResource( ResourceType.PluginSource, It.IsAny<Guid>()), Times.Once());
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("PluginServiceModel_EditSource")]
        public void PluginServiceModel_EditSource_PassesThroughToShell()
        {
            //------------Setup for test--------------------------
            var qm = new Mock<IQueryManager>();
            var src = new Mock<IPluginSource>();
            var shell = new Mock<IShellViewModel>();
            var pluginServiceModel = new PluginServiceModel(new Mock<IStudioUpdateManager>().Object, qm.Object, shell.Object, "");
            //------------Execute Test---------------------------

            pluginServiceModel.EditSource(src.Object);
            //------------Assert Results-------------------------
            shell.Verify(a => a.EditResource(src.Object), Times.Once());
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("PluginServiceModel_Test")]
        public void PluginServiceModel_TestPassesThroughl()
        {
            //------------Setup for test--------------------------
            var qm = new Mock<IQueryManager>();
            var src = new Mock<IPluginSource>();
            var shell = new Mock<IShellViewModel>();
            var um = new Mock<IStudioUpdateManager>();
            var pluginServiceModel = new PluginServiceModel(um.Object, qm.Object, shell.Object, "");
            um.Setup(a=>a.TestPluginService(It.IsAny<IPluginService>())).Returns("bob");
            //------------Execute Test---------------------------

           var x =  pluginServiceModel.TestService(It.IsAny<IPluginService>());
            //------------Assert Results-------------------------
          Assert.AreEqual("bob",x); 
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("PluginServiceModel_Save")]
        public void PluginServiceModel_Save_PassesThroughToUpdateProxy()
        {
            //------------Setup for test--------------------------
            var qm = new Mock<IQueryManager>();
            var src = new Mock<IPluginSource>();
            var shell = new Mock<IShellViewModel>();
            var um = new Mock<IStudioUpdateManager>();
            var pluginServiceModel = new PluginServiceModel(um.Object, qm.Object, shell.Object, "");            //------------Execute Test---------------------------

            pluginServiceModel.SaveService(It.IsAny<IPluginService>());
            //------------Assert Results-------------------------
            um.Verify(a => a.Save(It.IsAny<IPluginService>()), Times.Once());
        }



    }
}

using Dev2.Common.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass]
    public class EnvironmentViewModelTests
    {
        [TestMethod]
        public void TestDispose()
        {

            var svr = new Mock<IServer>();
            EnvironmentViewModel vm = new EnvironmentViewModel(svr.Object, new Mock<IShellViewModel>().Object);
            var child = new Mock<IExplorerItemViewModel>();
            vm.AddChild(child.Object);
            vm.Dispose();
            child.Verify(a => a.Dispose());
        }

        [TestMethod]
        public void EnvironmentViewModelVerifyNewFolderShowsContextMenu_HasParentPermissions()
        {
            var svr = new Mock<IServer>();
            EnvironmentViewModel vm = new EnvironmentViewModel(svr.Object, new Mock<IShellViewModel>().Object);
            var mckExp = new Mock<IExplorerRepository>();
            svr.Setup(a => a.ExplorerRepository).Returns(mckExp.Object);
            vm.CanCreateDbSource = true;
            vm.CanCreateDropboxSource = true;
            vm.CanCreatePluginSource = true;
            vm.ShowContextMenu = true;
            vm.CreateFolder();
            Assert.AreEqual(vm.Children.Count,1);
            Assert.IsTrue(vm.Children[0].CanCreateDbSource);
            Assert.IsTrue(vm.Children[0].CanCreateDropboxSource);
            Assert.IsTrue(vm.Children[0].CanCreatePluginSource);
            Assert.IsTrue(vm.Children[0].ShowContextMenu);
            Assert.IsFalse(vm.Children[0].CanCreateEmailSource);
            Assert.IsFalse(vm.Children[0].CanCreateSharePointSource);
            Assert.IsFalse(vm.Children[0].CanDelete);
            Assert.IsFalse(vm.Children[0].CanCreateFolder);
            Assert.IsFalse(vm.Children[0].CanShowVersions);
            Assert.IsFalse(vm.Children[0].CanCreateWorkflowService);
        }

    }
}
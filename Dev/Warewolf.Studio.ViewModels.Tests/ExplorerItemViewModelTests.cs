using Dev2.Common.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass]
    public class ExplorerItemViewModelTests
    {
        [TestMethod]
        public void TestDispose()
        {

            var svr = new Mock<IServer>();
            ExplorerItemViewModel vm = new ExplorerItemViewModel(svr.Object, new Mock<IExplorerTreeItem>().Object,
                a => { }, new Mock<IShellViewModel>().Object, new Mock<Dev2.Common.Interfaces.Studio.Controller.IPopupController>().Object);
            var child = new Mock<IExplorerItemViewModel>();
            vm.Children.Add(child.Object);
            vm.Dispose();
            child.Verify(a=>a.Dispose());
        }

    }

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

    }
}

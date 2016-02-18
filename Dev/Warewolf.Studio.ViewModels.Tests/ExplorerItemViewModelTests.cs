using System;
using System.Windows;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.PopupController;
using Dev2.Studio.Core.Interfaces;
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


        [TestMethod]
        public void TestDeleteClosesWindow()
        {

            var svr = new Mock<IServer>();
            var shell = new Mock<IShellViewModel>();
            var pop = new Mock<Dev2.Common.Interfaces.Studio.Controller.IPopupController>();
            pop.Setup(a => a.Show(It.IsAny<IPopupMessage>())).Returns(MessageBoxResult.Yes);
            ExplorerItemViewModel vm = new ExplorerItemViewModel(svr.Object, new Mock<IExplorerTreeItem>().Object,
                a => { }, shell.Object, pop.Object);
            vm.EnvironmentModel = new Mock<IEnvironmentModel>().Object;
            var child = new Mock<IExplorerItemViewModel>();
            vm.Children.Add(child.Object);
            try
            {
                vm.Delete();
            }
            catch (Exception)
            {
                // ignored
            }

            shell.Verify(a=>a.CloseResource(It.IsAny<Guid>(),It.IsAny<Guid>()));
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

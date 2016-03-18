using Dev2.Interfaces;
using Microsoft.Practices.Prism.Commands;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass]
    public class MenuViewModelTests
    {
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("MenuViewModel_ShowStartPage")]
        public void MenuViewModel_ShowStartPage_Execute_Result()
        {
            //------------Setup for test--------------------------
            var mockMainViewModel = new Mock<IMainViewModel>();
            bool call = false;
            var x = new DelegateCommand(() => { call = true; });
            mockMainViewModel.Setup(a => a.ShowStartPageCommand).Returns(x);
            var menuViewModel = new MenuViewModel(mockMainViewModel.Object);
            
            //------------Execute Test---------------------------
            menuViewModel.StartPageCommand.Execute(null);

            //------------Assert Results-------------------------
            Assert.IsTrue(call);
        }
    }
}
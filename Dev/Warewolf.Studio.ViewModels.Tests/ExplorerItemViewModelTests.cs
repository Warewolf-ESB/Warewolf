using System;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass]
    public class ExplorerItemViewModelTests
    {
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_NullShellViewModel_ExpectException()
        {
            //------------Setup for test--------------------------
            
            //------------Execute Test---------------------------
            new ExplorerItemViewModel(null);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("Constructor")]
        public void Constructor_SetsUpOpenCommand()
        {
            //------------Setup for test--------------------------
            var shellViewModelMock = new Mock<IShellViewModel>();
            shellViewModelMock.Setup(model => model.AddService(It.IsAny<IResource>())).Verifiable();
            //------------Execute Test---------------------------
            var explorerViewModel = new ExplorerItemViewModel(shellViewModelMock.Object);
            //------------Assert Results-------------------------
            explorerViewModel.OpenCommand.Execute(null);
            shellViewModelMock.Verify(model => model.AddService(It.IsAny<IResource>()),Times.Once());

        }
    }
}

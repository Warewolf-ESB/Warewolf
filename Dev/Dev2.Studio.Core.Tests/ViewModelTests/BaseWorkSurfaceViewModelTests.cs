using System;
using Caliburn.Micro;
using Dev2.Studio.ViewModels.WorkSurface;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Core.Tests.ViewModelTests
{
    [TestClass]
    public class BaseWorkSurfaceViewModelTests
    {
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("BaseWorkSurfaceViewModel_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void BaseWorkSurfaceViewModel_Constructor_NullEventPublisher_ThrowsArgumentNullException()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var viewModel = new BaseWorkSurfaceViewModel(null);

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("BaseWorkSurfaceViewModel_CanSave")]
        public void BaseWorkSurfaceViewModel_CanSave_ReturnsTrue()
        {
            //------------Setup for test--------------------------
            var viewModel = new BaseWorkSurfaceViewModel(new Mock<IEventAggregator>().Object);

            //------------Execute Test---------------------------
            var result = viewModel.CanSave;

            //------------Assert Results-------------------------
            Assert.IsTrue(result);
        }
    }
}

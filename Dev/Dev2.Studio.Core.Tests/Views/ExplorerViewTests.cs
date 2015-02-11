using System.Collections.ObjectModel;
using System.Windows;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Studio.Views.Explorer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Core.Tests.Views
{
    [TestClass]
    public class ExplorerViewTests
    {
        private Mock<IPopupController> _controller;

        [TestInitialize]
        public void Init()
        {
             _controller = new Mock<IPopupController>();
// ReSharper disable once RedundantCast
            CustomContainer.Register((_controller.Object as IPopupController));
        }


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ExplorerView_ShouldNotMoveItem")]
        public void ExplorerView_ShouldNotMoveItem_SourceDestinationSameResourcePath_ReturnsTrue()
        {
            //------------Setup for test--------------------------
            var sourceItem = new Mock<IExplorerItemModel>();
            sourceItem.Setup(model => model.IsVersion).Returns(false);
            sourceItem.Setup(model => model.ResourcePath).Returns("mypath");
            var destination = new Mock<IExplorerItemModel>();
            destination.Setup(model => model.IsVersion).Returns(false);
            destination.Setup(model => model.ResourcePath).Returns("mypath");
            destination.Setup(a => a.Parent).Returns(destination.Object);
            destination.Setup(a => a.Children).Returns(new ObservableCollection<IExplorerItemModel>());

            //------------Execute Test---------------------------
            
            var shouldNotMove = ExplorerView.ShouldNotMove(sourceItem.Object, destination.Object);
            //------------Assert Results-------------------------
            Assert.IsTrue(shouldNotMove);
        } 
        
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ExplorerView_ShouldNotMoveItem")]
        public void ExplorerView_ShouldNotMoveItem_SourceDestinationNotSameResourcePath_ReturnsFalse()
        {
            //------------Setup for test--------------------------
            var sourceItem = new Mock<IExplorerItemModel>();
            sourceItem.Setup(model => model.IsVersion).Returns(false);
            sourceItem.Setup(model => model.ResourcePath).Returns("mypath1");
            var destination = new Mock<IExplorerItemModel>();
            destination.Setup(model => model.IsVersion).Returns(false);
            destination.Setup(model => model.ResourcePath).Returns("mypath");
            destination.Setup(a => a.Children).Returns(new ObservableCollection<IExplorerItemModel>());
            destination.Setup(a => a.Parent).Returns(destination.Object);
            //------------Execute Test---------------------------
            
            var shouldNotMove = ExplorerView.ShouldNotMove(sourceItem.Object, destination.Object);
            //------------Assert Results-------------------------
            Assert.IsFalse(shouldNotMove);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ExplorerView_ShouldNotMoveItem")]
        public void ExplorerView_ShouldNotMoveItem_SourceDestinationSame_ReturnsTrue()
        {
            //------------Setup for test--------------------------
            var sourceItem = new Mock<IExplorerItemModel>();
            sourceItem.Setup(model => model.IsVersion).Returns(false);
            sourceItem.Setup(model => model.ResourcePath).Returns("mypath1");
            sourceItem.Setup(a => a.Children).Returns(new ObservableCollection<IExplorerItemModel>());
            sourceItem.Setup(a => a.Parent).Returns(sourceItem.Object);
            //------------Execute Test---------------------------

            var shouldNotMove = ExplorerView.ShouldNotMove(sourceItem.Object, sourceItem.Object);
            //------------Assert Results-------------------------
            Assert.IsTrue(shouldNotMove);
        }


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ExplorerView_ShouldNotMoveItem")]
        public void ExplorerView_ShouldNotMoveItem_SourceIsVersion_ReturnsTrue()
        {
            //------------Setup for test--------------------------
            var sourceItem = new Mock<IExplorerItemModel>();
            sourceItem.Setup(model => model.IsVersion).Returns(true);
            sourceItem.Setup(model => model.ResourcePath).Returns("mypath1");
            var destination = new Mock<IExplorerItemModel>();
            destination.Setup(model => model.IsVersion).Returns(false);
            destination.Setup(model => model.ResourcePath).Returns("mypath");
            destination.Setup(a => a.Children).Returns(new ObservableCollection<IExplorerItemModel>());
            destination.Setup(a => a.Parent).Returns(destination.Object);
            //------------Execute Test---------------------------

            var shouldNotMove = ExplorerView.ShouldNotMove(sourceItem.Object, destination.Object);
            //------------Assert Results-------------------------
            Assert.IsTrue(shouldNotMove);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ExplorerView_ShouldNotMoveItem")]
        public void ExplorerView_ShouldNotMoveItem_DestinationIsVersion_ReturnsTrue()
        {
            //------------Setup for test--------------------------
            var sourceItem = new Mock<IExplorerItemModel>();
            sourceItem.Setup(model => model.IsVersion).Returns(false);
            sourceItem.Setup(model => model.ResourcePath).Returns("mypath1");
            var destination = new Mock<IExplorerItemModel>();
            destination.Setup(model => model.IsVersion).Returns(true);
            destination.Setup(model => model.ResourcePath).Returns("mypath");
            destination.Setup(a => a.Children).Returns(new ObservableCollection<IExplorerItemModel>( ));
            destination.Setup(a => a.Parent).Returns(destination.Object);
            //------------Execute Test---------------------------

            var shouldNotMove = ExplorerView.ShouldNotMove(sourceItem.Object, destination.Object);
            //------------Assert Results-------------------------
            Assert.IsTrue(shouldNotMove);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ExplorerView_ShouldNotMoveItem")]
        public void ExplorerView_ShouldNotMoveItem_SourceDestinationHasDupicateChild_ReturnsTrue_NoMessagebox()
        {
            //------------Setup for test--------------------------
            var sourceItem = new Mock<IExplorerItemModel>();
            sourceItem.Setup(model => model.IsVersion).Returns(false);
            sourceItem.Setup(model => model.ResourcePath).Returns("mypath1");
            sourceItem.Setup(model => model.DisplayName).Returns("bob");
            var destination = new Mock<IExplorerItemModel>();
            destination.Setup(model => model.IsVersion).Returns(false);
            destination.Setup(model => model.ResourcePath).Returns("mypath");

            var child = new Mock<IExplorerItemModel>();
            child.Setup(model => model.IsVersion).Returns(false);
            child.Setup(model => model.ResourcePath).Returns("mypath1");
            child.Setup(model => model.DisplayName).Returns("bob");
            destination.Setup(a => a.Children).Returns(new ObservableCollection<IExplorerItemModel>(new[]{child.Object}));
            destination.Setup(a => a.Parent).Returns(destination.Object);
            //------------Execute Test---------------------------

            var shouldNotMove = ExplorerView.ShouldNotMove(sourceItem.Object, destination.Object);
            //------------Assert Results-------------------------
            Assert.IsFalse(shouldNotMove);
            _controller.Verify(a => a.Show("Conflicting resources found in the destination", "Conflicting Resources", MessageBoxButton.OK, MessageBoxImage.Error, ""),Times.Never());
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ExplorerView_ShouldNotMoveItem")]
        public void ExplorerView_ShouldNotMoveItem_SourceDestinationHasDupicateChild_ReturnsTrue_MessageBox()
        {
            //------------Setup for test--------------------------
            var sourceItem = new Mock<IExplorerItemModel>();
            sourceItem.Setup(model => model.IsVersion).Returns(false);
            sourceItem.Setup(model => model.ResourcePath).Returns("mypath1");
            sourceItem.Setup(model => model.DisplayName).Returns("bob");
            sourceItem.Setup(a => a.ResourcePathWithoutName).Returns("abc");
            var destination = new Mock<IExplorerItemModel>();
            destination.Setup(model => model.IsVersion).Returns(false);
            destination.Setup(model => model.ResourcePath).Returns("mypath");

            var child = new Mock<IExplorerItemModel>();
            child.Setup(model => model.IsVersion).Returns(false);
            child.Setup(model => model.ResourcePath).Returns("mypath2");
            child.Setup(model => model.DisplayName).Returns("bob");
            child.Setup(a => a.ResourcePathWithoutName).Returns("def");
            destination.Setup(a => a.Children).Returns(new ObservableCollection<IExplorerItemModel>(new[] { child.Object }));
            destination.Setup(a => a.Parent).Returns(destination.Object);
            destination.Setup(a => a.ResourcePathWithoutName).Returns("ghi");
            //------------Execute Test---------------------------

            var shouldNotMove = ExplorerView.ShouldNotMove(sourceItem.Object, destination.Object);
            //------------Assert Results-------------------------
            Assert.IsTrue(shouldNotMove);
            _controller.Verify(a => a.Show("Conflicting resources found in the destination", "Conflicting Resources", MessageBoxButton.OK, MessageBoxImage.Error, ""), Times.Once());
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ExplorerView_ShouldNotMoveItem")]
        public void ExplorerView_ShouldMoveItem_SourceDestinationHasDupicateChildIsFolder_ReturnsTrue()
        {
            //------------Setup for test--------------------------
            var sourceItem = new Mock<IExplorerItemModel>();
            sourceItem.Setup(model => model.IsVersion).Returns(false);
            sourceItem.Setup(model => model.ResourcePath).Returns("mypath1");
            sourceItem.Setup(model => model.DisplayName).Returns("bob");
            var destination = new Mock<IExplorerItemModel>();
            destination.Setup(model => model.IsVersion).Returns(false);
            destination.Setup(model => model.ResourcePath).Returns("mypath");

            var child = new Mock<IExplorerItemModel>();
            child.Setup(model => model.IsVersion).Returns(false);
            child.Setup(model => model.ResourcePath).Returns("mypath1");
            child.Setup(model => model.DisplayName).Returns("bob");
            child.Setup(model => model.ResourceType).Returns(ResourceType.Folder);
            destination.Setup(a => a.Parent).Returns(destination.Object);
            destination.Setup(a => a.Children).Returns(new ObservableCollection<IExplorerItemModel>(new[] { child.Object }));
            //------------Execute Test---------------------------

            var shouldNotMove = ExplorerView.ShouldNotMove(sourceItem.Object, destination.Object);
            //------------Assert Results-------------------------
            Assert.IsFalse(shouldNotMove);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ExplorerView_ShouldNotMoveItem")]
        public void ExplorerView_ShouldNotMoveItem_SourceDestinationNoDuplicateDestinationIsIsFolder_ReturnsTrue()
        {
            //------------Setup for test--------------------------
            var sourceItem = new Mock<IExplorerItemModel>();
            sourceItem.Setup(model => model.IsVersion).Returns(false);
            sourceItem.Setup(model => model.ResourcePath).Returns("mypath1");
            sourceItem.Setup(model => model.DisplayName).Returns("bob");
            var destination = new Mock<IExplorerItemModel>();
            destination.Setup(model => model.IsVersion).Returns(false);
            destination.Setup(model => model.ResourcePath).Returns("mypath");
            destination.Setup(a => a.ResourceType).Returns(ResourceType.Folder);
            var child = new Mock<IExplorerItemModel>();
            child.Setup(model => model.IsVersion).Returns(false);
            child.Setup(model => model.ResourcePath).Returns("mypath1");
            child.Setup(model => model.DisplayName).Returns("bob");
            child.Setup(model => model.ResourceType).Returns(ResourceType.Folder);
            destination.Setup(a => a.Parent).Returns(destination.Object);
            destination.Setup(a => a.Children).Returns(new ObservableCollection<IExplorerItemModel>(new[] { child.Object }));
            //------------Execute Test---------------------------

            var shouldNotMove = ExplorerView.ShouldNotMove(sourceItem.Object, destination.Object);
            //------------Assert Results-------------------------
            Assert.IsFalse(shouldNotMove);
        }

    }
}

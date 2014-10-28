using Dev2.Models;
using Dev2.Studio.Views.Explorer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Core.Tests.Views
{
    [TestClass]
    public class ExplorerViewTests
    {
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
            //------------Execute Test---------------------------

            var shouldNotMove = ExplorerView.ShouldNotMove(sourceItem.Object, destination.Object);
            //------------Assert Results-------------------------
            Assert.IsTrue(shouldNotMove);
        }
    }
}

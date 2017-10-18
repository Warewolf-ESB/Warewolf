using Dev2.Studio.Interfaces;
using Dev2.ViewModels.Merge;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Core.Tests
{
    [TestClass]
    public class ConflictModelFactoryTests
    {
        [TestMethod]
        public void Construct_GivenPassThrough()
        {
            //---------------Set up test pack-------------------
            var conflictNode = new Mock<IConflictNode>();
            var contextualResourceModel = new Mock<IContextualResourceModel>();
            var factory = new ConflictModelFactory(conflictNode.Object, contextualResourceModel.Object);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.IsNotNull(factory.Children);
            Assert.IsNull(factory.Model);
        }
    }
}

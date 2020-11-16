using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Dev2.Common.Interfaces.Toolbox;
using Moq;

namespace Warewolf.Studio.ViewModels.ToolBox.Tests
{
    [TestClass]
    public class ToolBoxCategoryViewModelTests
    {
        [TestMethod]
        [Timeout(1000)]
        public void TestToolBoxCategoryViewModel()
        {
            //arrange
            var toolDescriptorViewModelsMock = new Mock<ICollection<IToolDescriptorViewModel>>();
            var testName = "someName";
            var target = new ToolBoxCategoryViewModel(testName, toolDescriptorViewModelsMock.Object);

            //act
            var actualName = target.Name;
            var actualTools = target.Tools;

            //assert
            Assert.AreEqual(testName, actualName);
            Assert.AreSame(toolDescriptorViewModelsMock.Object, actualTools);
        }
    }
}
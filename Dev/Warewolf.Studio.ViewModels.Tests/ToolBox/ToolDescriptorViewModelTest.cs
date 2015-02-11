using Dev2.Common.Interfaces.Toolbox;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Studio.ViewModels.ToolBox;
using Warewolf.UnittestingUtils;

namespace Warewolf.Studio.ViewModels.Tests.ToolBox
{
    [TestClass]
    public class ToolDescriptorViewModelTest
    {
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ToolDescriptorViewModel_Ctor")]
        // ReSharper disable InconsistentNaming
        public void ToolDescriptorViewModel_Ctor_NullsPassedIn_ExpectErrors()

        {
          
            
            NullArgumentConstructorHelper.AssertNullConstructor(new object[]{ new Mock<IToolDescriptor>().Object,false},typeof(ToolDescriptorViewModel));

        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ToolDescriptorViewModel_Ctor")]
        public void ToolDescriptorViewModel_Ctor_ValidValues_ExpectValidPropertiesSet()
        {

            var tool = new Mock<IToolDescriptor>();
            ToolDescriptorViewModel a = new ToolDescriptorViewModel(tool.Object,true);

            Assert.IsTrue(a.IsEnabled);
            Assert.AreEqual(tool.Object,a.Tool);

        }
        // ReSharper restore InconsistentNaming
    }
}

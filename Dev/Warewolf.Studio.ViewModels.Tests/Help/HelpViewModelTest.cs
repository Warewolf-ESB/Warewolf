using Dev2.Common.Interfaces.Help;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Studio.ViewModels.Help;
using Warewolf.UnittestingUtils;

namespace Warewolf.Studio.ViewModels.Tests.Help
{
    [TestClass]
    public class HelpViewModelTest
    {
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("HelpViewModel_Ctor")]
        // ReSharper disable InconsistentNaming
        public void HelpViewModel_Ctor_NullValues_ExpectExceptions()

        {
            //------------Setup for test--------------------------
            NullArgumentConstructorHelper.AssertNullConstructor(new object[]{  new Mock<IHelpDescriptorViewModel>().Object,new Mock<IHelpWindowModel>().Object}, typeof(HelpWindowViewModel));
            
            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("HelpViewModel_Ctor")]
        public void HelpViewModel_Ctor_CorrectValues_AssertDefaultIsSet()
        {
            //------------Setup for test--------------------------
            var defaultVal = new Mock<IHelpDescriptorViewModel>();
            var model = new Mock<IHelpWindowModel>();
            

            //------------Execute Test---------------------------
            var help = new HelpWindowViewModel(defaultVal.Object, model.Object);
            //------------Assert Results-------------------------
            Assert.AreEqual(help.CurrentHelpText,defaultVal.Object);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("HelpViewModel_Ctor")]
        public void HelpViewModel_ModelFiresEvent_AssertDefaultIsReplaced()
        {
            //------------Setup for test--------------------------
            var defaultVal = new Mock<IHelpDescriptorViewModel>();
            var changedVal = new Mock<IHelpDescriptor>();
            var model = new Mock<IHelpWindowModel>();
            changedVal.Setup(a => a.Name).Returns("bob");

            //------------Execute Test---------------------------
            var help = new HelpWindowViewModel(defaultVal.Object, model.Object);
            model.Raise(a => a.OnHelpTextReceived += null, new object[] { this, changedVal.Object });
            //------------Assert Results-------------------------
            Assert.AreEqual("bob", help.CurrentHelpText.Name);
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("HelpViewModel_Ctor")]
        public void HelpViewModel_Dispose_Unsubscribe()
        {
            //------------Setup for test--------------------------
            var defaultVal = new Mock<IHelpDescriptorViewModel>();
            var changedVal = new Mock<IHelpDescriptor>();
            var model = new Mock<IHelpWindowModel>();
            changedVal.Setup(a => a.Name).Returns("bob");
            var changedVal2 = new Mock<IHelpDescriptor>();
            changedVal2.Setup(a => a.Name).Returns("dora");
            //------------Execute Test---------------------------
            var help = new HelpWindowViewModel(defaultVal.Object, model.Object);
            model.Raise(a => a.OnHelpTextReceived += null, new object[] { this, changedVal.Object });
            help.Dispose();
            model.Raise(a => a.OnHelpTextReceived += null, new object[] { this, changedVal2.Object });
            //------------Assert Results-------------------------
            Assert.AreEqual("bob", help.CurrentHelpText.Name);
        }
        // ReSharper restore InconsistentNaming
    }
}

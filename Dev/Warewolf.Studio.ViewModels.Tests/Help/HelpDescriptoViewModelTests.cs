using System;
using System.Windows.Media;
using Dev2.Common.Interfaces.Help;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Studio.Models.Help;
using Warewolf.Studio.ViewModels.Help;

namespace Warewolf.Studio.ViewModels.Tests.Help
{
    [TestClass]
    public class HelpDescriptoViewModelTests
    {
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("HelpDescriptorViewModel_Ctor")]
        [ExpectedException(typeof(ArgumentNullException))]
        // ReSharper disable InconsistentNaming
        public void HelpDescriptorViewModel_Ctor_NullParams_ExpectErrors()
        
        {
            //------------Setup for test--------------------------
            var helpDescriptorViewModel = new HelpDescriptorViewModel(null);
            
            //------------Execute Test---------------------------
            
            //------------Assert Results-------------------------
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("HelpDescriptorViewModel_Ctor")]
        [ExpectedException(typeof(ArgumentNullException))]
        // ReSharper disable InconsistentNaming
        public void HelpDescriptorViewModel_Ctor_ValidParams_VerifyProperties()
        {
            //------------Setup for test--------------------------
            var image = new DrawingImage();
            var helpDescriptorViewModel = new HelpDescriptorViewModel(new HelpDescriptor("name","desc",image));

            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            Assert.AreEqual("desc",helpDescriptorViewModel.Description);
            Assert.AreEqual("name",helpDescriptorViewModel.Name);
            Assert.AreEqual(image,helpDescriptorViewModel.Icon);
            Assert.IsTrue(helpDescriptorViewModel.IsEnabled);
        }

        // ReSharper restore InconsistentNaming
    }
}
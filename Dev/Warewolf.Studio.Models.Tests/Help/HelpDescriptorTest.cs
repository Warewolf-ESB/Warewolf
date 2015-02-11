using System.Windows.Media;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Core;
using Warewolf.Studio.Models.Help;
using Warewolf.UnittestingUtils;

namespace Warewolf.Studio.Models.Tests.Help
{
    [TestClass]
    public class HelpDescriptorTest
    {
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("HelpDescriptor_Ctor")]
        // ReSharper disable InconsistentNaming
        public void HelpDescriptor_Ctor_NullParams_ExpectExceptionsThrown()
     
        {
            //------------Setup for test--------------------------
                    NullArgumentConstructorHelper.AssertNullConstructor(new object [] { "bob", "dave", new DrawingImage() },typeof(HelpDescriptor));
            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("HelpDescriptor_Ctor")]
        public void HelpDescriptor_Ctor_ValidParams_VerifyProperties()
        {
            //------------Setup for test--------------------------
            var img = new DrawingImage();
            var helpDescriptor = new HelpDescriptor("bob", "dave", img);
            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            Assert.AreEqual("dave",helpDescriptor.Description);
            Assert.AreEqual("bob", helpDescriptor.Name);
            Assert.AreEqual(img, helpDescriptor.Icon);
        }
        // ReSharper restore InconsistentNaming
    }
}
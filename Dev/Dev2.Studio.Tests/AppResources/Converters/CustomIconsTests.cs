using System;
using Dev2.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[assembly: Parallelize(Workers = 0, Scope = ExecutionScope.MethodLevel)]
namespace Dev2.Studio.Tests.AppResources.Converters
{
    [TestClass]
    public class CustomIconsTests
    {
        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(CustomIcons))]
        public void CustomIcons_Error_Is_ExpectValue()
        {
            //-------------------Arrange---------------------
            var customIconsError = "pack://application:,,,/Warewolf Studio;component/Images/PopupError-32.png";
            //-------------------Act-------------------------
            //-------------------Assert----------------------
            Assert.IsNotNull(CustomIcons.Error);
            Assert.AreEqual(customIconsError, CustomIcons.Error);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(CustomIcons))]
        public void CustomIcons_Information_Is_ExpectValue()
        {
            //-------------------Arrange---------------------
            var customIconsInformation = "pack://application:,,,/Warewolf Studio;component/Images/PopupInformation-32.png";
            //-------------------Act-------------------------
            //-------------------Assert----------------------
            Assert.IsNotNull(CustomIcons.Information);
            Assert.AreEqual(customIconsInformation, CustomIcons.Information);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(CustomIcons))]
        public void CustomIcons_Question_Is_ExpectValue()
        {
            //-------------------Arrange---------------------
            var customIconsQuestion = "pack://application:,,,/Warewolf Studio;component/Images/GenericHelp-32.png";
            //-------------------Act-------------------------
            //-------------------Assert----------------------
            Assert.IsNotNull(CustomIcons.Question);
            Assert.AreEqual(customIconsQuestion, CustomIcons.Question);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(CustomIcons))]
        public void CustomIcons_Warning_Is_ExpectValue()
        {
            //-------------------Arrange---------------------
            var customIconsWarning = "pack://application:,,,/Warewolf Studio;component/Images/PopupNotSavedWarning-32.png";
            //-------------------Act-------------------------
            //-------------------Assert----------------------
            Assert.IsNotNull(CustomIcons.Warning);
            Assert.AreEqual(customIconsWarning, CustomIcons.Warning);
        }
    }
}

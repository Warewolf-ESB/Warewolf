using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Core.Tests.Settings
{
    [TestClass]
    public class SettingsItemViewModelTests
    {
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SettingsItemViewModel_Constructor")]
        public void SettingsItemViewModel_Constructor_Properties_Initialized()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var settingsItemViewModel = new TestSettingsItemViewModel();

            //------------Assert Results-------------------------
            Assert.IsNotNull(settingsItemViewModel.CloseHelpCommand);
        }
    }
}

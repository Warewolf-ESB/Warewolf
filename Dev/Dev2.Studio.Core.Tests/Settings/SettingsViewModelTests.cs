using Dev2.Settings;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Core.Tests.Settings
{
    [TestClass]
    public class SettingsViewModelTests
    {
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SettingsViewModel_Constructor")]
        public void SettingsViewModel_Constructor_Properties_Initialized()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var settingsViewModel = new SettingsViewModel();

            //------------Assert Results-------------------------
            Assert.IsFalse(settingsViewModel.ShowLogging);
            Assert.IsTrue(settingsViewModel.ShowSecurity);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SettingsViewModel_ShowLogging")]
        public void SettingsViewModel_ShowLogging_True_OtherShowPropertiesAreFalse()
        {
            //------------Setup for test--------------------------
            var settingsViewModel = new SettingsViewModel();

            //------------Execute Test---------------------------
            settingsViewModel.ShowLogging = true;

            //------------Assert Results-------------------------
            Assert.IsFalse(settingsViewModel.ShowSecurity);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SettingsViewModel_ShowLogging")]
        public void SettingsViewModel_ShowLogging_SameValue_DoesNotRaisePropertyChanged()
        {
            //------------Setup for test--------------------------
            var propertyChanged = false;

            var settingsViewModel = new SettingsViewModel();
            settingsViewModel.ShowLogging = true;
            settingsViewModel.PropertyChanged += (sender, args) => propertyChanged = true;

            //------------Execute Test---------------------------
            settingsViewModel.ShowLogging = true;

            //------------Assert Results-------------------------
            Assert.IsFalse(propertyChanged);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SettingsViewModel_ShowLogging")]
        public void SettingsViewModel_ShowLogging_DifferentValue_DoesRaisePropertyChanged()
        {
            //------------Setup for test--------------------------
            var propertyChanged = false;

            var settingsViewModel = new SettingsViewModel();
            settingsViewModel.ShowLogging = true;
            settingsViewModel.PropertyChanged += (sender, args) => propertyChanged = true;

            //------------Execute Test---------------------------
            settingsViewModel.ShowLogging = false;

            //------------Assert Results-------------------------
            Assert.IsTrue(propertyChanged);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SettingsViewModel_ShowSecurity")]
        public void SettingsViewModel_ShowSecurity_True_OtherShowPropertiesAreFalse()
        {
            //------------Setup for test--------------------------
            var settingsViewModel = new SettingsViewModel();
            settingsViewModel.ShowSecurity = false;

            //------------Execute Test---------------------------
            settingsViewModel.ShowSecurity = true;

            //------------Assert Results-------------------------
            Assert.IsFalse(settingsViewModel.ShowLogging);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SettingsViewModel_ShowSecurity")]
        public void SettingsViewModel_ShowSecurity_SameValue_DoesNotRaisePropertyChanged()
        {
            //------------Setup for test--------------------------
            var propertyChanged = false;

            var settingsViewModel = new SettingsViewModel();
            settingsViewModel.ShowSecurity = true;
            settingsViewModel.PropertyChanged += (sender, args) => propertyChanged = true;

            //------------Execute Test---------------------------
            settingsViewModel.ShowSecurity = true;

            //------------Assert Results-------------------------
            Assert.IsFalse(propertyChanged);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SettingsViewModel_ShowSecurity")]
        public void SettingsViewModel_ShowSecurity_DifferentValue_DoesRaisePropertyChanged()
        {
            //------------Setup for test--------------------------
            var propertyChanged = false;

            var settingsViewModel = new SettingsViewModel();
            settingsViewModel.ShowSecurity = true;
            settingsViewModel.PropertyChanged += (sender, args) => propertyChanged = true;

            //------------Execute Test---------------------------
            settingsViewModel.ShowSecurity = false;

            //------------Assert Results-------------------------
            Assert.IsTrue(propertyChanged);
        }
    }
}

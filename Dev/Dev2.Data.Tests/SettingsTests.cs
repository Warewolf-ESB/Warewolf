using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Data.Tests
{
    [TestClass]
    public class SettingsTests
    {
        [TestMethod]
        public void SettingsTests_ShouldHaveConstructor()
        {
            var settings = new Settings.Settings();
            Assert.IsNotNull(settings);
            Assert.IsTrue(string.IsNullOrEmpty(settings.Error));
            Assert.IsNotNull(settings.Logging);
            Assert.IsFalse(settings.HasError);            
        }
    }
}

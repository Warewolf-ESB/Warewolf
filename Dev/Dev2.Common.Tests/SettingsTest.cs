using Dev2.Common.Interfaces.Wrappers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Dev2.Common.Tests
{
    [TestClass]
    public class SettingsTest
    {
        class MockConfigurationManager : IConfigurationManager
        {
            readonly Dictionary<string, string> _store = new Dictionary<string, string>();
            public string this[string settingName, string defaultValue = null]
            {
                get => _store.ContainsKey(settingName) ? _store[settingName] : defaultValue;
                set => _store[settingName] = value;
            }
        }

            [TestMethod]
        public void Constructor_Setup_ServerSettings_NotNull_Expected()
        {
            Assert.IsNotNull(Config.Server);
        }

        [TestMethod]
        public void Get_AppConfig_AuditFilePath_WithDefault_Expected()
        {
            const string expectedPath = "SomePath";

            var mockConfig = new MockConfigurationManager();
            Config.ConfigureSettings(mockConfig);

            var value = Config.Server["AuditFilePath", expectedPath];
            Assert.AreEqual(expectedPath, value);
        }

        [TestMethod]
        public void Update_AppConfig_AuditFilePath_Default_Not_Persisted()
        {
            const string expectedPath1 = "SomePath";

            var mockConfig = new MockConfigurationManager();
            Config.ConfigureSettings(mockConfig);

            var value = Config.Server["AuditFilePath", expectedPath1];
            Assert.AreEqual(expectedPath1, value);
            Assert.AreNotEqual(expectedPath1, Config.Server["AuditFilePath"]);
        }

        [TestMethod]
        public void Update_AppConfig_AuditFilePath_Default_Not_Expected()
        {
            const string expectedPath1 = "SomePath";
            const string expectedPath2 = "SomeOtherPath";

            var mockConfig = new MockConfigurationManager();
            Config.ConfigureSettings(mockConfig);

            Config.Server["AuditFilePath"] = expectedPath2;
            Assert.AreEqual(expectedPath2, Config.Server["AuditFilePath", expectedPath1]);
        }
    }
}

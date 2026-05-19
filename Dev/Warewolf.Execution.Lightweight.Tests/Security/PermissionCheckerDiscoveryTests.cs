using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Warewolf.Execution.Lightweight.Security;

namespace Warewolf.Execution.Lightweight.Tests.Security
{
    /// <summary>
    /// Tests for <see cref="PermissionChecker.HasPublicDiscoveryPermission"/> and
    /// <see cref="PermissionChecker.HasUserDiscoveryPermission"/> which require
    /// both View AND Execute for apis.json discovery visibility (matching server
    /// ApisJsonBuilder.BuildForPath behavior).
    /// </summary>
    [TestClass]
    public class PermissionCheckerDiscoveryTests
    {
        static SecureConfigData MakeConfig(params PermissionEntry[] perms) =>
            new(isLoaded: true, secretKey: "", permissions: perms);

        static PermissionEntry PublicViewAndExecute(string resource, bool isGlobal = false) =>
            new(GroupName: "Public", IsGlobal: isGlobal, ResourceName: resource,
                View: true, Execute: true);

        static PermissionEntry PublicViewOnly(string resource, bool isGlobal = false) =>
            new(GroupName: "Public", IsGlobal: isGlobal, ResourceName: resource,
                View: true, Execute: false);

        static PermissionEntry PublicExecuteOnly(string resource, bool isGlobal = false) =>
            new(GroupName: "Public", IsGlobal: isGlobal, ResourceName: resource,
                View: false, Execute: true);

        static PermissionEntry GroupViewAndExecute(string group, string resource, bool isGlobal = false) =>
            new(GroupName: group, IsGlobal: isGlobal, ResourceName: resource,
                View: true, Execute: true);

        static PermissionEntry GroupViewOnly(string group, string resource, bool isGlobal = false) =>
            new(GroupName: group, IsGlobal: isGlobal, ResourceName: resource,
                View: true, Execute: false);

        // ══════════════════════════════════════════════════════════════════════════
        // HasPublicDiscoveryPermission
        // ══════════════════════════════════════════════════════════════════════════

        [TestMethod]
        [TestCategory("UnitTest")]
        public void HasPublicDiscoveryPermission_NoConfig_ReturnsTrue()
        {
            Assert.IsTrue(PermissionChecker.HasPublicDiscoveryPermission("Any", SecureConfigData.AllowAll));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void HasPublicDiscoveryPermission_GlobalViewAndExecute_ReturnsTrue()
        {
            var config = MakeConfig(PublicViewAndExecute("", isGlobal: true));
            Assert.IsTrue(PermissionChecker.HasPublicDiscoveryPermission("HelloWorld", config));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void HasPublicDiscoveryPermission_GlobalViewOnly_ReturnsFalse()
        {
            var config = MakeConfig(PublicViewOnly("", isGlobal: true));
            Assert.IsFalse(PermissionChecker.HasPublicDiscoveryPermission("HelloWorld", config));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void HasPublicDiscoveryPermission_GlobalExecuteOnly_ReturnsFalse()
        {
            var config = MakeConfig(PublicExecuteOnly("", isGlobal: true));
            Assert.IsFalse(PermissionChecker.HasPublicDiscoveryPermission("HelloWorld", config));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void HasPublicDiscoveryPermission_ResourceSpecificViewAndExecute_ReturnsTrue()
        {
            var config = MakeConfig(PublicViewAndExecute("HelloWorld"));
            Assert.IsTrue(PermissionChecker.HasPublicDiscoveryPermission("HelloWorld", config));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void HasPublicDiscoveryPermission_ResourceSpecificViewAndExecute_DifferentResource_ReturnsFalse()
        {
            var config = MakeConfig(PublicViewAndExecute("Other"));
            Assert.IsFalse(PermissionChecker.HasPublicDiscoveryPermission("HelloWorld", config));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void HasPublicDiscoveryPermission_ResourceSpecificViewOnly_ReturnsFalse()
        {
            var config = MakeConfig(PublicViewOnly("HelloWorld"));
            Assert.IsFalse(PermissionChecker.HasPublicDiscoveryPermission("HelloWorld", config));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void HasPublicDiscoveryPermission_CaseInsensitiveMatch()
        {
            var config = MakeConfig(PublicViewAndExecute("helloworld"));
            Assert.IsTrue(PermissionChecker.HasPublicDiscoveryPermission("HelloWorld", config));
        }

        // ══════════════════════════════════════════════════════════════════════════
        // HasUserDiscoveryPermission
        // ══════════════════════════════════════════════════════════════════════════

        [TestMethod]
        [TestCategory("UnitTest")]
        public void HasUserDiscoveryPermission_NoConfig_ReturnsTrue()
        {
            Assert.IsTrue(PermissionChecker.HasUserDiscoveryPermission(
                "Any", SecureConfigData.AllowAll, new List<string> { "Users" }));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void HasUserDiscoveryPermission_PublicHasGlobalViewAndExecute_ReturnsTrue()
        {
            var config = MakeConfig(PublicViewAndExecute("", isGlobal: true));
            Assert.IsTrue(PermissionChecker.HasUserDiscoveryPermission(
                "HelloWorld", config, new List<string> { "AnyGroup" }));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void HasUserDiscoveryPermission_UserGroupHasGlobalViewAndExecute_ReturnsTrue()
        {
            var config = MakeConfig(GroupViewAndExecute("Developers", "", isGlobal: true));
            Assert.IsTrue(PermissionChecker.HasUserDiscoveryPermission(
                "HelloWorld", config, new List<string> { "Developers" }));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void HasUserDiscoveryPermission_UserGroupHasResourceViewAndExecute_ReturnsTrue()
        {
            var config = MakeConfig(GroupViewAndExecute("Developers", "HelloWorld"));
            Assert.IsTrue(PermissionChecker.HasUserDiscoveryPermission(
                "HelloWorld", config, new List<string> { "Developers" }));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void HasUserDiscoveryPermission_UserGroupHasViewOnly_ReturnsFalse()
        {
            var config = MakeConfig(GroupViewOnly("Developers", "HelloWorld"));
            Assert.IsFalse(PermissionChecker.HasUserDiscoveryPermission(
                "HelloWorld", config, new List<string> { "Developers" }));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void HasUserDiscoveryPermission_UserNotInGroup_ReturnsFalse()
        {
            var config = MakeConfig(GroupViewAndExecute("Admins", "HelloWorld"));
            Assert.IsFalse(PermissionChecker.HasUserDiscoveryPermission(
                "HelloWorld", config, new List<string> { "Developers" }));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void HasUserDiscoveryPermission_MultipleGroups_MatchesCorrectOne()
        {
            var config = MakeConfig(
                GroupViewOnly("Readers", "HelloWorld"),
                GroupViewAndExecute("Executors", "HelloWorld"));

            Assert.IsTrue(PermissionChecker.HasUserDiscoveryPermission(
                "HelloWorld", config, new List<string> { "Readers", "Executors" }));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void HasUserDiscoveryPermission_ResourcePathStripping()
        {
            // Permission stored with bare name, request uses path format
            var config = MakeConfig(GroupViewAndExecute("Team", "Ping"));
            Assert.IsTrue(PermissionChecker.HasUserDiscoveryPermission(
                "Tools/Ping", config, new List<string> { "Team" }));
        }
    }
}

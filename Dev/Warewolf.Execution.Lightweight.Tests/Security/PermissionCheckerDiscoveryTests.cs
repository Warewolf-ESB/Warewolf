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
        public void HasUserDiscoveryPermission_BareName_DoesNotMatchSubfolderWorkflow()
        {
            // Permission ResourceName is bare "Ping" (root-level).
            // ApisJsonGenerator now passes the relative path "Tools/Ping" to the filter.
            // These are different resources — should not match.
            var config = MakeConfig(GroupViewAndExecute("Team", "Ping"));
            Assert.IsFalse(PermissionChecker.HasUserDiscoveryPermission(
                "Tools/Ping", config, new List<string> { "Team" }));
        }

        // ══════════════════════════════════════════════════════════════════════════
        // Resource-override precedence (server GetGroupPermissions behaviour)
        // When a group has a resource-specific entry, the global entry is suppressed.
        // ══════════════════════════════════════════════════════════════════════════

        [TestMethod]
        [TestCategory("UnitTest")]
        public void HasPublicDiscoveryPermission_ResourceSpecificDenies_GlobalGrantSuppressed()
        {
            // Public has global View+Execute BUT also has a resource-specific entry for
            // "HelloWorld" with Execute=false. The resource entry overrides the global one.
            var config = MakeConfig(
                PublicViewAndExecute("", isGlobal: true),           // global grant
                new PermissionEntry("Public", IsGlobal: false, ResourceName: "HelloWorld",
                    View: true, Execute: false));                    // resource override — Execute denied

            // Global execute should be suppressed by resource override.
            Assert.IsFalse(PermissionChecker.HasPublicDiscoveryPermission("HelloWorld", config));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void HasPublicDiscoveryPermission_ResourceSpecificDenies_UnrelatedWorkflowUnaffected()
        {
            // Resource override for "HelloWorld" should not affect "OtherWorkflow".
            var config = MakeConfig(
                PublicViewAndExecute("", isGlobal: true),
                new PermissionEntry("Public", IsGlobal: false, ResourceName: "HelloWorld",
                    View: true, Execute: false));

            // OtherWorkflow is not overridden — global grant applies.
            Assert.IsTrue(PermissionChecker.HasPublicDiscoveryPermission("OtherWorkflow", config));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void HasUserDiscoveryPermission_ResourceSpecificDenies_GlobalGrantSuppressed()
        {
            // Group "Developers" has global View+Execute but resource entry for "HelloWorld"
            // has Execute=false → should be denied.
            var config = MakeConfig(
                GroupViewAndExecute("Developers", "", isGlobal: true),
                new PermissionEntry("Developers", IsGlobal: false, ResourceName: "HelloWorld",
                    View: true, Execute: false));

            Assert.IsFalse(PermissionChecker.HasUserDiscoveryPermission(
                "HelloWorld", config, new List<string> { "Developers" }));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void HasUserDiscoveryPermission_ResourceSpecificGrants_WhenGlobalIsDeny()
        {
            // Group has global View-only but resource-specific View+Execute for "HelloWorld".
            var config = MakeConfig(
                GroupViewOnly("Developers", "", isGlobal: true),    // global: View only
                GroupViewAndExecute("Developers", "HelloWorld"));    // resource: View+Execute

            Assert.IsTrue(PermissionChecker.HasUserDiscoveryPermission(
                "HelloWorld", config, new List<string> { "Developers" }));
        }

        // ══════════════════════════════════════════════════════════════════════════
        // Separator normalisation — ResourcePath back-slash vs forward-slash
        // ══════════════════════════════════════════════════════════════════════════

        [TestMethod]
        [TestCategory("UnitTest")]
        public void NamesMatch_BackslashPath_MatchesForwardSlashWorkflow()
        {
            // ResourcePath from secure.config: "data\sales"
            // Workflow name from route/scanner: "data/sales"
            Assert.IsTrue(PermissionChecker.NamesMatch(@"data\sales", "data/sales"));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void NamesMatch_BackslashPath_DoesNotMatchBareWorkflowName()
        {
            // "data\sales" targets the sales workflow inside the data folder.
            // A bare "sales" refers to root-level sales — they are different resources.
            Assert.IsFalse(PermissionChecker.NamesMatch(@"data\sales", "sales"));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void NamesMatch_BarePermissionName_DoesNotMatchPathWorkflow()
        {
            // A bare-name permission "sales" targets root-level sales.
            // "Tools/sales" is a different resource inside the Tools folder.
            Assert.IsFalse(PermissionChecker.NamesMatch("sales", "Tools/sales"));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void NamesMatch_DifferentNames_ReturnsFalse()
        {
            Assert.IsFalse(PermissionChecker.NamesMatch(@"data\sales", "invoices"));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void HasPublicDiscoveryPermission_FolderWorkflow_BackslashResourcePath_DoesNotMatchBareName()
        {
            // ResourceName is "data\sales" (folder-qualified).
            // ApisJsonGenerator passes the relative path to the filter — a bare "sales"
            // would represent a root-level workflow and must NOT match.
            var config = MakeConfig(
                new PermissionEntry("Public", IsGlobal: false, ResourceName: @"data\sales",
                    View: true, Execute: true));

            Assert.IsFalse(PermissionChecker.HasPublicDiscoveryPermission("sales", config));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void HasPublicDiscoveryPermission_FolderWorkflow_ForwardSlashPath_ReturnsTrue()
        {
            var config = MakeConfig(
                new PermissionEntry("Public", IsGlobal: false, ResourceName: @"data\sales",
                    View: true, Execute: true));

            Assert.IsTrue(PermissionChecker.HasPublicDiscoveryPermission("data/sales", config));
        }
    }
}

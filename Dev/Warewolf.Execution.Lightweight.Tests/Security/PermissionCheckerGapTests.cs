/*
 * Gap test for PermissionChecker.HasPublicExecutePermission — only exercised
 * indirectly through HasUserDiscoveryPermission. The discovery path makes a
 * workflow visible to authenticated users when the Public group has Execute
 * (but not View) on it. That branch is missing from the BDD spec matrix and
 * shows as uncovered (L122-124 of PermissionChecker.cs).
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Warewolf.Execution.Lightweight.Security;

namespace Warewolf.Execution.Lightweight.Tests.Security
{
    [TestClass]
    public class PermissionCheckerGapTests
    {
        // SecureConfigData ctor: (isLoaded, secretKey, permissions, ...).
        // Tests need IsLoaded=true so the early-return-on-AllowAll branch is skipped.
        static SecureConfigData MakeConfig(params PermissionEntry[] perms) =>
            new(isLoaded: true, secretKey: "", permissions: perms);

        // PermissionEntry positional record: (GroupName, IsGlobal, ResourceName, View, Execute=...).
        static PermissionEntry PublicExecuteOn(string resource, bool isGlobal = false) =>
            new(GroupName: "Public", IsGlobal: isGlobal, ResourceName: resource,
                View: false, Execute: true);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void HasUserDiscoveryPermission_PublicHasResourceExecute_GrantsDiscoveryToAuthUser()
        {
            // Public group has Execute on "HelloWorld" but no View anywhere.
            // An authenticated user in some other group must still see HelloWorld in
            // discovery listings — that's the L122-124 branch.
            var config = MakeConfig(PublicExecuteOn("HelloWorld"));

            var ok = PermissionChecker.HasUserDiscoveryPermission(
                "HelloWorld", config, new List<string> { "SomeOtherGroup" });

            Assert.IsTrue(ok);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void HasUserDiscoveryPermission_PublicHasGlobalExecute_GrantsDiscoveryForAnyWorkflow()
        {
            var config = MakeConfig(PublicExecuteOn(resource: "", isGlobal: true));

            Assert.IsTrue(PermissionChecker.HasUserDiscoveryPermission(
                "AnyWorkflow", config, new List<string> { "SomeGroup" }));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void HasUserDiscoveryPermission_PublicHasExecuteOnDifferentResource_DoesNotGrant()
        {
            // Public has Execute on "Other", not on "HelloWorld" — and the user's
            // group has no permission at all. Discovery must be denied.
            var config = MakeConfig(PublicExecuteOn("Other"));

            Assert.IsFalse(PermissionChecker.HasUserDiscoveryPermission(
                "HelloWorld", config, new List<string> { "Users" }));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void HasUserViewPermission_PublicHasOnlyExecute_DoesNotGrantView()
        {
            // Execute alone does NOT imply View. View permission must come from
            // either Public/View or a matching group/View entry.
            var config = MakeConfig(PublicExecuteOn("HelloWorld"));

            Assert.IsFalse(PermissionChecker.HasUserViewPermission(
                "HelloWorld", config, new List<string> { "SomeGroup" }));
        }
    }
}

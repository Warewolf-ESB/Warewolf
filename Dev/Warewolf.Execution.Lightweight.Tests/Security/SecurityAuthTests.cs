/*
 * Security integration tests for the Warewolf lightweight execution engine.
 *
 * These tests exercise the full security pipeline:
 *   SecureConfigLoader.LoadFrom  → SecureConfigData
 *   JwtValidator.GetUserGroups   → IReadOnlyList<string>?
 *   PermissionChecker.*          → bool
 *
 * No HTTP server is required — all tests work directly against the in-process
 * security classes and against real/synthetic encrypted config files on disk.
 *
 * Test configuration matrix:
 *
 *   CONFIG_A  AllowAll      — no secure.config file present
 *   CONFIG_B  AllPublic     — Public group has global View (IsServer=true, Guid.Empty)
 *   CONFIG_C  NoPublic      — Public group has no View anywhere
 *   CONFIG_D  PartialPublic — Public has View only on "HelloWorld" and "Tools/Ping"
 *   CONFIG_E  GroupBased    — TeamA sees WorkflowA, TeamB sees WorkflowB, nothing public
 *   CONFIG_F  RealConfig    — loaded from C:\ProgramData\Warewolf\Server Settings\secure.config
 *                             (tests skipped when that file does not exist)
 */

using Dev2.Services.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Warewolf.Execution.Lightweight.Security;

namespace Warewolf.Execution.Lightweight.Tests.Security
{
    [TestClass]
    public class SecurityAuthTests
    {
        // ── Real secure.config path ───────────────────────────────────────────────
        // In CI set WAREWOLF_TEST_SECURE_CONFIG to the path of an encrypted config
        // file so these tests can run in Docker.  CiTestSetup (AssemblyInitialize)
        // auto-generates a minimal config and sets that variable when
        // WAREWOLF_GENERATE_CI_CONFIG=1.  Locally falls back to the standard
        // Warewolf server settings directory.
        //
        // This must be a property (not a readonly field) so that the env-var value
        // set by CiTestSetup during AssemblyInitialize is visible to the tests.

        static string RealConfigPath =>
            Environment.GetEnvironmentVariable("WAREWOLF_TEST_SECURE_CONFIG")
            ?? @"C:\ProgramData\Warewolf\Server Settings\secure.config";

        // ── Shared secret keys (generated once per test run) ─────────────────────

        static readonly string KeyB = SecureConfigBuilder.NewSecretKey();
        static readonly string KeyC = SecureConfigBuilder.NewSecretKey();
        static readonly string KeyD = SecureConfigBuilder.NewSecretKey();
        static readonly string KeyE = SecureConfigBuilder.NewSecretKey();

        // ── Temp file paths written in ClassInitialize ────────────────────────────

        static string _pathB = null!;
        static string _pathC = null!;
        static string _pathD = null!;
        static string _pathE = null!;

        // ── Loaded SecureConfigData instances ─────────────────────────────────────

        static SecureConfigData _cfgAllowAll  = null!;
        static SecureConfigData _cfgAllPublic = null!;
        static SecureConfigData _cfgNoPublic  = null!;
        static SecureConfigData _cfgPartial   = null!;
        static SecureConfigData _cfgGrouped   = null!;

        // ── Test setup ────────────────────────────────────────────────────────────

        [ClassInitialize]
        public static void ClassInit(TestContext _)
        {
            // CONFIG_A: no file → AllowAll sentinel
            _cfgAllowAll = SecureConfigLoader.LoadFrom("nonexistent_secure.config");

            // CONFIG_B: Public with global View
            _pathB = SecureConfigBuilder.WriteTempConfig(
                SecureConfigBuilder.AllPublicGlobal(KeyB));
            _cfgAllPublic = SecureConfigLoader.LoadFrom(_pathB);

            // CONFIG_C: Public with no View (server default after install)
            _pathC = SecureConfigBuilder.WriteTempConfig(
                SecureConfigBuilder.NoPublicAccess(KeyC));
            _cfgNoPublic = SecureConfigLoader.LoadFrom(_pathC);

            // CONFIG_D: Public has View on specific resources only
            _pathD = SecureConfigBuilder.WriteTempConfig(
                SecureConfigBuilder.PartiallyPublic(KeyD, "HelloWorld", "Tools/Ping"));
            _cfgPartial = SecureConfigLoader.LoadFrom(_pathD);

            // CONFIG_E: group-based access, nothing public
            _pathE = SecureConfigBuilder.WriteTempConfig(
                SecureConfigBuilder.GroupBasedAccess(KeyE,
                [
                    ("TeamA", ["WorkflowA", "Shared/Audit"]),
                    ("TeamB", ["WorkflowB", "Shared/Audit"]),
                    ("Admins", ["WorkflowA", "WorkflowB", "AdminDashboard"]),
                ]));
            _cfgGrouped = SecureConfigLoader.LoadFrom(_pathE);
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            foreach (var path in new[] { _pathB, _pathC, _pathD, _pathE })
            {
                try { if (path is not null) File.Delete(path); } catch { }
            }
        }

        // ══════════════════════════════════════════════════════════════════════════
        // CONFIG_A — No secure.config (open-access / AllowAll mode)
        // ══════════════════════════════════════════════════════════════════════════

        [TestMethod, TestCategory("Security_Config")]
        public void A_NoConfigFile_IsLoaded_False()
        {
            Assert.IsFalse(_cfgAllowAll.IsLoaded);
        }

        [TestMethod, TestCategory("Security_Config")]
        public void A_NoConfigFile_AllWorkflowsPublic()
        {
            Assert.IsTrue(PermissionChecker.HasPublicViewPermission("AnyWorkflow",   _cfgAllowAll));
            Assert.IsTrue(PermissionChecker.HasPublicViewPermission("Tools/Ping",    _cfgAllowAll));
            Assert.IsTrue(PermissionChecker.HasPublicViewPermission("Restricted/HR", _cfgAllowAll));
        }

        [TestMethod, TestCategory("Security_Config")]
        public void A_NoConfigFile_NoSecretKey_JwtValidation_ReturnsNull()
        {
            // No secret key exists → cannot validate any token.
            var fakeToken = "header.payload.sig";
            Assert.IsNull(JwtValidator.GetUserGroups(fakeToken, string.Empty));
            Assert.IsNull(JwtValidator.GetUserGroups(fakeToken, _cfgAllowAll.SecretKey));
        }

        // ══════════════════════════════════════════════════════════════════════════
        // CONFIG_B — Public group has global View
        // ══════════════════════════════════════════════════════════════════════════

        [TestMethod, TestCategory("Security_Config")]
        public void B_AllPublicGlobal_IsLoaded_True()
        {
            Assert.IsTrue(_cfgAllPublic.IsLoaded);
        }

        [TestMethod, TestCategory("Security_Config")]
        public void B_AllPublicGlobal_AllWorkflowsPublic()
        {
            Assert.IsTrue(PermissionChecker.HasPublicViewPermission("HelloWorld",    _cfgAllPublic));
            Assert.IsTrue(PermissionChecker.HasPublicViewPermission("Tools/Ping",    _cfgAllPublic));
            Assert.IsTrue(PermissionChecker.HasPublicViewPermission("Restricted/HR", _cfgAllPublic));
        }

        [TestMethod, TestCategory("Security_JWT")]
        public void B_AllPublicGlobal_ValidToken_GivesGroups()
        {
            var token  = JwtTestHelper.ValidToken(KeyB, "TeamA", "TeamB");
            var groups = JwtValidator.GetUserGroups(token, _cfgAllPublic.SecretKey);

            Assert.IsNotNull(groups);
            CollectionAssert.Contains(groups.ToList(), "TeamA");
            CollectionAssert.Contains(groups.ToList(), "TeamB");
        }

        [TestMethod, TestCategory("Security_JWT")]
        public void B_AllPublicGlobal_TokenWithBearerPrefix_GivesGroups()
        {
            var token  = "Bearer " + JwtTestHelper.ValidToken(KeyB, "Developers");
            var groups = JwtValidator.GetUserGroups(token, _cfgAllPublic.SecretKey);

            Assert.IsNotNull(groups);
            CollectionAssert.Contains(groups.ToList(), "Developers");
        }

        [TestMethod, TestCategory("Security_JWT")]
        public void B_AllPublicGlobal_TokenBearerCaseInsensitive_GivesGroups()
        {
            var token  = "BEARER " + JwtTestHelper.ValidToken(KeyB, "Developers");
            var groups = JwtValidator.GetUserGroups(token, _cfgAllPublic.SecretKey);
            Assert.IsNotNull(groups);
        }

        // ══════════════════════════════════════════════════════════════════════════
        // CONFIG_C — No public access (default after fresh Warewolf install)
        // ══════════════════════════════════════════════════════════════════════════

        [TestMethod, TestCategory("Security_Config")]
        public void C_NoPublicAccess_NoWorkflowPubliclyVisible()
        {
            Assert.IsFalse(PermissionChecker.HasPublicViewPermission("HelloWorld",    _cfgNoPublic));
            Assert.IsFalse(PermissionChecker.HasPublicViewPermission("Tools/Ping",    _cfgNoPublic));
            Assert.IsFalse(PermissionChecker.HasPublicViewPermission("Restricted/HR", _cfgNoPublic));
        }

        [TestMethod, TestCategory("Security_Config")]
        public void C_NoPublicAccess_AdminGroup_GlobalView_SeesEverything()
        {
            var token  = JwtTestHelper.ValidToken(KeyC, SecureConfigBuilder.AdminGroup);
            var groups = JwtValidator.GetUserGroups(token, _cfgNoPublic.SecretKey);

            // Admin has global View → all workflows accessible
            Assert.IsTrue(PermissionChecker.HasUserViewPermission("HelloWorld",    _cfgNoPublic, groups!));
            Assert.IsTrue(PermissionChecker.HasUserViewPermission("AnyWorkflow",   _cfgNoPublic, groups!));
            Assert.IsTrue(PermissionChecker.HasUserViewPermission("Restricted/HR", _cfgNoPublic, groups!));
        }

        [TestMethod, TestCategory("Security_Config")]
        public void C_NoPublicAccess_UnknownGroup_CannotSeeAnything()
        {
            var groups = new[] { "RandomGroup", "AnotherGroup" };
            Assert.IsFalse(PermissionChecker.HasUserViewPermission("HelloWorld", _cfgNoPublic, groups));
            Assert.IsFalse(PermissionChecker.HasUserViewPermission("AnyThing",   _cfgNoPublic, groups));
        }

        // ══════════════════════════════════════════════════════════════════════════
        // CONFIG_D — Public has View on "HelloWorld" and "Tools/Ping" only
        // ══════════════════════════════════════════════════════════════════════════

        [TestMethod, TestCategory("Security_Config")]
        public void D_PartialPublic_GrantedWorkflows_ArePublic()
        {
            Assert.IsTrue(PermissionChecker.HasPublicViewPermission("HelloWorld", _cfgPartial));
            Assert.IsTrue(PermissionChecker.HasPublicViewPermission("Tools/Ping", _cfgPartial));
        }

        [TestMethod, TestCategory("Security_Config")]
        public void D_PartialPublic_OtherWorkflows_AreNotPublic()
        {
            Assert.IsFalse(PermissionChecker.HasPublicViewPermission("SecretReport",   _cfgPartial));
            Assert.IsFalse(PermissionChecker.HasPublicViewPermission("HR/Payroll",      _cfgPartial));
            Assert.IsFalse(PermissionChecker.HasPublicViewPermission("AdminDashboard",  _cfgPartial));
        }

        [TestMethod, TestCategory("Security_Config")]
        public void D_PartialPublic_NameMatchIsCaseInsensitive()
        {
            // PermissionChecker.NamesMatch uses OrdinalIgnoreCase
            Assert.IsTrue(PermissionChecker.HasPublicViewPermission("helloworld", _cfgPartial));
            Assert.IsTrue(PermissionChecker.HasPublicViewPermission("TOOLS/PING", _cfgPartial));
        }

        [TestMethod, TestCategory("Security_Config")]
        public void D_PartialPublic_AdminToken_SeesEverything()
        {
            var token  = JwtTestHelper.ValidToken(KeyD, SecureConfigBuilder.AdminGroup);
            var groups = JwtValidator.GetUserGroups(token, _cfgPartial.SecretKey);

            Assert.IsTrue(PermissionChecker.HasUserViewPermission("HelloWorld",    _cfgPartial, groups!));
            Assert.IsTrue(PermissionChecker.HasUserViewPermission("SecretReport",  _cfgPartial, groups!));
            Assert.IsTrue(PermissionChecker.HasUserViewPermission("AdminDashboard", _cfgPartial, groups!));
        }

        // ══════════════════════════════════════════════════════════════════════════
        // CONFIG_E — Group-based access, nothing public
        // TeamA  → WorkflowA, Shared/Audit
        // TeamB  → WorkflowB, Shared/Audit
        // Admins → WorkflowA, WorkflowB, AdminDashboard
        // ══════════════════════════════════════════════════════════════════════════

        [TestMethod, TestCategory("Security_Config")]
        public void E_GroupBased_NothingIsPubliclyVisible()
        {
            foreach (var wf in new[] { "WorkflowA", "WorkflowB", "Shared/Audit", "AdminDashboard" })
                Assert.IsFalse(PermissionChecker.HasPublicViewPermission(wf, _cfgGrouped),
                    $"{wf} should not be public");
        }

        [TestMethod, TestCategory("Security_Config")]
        public void E_GroupBased_TeamA_SeesOnlyOwnWorkflows()
        {
            var token  = JwtTestHelper.ValidToken(KeyE, "TeamA");
            var groups = JwtValidator.GetUserGroups(token, _cfgGrouped.SecretKey);

            Assert.IsTrue( PermissionChecker.HasUserViewPermission("WorkflowA",    _cfgGrouped, groups!));
            Assert.IsTrue( PermissionChecker.HasUserViewPermission("Shared/Audit", _cfgGrouped, groups!));
            Assert.IsFalse(PermissionChecker.HasUserViewPermission("WorkflowB",    _cfgGrouped, groups!));
            Assert.IsFalse(PermissionChecker.HasUserViewPermission("AdminDashboard", _cfgGrouped, groups!));
        }

        [TestMethod, TestCategory("Security_Config")]
        public void E_GroupBased_TeamB_SeesOnlyOwnWorkflows()
        {
            var token  = JwtTestHelper.ValidToken(KeyE, "TeamB");
            var groups = JwtValidator.GetUserGroups(token, _cfgGrouped.SecretKey);

            Assert.IsTrue( PermissionChecker.HasUserViewPermission("WorkflowB",    _cfgGrouped, groups!));
            Assert.IsTrue( PermissionChecker.HasUserViewPermission("Shared/Audit", _cfgGrouped, groups!));
            Assert.IsFalse(PermissionChecker.HasUserViewPermission("WorkflowA",    _cfgGrouped, groups!));
            Assert.IsFalse(PermissionChecker.HasUserViewPermission("AdminDashboard", _cfgGrouped, groups!));
        }

        [TestMethod, TestCategory("Security_Config")]
        public void E_GroupBased_Admins_SeesAllGrantedResources()
        {
            var token  = JwtTestHelper.ValidToken(KeyE, "Admins");
            var groups = JwtValidator.GetUserGroups(token, _cfgGrouped.SecretKey);

            Assert.IsTrue(PermissionChecker.HasUserViewPermission("WorkflowA",      _cfgGrouped, groups!));
            Assert.IsTrue(PermissionChecker.HasUserViewPermission("WorkflowB",      _cfgGrouped, groups!));
            Assert.IsTrue(PermissionChecker.HasUserViewPermission("AdminDashboard",  _cfgGrouped, groups!));
        }

        [TestMethod, TestCategory("Security_Config")]
        public void E_GroupBased_Admins_CannotSeeUnlistedWorkflows()
        {
            var token  = JwtTestHelper.ValidToken(KeyE, "Admins");
            var groups = JwtValidator.GetUserGroups(token, _cfgGrouped.SecretKey);

            // "Shared/Audit" was NOT granted to Admins — only TeamA and TeamB
            Assert.IsFalse(PermissionChecker.HasUserViewPermission("Shared/Audit", _cfgGrouped, groups!));
            Assert.IsFalse(PermissionChecker.HasUserViewPermission("Unknown",      _cfgGrouped, groups!));
        }

        [TestMethod, TestCategory("Security_Config")]
        public void E_GroupBased_UserInMultipleGroups_SeesUnionOfPermissions()
        {
            // A user that belongs to both TeamA and TeamB should see both sets.
            var token  = JwtTestHelper.ValidToken(KeyE, "TeamA", "TeamB");
            var groups = JwtValidator.GetUserGroups(token, _cfgGrouped.SecretKey);

            Assert.IsTrue(PermissionChecker.HasUserViewPermission("WorkflowA",    _cfgGrouped, groups!));
            Assert.IsTrue(PermissionChecker.HasUserViewPermission("WorkflowB",    _cfgGrouped, groups!));
            Assert.IsTrue(PermissionChecker.HasUserViewPermission("Shared/Audit", _cfgGrouped, groups!));
            Assert.IsFalse(PermissionChecker.HasUserViewPermission("AdminDashboard", _cfgGrouped, groups!));
        }

        // ══════════════════════════════════════════════════════════════════════════
        // JWT validation edge cases (config-agnostic)
        // ══════════════════════════════════════════════════════════════════════════

        [TestMethod, TestCategory("Security_JWT")]
        public void JWT_ExpiredToken_ReturnsNull()
        {
            var token = JwtTestHelper.ExpiredToken(KeyB, "TeamA");
            Assert.IsNull(JwtValidator.GetUserGroups(token, KeyB));
        }

        [TestMethod, TestCategory("Security_JWT")]
        public void JWT_TamperedPayload_ReturnsNull()
        {
            var token = JwtTestHelper.TamperedPayloadToken(KeyB, "TeamA");
            Assert.IsNull(JwtValidator.GetUserGroups(token, KeyB));
        }

        [TestMethod, TestCategory("Security_JWT")]
        public void JWT_BadSignature_ReturnsNull()
        {
            var token = JwtTestHelper.BadSignatureToken(KeyB, "TeamA");
            Assert.IsNull(JwtValidator.GetUserGroups(token, KeyB));
        }

        [TestMethod, TestCategory("Security_JWT")]
        public void JWT_WrongKey_ReturnsNull()
        {
            var token = JwtTestHelper.WrongKeyToken(KeyB, "TeamA");
            Assert.IsNull(JwtValidator.GetUserGroups(token, KeyB));
        }

        [TestMethod, TestCategory("Security_JWT")]
        public void JWT_NullAuthHeader_ReturnsNull()
        {
            Assert.IsNull(JwtValidator.GetUserGroups(null,  KeyB));
            Assert.IsNull(JwtValidator.GetUserGroups("",    KeyB));
            Assert.IsNull(JwtValidator.GetUserGroups("   ", KeyB));
        }

        [TestMethod, TestCategory("Security_JWT")]
        public void JWT_EmptySecretKey_ReturnsNull()
        {
            var token = JwtTestHelper.ValidToken(KeyB, "TeamA");
            Assert.IsNull(JwtValidator.GetUserGroups(token, ""));
            Assert.IsNull(JwtValidator.GetUserGroups(token, null!));
        }

        [TestMethod, TestCategory("Security_JWT")]
        public void JWT_MalformedToken_ReturnsNull()
        {
            Assert.IsNull(JwtValidator.GetUserGroups("notavalidtoken",   KeyB));
            Assert.IsNull(JwtValidator.GetUserGroups("only.two.parts.x", KeyB));
            Assert.IsNull(JwtValidator.GetUserGroups("a.b",              KeyB));
        }

        [TestMethod, TestCategory("Security_JWT")]
        public void JWT_TokenWithNoGroups_ReturnsEmptyList()
        {
            // ValidToken with no groups → UserGroups: []
            var token  = JwtTestHelper.ValidToken(KeyB);
            var groups = JwtValidator.GetUserGroups(token, KeyB);

            Assert.IsNotNull(groups);
            Assert.AreEqual(0, groups.Count);
        }

        // ══════════════════════════════════════════════════════════════════════════
        // HasUserDiscoveryPermission — Execute (without View) grants discoverability
        // ══════════════════════════════════════════════════════════════════════════

        static SecureConfigData BuildExecuteOnlyPublicConfig()
        {
            var key  = SecureConfigBuilder.NewSecretKey();
            var path = SecureConfigBuilder.WriteTempConfig(
                SecureConfigBuilder.PublicExecuteGlobal(key));
            var cfg = SecureConfigLoader.LoadFrom(path);
            File.Delete(path);
            return cfg;
        }

        [TestMethod, TestCategory("Security_Config")]
        public void Discovery_NoConfig_AllWorkflowsDiscoverable()
        {
            var groups = new List<string> { "SomeGroup" };
            Assert.IsTrue(PermissionChecker.HasUserDiscoveryPermission("AnyWorkflow", _cfgAllowAll, groups));
        }

        [TestMethod, TestCategory("Security_Config")]
        public void Discovery_PublicGlobalViewOnly_NotDiscoverable_RequiresExecute()
        {
            // Discovery requires BOTH View AND Execute (PermissionChecker.HasUserDiscoveryPermission,
            // mirrored by PermissionCheckerDiscoveryTests). The Public group here has global View
            // only (no Execute), so nothing is discoverable.
            var groups = new List<string> { "SomeGroup" };
            Assert.IsFalse(PermissionChecker.HasUserDiscoveryPermission("AnyWorkflow",   _cfgAllPublic, groups));
            Assert.IsFalse(PermissionChecker.HasUserDiscoveryPermission("Tools/Ping",    _cfgAllPublic, groups));
            Assert.IsFalse(PermissionChecker.HasUserDiscoveryPermission("Restricted/HR", _cfgAllPublic, groups));
        }

        [TestMethod, TestCategory("Security_Config")]
        public void Discovery_PublicGlobalExecuteOnly_NotDiscoverable_RequiresView()
        {
            // Discovery requires BOTH View AND Execute. Public here has Execute only (no View),
            // so workflows are NOT discoverable.
            var cfg    = BuildExecuteOnlyPublicConfig();
            var groups = new List<string> { "SomeGroup" };
            Assert.IsFalse(PermissionChecker.HasUserDiscoveryPermission("AnyWorkflow", cfg, groups));
            Assert.IsFalse(PermissionChecker.HasUserDiscoveryPermission("Tools/Ping",  cfg, groups));
        }

        [TestMethod, TestCategory("Security_Config")]
        public void Discovery_PublicGlobalExecute_NotVisibleOnPublicEndpoint()
        {
            // Execute without View must NOT make workflows appear on /Public/apis.json.
            var cfg = BuildExecuteOnlyPublicConfig();
            Assert.IsFalse(PermissionChecker.HasPublicViewPermission("AnyWorkflow", cfg));
        }

        [TestMethod, TestCategory("Security_Config")]
        public void Discovery_NoPublicAccess_NoDiscoveryWithoutMatchingGroup()
        {
            var groups = new List<string> { "UnknownGroup" };
            Assert.IsFalse(PermissionChecker.HasUserDiscoveryPermission("HelloWorld", _cfgNoPublic, groups));
        }

        [TestMethod, TestCategory("Security_Config")]
        public void Discovery_GroupWithExecuteOnly_CannotDiscover_RequiresViewPlusExecute()
        {
            // "Runners" has a resource Execute-only (no View) entry on PingWorkflow. Discovery
            // requires BOTH View AND Execute, so PingWorkflow is NOT discoverable for Runners.
            var key  = SecureConfigBuilder.NewSecretKey();
            var path = SecureConfigBuilder.WriteTempConfig(
                SecureConfigBuilder.Build(key,
                    SecureConfigBuilder.Admin(),
                    SecureConfigBuilder.ServerPerm(SecureConfigBuilder.PublicGroup, View: false),
                    SecureConfigBuilder.ResourcePerm("Runners", "PingWorkflow", View: false, Execute: true)));
            try
            {
                var cfg    = SecureConfigLoader.LoadFrom(path);
                var groups = new List<string> { "Runners" };

                Assert.IsFalse(PermissionChecker.HasUserDiscoveryPermission("PingWorkflow",  cfg, groups),
                    "Execute-only (no View) must NOT be discoverable — discovery requires View AND Execute.");
                Assert.IsFalse(PermissionChecker.HasUserDiscoveryPermission("OtherWorkflow", cfg, groups));
                Assert.IsFalse(PermissionChecker.HasPublicViewPermission("PingWorkflow", cfg));
            }
            finally
            {
                File.Delete(path);
            }
        }

        // ══════════════════════════════════════════════════════════════════════════
        // CONFIG_F — Real secure.config (skipped if file not present)
        // ══════════════════════════════════════════════════════════════════════════

        [TestMethod, TestCategory("Security_RealConfig")]
        public void F_RealConfig_LoadsSuccessfully()
        {
            if (!File.Exists(RealConfigPath))
                Assert.Inconclusive($"Skipped: {RealConfigPath} not found.");

            var cfg = SecureConfigLoader.LoadFrom(RealConfigPath);

            Assert.IsTrue(cfg.IsLoaded,                $"Expected IsLoaded=true");
            Assert.IsFalse(string.IsNullOrEmpty(cfg.SecretKey), "Expected a non-empty SecretKey");
            Assert.IsNotNull(cfg.Permissions,           "Expected a Permissions list");
        }

        [TestMethod, TestCategory("Security_RealConfig")]
        public void F_RealConfig_PermissionList_IsNonEmpty()
        {
            if (!File.Exists(RealConfigPath))
                Assert.Inconclusive($"Skipped: {RealConfigPath} not found.");

            var cfg = SecureConfigLoader.LoadFrom(RealConfigPath);
            Assert.IsTrue(cfg.Permissions.Count > 0, "Expected at least one permission entry");

            // Log the decoded permissions for diagnostic purposes.
            foreach (var perm in cfg.Permissions)
            {
                Console.WriteLine(
                    $"  Group={perm.GroupName,-30} IsGlobal={perm.IsGlobal,-5} " +
                    $"View={perm.View,-5} Resource={perm.ResourceName}");
            }
        }

        [TestMethod, TestCategory("Security_RealConfig")]
        public void F_RealConfig_ValidToken_RoundTrips()
        {
            if (!File.Exists(RealConfigPath))
                Assert.Inconclusive($"Skipped: {RealConfigPath} not found.");

            var cfg   = SecureConfigLoader.LoadFrom(RealConfigPath);
            var token = JwtTestHelper.ValidToken(cfg.SecretKey, "Warewolf Administrators");

            var groups = JwtValidator.GetUserGroups(token, cfg.SecretKey);

            Assert.IsNotNull(groups, "Expected groups to be non-null for a valid token");
            CollectionAssert.Contains(groups.ToList(), "Warewolf Administrators");
        }

        [TestMethod, TestCategory("Security_RealConfig")]
        public void F_RealConfig_AdminGroup_SeesAllPermittedResources()
        {
            if (!File.Exists(RealConfigPath))
                Assert.Inconclusive($"Skipped: {RealConfigPath} not found.");

            var cfg    = SecureConfigLoader.LoadFrom(RealConfigPath);
            var token  = JwtTestHelper.ValidToken(cfg.SecretKey, SecureConfigBuilder.AdminGroup);
            var groups = JwtValidator.GetUserGroups(token, cfg.SecretKey);

            // Admin has global View in the default Warewolf config.
            Assert.IsTrue(
                PermissionChecker.HasUserViewPermission("SomeWorkflow", cfg, groups!),
                "Warewolf Administrators should have global View");
        }

        [TestMethod, TestCategory("Security_RealConfig")]
        public void F_RealConfig_PublicViewPermission_ReflectsActualConfig()
        {
            if (!File.Exists(RealConfigPath))
                Assert.Inconclusive($"Skipped: {RealConfigPath} not found.");

            var cfg = SecureConfigLoader.LoadFrom(RealConfigPath);

            // Determine whether the real config grants public access globally.
            var publicGlobalView = cfg.Permissions
                .Any(p => p.IsPublicGroup && p.View && p.IsGlobal);

            if (publicGlobalView)
            {
                Assert.IsTrue(PermissionChecker.HasPublicViewPermission("AnyWorkflow", cfg),
                    "Real config grants global public View, so any workflow should be public");
            }
            else
            {
                // Real config restricts public access — we just verify the method returns
                // a consistent bool without throwing.
                var result = PermissionChecker.HasPublicViewPermission("SomeWorkflow", cfg);
                Console.WriteLine($"HasPublicViewPermission('SomeWorkflow') = {result}");
            }
        }

        [TestMethod, TestCategory("Security_RealConfig")]
        public void F_RealConfig_CreateVariant_AllPublic_OverridesPermissions()
        {
            if (!File.Exists(RealConfigPath))
                Assert.Inconclusive($"Skipped: {RealConfigPath} not found.");

            // Start from the real config's secret key but override permissions to be all-public.
            var realCfg   = SecureConfigLoader.LoadFrom(RealConfigPath);
            var variantKey = realCfg.SecretKey;    // Reuse the same secret key.

            var variantPath = SecureConfigBuilder.WriteTempConfig(
                SecureConfigBuilder.AllPublicGlobal(variantKey));
            try
            {
                var variantCfg = SecureConfigLoader.LoadFrom(variantPath);

                // Variant uses the same key, so tokens issued against the real config
                // are still valid in the variant.
                var token  = JwtTestHelper.ValidToken(variantKey, "Developers");
                var groups = JwtValidator.GetUserGroups(token, variantCfg.SecretKey);

                Assert.IsNotNull(groups);
                Assert.IsTrue(PermissionChecker.HasPublicViewPermission("Anything", variantCfg));
                Assert.IsTrue(PermissionChecker.HasUserViewPermission("Anything", variantCfg, groups!));
            }
            finally
            {
                File.Delete(variantPath);
            }
        }

        [TestMethod, TestCategory("Security_RealConfig")]
        public void F_RealConfig_CreateVariant_RestrictedToSingleResource()
        {
            if (!File.Exists(RealConfigPath))
                Assert.Inconclusive($"Skipped: {RealConfigPath} not found.");

            var realCfg   = SecureConfigLoader.LoadFrom(RealConfigPath);
            var variantKey = realCfg.SecretKey;

            // Variant: only "HelloWorld" is public; TeamA gets access to "InternalReport".
            var variantPath = SecureConfigBuilder.WriteTempConfig(
                SecureConfigBuilder.Build(variantKey,
                    SecureConfigBuilder.Admin(),
                    SecureConfigBuilder.ServerPerm(SecureConfigBuilder.PublicGroup, View: false),
                    SecureConfigBuilder.ResourcePerm(SecureConfigBuilder.PublicGroup, "HelloWorld", View: true),
                    SecureConfigBuilder.ResourcePerm("TeamA", "InternalReport", View: true)));
            try
            {
                var variantCfg = SecureConfigLoader.LoadFrom(variantPath);

                // Public access
                Assert.IsTrue( PermissionChecker.HasPublicViewPermission("HelloWorld",     variantCfg));
                Assert.IsFalse(PermissionChecker.HasPublicViewPermission("InternalReport", variantCfg));

                // TeamA token from real config key — validates against variant (same key)
                var tokenTeamA  = JwtTestHelper.ValidToken(variantKey, "TeamA");
                var groupsTeamA = JwtValidator.GetUserGroups(tokenTeamA, variantCfg.SecretKey);

                Assert.IsTrue( PermissionChecker.HasUserViewPermission("InternalReport", variantCfg, groupsTeamA!));
                Assert.IsFalse(PermissionChecker.HasUserViewPermission("AdminDashboard", variantCfg, groupsTeamA!));

                // Token for unknown group
                var tokenOther  = JwtTestHelper.ValidToken(variantKey, "Guests");
                var groupsOther = JwtValidator.GetUserGroups(tokenOther, variantCfg.SecretKey);

                Assert.IsFalse(PermissionChecker.HasUserViewPermission("InternalReport", variantCfg, groupsOther!));
            }
            finally
            {
                File.Delete(variantPath);
            }
        }
    }
}

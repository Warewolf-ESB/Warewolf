/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Execution.Lightweight.Infrastructure;

namespace Warewolf.Execution.Lightweight.Tests.Infrastructure
{
    /// <summary>
    /// Coverage for <see cref="SecurityFlags"/> - the WOLF-8516 JSON app setting that replaced the
    /// three standalone security kill-switch environment variables.
    ///
    /// <para><b>Why this class matters more than its size suggests.</b> The parser fails
    /// <i>safely</i> (any unparseable value =&gt; every flag false), and that safe default is also
    /// its most dangerous property: on a host relying on <c>bypassSecureConfig=true</c>, silently
    /// falling back to false re-enables policy enforcement, so <c>WorkflowAuthPolicyLoader</c>
    /// returns <c>ConfigMissingDeny</c> and <b>every</b> workflow request answers 503
    /// <c>config_missing</c>. WOLF-8512 hit exactly that: a Windows CI agent's <c>az.cmd</c>
    /// stripped the inner double quotes from an inline <c>--settings</c> value, so Azure stored
    /// <c>{bypassSecureConfig:true}</c> - valid-looking, unparseable - and a 1000-message load
    /// test returned 0/1000 with no clue in the pipeline log.</para>
    ///
    /// <see cref="SecurityFlags.FromEnvironment"/> reads process-wide environment state with no
    /// seam to fake, so this class is <see cref="DoNotParallelizeAttribute"/>-marked and restores
    /// the pre-test value in <see cref="Cleanup"/>, matching the convention in
    /// <c>ThreadPoolStartupConfiguratorTests</c>.
    /// </summary>
    [TestClass]
    [DoNotParallelize]
    public class SecurityFlagsTests
    {
        string _original;

        [TestInitialize]
        public void Init() => _original = Environment.GetEnvironmentVariable(SecurityFlags.EnvVar);

        [TestCleanup]
        public void Cleanup() => Environment.SetEnvironmentVariable(SecurityFlags.EnvVar, _original);

        static SecurityFlags Read(string value)
        {
            Environment.SetEnvironmentVariable(SecurityFlags.EnvVar, value);
            return SecurityFlags.FromEnvironment();
        }

        static void AssertAllFalse(SecurityFlags flags, string because)
        {
            Assert.IsFalse(flags.BypassSecureConfig, "BypassSecureConfig should be false " + because);
            Assert.IsFalse(flags.SuperAdminEnabled, "SuperAdminEnabled should be false " + because);
            Assert.IsFalse(flags.SkipFailureToRetrieveSecret, "SkipFailureToRetrieveSecret should be false " + because);
        }

        // -- Absent / blank => safe default --------------------------------------

        [TestMethod]
        [TestCategory("UnitTest")]
        public void FromEnvironment_VariableNotSet_AllFlagsFalse()
            => AssertAllFalse(Read(null), "when the variable is not set");

        [DataTestMethod]
        [TestCategory("UnitTest")]
        [DataRow("")]
        [DataRow("   ")]
        [DataRow("\t")]
        public void FromEnvironment_BlankValue_AllFlagsFalse(string value)
            => AssertAllFalse(Read(value), "for a blank value");

        // -- Well-formed JSON ----------------------------------------------------

        [TestMethod]
        [TestCategory("UnitTest")]
        public void FromEnvironment_AllThreeFlagsTrue_AllParsed()
        {
            var flags = Read("{\"bypassSecureConfig\":true,\"superAdminEnabled\":true,\"skipFailureToRetrieveSecret\":true}");

            Assert.IsTrue(flags.BypassSecureConfig);
            Assert.IsTrue(flags.SuperAdminEnabled);
            Assert.IsTrue(flags.SkipFailureToRetrieveSecret);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void FromEnvironment_ExplicitFalses_AllFalse()
            => AssertAllFalse(
                Read("{\"bypassSecureConfig\":false,\"superAdminEnabled\":false,\"skipFailureToRetrieveSecret\":false}"),
                "when every flag is explicitly false");

        /// <summary>Each flag is independent - setting one must not imply the others.</summary>
        [TestMethod]
        [TestCategory("UnitTest")]
        public void FromEnvironment_OnlyBypassSet_OtherFlagsRemainFalse()
        {
            var flags = Read("{\"bypassSecureConfig\":true}");

            Assert.IsTrue(flags.BypassSecureConfig, "the one flag that was set should be true");
            Assert.IsFalse(flags.SuperAdminEnabled, "an absent flag must default to false, not inherit");
            Assert.IsFalse(flags.SkipFailureToRetrieveSecret, "an absent flag must default to false, not inherit");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void FromEnvironment_OnlySuperAdminSet_OtherFlagsRemainFalse()
        {
            var flags = Read("{\"superAdminEnabled\":true}");

            Assert.IsTrue(flags.SuperAdminEnabled);
            Assert.IsFalse(flags.BypassSecureConfig, "super-admin must never imply the secure.config bypass");
            Assert.IsFalse(flags.SkipFailureToRetrieveSecret);
        }

        /// <summary>
        /// The parser opts into <c>PropertyNameCaseInsensitive</c>, so an operator typing
        /// PascalCase mid-incident still gets the flag they asked for.
        /// </summary>
        [DataTestMethod]
        [TestCategory("UnitTest")]
        [DataRow("{\"BypassSecureConfig\":true}")]
        [DataRow("{\"BYPASSSECURECONFIG\":true}")]
        [DataRow("{\"bypasssecureconfig\":true}")]
        public void FromEnvironment_PropertyNameCasingIgnored_FlagStillParsed(string json)
            => Assert.IsTrue(Read(json).BypassSecureConfig, "casing should not matter: " + json);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void FromEnvironment_UnknownPropertiesIgnored_KnownFlagStillParsed()
        {
            var flags = Read("{\"bypassSecureConfig\":true,\"someFutureFlag\":true,\"note\":\"ignore me\"}");

            Assert.IsTrue(flags.BypassSecureConfig, "an unrecognised sibling property must not discard the whole object");
            Assert.IsFalse(flags.SuperAdminEnabled);
        }

        // -- Malformed => safe default (the WOLF-8512 failure mode) --------------

        /// <summary>
        /// Regression guard for WOLF-8512. This is the <b>exact</b> value read back from
        /// WarewolfServer-UAT after <c>az.cmd</c> stripped the quotes - unquoted property name, so
        /// not JSON. It must degrade to all-false rather than throw, and callers therefore cannot
        /// distinguish "flags are false" from "flags were never set" - which is why the pipeline
        /// now reads the stored value back and asserts that it parses.
        /// </summary>
        [TestMethod]
        [TestCategory("UnitTest")]
        public void FromEnvironment_UnquotedPropertyName_AsMangledByAzCmd_DegradesToAllFalse()
            => AssertAllFalse(Read("{bypassSecureConfig:true}"),
                "for the az.cmd-mangled value {bypassSecureConfig:true} (WOLF-8512)");

        [DataTestMethod]
        [TestCategory("UnitTest")]
        [DataRow("not json at all")]
        [DataRow("{")]
        [DataRow("{\"bypassSecureConfig\":}")]
        [DataRow("{'bypassSecureConfig':true}")]
        [DataRow("{\"bypassSecureConfig\":\"notaboolean\"}")]
        [DataRow("[]")]
        [DataRow("true")]
        public void FromEnvironment_MalformedValue_DegradesToAllFalse(string value)
            => AssertAllFalse(Read(value), "for a malformed value");

        /// <summary>A malformed value must never propagate - this runs on every authorised request.</summary>
        [TestMethod]
        [TestCategory("UnitTest")]
        public void FromEnvironment_MalformedValue_DoesNotThrow()
        {
            Environment.SetEnvironmentVariable(SecurityFlags.EnvVar, "{bypassSecureConfig:true}");

            SecurityFlags.FromEnvironment();
        }

        /// <summary>
        /// Not cached by design - <c>WorkflowAuthPolicyLoader</c> re-reads on every call so an
        /// operator can hot-toggle a flag without restarting the Function App.
        /// </summary>
        [TestMethod]
        [TestCategory("UnitTest")]
        public void FromEnvironment_ValueChangedBetweenCalls_IsNotCached()
        {
            Assert.IsFalse(Read("{\"bypassSecureConfig\":false}").BypassSecureConfig);
            Assert.IsTrue(Read("{\"bypassSecureConfig\":true}").BypassSecureConfig,
                "the flag must be re-read, not served from a cached first parse");
        }
    }
}

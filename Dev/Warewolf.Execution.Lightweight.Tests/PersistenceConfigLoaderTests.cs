/*
 * Tests for PersistenceConfigLoader / LightweightPersistenceSettings — the engine's
 * cold-start hydration of Config.Persistence from the deployable Settings pair:
 *
 *   Settings/persistencesettings.json          (Enable / scheduler / flags)
 *   Settings/persistencesettingsdbsource.bite  (Hangfire SQL DbSource, WFAES-encryptable)
 *
 * Pins:
 *   - missing settings file  → no-op (engine starts, persistence unconfigured)
 *   - Enable=false           → hydrated without requiring the DbSource .bite
 *   - Enable=true + plaintext .bite → payload consumable by HangfireScheduler
 *                               (Dev2JsonSerializer → DbSource → ConnectionString)
 *   - Enable=true + WFAES .bite     → ConnectionString decrypted via AesDecryptHook
 *   - Enable=true + missing .bite   → fail-fast (InvalidOperationException)
 *   - Enable=true + empty ConnectionString → fail-fast
 *   - hydration NEVER writes to the settings directory (read-only package safety)
 */

using Dev2.Common;
using Dev2.Common.Serializers;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;
using Warewolf.Execution.Lightweight.Infrastructure;
using Warewolf.Security.Encryption;

namespace Warewolf.Execution.Lightweight.Tests
{
    [TestClass]
    [DoNotParallelize] // mutates process-global Config.Persistence and DpapiWrapper hooks
    public class PersistenceConfigLoaderTests
    {
        const string SourceId = "6ec3fbc4-2a3b-4c15-9f0e-8d7f3a5c9b21";
        const string SourceName = "persistencesettingsdbsource";
        const string PlainConnectionString =
            "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=WarewolfPersistence;Integrated Security=True";

        PersistenceSettings _savedPersistence = null!;
        Func<string, string>? _savedDecryptHook;
        string _dir = null!;

        [TestInitialize]
        public void Setup()
        {
            _savedPersistence = Config.Persistence;
            _savedDecryptHook = DpapiWrapper.AesDecryptHook;
            _dir = Directory.CreateTempSubdirectory("wwpersistence-").FullName;
        }

        [TestCleanup]
        public void Cleanup()
        {
            Config.Persistence = _savedPersistence;
            DpapiWrapper.AesDecryptHook = _savedDecryptHook;
            try { Directory.Delete(_dir, recursive: true); } catch (IOException) { }
        }

        // ── Test data helpers ─────────────────────────────────────────────────────

        void WriteSettingsJson(bool enable) =>
            File.WriteAllText(Path.Combine(_dir, PersistenceConfigLoader.SettingsFileName),
                $$"""
                {
                  "Enable": {{(enable ? "true" : "false")}},
                  "PersistenceScheduler": "Hangfire",
                  "EncryptDataSource": false,
                  "PrepareSchemaIfNecessary": true,
                  "UseAsServer": false
                }
                """);

        void WriteDbSourceBite(string connectionString) =>
            File.WriteAllText(Path.Combine(_dir, PersistenceConfigLoader.DbSourceFileName),
                $"""
                 <Source ID="{SourceId}" Name="{SourceName}" ResourceType="DbSource" IsValid="false" ConnectionString="{connectionString}" Type="DbSource" ServerType="SqlDatabase" ServerVersion="0.0.0.0" ServerID="51a58300-7e9d-4927-a57b-e5d700b11b55">
                   <DisplayName>{SourceName}</DisplayName>
                   <AuthorRoles></AuthorRoles>
                   <ErrorMessages />
                   <TypeOf>DbSource</TypeOf>
                 </Source>
                 """);

        static DbSource DeserializePayload() =>
            new Dev2JsonSerializer().Deserialize<DbSource>(Config.Persistence.PersistenceDataSource.Payload);

        // ── Scenarios ─────────────────────────────────────────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Initialize_MissingSettingsFile_LeavesConfigUntouched()
        {
            var before = Config.Persistence;

            PersistenceConfigLoader.Initialize(_dir);

            Assert.AreSame(before, Config.Persistence,
                "Without a settings file the engine must start with persistence unconfigured.");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Initialize_EnableFalse_HydratesWithoutRequiringDbSource_AndClearsScheduler()
        {
            WriteSettingsJson(enable: false); // no .bite on disk at all

            PersistenceConfigLoader.Initialize(_dir);

            Assert.IsInstanceOfType(Config.Persistence, typeof(LightweightPersistenceSettings));
            Assert.IsFalse(Config.Persistence.Enable);
            // Disabled persistence must clear the scheduler name in memory:
            // PersistenceScheduler == "Hangfire" makes PersistenceExecution.GetScheduler()
            // eagerly construct a HangfireScheduler (and open SQL storage) inside
            // SuspendExecutionActivity's constructor — which would make any workflow
            // containing a suspend/resume tool fail to PARSE on an unconfigured engine.
            Assert.IsNull(Config.Persistence.PersistenceScheduler,
                "Enable=false must clear PersistenceScheduler so suspend/resume workflows still parse.");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Initialize_EnableTrue_PlaintextBite_BuildsPayloadHangfireSchedulerCanConsume()
        {
            WriteSettingsJson(enable: true);
            WriteDbSourceBite(PlainConnectionString);

            PersistenceConfigLoader.Initialize(_dir);

            Assert.IsTrue(Config.Persistence.Enable);
            Assert.IsFalse(Config.Persistence.EncryptDataSource,
                "The in-memory payload is plaintext — EncryptDataSource must be forced false.");
            Assert.AreEqual(SourceName, Config.Persistence.PersistenceDataSource.Name);
            Assert.AreEqual(Guid.Parse(SourceId), Config.Persistence.PersistenceDataSource.Value);

            // Exactly what HangfireScheduler.ConnectionString does with the payload.
            // DbSource parses the input into fields and re-emits a CANONICAL connection
            // string (Integrated Security=True → SSPI, Connection Timeout appended) —
            // identical to how the Server persists DbSource payloads today.
            var source = DeserializePayload();
            AssertCanonicalLocalDbConnectionString(source.ConnectionString);
            Assert.AreEqual(Guid.Parse(SourceId), source.ResourceID);
        }

        static void AssertCanonicalLocalDbConnectionString(string connectionString)
        {
            StringAssert.Contains(connectionString, @"Data Source=(localdb)\MSSQLLocalDB");
            StringAssert.Contains(connectionString, "Initial Catalog=WarewolfPersistence");
            StringAssert.Contains(connectionString, "Integrated Security=SSPI");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Initialize_EnableTrue_WfAesBite_DecryptsConnectionStringViaHook()
        {
            // Fake WFAES hook (base64 passthrough) — same pattern as ResourceCatalogTests.
            DpapiWrapper.AesDecryptHook = cipher =>
                Encoding.UTF8.GetString(Convert.FromBase64String(cipher["WFAES::".Length..]));
            var encrypted = "WFAES::" + Convert.ToBase64String(Encoding.UTF8.GetBytes(PlainConnectionString));

            WriteSettingsJson(enable: true);
            WriteDbSourceBite(encrypted);

            PersistenceConfigLoader.Initialize(_dir);

            // Canonical field values can only be present if DbSource(XElement) decrypted
            // the WFAES:: attribute through the AES hook before parsing it.
            AssertCanonicalLocalDbConnectionString(DeserializePayload().ConnectionString);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Initialize_EnableTrue_MissingBite_FailsFast()
        {
            WriteSettingsJson(enable: true); // deliberately no .bite

            var ex = Assert.ThrowsException<InvalidOperationException>(
                () => PersistenceConfigLoader.Initialize(_dir));
            StringAssert.Contains(ex.Message, PersistenceConfigLoader.DbSourceFileName);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Initialize_EnableTrue_EmptyConnectionString_FailsFast()
        {
            WriteSettingsJson(enable: true);
            WriteDbSourceBite(string.Empty);

            Assert.ThrowsException<InvalidOperationException>(
                () => PersistenceConfigLoader.Initialize(_dir));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Initialize_NeverWritesToSettingsDirectory()
        {
            WriteSettingsJson(enable: true);
            WriteDbSourceBite(PlainConnectionString);
            var before = Snapshot();

            PersistenceConfigLoader.Initialize(_dir);

            CollectionAssert.AreEqual(before, Snapshot(),
                "Hydration must be read-only — the deployed package directory may not be writable " +
                "(zip-deploy / run-from-package).");

            string[] Snapshot() =>
                Directory.GetFiles(_dir, "*", SearchOption.AllDirectories)
                         .OrderBy(f => f, StringComparer.Ordinal)
                         .Select(f => $"{f}|{new FileInfo(f).Length}|{File.GetLastWriteTimeUtc(f):O}")
                         .ToArray();
        }
    }
}

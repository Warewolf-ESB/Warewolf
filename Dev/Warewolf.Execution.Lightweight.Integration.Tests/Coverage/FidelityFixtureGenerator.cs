/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  One-shot generator for the round-trip fidelity corpus fixtures that no real workflow in the
 *  repository covers.
 *
 *  WHY THIS EXISTS
 *  ---------------
 *  RoundTripFidelityTests measures one verdict per toolbox type, and AssertCorpusCoversEveryTool
 *  now FAILS the build when any type has no corpus sample at all. Five types had none that the
 *  sweep could use:
 *
 *    Decision (legacy) / Redis Cache / Redis Remove   no sample anywhere in the repository
 *    ODBC Database     / SQL Bulk Insert              sampled only from All Tools.xml, which is a
 *                                                     single monolithic workflow containing every
 *                                                     tool - X6ToWorkflowConverter aborts the whole
 *                                                     conversion on the first activity it does not
 *                                                     support, so that file can never convert and
 *                                                     those rows could only ever report
 *                                                     TranslationFailed regardless of whether the
 *                                                     activity itself round-trips.
 *
 *  HOW THE FIXTURES ARE BUILT
 *  --------------------------
 *  The activity graph is composed in C# and serialised with WorkflowHelper.GetXamlDefinition - the
 *  same serialiser the Studio itself saves through - then wrapped in the .bite <Service> envelope by
 *  EnvelopeBiteWriter. It deliberately does NOT go through X6ToWorkflowConverter: a fixture emitted
 *  by the converter under test would be convertible by construction, making TranslationFailed
 *  impossible for these rows and the round-trip assertion partly tautological.
 *
 *  The Redis composition mirrors Warewolf.Tools.Specs\Toolbox\Utility\Redis\Cache\CacheRedisSteps.cs,
 *  which is authoritative for the ActivityFunc<string,bool> nesting that RedisCacheActivity uses to
 *  wrap the inner activity whose result it caches (and which
 *  X6ToWorkflowConverter.EmbedNestedActivitiesIntoRedisCacheActivities rebuilds on the way back).
 *
 *  RUNNING IT
 *  ----------
 *  [Ignore]d on purpose. These fixtures are generated ONCE and the .bite files are committed, so
 *  they are reviewable artefacts rather than build-time output. Remove the [Ignore] locally to
 *  regenerate after an intended change, then commit the result and restore the attribute. It also
 *  carries TestCategory("RoundTripFidelity") so it stays inside the fidelity partition - this
 *  assembly is split between two CI jobs by exactly complementary TestCategory filters, and a new
 *  category here would be picked up by the OTHER job and would overwrite the committed fixtures.
 */

using System;
using System.Activities;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Dev2.Activities;
using Dev2.Activities.RedisCache;
using Dev2.Activities.SelectAndApply;
using Dev2.Activities.RedisRemove;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.DB;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.TO;
using Dev2.Data.Interfaces.Enums;
using Dev2.Utilities;
using Dev2.Data.Decisions.Operations;
using Dev2.Data.SystemTemplates.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Data.Options;
using Warewolf.Execution.Lightweight.Mcp;

namespace Warewolf.Execution.Lightweight.Integration.Tests.Coverage
{
    [TestClass]
    [TestCategory("RoundTripFidelity")]
    public class FidelityFixtureGenerator
    {
        /// <summary>
        /// Host/port the generated Redis fixtures point at. Matches TestRun.ps1's
        /// <c>Start-HostRedisServer</c>, which binds 127.0.0.1:6379 both on the Chocolatey
        /// <c>redis-64</c> Windows-service path (<c>-LegacyWindowsDeps</c>, the one hosted
        /// windows-2022 agents can actually use) and on the docker path.
        /// </summary>
        const string RedisHost = "localhost";
        const string RedisPort = "6379";

        /// <summary>
        /// Stable id shared by the generated Redis source resource and the SourceId on both Redis
        /// activities. Hard-coded rather than generated so re-running this generator does not
        /// produce a gratuitous diff on every fixture.
        /// </summary>
        static readonly Guid RedisSourceId = new("6c2f5f4e-9d1a-4f27-9d3e-2a5b8c7e1d40");

        /// <summary>
        /// Host/credentials the generated SQL Server fixture points at. Matches TestRun.ps1's
        /// <c>Start-HostMSSQLServer</c> (the native-Windows path hosted windows-2022 agents use),
        /// which provisions login <c>testUser</c>/<c>Ex@mple!23Secure#PWD</c> against database
        /// <c>Dev2TestingDB</c> on localhost:1433, and its deterministic seed which creates
        /// <c>dbo.FidelityPing</c> — a parameterless stored procedure guaranteed present even when
        /// no .bak/.bacpac fixture is available.
        ///
        /// Deliberately NOT the committed 'Resources - ServerTests\Resources\Sources\Database\
        /// NewSqlServerSource.bite': that source's ConnectionString is a DPAPI-encrypted blob from
        /// a 2020 dev machine, and DPAPI ciphertext cannot be decrypted on a different machine/user
        /// — on any other host it deserialises to unparseable garbage, which crashes
        /// Microsoft.Data.SqlClient deep inside SqlConnection's endpoint-detection with an
        /// unhandled IndexOutOfRangeException on an empty Data Source, rather than failing cleanly.
        /// Written here as a fresh, portable, PLAINTEXT source for the same reason RedisSource is
        /// plaintext below — see WriteRedisSource's remarks.
        ///
        /// <para>
        /// <c>TrustServerCertificate=True</c> rather than <c>Encrypt=False</c>, which is not a
        /// stylistic choice: a connection string does not survive DbSource intact. Its SETTER
        /// parses the string into Server/DatabaseName/UserID/Password/Port/ConnectionTimeout/
        /// TrustServerCertificate, and its GETTER rebuilds one from exactly those properties - so
        /// any keyword DbSource does not model is silently dropped the moment the source is loaded
        /// from disk, <c>Encrypt</c> included. Microsoft.Data.SqlClient then defaults Encrypt=true
        /// and the login handshake fails against Start-HostMSSQLServer's self-signed certificate.
        /// Measured, not theorised: with <c>Encrypt=False</c> here the SQL Bulk Insert row reported
        /// "A connection was successfully established with the server, but then an error occurred
        /// during the login process. (provider: SSL Provider, error: 0 - The certificate chain was
        /// issued by an authority that is not trusted.)" - it had resolved its source and reached
        /// the server, and fell over on TLS alone. TrustServerCertificate IS modelled, so it
        /// survives that parse/rebuild and re-emits, keeping the connection encrypted while
        /// skipping chain validation.
        /// </para>
        /// </summary>
        internal const string MssqlConnectionString =
            "Data Source=localhost,1433;Initial Catalog=Dev2TestingDB;User ID=testUser;Password=Ex@mple!23Secure#PWD;TrustServerCertificate=True;";

        static readonly Guid MssqlSourceId = new("8a3f1c2d-5e6b-4a90-9c1f-7b2d4e8a6f30");

        static string FixtureRoot
        {
            get
            {
                var devRoot = RoundTripFidelityCorpus.FindRepoDevRoot();
                Assert.IsNotNull(devRoot, "Could not locate the Dev/ root — run from a full checkout.");
                return Path.Combine(devRoot, "Warewolf.Execution.Lightweight", "Resources", "tools");
            }
        }

        [TestMethod]
        [Ignore("Generates committed corpus fixtures. Remove [Ignore] locally to regenerate, then commit the .bite files and restore it.")]
        public void Generate_MissingCorpusFixtures()
        {
            var written = new List<string>
            {
                WriteRedisSource(),
                WriteWorkflow("redis cache", "Fidelity_RedisCache", BuildRedisCacheStep(), RedisDataList()),
                WriteWorkflow("redis remove", "Fidelity_RedisRemove", BuildRedisRemoveStep(), RedisDataList()),
                WriteWorkflow("decision legacy", "Fidelity_DecisionLegacy", BuildLegacyDecisionStep(), DecisionDataList()),
                WriteWorkflow("odbc database", "Fidelity_OdbcDatabase", BuildOdbcStep(), SimpleDataList("result")),
                WriteMssqlSource("sql bulk insert"),
                WriteWorkflow("sql bulk insert", "Fidelity_SqlBulkInsert", BuildSqlBulkInsertStep(), SqlBulkInsertDataList()),
                WriteMssqlSource("sql server database"),
                WriteWorkflow("sql server database", "Fidelity_SqlServerDatabase", BuildSqlServerStep(), SimpleDataList("result")),
                WriteWorkflow("suspend execution", "Fidelity_SuspendExecution", BuildSuspendExecutionStep(), SuspendExecutionDataList()),
                WriteWorkflow("gate", "Fidelity_Gate", BuildGateStep(), EmptyDataList()),
                WriteWorkflow("select and apply", "Fidelity_SelectAndApply", BuildSelectAndApplyStep(), SelectAndApplyDataList()),
                WriteWorkflow("length", "Fidelity_Length", BuildLengthStep(), LengthDataList()),
                WriteWorkflow("delete records", "Fidelity_DeleteRecords", BuildDeleteRecordsStep(), DeleteRecordsDataList()),
                WriteWorkflow("service", "Fidelity_ServiceTarget", BuildServiceTargetStep(), ServiceTargetDataList()),
                WriteWorkflow("service", "Fidelity_Service", BuildServiceStep(), ServiceDataList()),
            };

            foreach (var path in written)
            {
                Console.WriteLine("wrote " + path);
                Assert.IsTrue(File.Exists(path), "generator reported writing " + path + " but it is not there");
            }
        }

        /// <summary>
        /// Guards the specific gap the committed <c>Fidelity_SuspendExecution.bite</c> fixture
        /// exists to avoid (see <see cref="BuildSuspendExecutionStep"/>'s remarks): the only real
        /// corpus sample for Suspend Execution also embeds a legacy
        /// <c>Unlimited.Applications.BusinessDesignStudio.Activities.DsfActivity</c> step, which
        /// <c>X6ToWorkflowConverter</c> does not support and which aborts conversion of the whole
        /// workflow — reported as <c>TranslationFailed</c> for a reason unrelated to Suspend
        /// Execution's own fidelity. This composes the same fixture the generator commits and
        /// proves, without touching disk or the real corpus, that the composed XAML contains
        /// SuspendExecutionActivity, contains no legacy DsfActivity step, and survives a full
        /// XAML → X6 JSON → XAML round trip — the exact pipeline
        /// <see cref="RoundTripFidelityTests"/> exercises — without throwing.
        /// </summary>
        [TestMethod]
        public void BuildSuspendExecutionStep_ComposesFixtureWithoutLegacyDsfActivity_AndRoundTripsCleanly()
        {
            var xaml = BuildXaml(BuildSuspendExecutionStep(), "Fidelity_SuspendExecution");
            var xamlText = xaml.ToString();

            StringAssert.Contains(xamlText, "SuspendExecutionActivity",
                "the fixture must actually exercise SuspendExecutionActivity");
            Assert.IsFalse(xamlText.Contains(":DsfActivity "),
                "the fixture must not contain the legacy DsfActivity sub-workflow invocation step " +
                "that makes the real hangfiredemo sample TranslationFailed");

            string roundTripped;
            try
            {
                roundTripped = X6RoundTripBridge.RoundTripXaml(xaml);
            }
            catch (Exception ex)
            {
                Assert.Fail("Round-tripping the Suspend Execution fixture through the X6 converters " +
                            "threw " + ex.GetType().Name + ": " + ex.Message +
                            " — this is exactly the TranslationFailed regression this fixture exists to prevent.");
                return;
            }

            StringAssert.Contains(roundTripped, "SuspendExecutionActivity",
                "SuspendExecutionActivity must survive the round trip, not be dropped or replaced");
        }

        /// <summary>
        /// Guards the specific gap the committed <c>Fidelity_Gate.bite</c> fixture exists to
        /// avoid (see <see cref="BuildGateStep"/>'s remarks): proves the empty-<c>Conditions</c>
        /// composition actually round-trips through the same XAML → X6 JSON → XAML pipeline
        /// <see cref="RoundTripFidelityTests"/> exercises, and that <c>Conditions</c> survives as
        /// an empty (not null) list — a null would make <c>GateActivity.Passing</c> throw and be
        /// swallowed into a false "gate conditions failed" result, defeating the fixture's whole
        /// point.
        /// </summary>
        [TestMethod]
        public void BuildGateStep_ComposesEmptyConditionsFixture_AndRoundTripsCleanly()
        {
            var xaml = BuildXaml(BuildGateStep(), "Fidelity_Gate");
            var xamlText = xaml.ToString();

            StringAssert.Contains(xamlText, "GateActivity",
                "the fixture must actually exercise GateActivity");

            string roundTripped;
            try
            {
                roundTripped = X6RoundTripBridge.RoundTripXaml(xaml);
            }
            catch (Exception ex)
            {
                Assert.Fail("Round-tripping the Gate fixture through the X6 converters threw " +
                            ex.GetType().Name + ": " + ex.Message +
                            " — this is exactly the TranslationFailed regression this fixture exists to prevent.");
                return;
            }

            StringAssert.Contains(roundTripped, "GateActivity",
                "GateActivity must survive the round trip, not be dropped or replaced");
        }

        /// <summary>
        /// Guards the specific gap the committed <c>Fidelity_SelectAndApply.bite</c> fixture exists
        /// to avoid (see <see cref="BuildSelectAndApplyStep"/>'s remarks), and one no other fixture
        /// test covers: this is the only toolbox row whose round trip has to survive being taken
        /// APART. <c>WorkflowToX6Converter.ProcessSelectAndApplyNestedActivities</c> flattens
        /// <c>ApplyActivityFunc.Handler</c> into a separate X6 cell tagged isNested/parentId, and
        /// <c>X6ToWorkflowConverter.EmbedNestedActivitiesIntoSelectAndApplyActivities</c> rebuilds
        /// the ActivityFunc from that cell hierarchy on the way back. Proves the composed fixture
        /// carries a nested handler, that the full XAML → X6 JSON → XAML pipeline
        /// <see cref="RoundTripFidelityTests"/> exercises does not throw, and that the handler is
        /// still there afterwards — a drop would otherwise surface in the sweep only indirectly, as
        /// an empty <c>[[applied]]</c> output.
        /// </summary>
        [TestMethod]
        public void BuildSelectAndApplyStep_ComposesNestedFixture_AndRoundTripsCleanly()
        {
            var xaml = BuildXaml(BuildSelectAndApplyStep(), "Fidelity_SelectAndApply");
            var xamlText = xaml.ToString();

            StringAssert.Contains(xamlText, "DsfSelectAndApplyActivity",
                "the fixture must actually exercise DsfSelectAndApplyActivity");
            StringAssert.Contains(xamlText, "Assign applied item",
                "the nested ApplyActivityFunc handler must be serialised, not dropped at compose time");

            string roundTripped;
            try
            {
                roundTripped = X6RoundTripBridge.RoundTripXaml(xaml);
            }
            catch (Exception ex)
            {
                Assert.Fail("Round-tripping the Select and apply fixture through the X6 converters threw " +
                            ex.GetType().Name + ": " + ex.Message +
                            " — this is exactly the TranslationFailed regression this fixture exists to prevent.");
                return;
            }

            StringAssert.Contains(roundTripped, "DsfSelectAndApplyActivity",
                "DsfSelectAndApplyActivity must survive the round trip, not be dropped or replaced");
            StringAssert.Contains(roundTripped, "Assign applied item",
                "the nested handler must be rebuilt by EmbedNestedActivitiesIntoSelectAndApplyActivities — " +
                "losing it is the fidelity gap this row exists to catch");
        }

        /// <summary>
        /// Guards the specific gap the committed <c>Fidelity_Length.bite</c> fixture exists to
        /// avoid (see <see cref="BuildLengthStep"/>'s remarks) and, beyond it, the one detail of
        /// this row the sweep structurally cannot see. <c>TreatNullAsZero</c> is the only piece of
        /// Length's state that changes behaviour without changing the composed graph, and on the
        /// happy path the fixture seeds its own recordset, so a converter that silently dropped the
        /// flag back to the constructor's <c>true</c> would still produce <c>[[len]]</c> = 2 on
        /// both sides and be scored Pass. Asserted here instead, on the false the fixture
        /// deliberately carries: the value that is NOT the default, and therefore the only one
        /// whose survival is evidence of anything.
        /// </summary>
        [TestMethod]
        public void BuildLengthStep_ComposesSeededFixture_AndRoundTripsCleanly()
        {
            var xaml = BuildXaml(BuildLengthStep(), "Fidelity_Length");
            var xamlText = xaml.ToString();

            StringAssert.Contains(xamlText, "DsfRecordsetNullhandlerLengthActivity",
                "the fixture must actually exercise DsfRecordsetNullhandlerLengthActivity");
            StringAssert.Contains(xamlText, "Seed rows",
                "the seeding Assign must be serialised - without it Length has no recordset to " +
                "measure and the fixture is no better than the corpus samples it replaces");
            StringAssert.Contains(xamlText, "TreatNullAsZero=\"False\"",
                "the fixture must carry the non-default TreatNullAsZero=false, the only value " +
                "whose survival across the round trip proves anything");

            string roundTripped;
            try
            {
                roundTripped = X6RoundTripBridge.RoundTripXaml(xaml);
            }
            catch (Exception ex)
            {
                Assert.Fail("Round-tripping the Length fixture through the X6 converters threw " +
                            ex.GetType().Name + ": " + ex.Message +
                            " - this is exactly the TranslationFailed regression this fixture exists to prevent.");
                return;
            }

            StringAssert.Contains(roundTripped, "DsfRecordsetNullhandlerLengthActivity",
                "DsfRecordsetNullhandlerLengthActivity must survive the round trip, not be dropped or replaced");
            StringAssert.Contains(roundTripped, "Seed rows",
                "the seeding Assign must survive the round trip - losing it would put the row " +
                "straight back on the null-recordset error this fixture exists to get past");
            StringAssert.Contains(roundTripped, "TreatNullAsZero=\"False\"",
                "TreatNullAsZero must survive as false - X6ToWorkflowConverter.CreateRecordsetLengthActivity " +
                "news up the activity (whose constructor sets it TRUE) and relies on FromX6Json to " +
                "read Constants.LENGTH_TREATNULLASZERO back, so a regression there silently flips it");
        }

        /// <summary>
        /// Guards the committed <c>Fidelity_DeleteRecords.bite</c> fixture (see
        /// <see cref="BuildDeleteRecordsStep"/>'s remarks) and, as with Length, the one detail of
        /// the row the sweep structurally cannot see. A dropped <c>TreatNullAsZero</c> reverts to
        /// the constructor's <c>true</c>, and on this fixture's happy path - where the seed ran and
        /// the recordset IS there - the flag is never consulted, so <c>[[deleted]]</c> is "Success"
        /// either way and the payload comparison reports Pass over the loss. Asserted here on the
        /// false the fixture deliberately carries, the value that is not the default.
        /// </summary>
        [TestMethod]
        public void BuildDeleteRecordsStep_ComposesSeededFixture_AndRoundTripsCleanly()
        {
            var xaml = BuildXaml(BuildDeleteRecordsStep(), "Fidelity_DeleteRecords");
            var xamlText = xaml.ToString();

            StringAssert.Contains(xamlText, "DsfDeleteRecordNullHandlerActivity",
                "the fixture must exercise the descriptor-carrying Delete tool, not the " +
                "descriptor-less DsfDeleteRecordActivity");
            StringAssert.Contains(xamlText, "Seed rows",
                "the seeding Assign must be serialised - without it Delete has no recordset to " +
                "remove and the fixture is no better than the corpus samples it replaces");
            StringAssert.Contains(xamlText, "TreatNullAsZero=\"False\"",
                "the fixture must carry the non-default TreatNullAsZero=false, the only value " +
                "whose survival across the round trip proves anything");

            string roundTripped;
            try
            {
                roundTripped = X6RoundTripBridge.RoundTripXaml(xaml);
            }
            catch (Exception ex)
            {
                Assert.Fail("Round-tripping the Delete Records fixture through the X6 converters threw " +
                            ex.GetType().Name + ": " + ex.Message +
                            " - this is exactly the TranslationFailed regression this fixture exists to prevent.");
                return;
            }

            StringAssert.Contains(roundTripped, "DsfDeleteRecordNullHandlerActivity",
                "DsfDeleteRecordNullHandlerActivity must survive the round trip, not be dropped or replaced");
            StringAssert.Contains(roundTripped, "Seed rows",
                "the seeding Assign must survive the round trip - losing it would put the row " +
                "straight back on the null-recordset error this fixture exists to get past");
            StringAssert.Contains(roundTripped, "TreatNullAsZero=\"False\"",
                "TreatNullAsZero must survive as false - X6ToWorkflowConverter.CreateDsfDeleteRecordNullHandlerActivity " +
                "news up the activity (whose constructor sets it TRUE) and relies on FromX6Json to " +
                "read Constants.DELETERECORDS_TREATNULLASZERO back, so a regression there silently flips it");
        }

        /// <summary>
        /// Guards the committed <c>Fidelity_SqlBulkInsert.bite</c> fixture (see
        /// <see cref="BuildSqlBulkInsertStep"/>'s remarks). Unlike every other fixture test here,
        /// the interesting part is not whether the ACTIVITY survives but whether its
        /// <c>InputMappings</c> do: <c>ToX6Json</c> hands the live
        /// <c>IList&lt;DataColumnMapping&gt;</c> straight to the cell, and
        /// <c>CommonHelper.TryGetInputMappings</c> has two completely different code paths for
        /// getting it back - a by-reference branch (<c>raw is IList&lt;DataColumnMapping&gt;</c>)
        /// and a JSON branch that rebuilds every DataColumnMapping and DbColumn by hand, including
        /// mapping <c>DbColumn.DataType</c> back from a type-NAME string through a switch.
        ///
        /// <para>
        /// <see cref="X6RoundTripBridge.RoundTripXaml"/> serialises the X6 model to real JSON text
        /// between the two converters, so it is the JSON branch that runs here - the one nothing
        /// else in the suite exercises. A mapping list that came back empty, short, or with its
        /// column types collapsed would leave <c>BuildDataTableToInsert</c> assembling a different
        /// DataTable on the round-tripped side, which is the whole failure mode this row exists to
        /// detect.
        /// </para>
        /// </summary>
        [TestMethod]
        public void BuildSqlBulkInsertStep_ComposesMappedFixture_AndRoundTripsCleanly()
        {
            var xaml = BuildXaml(BuildSqlBulkInsertStep(), "Fidelity_SqlBulkInsert");
            var xamlText = xaml.ToString();

            StringAssert.Contains(xamlText, "DsfSqlBulkInsertActivity",
                "the fixture must actually exercise DsfSqlBulkInsertActivity");
            StringAssert.Contains(xamlText, "Seed rows",
                "the seeding Assign must be serialised - without it there is nothing to insert");
            StringAssert.Contains(xamlText, MssqlSourceId.ToString(),
                "the fixture must reference the generated SQL Server source by ResourceID - that " +
                "id is all FromX6Json gets back, and all AmbientSourceLoader has to resolve");

            // Database is deliberately NOT asserted on the round-tripped side.
            // DsfSqlBulkInsertActivity.FromX6Json does not rebuild a DbSource from the id; it LOOKS
            // THE SOURCE UP (ResourceCatalog, then AmbientSourceLoader.EnsureSourceLoaded) and
            // assigns whatever it finds - null included. Nothing indexes a source directory in this
            // test, so Database comes back {x:Null} here by design, not by defect. That resolution
            // is environmental, and proving it is precisely the sweep's job: it calls
            // EnsureIndexed on the sample's own directory, which is why the generator writes a copy
            // of the SQL Server source into this fixture's folder.

            string roundTripped;
            try
            {
                roundTripped = X6RoundTripBridge.RoundTripXaml(xaml);
            }
            catch (Exception ex)
            {
                Assert.Fail("Round-tripping the SQL Bulk Insert fixture through the X6 converters threw " +
                            ex.GetType().Name + ": " + ex.Message +
                            " - this is exactly the TranslationFailed regression this fixture exists to prevent.");
                return;
            }

            StringAssert.Contains(roundTripped, "DsfSqlBulkInsertActivity",
                "DsfSqlBulkInsertActivity must survive the round trip, not be dropped or replaced");
            StringAssert.Contains(roundTripped, "Seed rows",
                "the seeding Assign must survive the round trip");
            StringAssert.Contains(roundTripped, BulkInsertTableName,
                "TableName must survive - a lost destination table is an immediate execution failure");
            foreach (var mapping in BulkInsertMappings())
            {
                StringAssert.Contains(roundTripped, mapping.InputColumn,
                    "input expression " + mapping.InputColumn + " must survive CommonHelper.TryGetInputMappings' " +
                    "JSON branch - a dropped mapping silently inserts fewer columns");
                StringAssert.Contains(roundTripped, mapping.OutputColumn.ColumnName,
                    "target column " + mapping.OutputColumn.ColumnName + " must survive - it names both " +
                    "the DataTable column and the SqlBulkCopyColumnMapping");
            }

            Assert.AreEqual(BulkInsertMappings().Count, CountOccurrences(roundTripped, "DataType=\"x:String\""),
                "every mapped column must come back typed as a string. DbColumn.SqlDataType's setter " +
                "overwrites DataType and TryGetInputMappings restores SqlDataType FIRST, so a column " +
                "whose type does not survive does not throw - it silently becomes Int64 (BigInt is " +
                "SqlDbType 0) and SqlBulkCopy is handed a different DataTable on the round-tripped " +
                "side only. See BulkInsertMappings' remarks.");
            Assert.AreEqual(BulkInsertMappings().Count, CountOccurrences(roundTripped, "SqlDataType=\"VarChar\""),
                "SqlDataType is the property that actually carries the column type across the round " +
                "trip; DataType is derived from it on the way back");
        }

        /// <summary>
        /// Guards the committed <c>Fidelity_Service.bite</c> fixture (see
        /// <see cref="BuildServiceStep"/>'s remarks). The thing most likely to be lost here is the
        /// callee's identity: <c>DsfWorkflowActivity.ResourceID</c> is an
        /// <c>InArgument&lt;Guid&gt;</c>, which <c>ToX6Json</c> flattens to
        /// <c>ResourceID?.Expression?.ToString()</c> and <c>FromX6Json</c> rebuilds through
        /// <c>TryGetGuid</c> - two lossy-looking hops either side of the JSON, and if the id does
        /// not come back the round-tripped copy falls through to the ServiceName fallback in
        /// <c>WorkflowResourceCache.Resolve</c> (or to no callee at all). Both are asserted, since
        /// the fallback masks the loss of the primary key.
        /// </summary>
        [TestMethod]
        public void BuildServiceStep_ComposesSubWorkflowFixture_AndRoundTripsCleanly()
        {
            var xaml = BuildXaml(BuildServiceStep(), "Fidelity_Service");
            var xamlText = xaml.ToString();

            StringAssert.Contains(xamlText, "DsfWorkflowActivity",
                "the fixture must exercise DsfWorkflowActivity - the toolbox's Service row - and " +
                "NOT the legacy DsfActivity that X6ToWorkflowConverter cannot convert at all");
            StringAssert.Contains(xamlText, ServiceTargetId.ToString(),
                "the caller must name its callee by ResourceID");

            string roundTripped;
            try
            {
                roundTripped = X6RoundTripBridge.RoundTripXaml(xaml);
            }
            catch (Exception ex)
            {
                Assert.Fail("Round-tripping the Service fixture through the X6 converters threw " +
                            ex.GetType().Name + ": " + ex.Message +
                            " - this is exactly the TranslationFailed regression this fixture exists to prevent.");
                return;
            }

            StringAssert.Contains(roundTripped, "DsfWorkflowActivity",
                "DsfWorkflowActivity must survive the round trip, not be dropped or replaced");
            StringAssert.Contains(roundTripped, ServiceTargetId.ToString(),
                "the callee's ResourceID must survive. It is the PRIMARY key " +
                "WorkflowResourceCache.Resolve matches on, and it makes two lossy-looking hops - " +
                "InArgument<Guid> flattened to Expression.ToString() on the way out, TryGetGuid on " +
                "the way back. Losing it does not fail loudly: resolution silently falls through to " +
                "the ServiceName fallback");
            StringAssert.Contains(roundTripped, ServiceTargetName,
                "the callee's ServiceName must survive - it is Resolve's fallback key, and the only " +
                "one left if the ResourceID hop ever regresses");
            StringAssert.Contains(roundTripped, "[[echo]]",
                "the output mapping must survive - it is what carries the sub-workflow's result " +
                "into the payload the sweep compares, and a dropped mapping would leave both sides " +
                "agreeing on an empty result while proving nothing");
        }

        /// <summary>Non-overlapping occurrence count - <c>string.Split</c> semantics, no regex.</summary>
        static int CountOccurrences(string haystack, string needle)
        {
            var count = 0;
            for (var i = haystack.IndexOf(needle, StringComparison.Ordinal);
                 i >= 0;
                 i = haystack.IndexOf(needle, i + needle.Length, StringComparison.Ordinal))
            {
                count++;
            }
            return count;
        }

        // ── Activity composition ──────────────────────────────────────────────

        static FlowStep BuildRedisCacheStep() => new()
        {
            Action = new RedisCacheActivity
            {
                DisplayName = "Redis Cache",
                SourceId = RedisSourceId,
                Key = "fidelity_key",
                TTL = 5,
                Result = "[[cacheResult]]",
                // The inner activity is the .NET Assign, NOT the legacy DsfMultiAssignActivity the
                // Redis specs compose. X6ToWorkflowConverter supports only the DotNet variant, so a
                // legacy handler makes the whole workflow unconvertible and this row reports
                // TranslationFailed for a reason that has nothing to do with Redis - the same gap
                // that made "Select and apply" TranslationFailed via DsfMultiAssignObjectActivity.
                ActivityFunc = new ActivityFunc<string, bool>
                {
                    Handler = new DsfDotNetMultiAssignActivity
                    {
                        DisplayName = "Assign cached value",
                        FieldsCollection = new List<ActivityDTO>
                        {
                            new("[[cached]]", "fidelity value", 1),
                        },
                    },
                },
            },
        };

        static FlowStep BuildRedisRemoveStep() => new()
        {
            Action = new RedisRemoveActivity
            {
                DisplayName = "Redis Remove",
                SourceId = RedisSourceId,
                Key = "fidelity_key",
                Result = "[[removeResult]]",
            },
        };

        /// <summary>
        /// The legacy <see cref="DsfDecision"/>, not <c>DsfFlowDecisionActivity</c> — the two are
        /// separate toolbox rows and only the flow variant had corpus coverage.
        /// </summary>
        static FlowStep BuildLegacyDecisionStep() => new()
        {
            Action = new DsfDecision
            {
                DisplayName = "If [[a]] = 1",
                Conditions = new Dev2DecisionStack
                {
                    TheStack = new List<Dev2Decision>
                    {
                        new()
                        {
                            Col1 = "[[a]]",
                            Col2 = "1",
                            Col3 = string.Empty,
                            EvaluationFn = enDecisionType.IsEqual,
                        },
                    },
                    TrueArmText = "Yes",
                    FalseArmText = "No",
                    DisplayText = "If [[a]] = 1",
                },
            },
        };

        static FlowStep BuildOdbcStep() => new()
        {
            Action = new DsfODBCDatabaseActivity
            {
                DisplayName = "ODBC Database",
                CommandText = "select 1",
                CommandTimeout = 30,
            },
        };

        /// <summary>
        /// The destination table the fixture inserts into, created and emptied by TestRun.ps1's
        /// <c>Start-HostMSSQLServer</c> deterministic seed alongside <c>dbo.FidelityPing</c>. Its
        /// columns are all varchar, NOT NULL and non-identity on purpose - see
        /// <see cref="BulkInsertMappings"/>.
        /// </summary>
        internal const string BulkInsertTableName = "dbo.FidelityBulkTarget";

        /// <summary>
        /// The column mappings the fixture inserts through, shared by
        /// <see cref="BuildSqlBulkInsertStep"/> and the test that asserts they survive the round
        /// trip, so the two can never describe different columns.
        ///
        /// <para>
        /// Both target columns are <c>varchar NOT NULL</c> and non-identity, which sidesteps every
        /// throw-branch in <c>DsfSqlBulkInsertActivity.BuildDataTableToInsert</c>: a null value in a
        /// non-nullable column, an identity column with KeepIdentity off, and an identity column
        /// with KeepIdentity on all raise before the insert is attempted, and none of them would
        /// say anything about round-trip fidelity. <c>MaxLength</c> matches the seed exactly
        /// because BuildDataTableToInsert copies it onto the DataColumn for string types.
        /// </para>
        ///
        /// <para>
        /// <c>InputColumn</c> uses the STAR form <c>[[rows(*).id]]</c>, which is what makes this a
        /// bulk insert at all: <c>GetIteratorsFromInputMappings</c> turns each InputColumn into a
        /// WarewolfIterator, and only the star form walks every row. Measured, not assumed: with
        /// <c>[[rows().id]]</c> the fixture reported Success and inserted exactly ONE row per
        /// execution - the round trip was faithful and the row still passed, which is precisely how
        /// a fixture can look green while proving less than its remarks claim.
        /// </para>
        ///
        /// <para>
        /// The column type is set through <c>SqlDataType</c> and NOT through <c>DataType</c>, which
        /// looks redundant and is not. <c>DbColumn.SqlDataType</c>'s SETTER assigns
        /// <c>DataType = ConvertSqlDbType(value)</c>, and <c>SqlDbType.BigInt</c> is 0 - so a
        /// DbColumn built by assigning DataType alone carries an unset SqlDataType that serialises
        /// as BigInt, and <c>CommonHelper.TryGetInputMappings</c> restores SqlDataType BEFORE
        /// DataType on the way back, clobbering <c>typeof(string)</c> with <c>typeof(long)</c>.
        /// Measured, not theorised: composing these mappings with <c>DataType = typeof(string)</c>
        /// produced <c>DataType="x:Int64" SqlDataType="BigInt"</c> in the round-tripped XAML, which
        /// would have handed SqlBulkCopy a DataTable of Int64 columns on the round-tripped side
        /// only. Driving SqlDataType sets both consistently and is what the Studio itself does,
        /// since it populates DbColumns from SQL Server metadata.
        /// </para>
        /// </summary>
        static List<DataColumnMapping> BulkInsertMappings() => new()
        {
            new DataColumnMapping
            {
                IndexNumber = 1,
                InputColumn = "[[rows(*).id]]",
                OutputColumn = new DbColumn
                {
                    ColumnName = "Id",
                    SqlDataType = SqlDbType.VarChar,
                    MaxLength = 20,
                    IsNullable = false,
                    IsAutoIncrement = false,
                },
            },
            new DataColumnMapping
            {
                IndexNumber = 2,
                InputColumn = "[[rows(*).description]]",
                OutputColumn = new DbColumn
                {
                    ColumnName = "Description",
                    SqlDataType = SqlDbType.VarChar,
                    MaxLength = 50,
                    IsNullable = false,
                    IsAutoIncrement = false,
                },
            },
        };

        /// <summary>
        /// The original fixture for this row was built to answer one question only - can
        /// DsfSqlBulkInsertActivity be CONVERTED - because its only real corpus sample was
        /// 'Resources - Load\Resources\All Tools.xml', the monolithic every-tool workflow that can
        /// never convert (see the file header). It carried nothing but a TableName, so the first
        /// line of <c>DoInsertForSqlServer</c>'s caller - <c>Database.ResourceID</c> - dereferenced
        /// null and the row reported PassBothFailedIdentically on "Object reference not set to an
        /// instance of an object", never reaching SQL Server at all.
        ///
        /// <para>
        /// This composition makes it EXECUTE. An Assign seeds two rows into <c>[[rows()]]</c>, and
        /// the activity bulk-copies them into <see cref="BulkInsertTableName"/> through
        /// <see cref="BulkInsertMappings"/>, reporting "Success" into <c>[[result]]</c> - the one
        /// Output the sweep's payload comparison reads.
        /// </para>
        ///
        /// <para>
        /// <c>Database</c> carries the <see cref="MssqlSourceId"/> and little else, deliberately.
        /// The embedded DbSource is a REFERENCE, not a connection: both
        /// <c>DsfSqlBulkInsertActivity.ExecuteTool</c> and its <c>FromX6Json</c> resolve the real
        /// source by <c>Database.ResourceID</c> through ResourceCatalog / AmbientSourceLoader and
        /// use THAT object's ConnectionString. Spelling out a second connection string here would
        /// duplicate <see cref="MssqlConnectionString"/> with no effect on execution.
        /// </para>
        ///
        /// <para>
        /// That resolution is also why <see cref="Generate_MissingCorpusFixtures"/> writes a copy of
        /// the SQL Server source into this fixture's OWN folder rather than reusing the one beside
        /// Fidelity_SqlServerDatabase. The sweep indexes exactly one directory per sample - the
        /// sample's own (RoundTripFidelityTests: <c>EnsureIndexed(Path.GetDirectoryName(samplePath))</c>).
        /// Fidelity_RedisRemove gets away with resolving a source from a sibling folder only
        /// because LightweightSourceLoader's registration map is keyed by ResourceID across the
        /// whole sweep and the Redis Cache row happens to run first; that is iteration order, not a
        /// guarantee, and one row depending on it is enough.
        /// </para>
        ///
        /// <para>
        /// The fixture INSERTs, which makes it the only one here with an effect outside its temp
        /// sandbox. The compared payload is the activity's own "Success", not a row count, so
        /// accumulated rows cannot change the verdict - and the seed TRUNCATEs the table anyway.
        /// </para>
        /// </summary>
        static FlowStep BuildSqlBulkInsertStep()
        {
            var bulkInsertStep = new FlowStep
            {
                Action = new DsfSqlBulkInsertActivity
                {
                    DisplayName = "SQL Bulk Insert",
                    TableName = BulkInsertTableName,
                    Result = "[[result]]",
                    Database = new DbSource
                    {
                        ResourceID = MssqlSourceId,
                        ResourceName = "Fidelity SQL Server Source",
                        ServerType = enSourceType.SqlDatabase,
                    },
                    InputMappings = BulkInsertMappings(),
                },
            };

            return new FlowStep
            {
                Action = new DsfDotNetMultiAssignActivity
                {
                    DisplayName = "Seed rows",
                    FieldsCollection = new List<ActivityDTO>
                    {
                        new("[[rows(1).id]]", "fidelity-1", 1),
                        new("[[rows(1).description]]", "alpha", 2),
                        new("[[rows(2).id]]", "fidelity-2", 3),
                        new("[[rows(2).description]]", "beta", 4),
                    },
                },
                Next = bulkInsertStep,
            };
        }

        /// <summary>
        /// Calls dbo.FidelityPing (see MssqlConnectionString remarks) — a real, parameterless
        /// stored procedure guaranteed present by Start-HostMSSQLServer's deterministic seed, so
        /// this fixture executes against actual SQL Server logic rather than merely resolving a
        /// source. Outputs is set (empty, not null) only to satisfy DsfSqlServerDatabaseActivity's
        /// null guard — exact column mapping is not needed for round-trip fidelity, only that both
        /// the original and round-tripped executions succeed identically.
        /// </summary>
        static FlowStep BuildSqlServerStep() => new()
        {
            Action = new DsfSqlServerDatabaseActivity
            {
                DisplayName = "SQL Server Database",
                SourceId = MssqlSourceId,
                ActionName = "dbo.FidelityPing",
                ProcedureName = "dbo.FidelityPing",
                Inputs = new List<IServiceInput>(),
                Outputs = new List<IServiceOutputMapping>(),
            },
        };

        /// <summary>
        /// The only real corpus sample containing <c>SuspendExecutionActivity</c> —
        /// <c>Resources\hangfiredemo\Suspend Execution Example.bite</c> — also embeds a legacy
        /// <c>Unlimited.Applications.BusinessDesignStudio.Activities.DsfActivity</c> "Hello World"
        /// sub-workflow invocation step, which <c>X6ToWorkflowConverter</c> does not support at
        /// all. That aborts conversion of the WHOLE workflow, so the row reports
        /// TranslationFailed for a reason that has nothing to do with Suspend Execution's own
        /// round-trip fidelity — the same class of gap Redis Cache's remarks above describe for
        /// the legacy DsfMultiAssignActivity handler. That sample is left untouched (it is a real
        /// demo workflow also exercised by ResumeEndToEndTests/WorkflowPoolConcurrencyTests and
        /// referenced by the HangFire demo runbooks, so hand-editing it to drop a step risks
        /// changing behaviour those other consumers rely on); this fixture gives the row a clean
        /// sample instead, mirroring the same "generated fixture wins the corpus classifier's
        /// first shot" mechanism (see RoundTripFidelityCorpus.ClassifyCorpus's remarks).
        ///
        /// Chains SuspendExecutionActivity into a following Assign step (SuspendExecutionActivity
        /// throws NextNodeRequiredForSuspendExecution — see its Execute — when NextNodes is empty)
        /// rather than the single-FlowStep shape every other fixture in this file uses.
        /// AllowManualResumption is left false so no SaveDataFunc handler is needed.
        /// </summary>
        static FlowStep BuildSuspendExecutionStep()
        {
            var assignStep = new FlowStep
            {
                Action = new DsfDotNetMultiAssignActivity
                {
                    DisplayName = "Assign after resume",
                    FieldsCollection = new List<ActivityDTO>
                    {
                        new("[[message]]", "resumed", 1),
                    },
                },
            };

            return new FlowStep
            {
                Action = new SuspendExecutionActivity
                {
                    DisplayName = "Suspend Execution",
                    SuspendOption = enSuspendOption.SuspendForSeconds,
                    PersistValue = "5",
                    AllowManualResumption = false,
                    Result = "[[suspensionId]]",
                },
                Next = assignStep,
            };
        }

        /// <summary>
        /// The only real corpus sample containing <c>GateActivity</c> —
        /// <c>Resources\Examples\Control Flow - Gate.bite</c> — declares 4 conditions checking
        /// <c>[[UserId]]</c>/<c>[[loopCount]]</c>, which stay unbound under this harness's
        /// no-input and synthetic-input attempts, so <c>Passing()</c> (see
        /// <c>GateActivity.Passing</c>) legitimately returns false and the workflow halts on
        /// "gate conditions failed; execution stopped" before Gate's own round-trip logic is
        /// ever exercised on a successful path — the same class of gap Suspend Execution's
        /// remarks above describe. <c>Passing()</c> is explicitly documented to return true when
        /// there are no conditions at all, so this fixture supplies an empty <c>Conditions</c>
        /// list: the gate always passes and the workflow completes cleanly with no external
        /// dependency, proving GateActivity's own XAML → X6 JSON → XAML round trip rather than
        /// its condition-evaluation semantics (which are exercised elsewhere by
        /// <c>GateActivityTests</c>).
        /// </summary>
        static FlowStep BuildGateStep() => new()
        {
            Action = new GateActivity
            {
                DisplayName = "Gate",
                Conditions = new List<ConditionExpression>(),
            },
        };

        /// <summary>
        /// All 9 real corpus samples containing <c>DsfSelectAndApplyActivity</c> fail the activity's
        /// OWN preconditions under this harness before its round trip is ever exercised — the
        /// representative one (<c>Resources - ServerTests\Resources\SelectAndApplyExample.xml</c>)
        /// reports "The given key was not present in the dictionary", which
        /// <c>DsfSelectAndApplyActivity.ExecuteTool</c> catches and folds into the error list — so
        /// the row could only ever report PassBothFailedIdentically, the same class of gap Gate and
        /// Suspend Execution's remarks above describe.
        ///
        /// <para>
        /// This fixture brings its own data source so nothing outside the workflow is needed: an
        /// Assign seeds <c>[[rows().val]]</c> with two rows, then Select and apply iterates
        /// <c>[[rows(*).val]]</c> under the alias <c>[[item]]</c> and its nested handler copies the
        /// current item into <c>[[applied]]</c> — the one Output the sweep's payload comparison
        /// reads, and therefore the thing that proves the handler survived and ran.
        /// </para>
        ///
        /// <para>
        /// The handler is the .NET Assign, NOT the legacy <c>DsfMultiAssignActivity</c>, for the
        /// reason <see cref="BuildRedisCacheStep"/>'s remarks give: X6ToWorkflowConverter supports
        /// only the DotNet variant, and a legacy one makes the whole workflow unconvertible — which
        /// is exactly how this row used to report TranslationFailed via
        /// DsfMultiAssignObjectActivity.
        /// </para>
        ///
        /// <para>
        /// <c>ApplyActivityFunc.Argument</c> is named explicitly rather than left to
        /// <c>DsfSelectAndApplyActivity</c>'s constructor, which mints
        /// <c>explicitData_{DateTime.Now:yyyyMMddhhmmss}</c> — a value that would differ on every
        /// regeneration and defeat FixedTimestamp/DeterministicId's diff-free intent. The name is
        /// not used at execution (ExecuteTool binds the alias through ScopedEnvironment, never
        /// through the DelegateInArgument) and the converter mints its own on the way back, so
        /// pinning it here costs nothing.
        /// </para>
        /// </summary>
        static FlowStep BuildSelectAndApplyStep()
        {
            var selectAndApplyStep = new FlowStep
            {
                Action = new DsfSelectAndApplyActivity
                {
                    DisplayName = "Select and apply",
                    DataSource = "[[rows(*).val]]",
                    Alias = "[[item]]",
                    ApplyActivityFunc = new ActivityFunc<string, bool>
                    {
                        DisplayName = "Data Action",
                        Argument = new DelegateInArgument<string>("explicitData"),
                        Handler = new DsfDotNetMultiAssignActivity
                        {
                            DisplayName = "Assign applied item",
                            FieldsCollection = new List<ActivityDTO>
                            {
                                new("[[applied]]", "[[item]]", 1),
                            },
                        },
                    },
                },
            };

            return new FlowStep
            {
                Action = new DsfDotNetMultiAssignActivity
                {
                    DisplayName = "Seed rows",
                    FieldsCollection = new List<ActivityDTO>
                    {
                        new("[[rows().val]]", "alpha", 1),
                        new("[[rows().val]]", "beta", 2),
                    },
                },
                Next = selectAndApplyStep,
            };
        }

        /// <summary>
        /// Neither real corpus sample containing <c>DsfRecordsetNullhandlerLengthActivity</c> can
        /// report on this row's round trip. <c>Resources - Load\Resources\All Tools.xml</c> is the
        /// monolithic every-tool workflow the file header describes, which can never convert at
        /// all; and <c>Resources - ServerTests\Resources\Error WF.xml</c> is a workflow whose
        /// POINT is the error path - it measures <c>[[rec()]]</c>, which nothing ever populates,
        /// with <c>TreatNullAsZero="False"</c>, so <c>TryExecuteTool</c> raises
        /// <c>ErrorResource.NullRecordSet</c> ("Recordset is null [[rec()]]") before the round trip
        /// is ever exercised, and the row could only ever report PassBothFailedIdentically.
        ///
        /// <para>
        /// Binding inputs cannot rescue it: <c>DsfActivityAbstract.ValidateRecordsetName</c>
        /// requires a recordset expression WITHOUT fields, and the sweep's PrepareInputs pass binds
        /// scalars only (see <c>RoundTripFidelityTests.ScalarReferenceRegex</c>), so no synthetic
        /// value it can supply ever brings a recordset into scope.
        /// </para>
        ///
        /// <para>
        /// This fixture therefore brings its own: an Assign seeds two rows, then Length measures
        /// <c>[[rows()]]</c> into <c>[[len]]</c> - the one Output the sweep's payload comparison
        /// reads, and 2 exactly when the seed survived the round trip and ran before the
        /// measurement.
        /// </para>
        ///
        /// <para>
        /// The rows are addressed by explicit index (<c>[[rows(1).val]]</c>, <c>[[rows(2).val]]</c>)
        /// rather than by the append form <c>[[rows().val]]</c>. Measured, not assumed: within a
        /// SINGLE Assign tool the append form resolves to ONE row that each subsequent assignment
        /// overwrites, so four appends in the SQL Bulk Insert fixture inserted one row carrying only
        /// the last pair of values. Indexing says what is meant and leaves the recordset the size
        /// these remarks claim.
        /// </para>
        ///
        /// <para>
        /// <c>TreatNullAsZero</c> is set false rather than left at the constructor's true, which
        /// makes the fixture fail loudly instead of quietly: with true, a round trip that dropped
        /// the seeding Assign would still assign 0 to <c>[[len]]</c> on BOTH sides and be scored
        /// Pass. With false that same drop raises NullRecordSet on the copy alone, which the sweep
        /// scores ExecutionAsymmetric - the gap being visible is the whole point. It doubles as the
        /// non-default value <see cref="BuildLengthStep_ComposesSeededFixture_AndRoundTripsCleanly"/>
        /// asserts on.
        /// </para>
        ///
        /// <para>
        /// The Assign is the .NET one, NOT the legacy <c>DsfMultiAssignActivity</c>, for the reason
        /// <see cref="BuildRedisCacheStep"/>'s remarks give: X6ToWorkflowConverter supports only the
        /// DotNet variant, and a legacy one makes the whole workflow unconvertible.
        /// </para>
        /// </summary>
        static FlowStep BuildLengthStep()
        {
            var lengthStep = new FlowStep
            {
                Action = new DsfRecordsetNullhandlerLengthActivity
                {
                    DisplayName = "Length",
                    RecordsetName = "[[rows()]]",
                    RecordsLength = "[[len]]",
                    TreatNullAsZero = false,
                },
            };

            return new FlowStep
            {
                Action = new DsfDotNetMultiAssignActivity
                {
                    DisplayName = "Seed rows",
                    FieldsCollection = new List<ActivityDTO>
                    {
                        new("[[rows(1).val]]", "alpha", 1),
                        new("[[rows(2).val]]", "beta", 2),
                    },
                },
                Next = lengthStep,
            };
        }

        /// <summary>
        /// Delete Records has the largest sample count of any row in the corpus - 14 - and not one
        /// of them can report on its round trip. <c>DsfDeleteRecordNullHandlerActivity.ExecuteTool</c>
        /// gates everything on <c>Environment.HasRecordSet(RecordsetName)</c>, and the best of the
        /// 14 reaches it with nothing in scope, so the row reports PassBothFailedIdentically on
        /// "Variable is null; recordset does not exist". The sweep's own remarks record the second
        /// half of the trap: binding made it WORSE, not better - see
        /// <c>RoundTripFidelityTests.EvaluateSampleAsync</c>, where Delete Records is the named
        /// example of a sample that passed unbound and failed once "1" was bound to its path
        /// variable.
        ///
        /// <para>
        /// This fixture seeds its own recordset - an Assign writes two rows to
        /// <c>[[rows().val]]</c> - and then removes it, reporting into <c>[[deleted]]</c>, the one
        /// Output the sweep's payload comparison reads. "Success" there means the activity was
        /// present after the round trip and found something to delete; an empty value means it was
        /// dropped.
        /// </para>
        ///
        /// <para>
        /// <c>RecordsetName</c> is the bare, index-less, field-less <c>[[rows()]]</c>. That is the
        /// only form <c>HasRecordSet</c> accepts (it matches a RecordSetNameExpression and nothing
        /// else) and the only one every existing unit test drives the activity with - see
        /// <c>DeleteRecordsNullHandlerActivityTest</c>, which uses <c>[[Numeric()]]</c> throughout.
        /// An indexed form would additionally rest on post-delete row-shift semantics that nothing
        /// here needs.
        /// </para>
        ///
        /// <para>
        /// <c>TreatNullAsZero</c> is set false rather than left at the constructor's true for the
        /// reason <see cref="BuildLengthStep"/>'s remarks give: with true, a round trip that dropped
        /// the seeding Assign would still assign "Success" on BOTH sides and be scored Pass, where
        /// false raises NullRecordSet on the copy alone and the sweep scores ExecutionAsymmetric.
        /// </para>
        ///
        /// <para>
        /// The activity is the NullHandler variant because that is the one the toolbox actually
        /// ships: it carries <c>ToolDescriptorInfo("RecordSet-Delete", "Delete", ...)</c> and
        /// <c>DsfDeleteRecordActivity</c> carries no descriptor at all. The corpus row accepts
        /// either token, so this choice is the fixture's alone to make.
        /// </para>
        /// </summary>
        static FlowStep BuildDeleteRecordsStep()
        {
            var deleteStep = new FlowStep
            {
                Action = new DsfDeleteRecordNullHandlerActivity
                {
                    DisplayName = "Delete Records",
                    RecordsetName = "[[rows()]]",
                    Result = "[[deleted]]",
                    TreatNullAsZero = false,
                },
            };

            return new FlowStep
            {
                Action = new DsfDotNetMultiAssignActivity
                {
                    DisplayName = "Seed rows",
                    FieldsCollection = new List<ActivityDTO>
                    {
                        new("[[rows(1).val]]", "alpha", 1),
                        new("[[rows(2).val]]", "beta", 2),
                    },
                },
                Next = deleteStep,
            };
        }

        /// <summary>
        /// Stable id/name of the callee <c>Fidelity_ServiceTarget.bite</c>, shared by the caller
        /// fixture and the test that asserts both survive the round trip. The id is what
        /// <c>WorkflowResourceCache.Resolve</c> matches on first; the name is its fallback.
        /// </summary>
        static readonly Guid ServiceTargetId = DeterministicId("Fidelity_ServiceTarget");

        const string ServiceTargetName = "Fidelity_ServiceTarget";

        /// <summary>
        /// The callee. Deliberately trivial - one Assign putting a constant into <c>[[echo]]</c> -
        /// because nothing about THIS workflow is under test: it exists so the caller has something
        /// real to invoke, and so a value has to travel back across the sub-workflow boundary for
        /// the caller's payload to be non-empty.
        /// </summary>
        static FlowStep BuildServiceTargetStep() => new()
        {
            Action = new DsfDotNetMultiAssignActivity
            {
                DisplayName = "Echo",
                FieldsCollection = new List<ActivityDTO>
                {
                    new("[[echo]]", "sub-workflow ran", 1),
                },
            },
        };

        /// <summary>
        /// The only real corpus sample for this row -
        /// <c>Resources - Release\Resources\Examples\Resources - Service.bite</c> - calls a
        /// sub-workflow ('Double Roll and Check') that is not reachable from where the sweep
        /// executes it, so the row reports PassBothFailedIdentically on "Sub-workflow ... not found
        /// in ...". That is not a property of the sample: the sweep runs every copy from its own
        /// fresh temp directory holding exactly one file, and
        /// <c>LightweightEsbChannel.ExecuteSubRequest</c> resolves the callee relative to the
        /// directory of the file being executed. NO sample of this row can pass without a second
        /// resource sitting beside it, which is why this fixture ships with its callee and why
        /// <c>RoundTripFidelityTests.CopyCompanionResources</c> stages it - see that method's
        /// remarks for why staging into the temp directory is not the same as setting
        /// request.WorkflowsDirectory, which is measured as harmful.
        ///
        /// <para>
        /// <c>DsfWorkflowActivity</c>, not the legacy <c>DsfActivity</c> it derives from: the
        /// former is what the corpus row matches on (SearchTokens) and what both converters
        /// support, while the latter is the unsupported step that makes the real hangfiredemo
        /// workflows TranslationFailed - see <see cref="BuildSuspendExecutionStep"/>'s remarks.
        /// </para>
        ///
        /// <para>
        /// Both <c>ResourceID</c> and <c>ServiceName</c> are set because
        /// <c>WorkflowResourceCache.Resolve</c> tries them in that order, and because the caller
        /// must still resolve its callee after the round trip - the point of the row. The
        /// OutputMapping brings <c>[[echo]]</c> back into the caller's environment, where the
        /// DataList declares it Output and the sweep's payload comparison reads it: an empty
        /// <c>[[echo]]</c> means the sub-workflow did not run, whatever the activity reported.
        /// </para>
        /// </summary>
        static FlowStep BuildServiceStep() => new()
        {
            Action = new DsfWorkflowActivity
            {
                DisplayName = ServiceTargetName,
                ServiceName = ServiceTargetName,
                ResourceID = new InArgument<Guid>(ServiceTargetId),
                IsWorkflow = true,
                Type = "Workflow",
                InputMapping = "<Inputs></Inputs>",
                OutputMapping = "<Outputs><Output Name=\"echo\" MapsTo=\"echo\" Value=\"[[echo]]\" /></Outputs>",
            },
        };

        // ── DataLists ─────────────────────────────────────────────────────────

        static XElement RedisDataList() => new("DataList",
            Scalar("cached", "Both"),
            Scalar("cacheResult", "Output"),
            Scalar("removeResult", "Output"));

        static XElement DecisionDataList() => new("DataList",
            Scalar("a", "Input"));

        /// <summary>
        /// <c>suspensionId</c> is deliberately NOT an "Output" column here, even though
        /// <see cref="BuildSuspendExecutionStep"/> assigns it via <c>Result="[[suspensionId]]"</c>.
        /// A real Hangfire-backed scheduler (see <see cref="SuspendExecutionPersistenceSupport"/>)
        /// mints a fresh job id every execution — that id is a live-scheduler artifact, not a
        /// behavioural property of the round-trip, exactly like the per-run temp directory paths
        /// <see cref="RoundTripFidelityTests"/> already strips from error messages before comparing
        /// them. Including it in the compared JSON payload would report a real "Pass" as
        /// NonDeterministic forever, for a reason that has nothing to do with X6 conversion
        /// fidelity. "None" keeps the assignment itself exercised (proving Result resolves and the
        /// activity actually ran) without exposing the ephemeral value to the harness's payload
        /// comparison — <c>message</c> alone (which stays "" here, since nothing resumes the
        /// suspended job in this harness) is what proves the round-trip preserved behaviour.
        /// </summary>
        static XElement SuspendExecutionDataList() => new("DataList",
            Scalar("suspensionId", "None"),
            Scalar("message", "Output"));

        /// <summary>
        /// <c>rows</c> and <c>item</c> are declared with direction "None" so the sweep's
        /// PrepareInputs pass leaves them alone: this fixture seeds its own data, and binding the
        /// synthetic "1" to the alias would overwrite exactly what Select and apply is meant to
        /// iterate. <c>applied</c> is the only Output, and the one the payload comparison reads.
        /// </summary>
        static XElement SelectAndApplyDataList() => new("DataList",
            Recordset("rows", "None", "val"),
            Scalar("item", "None"),
            Scalar("applied", "Output"));

        /// <summary>
        /// <c>rows</c> is declared "None" so the sweep's PrepareInputs pass leaves it alone - it is
        /// seeded by the workflow itself, and it is a recordset, which that pass skips anyway.
        /// <c>len</c> is the only Output, and the one the payload comparison reads.
        /// </summary>
        static XElement LengthDataList() => new("DataList",
            Recordset("rows", "None", "val"),
            Scalar("len", "Output"));

        /// <summary>
        /// Shaped like <see cref="LengthDataList"/> and for the same reasons: <c>rows</c> is "None"
        /// because the workflow seeds it itself, and <c>deleted</c> is the only Output, carrying the
        /// activity's own "Success"/"Failure" verdict into the payload the sweep compares.
        /// </summary>
        static XElement DeleteRecordsDataList() => new("DataList",
            Recordset("rows", "None", "val"),
            Scalar("deleted", "Output"));

        /// <summary>
        /// <c>rows</c> is "None" because the workflow seeds it itself; <c>result</c> is the only
        /// Output, carrying DsfSqlBulkInsertActivity's own "Success" into the compared payload.
        /// </summary>
        static XElement SqlBulkInsertDataList() => new("DataList",
            Recordset("rows", "None", "id", "description"),
            Scalar("result", "Output"));

        /// <summary>The callee returns <c>echo</c>; nothing goes in.</summary>
        static XElement ServiceTargetDataList() => new("DataList",
            Scalar("echo", "Output"));

        /// <summary>
        /// The caller declares <c>echo</c> Output so the value the OutputMapping brings back across
        /// the sub-workflow boundary lands in the payload the sweep compares.
        /// </summary>
        static XElement ServiceDataList() => new("DataList",
            Scalar("echo", "Output"));

        static XElement SimpleDataList(string name) => new("DataList",
            Scalar(name, "Output"));

        /// <summary>Gate declares no inputs or outputs — <see cref="GateActivity.GetOutputs"/> returns none.</summary>
        static XElement EmptyDataList() => new("DataList");

        static XElement Scalar(string name, string direction) => new(name,
            new XAttribute("Description", string.Empty),
            new XAttribute("IsEditable", "True"),
            new XAttribute("ColumnIODirection", direction));

        /// <summary>A recordset declares its fields as child elements — see any corpus DataList.</summary>
        static XElement Recordset(string name, string direction, params string[] fields) => new(name,
            new XAttribute("Description", string.Empty),
            new XAttribute("IsEditable", "True"),
            new XAttribute("ColumnIODirection", direction),
            fields.Select(field => Scalar(field, direction)));

        // ── Emission ──────────────────────────────────────────────────────────

        /// <summary>
        /// Composes <paramref name="step"/> (and every node reachable from it via
        /// FlowStep.Next/FlowDecision.True|False — not just the start node, needed once a fixture
        /// chains more than one step, e.g. Suspend Execution; a no-op for every single-FlowStep
        /// fixture in this file) into a Flowchart and serialises it exactly as Studio itself would
        /// via WorkflowHelper.GetXamlDefinition. Shared by WriteWorkflow (which wraps and commits
        /// the result) and by tests that need the raw XAML without writing a fixture file.
        /// </summary>
        static StringBuilder BuildXaml(FlowStep step, string name)
        {
            var chart = new Flowchart { StartNode = step };
            foreach (var node in WorkflowHelper.CollectAllNodes(chart))
            {
                chart.Nodes.Add(node);
            }

            var builder = new ActivityBuilder { Name = name, Implementation = chart };
            var helper = new WorkflowHelper();
            helper.EnsureImplementation(builder, chart);

            var xaml = helper.GetXamlDefinition(builder);
            Assert.IsTrue(xaml is { Length: > 0 },
                "WorkflowHelper produced no XAML for " + name + " — the composed graph is not serialisable.");
            return xaml;
        }

        static string WriteWorkflow(string folder, string name, FlowStep step, XElement dataList)
        {
            var xaml = BuildXaml(step, name);

            var contents = EnvelopeBiteWriter.BuildBiteFileContents(
                serviceId: DeterministicId(name).ToString(),
                displayName: name,
                description: "Round-trip fidelity corpus fixture (generated). See FidelityFixtureGenerator.",
                dataList: dataList,
                xamlDefinition: xaml.ToString(),
                versionNumber: 1,
                timestampUtc: FixedTimestamp,
                user: "FidelityFixtureGenerator");

            return WriteFixture(folder, name + ".bite", contents);
        }

        /// <summary>
        /// The <c>RedisSource</c> both Redis fixtures resolve through <c>SourceId</c>. Written with a
        /// PLAINTEXT ConnectionString on purpose: <c>RedisSource.ToXml()</c> DPAPI-encrypts, and DPAPI
        /// is scoped to the machine/user that encrypted it, so an encrypted value committed to source
        /// control cannot be decrypted on a CI agent. <c>RedisSource(XElement)</c> accepts either —
        /// <c>conString.CanBeDecrypted() ? Decrypt(conString) : conString</c> — so plaintext is the
        /// portable choice.
        /// </summary>
        static string WriteRedisSource()
        {
            var source = new XElement("Source",
                new XAttribute("ID", RedisSourceId.ToString()),
                new XAttribute("Name", "Fidelity Redis Source"),
                new XAttribute("ResourceType", "RedisSource"),
                new XAttribute("IsValid", "true"),
                new XAttribute("ConnectionString",
                    "HostName=" + RedisHost + ";Port=" + RedisPort + ";AuthenticationType=Anonymous"),
                new XAttribute("Type", "RedisSource"),
                new XAttribute("ServerVersion", "0.0.0.0"),
                new XAttribute("ServerID", Guid.Empty.ToString()),
                new XElement("DisplayName", "Fidelity Redis Source"),
                new XElement("AuthorRoles", string.Empty),
                new XElement("ErrorMessages"),
                new XElement("TypeOf", "RedisSource"),
                new XElement("VersionInfo",
                    new XAttribute("DateTimeStamp", FixedTimestamp.ToString("o")),
                    new XAttribute("Reason", string.Empty),
                    new XAttribute("User", "FidelityFixtureGenerator"),
                    new XAttribute("VersionNumber", "1"),
                    new XAttribute("ResourceId", RedisSourceId.ToString()),
                    new XAttribute("VersionId", RedisSourceId.ToString())));

            return WriteFixture("redis cache", "Fidelity Redis Source.bite", source.ToString());
        }

        /// <summary>
        /// The SQL Server source Fidelity_SqlServerDatabase resolves through <c>SourceId</c>. See
        /// MssqlConnectionString's remarks for why this is a fresh, plaintext source rather than
        /// the committed (and unusable-off-machine) NewSqlServerSource.bite.
        /// </summary>
        static string WriteMssqlSource(string folder)
        {
            var source = new XElement("Source",
                new XAttribute("ID", MssqlSourceId.ToString()),
                new XAttribute("Name", "Fidelity SQL Server Source"),
                new XAttribute("ResourceType", "SqlDatabase"),
                new XAttribute("IsValid", "true"),
                new XAttribute("ConnectionString", MssqlConnectionString),
                new XAttribute("Type", "DbSource"),
                new XAttribute("ServerType", "SqlDatabase"),
                new XAttribute("ServerVersion", "0.0.0.0"),
                new XAttribute("ServerID", Guid.Empty.ToString()),
                new XElement("DisplayName", "Fidelity SQL Server Source"),
                new XElement("AuthorRoles", string.Empty),
                new XElement("ErrorMessages"),
                new XElement("TypeOf", "DbSource"),
                new XElement("VersionInfo",
                    new XAttribute("DateTimeStamp", FixedTimestamp.ToString("o")),
                    new XAttribute("Reason", string.Empty),
                    new XAttribute("User", "FidelityFixtureGenerator"),
                    new XAttribute("VersionNumber", "1"),
                    new XAttribute("ResourceId", MssqlSourceId.ToString()),
                    new XAttribute("VersionId", MssqlSourceId.ToString())));

            return WriteFixture(folder, "Fidelity SQL Server Source.bite", source.ToString());
        }

        static string WriteFixture(string folder, string fileName, string contents)
        {
            var dir = Path.Combine(FixtureRoot, folder);
            Directory.CreateDirectory(dir);
            var path = Path.Combine(dir, fileName);
            File.WriteAllText(path, contents, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            return path;
        }

        /// <summary>
        /// A fixed timestamp and name-derived ids keep regeneration diff-free: re-running the
        /// generator without changing a fixture must not rewrite its GUIDs or dates, or every
        /// regeneration would look like a change in review.
        /// </summary>
        static readonly DateTimeOffset FixedTimestamp = new(2026, 8, 23, 0, 0, 0, TimeSpan.Zero);

        static Guid DeterministicId(string name)
        {
            var bytes = System.Security.Cryptography.MD5.HashData(Encoding.UTF8.GetBytes(name));
            return new Guid(bytes);
        }
    }
}

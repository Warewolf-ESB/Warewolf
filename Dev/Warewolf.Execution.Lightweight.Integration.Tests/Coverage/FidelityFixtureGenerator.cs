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
using Warewolf.Execution.Lightweight.Integration.Tests.InProcess;
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

        /// <summary>
        /// Stable id shared by the generated EmailSource resource and the SelectedEmailSource on the
        /// Send Email fixture, for the same no-gratuitous-diff reason as <see cref="RedisSourceId"/>.
        /// </summary>
        static readonly Guid SmtpSourceId = new("3d7b9e21-4c58-4ab6-8f0d-1e6c9a4b7d52");

        /// <summary>
        /// The From address the Send Email fixture sends from. RFC-valid in a domain that resolves
        /// nowhere, and nothing is delivered anyway - SmtpEmulator accepts the transaction and drops
        /// the message.
        ///
        /// <para>
        /// It MUST be non-empty. <c>DsfSendEmailActivity.SendEmail</c> falls back to
        /// <c>runtimeSource.UserName</c> when FromAccount is blank, and the generated source
        /// deliberately carries no UserName, so a blank FromAccount reaches
        /// <c>new MailAddress("")</c> and the activity returns "Failure" on a FROM-address error
        /// before any SMTP connection is made. Setting it has a side effect worth knowing about:
        /// the same method then copies it onto <c>runtimeSource.UserName</c>, which makes
        /// <c>EmailSource.Send</c> authenticate - hence the AUTH PLAIN support in FakeSmtpServer.
        /// </para>
        /// </summary>
        internal const string SmtpFromAddress = "fidelity-sender@warewolf.invalid";

        internal const string SmtpToAddress = "fidelity-recipient@warewolf.invalid";

        /// <summary>
        /// Subject and body are asserted verbatim on both sides of the round trip by
        /// <see cref="BuildSendEmailStep_ComposesSourceBackedFixture_AndRoundTripsCleanly"/>, so they
        /// are named constants rather than literals repeated in two places.
        /// </summary>
        internal const string SmtpSubject = "Fidelity round-trip probe";

        internal const string SmtpBody = "Sent by the round-trip fidelity corpus fixture.";

        /// <summary>
        /// Host/credentials the generated MySQL fixture points at. Matches TestRun.ps1's
        /// <c>Start-HostMySQLServer</c> on both of its paths - the choco/native one the
        /// <c>-LegacyWindowsDeps</c> CI job takes, and the docker one
        /// (registry.gitlab.com/warewolf/mysql-connector-testing) - which provision root/admin on
        /// localhost:3306 and seed <c>dev2testingdb.FidelityPing</c>, a parameterless procedure
        /// whose body is a single <c>SELECT 1 AS Result</c>.
        ///
        /// <para>
        /// The database name is load-bearing twice over, not just for connecting:
        /// <c>DatabaseServiceExecution.MySqlExecution</c> passes <c>Source.DatabaseName</c> to
        /// <c>MySqlServer.GetProcedureOutParams</c>, which reads INFORMATION_SCHEMA.PARAMETERS
        /// filtered on <c>SPECIFIC_SCHEMA</c> - so a source whose DatabaseName does not match the
        /// schema the procedure lives in finds no parameter metadata for it.
        /// </para>
        ///
        /// <para>
        /// Written PLAINTEXT for the same portability reason as <see cref="MssqlConnectionString"/>,
        /// and it survives DbSource's parse/rebuild intact: the MySqlDatabase branch of the
        /// ConnectionString getter re-emits Server/Port/Database/Uid/Pwd/Connect Timeout, every one
        /// of which the setter parses back - unlike the ODBC branch, which reduces a source to
        /// <c>DSN={DatabaseName};</c> and drops the credentials entirely.
        /// </para>
        /// </summary>
        internal const string MySqlConnectionString =
            "Server=localhost;Port=3306;Database=dev2testingdb;Uid=root;Pwd=admin;Connect Timeout=30;";

        static readonly Guid MySqlSourceId = new("2b6d4f18-7c93-4a15-8e02-9f5a3c1b7d64");

        /// <summary>The procedure TestRun.ps1's $MySqlFidelitySeed creates. Bare name, no schema
        /// prefix - see <see cref="MySqlConnectionString"/>'s remarks.</summary>
        internal const string MySqlProcedureName = "FidelityPing";

        /// <summary>
        /// Host/credentials the generated PostgreSQL fixture points at. Matches TestRun.ps1's
        /// <c>Start-HostPostgresServer</c>: superuser postgres/admin on localhost:5432, database
        /// dev2testingdb, function <c>public.fidelity_ping()</c>.
        ///
        /// <para>
        /// Lower case throughout, and that is a requirement rather than a style choice. PostgreSQL
        /// folds unquoted identifiers to lower case and information_schema reports them folded, so
        /// a mixed-case routine name here would not match
        /// <c>PostgreServer.GetProcedureReturnType</c>'s
        /// <c>routine_name='{0}'</c> lookup - and that lookup decides how the call is built:
        /// a scalar-returning function is executed as <c>SELECT * FROM fn()</c>
        /// (PostgreSqlDataBaseBroker.ConfigureCommandForExecution), a miss falls through to
        /// <c>&lt;void&gt;</c> and a different statement shape.
        /// </para>
        ///
        /// <para>
        /// The routine must also live in schema <c>public</c>: both metadata queries hard-code
        /// <c>specific_schema='public'</c>.
        /// </para>
        /// </summary>
        internal const string PostgresConnectionString =
            "Host=localhost;Port=5432;Database=dev2testingdb;Username=postgres;Password=admin;Timeout=30";

        static readonly Guid PostgresSourceId = new("7e4a2d95-0f81-4c36-b5d7-3a9e6c8f1b20");

        /// <summary>The function TestRun.ps1's $PostgresFidelityFunc creates.</summary>
        internal const string PostgresProcedureName = "fidelity_ping";

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
                WriteEmailSource("send email"),
                WriteWorkflow("send email", "Fidelity_SendEmail", BuildSendEmailStep(), SimpleDataList("result")),
                WriteMySqlSource("mysql database"),
                WriteWorkflow("mysql database", "Fidelity_MySqlDatabase", BuildMySqlStep(), SimpleDataList("result")),
                WritePostgresSource("postgresql database"),
                WriteWorkflow("postgresql database", "Fidelity_PostgreSqlDatabase", BuildPostgresStep(), SimpleDataList("result")),
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
        /// Guards the committed MySQL fixture pair (see <see cref="BuildMySqlStep"/>). Only two
        /// fields decide whether this activity can do anything at all, and both cross the JSON as
        /// flat values on <c>cell.data</c> that <c>FromX6Json</c> restores with a Try* that leaves
        /// the property untouched on a miss (DsfMySqlDatabaseActivity.ToX6Json / FromX6Json):
        /// <c>SourceId</c>, without which there is no server to reach, and
        /// <c>ProcedureName</c>, which is both the routine to call and the
        /// <c>SPECIFIC_NAME</c> the parameter-metadata lookup filters on. Losing either fails the
        /// round-tripped side alone, which the sweep reports as ExecutionAsymmetric.
        /// </summary>
        [TestMethod]
        public void BuildMySqlStep_ComposesSourceBackedFixture_AndRoundTripsCleanly()
        {
            AssertDatabaseFixtureRoundTrips(
                BuildMySqlStep(), "Fidelity_MySqlDatabase", "DsfMySqlDatabaseActivity",
                MySqlSourceId, MySqlProcedureName);
        }

        /// <summary>
        /// Guards the committed PostgreSQL fixture pair (see <see cref="BuildPostgresStep"/>). Same
        /// two load-bearing fields, and the same asymmetric failure mode, as the MySQL row above -
        /// see <see cref="BuildMySqlStep_ComposesSourceBackedFixture_AndRoundTripsCleanly"/>. Here
        /// ProcedureName additionally decides the STATEMENT SHAPE: it is the
        /// <c>routine_name</c> PostgreServer.GetProcedureReturnType looks up, and a miss returns
        /// <c>&lt;void&gt;</c>, which builds a bare <c>SELECT fn()</c> instead of
        /// <c>SELECT * FROM fn()</c> and fetches nothing.
        /// </summary>
        [TestMethod]
        public void BuildPostgresStep_ComposesSourceBackedFixture_AndRoundTripsCleanly()
        {
            AssertDatabaseFixtureRoundTrips(
                BuildPostgresStep(), "Fidelity_PostgreSqlDatabase", "DsfPostgreSqlActivity",
                PostgresSourceId, PostgresProcedureName);
        }

        /// <summary>
        /// Shared body for the two database round-trip guards: compose the fixture, prove the
        /// activity type, its source id and its procedure name are all in the composed XAML, then
        /// round-trip through the real converters and prove all three survived.
        /// </summary>
        static void AssertDatabaseFixtureRoundTrips(FlowStep step, string name, string activityType,
            Guid sourceId, string procedureName)
        {
            var xaml = BuildXaml(step, name);
            var xamlText = xaml.ToString();

            StringAssert.Contains(xamlText, activityType,
                "the fixture must actually exercise " + activityType);
            StringAssert.Contains(xamlText, sourceId.ToString(),
                "the fixture must reference its generated DbSource by ResourceID - that id is all " +
                "FromX6Json gets back, and all AmbientSourceLoader has to resolve");
            StringAssert.Contains(xamlText, procedureName,
                "the fixture must name the routine TestRun.ps1 seeds");

            string roundTripped;
            try
            {
                roundTripped = X6RoundTripBridge.RoundTripXaml(xaml);
            }
            catch (Exception ex)
            {
                Assert.Fail("Round-tripping the " + name + " fixture through the X6 converters threw " +
                            ex.GetType().Name + ": " + ex.Message +
                            " - this is exactly the TranslationFailed regression this fixture exists to prevent.");
                return;
            }

            StringAssert.Contains(roundTripped, activityType,
                activityType + " must survive the round trip, not be dropped or replaced");
            StringAssert.Contains(roundTripped, sourceId.ToString(),
                "the DbSource id must survive - without it the round-tripped copy alone cannot " +
                "resolve a server, which the sweep reports as ExecutionAsymmetric");
            StringAssert.Contains(roundTripped, procedureName,
                "ProcedureName must survive - it is both the routine to execute and the name the " +
                "server's own metadata lookup filters on");
        }

        /// <summary>
        /// The committed MySQL/PostgreSQL source fixtures name a host, port, database and
        /// credentials; TestRun.ps1's <c>Start-HostMySQLServer</c>/<c>Start-HostPostgresServer</c>
        /// create exactly those, plus the routine each fixture calls. NOTHING couples the two -
        /// they are a .bite file and a PowerShell script - and the failure mode is silent: rename
        /// the seeded database, move a port or change a password on either side and both rows slide
        /// back to PassBothFailedIdentically, which reads in the generated allow-list as the
        /// entirely respectable "expected - this activity needs a live source not present in this
        /// sandbox". This asserts the contract in both directions instead.
        ///
        /// <para>
        /// It also pins the property-level parse, which is where a database source can lose data
        /// that a substring check would miss: <c>DbSource</c>'s ConnectionString SETTER parses the
        /// string into Server/Port/DatabaseName/UserID/Password and its GETTER rebuilds one from
        /// exactly those properties, so any keyword it does not model is dropped on load. That is
        /// not hypothetical - it is what makes the ODBC row unfixable (its branch reduces a whole
        /// source to <c>DSN={DatabaseName};</c>), and asserting on the REBUILT string is what
        /// proves these two do not have the same hole.
        /// </para>
        /// </summary>
        [TestMethod]
        public void CommittedDbSourceFixtures_MatchTestRunProvisioning()
        {
            var devRoot = RoundTripFidelityCorpus.FindRepoDevRoot();
            if (devRoot == null)
            {
                Assert.Inconclusive("Could not locate the Dev/ root - the committed fixtures are not reachable from here.");
                return;
            }

            // TestRun.ps1 lives at the REPO root in a source checkout, and beside the test binaries
            // in the CI TestBinaries artifact (the fidelity job invokes it as
            // <artifact>\TestRun.ps1, and the corpus is staged into that same directory, so
            // FindRepoDevRoot returns it).
            var testRunPath = new[]
                {
                    Path.Combine(devRoot, "TestRun.ps1"),
                    Path.Combine(devRoot, "..", "TestRun.ps1"),
                }
                .FirstOrDefault(File.Exists);
            if (testRunPath == null)
            {
                Assert.Inconclusive("TestRun.ps1 is not reachable from " + devRoot +
                                    " - the provisioning side of this contract cannot be checked here.");
                return;
            }
            var testRun = File.ReadAllText(testRunPath);

            // ── MySQL ────────────────────────────────────────────────────────────────────────
            var mysql = LoadCommittedDbSource(devRoot, "mysql database", "Fidelity MySQL Source.bite");

            Assert.AreEqual(enSourceType.MySqlDatabase, mysql.ServerType,
                "ServerType decides which branch of the ConnectionString getter re-emits the " +
                "string; an unrecognised value silently yields enSourceType.Unknown and an empty " +
                "connection string");
            Assert.AreEqual("localhost", mysql.Server, "host must match the provisioned server");
            Assert.AreEqual(3306, mysql.Port, "port must match the provisioned server");
            Assert.AreEqual("dev2testingdb", mysql.DatabaseName,
                "DatabaseName is passed to MySqlServer.GetProcedureOutParams as the SPECIFIC_SCHEMA " +
                "to look the procedure's parameters up in, so it must be the schema the seed creates");
            Assert.AreEqual("root", mysql.UserID, "credentials must match the provisioned server");
            Assert.AreEqual("admin", mysql.Password, "credentials must match the provisioned server");

            // The REBUILT string, not the authored one - see this test's remarks.
            var mysqlRebuilt = mysql.ConnectionString;
            foreach (var expected in new[] { "Server=localhost", "Port=3306", "Database=dev2testingdb", "Uid=root", "Pwd=admin" })
            {
                StringAssert.Contains(mysqlRebuilt, expected,
                    "DbSource must re-emit " + expected + " after parsing the committed fixture; a " +
                    "keyword it does not model is dropped on load, exactly as the ODBC branch drops " +
                    "credentials entirely. Rebuilt: " + mysqlRebuilt);
            }

            // Pin the CREATE, not just a mention: the seed also DROPs the procedure by the same
            // name, so a looser check stays satisfied by the teardown half after the CREATE has
            // been renamed - measured 2026-09-04, this guard passed against a seed that no longer
            // created FidelityPing at all.
            StringAssert.Contains(testRun, "CREATE PROCEDURE dev2testingdb." + MySqlProcedureName,
                "TestRun.ps1's MySQL seed must still CREATE the procedure the committed fixture calls");
            StringAssert.Contains(testRun, "-uroot -padmin",
                "TestRun.ps1 must still provision the credentials the committed fixture uses");
            StringAssert.Contains(testRun, "3306",
                "TestRun.ps1 must still expose MySQL on the port the committed fixture dials");

            // ── PostgreSQL ───────────────────────────────────────────────────────────────────
            var postgres = LoadCommittedDbSource(devRoot, "postgresql database", "Fidelity PostgreSQL Source.bite");

            Assert.AreEqual(enSourceType.PostgreSQL, postgres.ServerType, "ServerType decides the connection-string branch");
            Assert.AreEqual("localhost", postgres.Server, "host must match the provisioned server");
            Assert.AreEqual(5432, postgres.Port, "port must match the provisioned server");
            Assert.AreEqual("dev2testingdb", postgres.DatabaseName, "database must match the provisioned server");
            Assert.AreEqual("postgres", postgres.UserID, "credentials must match the provisioned server");
            Assert.AreEqual("admin", postgres.Password, "credentials must match the provisioned server");

            var postgresRebuilt = postgres.ConnectionString;
            foreach (var expected in new[] { "Host=localhost", "Port=5432", "Username=postgres", "Password=admin", "Database=dev2testingdb" })
            {
                StringAssert.Contains(postgresRebuilt, expected,
                    "DbSource must re-emit " + expected + " after parsing the committed fixture. " +
                    "Rebuilt: " + postgresRebuilt);
            }

            StringAssert.Contains(testRun, "CREATE OR REPLACE FUNCTION public." + PostgresProcedureName,
                "TestRun.ps1's Postgres seed must still CREATE the function the committed fixture " +
                "calls, in schema public - the only schema PostgreServer's metadata queries look in");
            Assert.AreEqual(PostgresProcedureName, PostgresProcedureName.ToLowerInvariant(),
                "the function name must be lower case: PostgreSQL folds unquoted identifiers and " +
                "information_schema reports them folded, so a mixed-case name could never match " +
                "GetProcedureReturnType's routine_name lookup");
            StringAssert.Contains(testRun, "-U postgres",
                "TestRun.ps1 must still provision the superuser the committed fixture connects as");
            StringAssert.Contains(testRun, "-p 5432",
                "TestRun.ps1 must still start Postgres on the port the committed fixture dials");
        }

        /// <summary>
        /// Loads one committed source fixture through the real <c>DbSource(XElement)</c> ctor - the
        /// same path the sweep's source loader takes - so the assertions see what execution would.
        /// </summary>
        static DbSource LoadCommittedDbSource(string devRoot, string folder, string fileName)
        {
            var path = Path.Combine(devRoot, "Warewolf.Execution.Lightweight", "Resources", "tools", folder, fileName);
            Assert.IsTrue(File.Exists(path),
                "the committed source fixture is missing at " + path +
                " - regenerate it with Generate_MissingCorpusFixtures (remove its [Ignore] locally) " +
                "and commit the result");
            return new DbSource(XElement.Parse(File.ReadAllText(path)));
        }

        /// <summary>
        /// Guards the committed Send Email fixture pair (see <see cref="BuildSendEmailStep"/>).
        /// Every field this activity needs to reach a successful send crosses the JSON as a flat
        /// string on <c>cell.data</c> and is restored by a <c>TryGetString</c> that leaves the
        /// property untouched when the key is absent (see <c>DsfSendEmailActivity.ToX6Json</c> /
        /// <c>FromX6Json</c>), so a dropped key does not throw - it silently produces a different
        /// email, or none. Two of them are load-bearing in a way the sweep would report
        /// confusingly:
        ///
        /// <list type="bullet">
        ///   <item>
        ///     <c>SelectedEmailSource.ResourceID</c> is the only handle on the source. FromX6Json
        ///     rebuilds a bare <c>new EmailSource { ResourceID = ... }</c> with no Host, exactly as
        ///     the SQL rows do, and the activity re-resolves the real source from ResourceCatalog /
        ///     AmbientSourceLoader at execution time - so losing the id means "Invalid email source"
        ///     on the round-tripped side only, i.e. ExecutionAsymmetric.
        ///   </item>
        ///   <item>
        ///     <c>FromAccount</c> and <c>To</c> are what make the send legal at all: a lost
        ///     FromAccount falls back to the source's empty UserName and fails on the FROM address
        ///     (see <see cref="SmtpFromAddress"/>), and a lost To leaves nothing to deliver to.
        ///   </item>
        /// </list>
        /// </summary>
        [TestMethod]
        public void BuildSendEmailStep_ComposesSourceBackedFixture_AndRoundTripsCleanly()
        {
            var xaml = BuildXaml(BuildSendEmailStep(), "Fidelity_SendEmail");
            var xamlText = xaml.ToString();

            StringAssert.Contains(xamlText, "DsfSendEmailActivity",
                "the fixture must actually exercise DsfSendEmailActivity");
            StringAssert.Contains(xamlText, SmtpSourceId.ToString(),
                "the fixture must reference the generated EmailSource by ResourceID - that id is " +
                "all FromX6Json gets back, and all AmbientSourceLoader has to resolve");

            string roundTripped;
            try
            {
                roundTripped = X6RoundTripBridge.RoundTripXaml(xaml);
            }
            catch (Exception ex)
            {
                Assert.Fail("Round-tripping the Send Email fixture through the X6 converters threw " +
                            ex.GetType().Name + ": " + ex.Message +
                            " - this is exactly the TranslationFailed regression this fixture exists to prevent.");
                return;
            }

            StringAssert.Contains(roundTripped, "DsfSendEmailActivity",
                "DsfSendEmailActivity must survive the round trip, not be dropped or replaced");
            StringAssert.Contains(roundTripped, SmtpSourceId.ToString(),
                "the EmailSource id must survive - without it the round-tripped copy alone fails " +
                "on \"Invalid email source\", which the sweep reports as ExecutionAsymmetric");
            StringAssert.Contains(roundTripped, SmtpFromAddress,
                "FromAccount must survive - an empty one falls back to the source's empty UserName " +
                "and the activity fails on the FROM address before connecting");
            StringAssert.Contains(roundTripped, SmtpToAddress,
                "To must survive - there is nothing to deliver to without it");
            StringAssert.Contains(roundTripped, SmtpSubject,
                "Subject must survive");
            StringAssert.Contains(roundTripped, SmtpBody,
                "Body must survive");
            StringAssert.Contains(roundTripped, "[[result]]",
                "Result must survive - it is the only value this activity contributes to the " +
                "payload the sweep compares");
        }

        /// <summary>
        /// The committed <c>Fidelity SMTP Source.bite</c> carries a literal port, while the
        /// emulator that has to answer on it is a compile-time constant
        /// (<see cref="InProcess.SmtpEmulator.Port"/>). Nothing else couples the two: moving the
        /// emulator would leave the fixture dialling a dead port, the send would fail on both sides
        /// identically, and the row would slide back to PassBothFailedIdentically - a silent loss of
        /// coverage that reads as "expected, needs a live source" in the generated allow-list. This
        /// fails loudly instead, on the file the sweep actually loads.
        /// </summary>
        [TestMethod]
        public void CommittedEmailSourceFixture_PointsAtTheSmtpEmulatorPort()
        {
            var devRoot = RoundTripFidelityCorpus.FindRepoDevRoot();
            if (devRoot == null)
            {
                Assert.Inconclusive("Could not locate the Dev/ root - the committed fixtures are not reachable from here.");
                return;
            }

            var fixturePath = Path.Combine(devRoot, "Warewolf.Execution.Lightweight", "Resources",
                "tools", "send email", "Fidelity SMTP Source.bite");
            Assert.IsTrue(File.Exists(fixturePath),
                "the committed EmailSource fixture is missing at " + fixturePath +
                " - regenerate it with Generate_MissingCorpusFixtures (remove its [Ignore] locally) " +
                "and commit the result");

            var source = new EmailSource(XElement.Parse(File.ReadAllText(fixturePath)));

            Assert.AreEqual(SmtpEmulator.Port, source.Port,
                "the committed fixture's port must match the in-process emulator's, or nothing " +
                "answers the send and the Send Email row silently regresses to PassBothFailedIdentically");
            Assert.AreEqual(SmtpEmulator.Host, source.Host,
                "the committed fixture's host must match the emulator's loopback binding");
            Assert.AreEqual(SmtpSourceId, source.ResourceID,
                "the committed fixture must carry the id the workflow fixture resolves through " +
                "SelectedEmailSource");
            Assert.IsFalse(source.EnableSsl,
                "EnableSsl must stay false - EmailSource.Send would otherwise negotiate TLS " +
                "(SecureSocketOptions.Auto) against an emulator that speaks plaintext only");
            Assert.AreEqual(string.Empty, source.UserName,
                "the committed fixture must carry no credential: the activity overwrites UserName " +
                "with FromAccount before sending (see SmtpFromAddress), so a stored one is both " +
                "unused and misleading");
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
        /// Calls <c>dev2testingdb.FidelityPing</c> on the MySQL server TestRun.ps1
        /// <c>-StartMySQLServer</c> provisions (see <see cref="MySqlConnectionString"/>), so this
        /// fixture executes real MySQL logic rather than merely resolving a source.
        ///
        /// <para>
        /// Every real corpus sample for this row (5 of them, e.g.
        /// <c>StopExecutionOnMySQLTimeoutError.bite</c>) resolves a source that is not configured in
        /// this sandbox and fails on "Database source is not configured" before the activity's own
        /// logic runs. Unlike the Exchange row, a Pass here cannot be hollow: both
        /// <c>SetupMySqlServer</c> and <c>MySqlExecution</c> add their exception message to the
        /// error result, which the activity merges and the workflow fails on - so an unreachable
        /// server, a missing database or a missing procedure all show up as a failure rather than a
        /// silent success. Proven by pointing the source at a dead port - see the report on this
        /// change.
        /// </para>
        ///
        /// <para>
        /// Outputs is set (empty, not null) only to satisfy the null guard, exactly as
        /// <see cref="BuildSqlServerStep"/> does; the compared payload is the activity's own result,
        /// not a column mapping.
        /// </para>
        /// </summary>
        static FlowStep BuildMySqlStep() => new()
        {
            Action = new DsfMySqlDatabaseActivity
            {
                DisplayName = "MySQL Database",
                SourceId = MySqlSourceId,
                ActionName = MySqlProcedureName,
                ProcedureName = MySqlProcedureName,
                Inputs = new List<IServiceInput>(),
                Outputs = new List<IServiceOutputMapping>(),
            },
        };

        /// <summary>
        /// Calls <c>public.fidelity_ping()</c> on the PostgreSQL server TestRun.ps1
        /// <c>-StartPostgresServer</c> provisions (see <see cref="PostgresConnectionString"/>).
        ///
        /// <para>
        /// A scalar-returning FUNCTION rather than a PROCEDURE on purpose: it is the branch that
        /// exercises the most of the real path. <c>PostgreSqlDataBaseBroker.ConfigureCommandForExecution</c>
        /// rewrites a function with a return type into <c>SELECT * FROM fidelity_ping()</c> and the
        /// broker then fetches a DataTable from it, whereas a procedure is issued as <c>CALL</c> and
        /// the broker deliberately skips the fetch (<c>returnType == "&lt;procedure&gt;"</c> yields an
        /// empty DataTable), which would leave the read path untested.
        /// </para>
        /// </summary>
        static FlowStep BuildPostgresStep() => new()
        {
            Action = new DsfPostgreSqlActivity
            {
                DisplayName = "PostgreSQL Database",
                SourceId = PostgresSourceId,
                ActionName = PostgresProcedureName,
                ProcedureName = PostgresProcedureName,
                Inputs = new List<IServiceInput>(),
                Outputs = new List<IServiceOutputMapping>(),
            },
        };

        /// <summary>
        /// Sends one mail through <see cref="InProcess.SmtpEmulator"/>, the in-process fake SMTP
        /// acceptor the assembly fixture starts on <see cref="InProcess.SmtpEmulator.Port"/>.
        ///
        /// <para>
        /// Every real corpus sample for this row resolves an EmailSource whose Host is empty (they
        /// were authored against a developer's own mail server and saved without one), so the sweep
        /// could only ever report PassBothFailedIdentically on "Invalid URI: The hostname could not
        /// be parsed." - both sides failing before DsfSendEmailActivity's own logic ran. Unlike the
        /// Redis/SQL Server rows this needs no live service and no TestRun.ps1 -Start* flag: the
        /// emulator is in-process, so a dev checkout and both CI jobs this assembly is partitioned
        /// between measure it identically.
        /// </para>
        ///
        /// <para>
        /// EnableSsl is false on the source, which selects <c>SecureSocketOptions.None</c> in
        /// <c>EmailSource.Send</c> - the emulator speaks plaintext SMTP and advertises no STARTTLS.
        /// See <see cref="SmtpFromAddress"/> for why FromAccount must be set, and what that implies
        /// for authentication.
        /// </para>
        /// </summary>
        static FlowStep BuildSendEmailStep() => new()
        {
            Action = new DsfSendEmailActivity
            {
                DisplayName = "Send Email",
                SelectedEmailSource = new EmailSource { ResourceID = SmtpSourceId },
                FromAccount = SmtpFromAddress,
                To = SmtpToAddress,
                Subject = SmtpSubject,
                Body = SmtpBody,
                Result = "[[result]]",
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

        /// <summary>
        /// The <c>EmailSource</c> Fidelity_SendEmail resolves through <c>SelectedEmailSource</c>,
        /// pointed at the in-process <see cref="InProcess.SmtpEmulator"/>.
        ///
        /// <para>
        /// PLAINTEXT ConnectionString for the same reason as <see cref="WriteRedisSource"/>:
        /// <c>EmailSource.ToXml()</c> DPAPI-encrypts, and DPAPI ciphertext cannot be decrypted on a
        /// different machine/user, so an encrypted value committed to source control is unusable on
        /// a CI agent. <c>EmailSource(XElement)</c> accepts either - <c>conString.CanBeDecrypted()
        /// ? Decrypt(conString) : conString</c> - so plaintext is the portable choice. Unlike
        /// <see cref="MssqlConnectionString"/> there is no parse/rebuild hazard here: EmailSource
        /// keeps Host/Port/UserName/Password/EnableSsl/Timeout as plain properties and never
        /// reconstructs a connection string on the read path.
        /// </para>
        ///
        /// <para>
        /// UserName/Password are deliberately empty: nothing here depends on a credential, and the
        /// activity overwrites UserName with FromAccount before sending anyway (see
        /// <see cref="SmtpFromAddress"/>). Port comes from the emulator's own constant so the two
        /// cannot drift silently - the guard is
        /// <see cref="CommittedEmailSourceFixture_PointsAtTheSmtpEmulatorPort"/>.
        /// </para>
        /// </summary>
        static string WriteEmailSource(string folder)
        {
            var source = new XElement("Source",
                new XAttribute("ID", SmtpSourceId.ToString()),
                new XAttribute("ResourceID", SmtpSourceId.ToString()),
                new XAttribute("Name", "Fidelity SMTP Source"),
                new XAttribute("ResourceType", "EmailSource"),
                new XAttribute("IsValid", "true"),
                new XAttribute("ConnectionString",
                    "Host=" + SmtpEmulator.Host +
                    ";UserName=;Password=;Port=" + SmtpEmulator.Port +
                    ";EnableSsl=False;Timeout=10000"),
                new XAttribute("Type", "EmailSource"),
                new XAttribute("ServerVersion", "0.0.0.0"),
                new XAttribute("ServerID", Guid.Empty.ToString()),
                new XElement("DisplayName", "Fidelity SMTP Source"),
                new XElement("AuthorRoles", string.Empty),
                new XElement("ErrorMessages"),
                new XElement("TypeOf", "EmailSource"),
                new XElement("VersionInfo",
                    new XAttribute("DateTimeStamp", FixedTimestamp.ToString("o")),
                    new XAttribute("Reason", string.Empty),
                    new XAttribute("User", "FidelityFixtureGenerator"),
                    new XAttribute("VersionNumber", "1"),
                    new XAttribute("ResourceId", SmtpSourceId.ToString()),
                    new XAttribute("VersionId", SmtpSourceId.ToString())));

            return WriteFixture(folder, "Fidelity SMTP Source.bite", source.ToString());
        }

        /// <summary>
        /// The MySQL DbSource Fidelity_MySqlDatabase resolves through <c>SourceId</c>. Plaintext,
        /// portable, and lossless across DbSource's parse/rebuild - see
        /// <see cref="MySqlConnectionString"/>.
        /// </summary>
        static string WriteMySqlSource(string folder) =>
            WriteDbSource(folder, "Fidelity MySQL Source", MySqlSourceId, "MySqlDatabase", MySqlConnectionString);

        /// <summary>
        /// The PostgreSQL DbSource Fidelity_PostgreSqlDatabase resolves through <c>SourceId</c>.
        /// See <see cref="PostgresConnectionString"/>.
        /// </summary>
        static string WritePostgresSource(string folder) =>
            WriteDbSource(folder, "Fidelity PostgreSQL Source", PostgresSourceId, "PostgreSQL", PostgresConnectionString);

        /// <summary>
        /// Emits one DbSource resource. The <paramref name="serverType"/> string is what
        /// <c>DbSource(XElement)</c> switches on to pick <c>enSourceType</c> (and therefore which
        /// branch of the ConnectionString getter re-emits the string), so it must be one of the
        /// values that ctor recognises: sqldatabase / mysqldatabase / postgresql / oracle / odbc /
        /// sqlite. Anything else silently becomes <c>enSourceType.Unknown</c> and the source's
        /// connection string comes back as the empty string.
        /// </summary>
        static string WriteDbSource(string folder, string name, Guid id, string serverType, string connectionString)
        {
            var source = new XElement("Source",
                new XAttribute("ID", id.ToString()),
                new XAttribute("ResourceID", id.ToString()),
                new XAttribute("Name", name),
                new XAttribute("ResourceType", serverType),
                new XAttribute("IsValid", "true"),
                new XAttribute("ConnectionString", connectionString),
                new XAttribute("Type", "DbSource"),
                new XAttribute("ServerType", serverType),
                new XAttribute("ServerVersion", "0.0.0.0"),
                new XAttribute("ServerID", Guid.Empty.ToString()),
                new XElement("DisplayName", name),
                new XElement("AuthorRoles", string.Empty),
                new XElement("ErrorMessages"),
                new XElement("TypeOf", "DbSource"),
                new XElement("VersionInfo",
                    new XAttribute("DateTimeStamp", FixedTimestamp.ToString("o")),
                    new XAttribute("Reason", string.Empty),
                    new XAttribute("User", "FidelityFixtureGenerator"),
                    new XAttribute("VersionNumber", "1"),
                    new XAttribute("ResourceId", id.ToString()),
                    new XAttribute("VersionId", id.ToString())));

            return WriteFixture(folder, name + ".bite", source.ToString());
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
        /// A fixed timestamp and name-derived ids keep everything THIS file controls stable across
        /// regenerations: re-running the generator without changing a fixture must not rewrite the
        /// GUIDs or dates it chooses, or every regeneration would look like a change in review. It
        /// makes the generated SOURCE resources byte-identical.
        ///
        /// <para>
        /// The generated WORKFLOWS are not, and cannot be made so from here:
        /// <c>WorkflowHelper.GetXamlDefinition</c> stamps a fresh <c>UniqueID</c> into every
        /// activity and names its ActivityFunc argument <c>explicitData_&lt;yyyyMMddHHmmss&gt;</c>,
        /// both regenerated on every call. So a full <see cref="Generate_MissingCorpusFixtures"/>
        /// run rewrites every committed .bite even when no fixture changed - measured 2026-09-04,
        /// 13 files churned for one added fixture. Revert the ones you did not intend to change and
        /// commit only the new/edited fixture, or the diff hides the real change.
        /// </para>
        /// </summary>
        static readonly DateTimeOffset FixedTimestamp = new(2026, 8, 23, 0, 0, 0, TimeSpan.Zero);

        static Guid DeterministicId(string name)
        {
            var bytes = System.Security.Cryptography.MD5.HashData(Encoding.UTF8.GetBytes(name));
            return new Guid(bytes);
        }
    }
}

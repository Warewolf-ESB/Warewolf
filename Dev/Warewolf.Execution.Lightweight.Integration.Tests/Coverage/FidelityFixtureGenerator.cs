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
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Dev2.Activities;
using Dev2.Activities.RedisCache;
using Dev2.Activities.RedisRemove;
using Dev2.Common.Interfaces.DB;
using Dev2.Data.Interfaces.Enums;
using Dev2.Utilities;
using Dev2.Data.Decisions.Operations;
using Dev2.Data.SystemTemplates.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;
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
        /// </summary>
        internal const string MssqlConnectionString =
            "Data Source=localhost,1433;Initial Catalog=Dev2TestingDB;User ID=testUser;Password=Ex@mple!23Secure#PWD;Encrypt=False;";

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
                WriteWorkflow("sql bulk insert", "Fidelity_SqlBulkInsert", BuildSqlBulkInsertStep(), SimpleDataList("result")),
                WriteMssqlSource(),
                WriteWorkflow("sql server database", "Fidelity_SqlServerDatabase", BuildSqlServerStep(), SimpleDataList("result")),
                WriteWorkflow("suspend execution", "Fidelity_SuspendExecution", BuildSuspendExecutionStep(), SuspendExecutionDataList()),
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

        static FlowStep BuildSqlBulkInsertStep() => new()
        {
            Action = new DsfSqlBulkInsertActivity
            {
                DisplayName = "SQL Bulk Insert",
                TableName = "FidelityTable",
            },
        };

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

        static XElement SimpleDataList(string name) => new("DataList",
            Scalar(name, "Output"));

        static XElement Scalar(string name, string direction) => new(name,
            new XAttribute("Description", string.Empty),
            new XAttribute("IsEditable", "True"),
            new XAttribute("ColumnIODirection", direction));

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
        static string WriteMssqlSource()
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

            return WriteFixture("sql server database", "Fidelity SQL Server Source.bite", source.ToString());
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

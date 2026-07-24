/*
 * Phase-3 suspend-support tests:
 *
 *   - WorkflowExecutor.ResolveExecutingUser  — named principal kept; null / nameless
 *     (WorkflowClaimsPrincipal.Anonymous has a null Identity.Name) → GenericPrincipal("Public").
 *     A null/nameless ExecutingUser would NRE in SuspendExecutionActivity (line 152) or
 *     fail principal reconstruction on resume.
 *   - WorkflowExecutor.ExtractResourceIdentity — .bite root ID + VersionInfo/@VersionNumber,
 *     with safe defaults (Guid.Empty / 1) on missing or malformed input. Suspend persists
 *     ResourceID + VersionNumber, and resume resolves the workflow by that ID.
 *   - SuspendSnapshotContext / LightweightJobValuesEnricher — engine-resume metadata is
 *     stamped only inside an execution scope, is additive (legacy keys untouched), and
 *     scopes nest/restore correctly.
 *   - WorkflowFunctionHelper.ParseRequestAsync — the principal comes ONLY from the auth
 *     middleware's FunctionContext.Items entry; a JSON body cannot inject one.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Security.Principal;
using System.Text;
using Warewolf.Execution.Lightweight.Auth;
using Warewolf.Execution.Lightweight.Auth.Models;
using Warewolf.Execution.Lightweight.Tests.Auth;

namespace Warewolf.Execution.Lightweight.Tests.Execution
{
    [TestClass]
    public class ResolveExecutingUserTests
    {
        [TestMethod]
        [TestCategory("UnitTest")]
        public void ResolveExecutingUser_NamedPrincipal_ReturnedUnchanged()
        {
            var principal = new GenericPrincipal(new GenericIdentity("alice@contoso.com"), Array.Empty<string>());

            Assert.AreSame(principal, WorkflowExecutor.ResolveExecutingUser(principal));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void ResolveExecutingUser_Null_FallsBackToNamedPublicPrincipal()
        {
            var resolved = WorkflowExecutor.ResolveExecutingUser(null!);

            Assert.IsNotNull(resolved);
            Assert.AreEqual("Public", resolved.Identity.Name,
                "Suspend persists ExecutingUser.Identity.Name — the fallback must carry a usable name.");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void ResolveExecutingUser_AnonymousClaimsPrincipal_FallsBackToNamedPublicPrincipal()
        {
            // WorkflowClaimsPrincipal.Anonymous() (stored by the middleware on /public
            // routes) has a NULL Identity.Name — it must not leak into persistence.
            var resolved = WorkflowExecutor.ResolveExecutingUser(WorkflowClaimsPrincipal.Anonymous());

            Assert.AreEqual("Public", resolved.Identity.Name);
        }
    }

    [TestClass]
    public class ExtractResourceIdentityTests
    {
        const string ResourceId = "11e5b1a4-3f66-4c60-a292-9861c2b0e37d";

        static StringBuilder Workflow(string idAttribute, string versionInfo) => new(
            $"""
             <Service {idAttribute} Name="Suspend Test">
               <Actions><Action Type="Workflow"><XamlDefinition>x</XamlDefinition></Action></Actions>
               <DataList />
               {versionInfo}
             </Service>
             """);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void ExtractResourceIdentity_ParsesIdAndVersion()
        {
            var contents = Workflow($"ID=\"{ResourceId}\"", "<VersionInfo VersionNumber=\"7\" />");

            var (resourceId, versionNumber) = WorkflowExecutor.ExtractResourceIdentity(contents);

            Assert.AreEqual(Guid.Parse(ResourceId), resourceId);
            Assert.AreEqual(7, versionNumber);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void ExtractResourceIdentity_MissingIdAndVersion_UsesSafeDefaults()
        {
            var (resourceId, versionNumber) = WorkflowExecutor.ExtractResourceIdentity(
                Workflow(string.Empty, string.Empty));

            Assert.AreEqual(Guid.Empty, resourceId);
            Assert.AreEqual(1, versionNumber, "Version defaults to 1 — parity with server resources.");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void ExtractResourceIdentity_MalformedXml_UsesSafeDefaults()
        {
            var (resourceId, versionNumber) = WorkflowExecutor.ExtractResourceIdentity(
                new StringBuilder("<not-closed"));

            Assert.AreEqual(Guid.Empty, resourceId);
            Assert.AreEqual(1, versionNumber);
        }
    }

    [TestClass]
    public class SuspendSnapshotContextTests
    {
        static readonly Guid ExecutionId = Guid.Parse("7c9e6679-7425-40de-944b-e07fc1f90ae7");

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Enricher_OutsideScope_IsNoOp()
        {
            var values = LegacyValues();
            var before = values.Keys.ToArray();

            new LightweightJobValuesEnricher().Enrich(values);

            CollectionAssert.AreEquivalent(before, values.Keys.ToArray(),
                "Without an active execution scope no keys may be added.");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Enricher_InsideScope_StampsAdditiveEngineKeys_LegacyKeysUntouched()
        {
            var values = LegacyValues();
            var legacyEnvironment = values["environment"];

            using (SuspendSnapshotContext.BeginScope("Hello Suspend", @"C:\resources\Hello Suspend.bite", ExecutionId))
            {
                new LightweightJobValuesEnricher().Enrich(values);
            }

            Assert.AreEqual("Hello Suspend", values[LightweightJobValuesEnricher.WorkflowNameKey].ToString());
            Assert.AreEqual(@"C:\resources\Hello Suspend.bite", values[LightweightJobValuesEnricher.WorkflowFilePathKey].ToString());
            Assert.AreEqual(ExecutionId.ToString(), values[LightweightJobValuesEnricher.ExecutionIdKey].ToString());
            Assert.IsTrue(DateTime.TryParse(values[LightweightJobValuesEnricher.SuspendedAtUtcKey].ToString(), out _));

            // Additive contract: the five legacy keys are byte-identical.
            Assert.AreSame(legacyEnvironment, values["environment"]);
            Assert.AreEqual(9, values.Count, "5 legacy + 4 engine keys.");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void BeginScope_Nested_RestoresPreviousScopeOnDispose()
        {
            Assert.IsNull(SuspendSnapshotContext.Current);

            using (SuspendSnapshotContext.BeginScope("outer", "outer.bite", ExecutionId))
            {
                using (SuspendSnapshotContext.BeginScope("inner", "inner.bite", Guid.NewGuid()))
                {
                    Assert.AreEqual("inner", SuspendSnapshotContext.Current!.WorkflowName);
                }

                Assert.AreEqual("outer", SuspendSnapshotContext.Current!.WorkflowName);
            }

            Assert.IsNull(SuspendSnapshotContext.Current);
        }

        static Dictionary<string, StringBuilder> LegacyValues() => new()
        {
            {"resourceID", new StringBuilder("ab04663e-1e09-4338-8f61-a06a7ae5ebab")},
            {"environment", new StringBuilder("{}")},
            {"startActivityId", new StringBuilder("4032a11e-4fb3-4208-af48-b92a0602ab4b")},
            {"versionNumber", new StringBuilder("1")},
            {"currentuserprincipal", new StringBuilder("alice")},
        };
    }

    [TestClass]
    public class ParseRequestPrincipalTests
    {
        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task ParseRequestAsync_AttachesPrincipalFromFunctionContextItems()
        {
            var ctx = new HttpFunctionContext();
            var principal = new GenericPrincipal(new GenericIdentity("daemon-app-id"), Array.Empty<string>());
            ctx.Items[AuthConstants.PrincipalContextKey] = principal;
            var request = new FakeHttpRequestData(ctx, new Uri("https://engine.test/secure/Hello.json"));

            var executionRequest = await WorkflowFunctionHelper.ParseRequestAsync(request, workflowsDirectory: null);

            Assert.AreSame(principal, executionRequest.ExecutingPrincipal);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task ParseRequestAsync_BodyJsonCannotInjectPrincipal()
        {
            var ctx = new HttpFunctionContext(); // no principal stored by middleware
            var request = new FakeHttpRequestData(ctx, new Uri("https://engine.test/secure/Hello.json"), "POST");
            var payload = Encoding.UTF8.GetBytes(
                """{"ExecutingPrincipal":{"Identity":{"Name":"attacker"}},"InputParameters":{"Name":"x"}}""");
            request.Body.Write(payload, 0, payload.Length);
            request.Body.Position = 0;

            var executionRequest = await WorkflowFunctionHelper.ParseRequestAsync(request, workflowsDirectory: null);

            Assert.IsNull(executionRequest.ExecutingPrincipal,
                "The principal must come exclusively from the auth middleware, never from the payload.");
        }
    }
}

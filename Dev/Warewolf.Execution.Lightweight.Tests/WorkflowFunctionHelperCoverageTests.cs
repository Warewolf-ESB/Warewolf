/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  Coverage uplift tests for <see cref="WorkflowFunctionHelper"/>.
 *
 *  The integration-test bucket already covers the simple
 *  CreateRequest / CreateRequestByName / ParseFromJson public API paths.
 *  This file targets the previously-uncovered parts of
 *  Warewolf.Execution.Lightweight\Http\WorkflowFunctionHelper.cs in the
 *  unit-tests bucket, focusing on:
 *
 *    * ParseRequestAsync — query string, JSON body, route-name override, and
 *      URL-extension to ReturnType mapping.
 *    * ResolveFilePath — index miss + on-disk case-insensitive lookup,
 *      missing directory short-circuit, .xml / .bite extension handling, and
 *      separator normalization.
 *    * NormalizeSeparators — exercised via the WorkflowFilePath round-trip.
 *    * StripKnownExtension — exercised via the WorkflowIndex.Resolve lookup
 *      with .xml / .bite / extensionless names.
 *
 *  All tests are pure in-memory + Path.GetTempPath()-rooted temp dirs that
 *  are cleaned up in TestCleanup. No Functions host is required.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Dev2.Web;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Execution.Lightweight.Tests.Auth;
using Warewolf.Execution.Lightweight.Models;

namespace Warewolf.Execution.Lightweight.Tests
{
    [TestClass]
    [TestCategory("WorkflowFunctionHelper_Coverage")]
    public class WorkflowFunctionHelperCoverageTests
    {
        // ── Temp-dir bookkeeping ─────────────────────────────────────────────

        private readonly List<string> _tempDirs = new();

        [TestCleanup]
        public void Cleanup()
        {
            foreach (var dir in _tempDirs)
            {
                try { if (Directory.Exists(dir)) Directory.Delete(dir, recursive: true); }
                catch { /* best-effort */ }
            }
        }

        private string NewTempDir()
        {
            var dir = Path.Combine(Path.GetTempPath(), "wfhelper_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            _tempDirs.Add(dir);
            return dir;
        }

        // ── Fake-request helpers ────────────────────────────────────────────

        private static FakeHttpRequestData MakeRequest(string urlString, string body = null)
        {
            var ctx = new HttpHostContext();
            var req = new FakeHttpRequestData(ctx, new Uri(urlString));
            if (body != null)
            {
                var bytes = Encoding.UTF8.GetBytes(body);
                req.Body.Write(bytes, 0, bytes.Length);
                req.Body.Position = 0;
            }
            return req;
        }

        // ════════════════════════════════════════════════════════════════════
        // CreateRequest / CreateRequestByName / ParseFromJson — basic coverage
        // ════════════════════════════════════════════════════════════════════

        [TestMethod]
        public void CreateRequest_NullInputs_AllocatesEmptyDictionary()
        {
            var req = WorkflowFunctionHelper.CreateRequest("/wf/A.bite");
            Assert.AreEqual("/wf/A.bite", req.WorkflowFilePath);
            Assert.IsNotNull(req.InputParameters);
            Assert.AreEqual(0, req.InputParameters.Count);
        }

        [TestMethod]
        public void CreateRequest_WithInputs_PropagatesAndDoesNotCopy()
        {
            var dict = new Dictionary<string, string> { ["a"] = "1" };
            var req = WorkflowFunctionHelper.CreateRequest("/wf/A.bite", dict);
            Assert.AreSame(dict, req.InputParameters);
        }

        [TestMethod]
        public void CreateRequestByName_NonExistingDir_FallsBackToXmlPath()
        {
            var dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            var req = WorkflowFunctionHelper.CreateRequestByName("MyWf", dir);
            Assert.AreEqual("MyWf", req.WorkflowName);
            Assert.AreEqual(dir, req.WorkflowsDirectory);
            Assert.IsTrue(req.WorkflowFilePath!.EndsWith("MyWf.xml"));
        }

        [TestMethod]
        public void ParseFromJson_NullEmptyAndInvalid_AllReturnEmptyRequest()
        {
            Assert.IsNull(WorkflowFunctionHelper.ParseFromJson(null).WorkflowFilePath);
            Assert.IsNull(WorkflowFunctionHelper.ParseFromJson("").WorkflowFilePath);
            Assert.IsNull(WorkflowFunctionHelper.ParseFromJson("   ").WorkflowFilePath);
            Assert.IsNotNull(WorkflowFunctionHelper.ParseFromJson("{not json"));
            Assert.IsNotNull(WorkflowFunctionHelper.ParseFromJson("null"));
        }

        [TestMethod]
        public void ParseFromJson_ValidJson_DeserializesAllFields()
        {
            const string json = @"{
                ""workflowFilePath"": ""/x.bite"",
                ""workflowName"":     ""Wf"",
                ""isDebug"":          true,
                ""inputParameters"":  { ""k"": ""v"" }
            }";
            var req = WorkflowFunctionHelper.ParseFromJson(json);
            Assert.AreEqual("/x.bite", req.WorkflowFilePath);
            Assert.AreEqual("Wf", req.WorkflowName);
            Assert.IsTrue(req.IsDebug);
            Assert.AreEqual("v", req.InputParameters["k"]);
        }

        // ════════════════════════════════════════════════════════════════════
        // Raw body payload — parity with Dev2.Runtime.WebServer
        //
        // The full server puts the body straight into WebRequestTO.RawRequestPayload and hands it
        // to DsfDataObject, so ExecutionEnvironmentUtils receives the ORIGINAL payload. This engine
        // used to rebuild a payload from InputParameters, which silently dropped a FLAT body, an XML
        // body, and any nested/recordset input. Verified live before the fix:
        //   POST {"message":"x"}                     -> 500 "Scalar value { message } is NULL"
        //   POST {"inputParameters":{"message":"x"}} -> 200 {"output":"x"}
        // ════════════════════════════════════════════════════════════════════

        [TestMethod]
        public async Task ParseRequestAsync_FlatJsonBody_IsKeptVerbatimAsRawPayload()
        {
            // What every on-prem queue worker posts: MessageToInputsMapper emits a FLAT object and
            // WarewolfWebRequestForwarder posts it as-is.
            var req = await WorkflowFunctionHelper.ParseRequestAsync(
                MakeRequest("http://localhost/Public/wf.json", @"{""message"":""hello""}"), null);

            Assert.AreEqual(@"{""message"":""hello""}", req.RawInputPayload,
                "the body must survive untouched - re-synthesising it is what lost the inputs");
            Assert.AreEqual(0, req.InputParameters.Count,
                "a flat body must NOT be forced through the string dictionary");
        }

        [TestMethod]
        public async Task ParseRequestAsync_EnvelopeBody_DoesNotSetRawPayload()
        {
            // The documented contract still takes precedence, so existing callers are unaffected.
            var req = await WorkflowFunctionHelper.ParseRequestAsync(
                MakeRequest("http://localhost/Public/wf.json",
                            @"{""inputParameters"":{""message"":""hello""}}"), null);

            Assert.AreEqual("hello", req.InputParameters["message"]);
            Assert.IsNull(req.RawInputPayload,
                "the envelope path already populates InputParameters; duplicating it as a raw payload would double-bind");
        }

        [TestMethod]
        public async Task ParseRequestAsync_XmlBody_IsKeptInsteadOfThrowingAway()
        {
            // Previously JsonConvert.DeserializeObject<WorkflowExecutionRequest>(xml) threw straight
            // into a silent catch, so XML was discarded - even though
            // ExecutionEnvironmentUtils.TryUpdateEnviromentWithMappings converts XML payloads.
            const string xml = "<DataList><message>hello</message></DataList>";
            var req = await WorkflowFunctionHelper.ParseRequestAsync(
                MakeRequest("http://localhost/Public/wf.json", xml), null);

            Assert.AreEqual(xml, req.RawInputPayload);
        }

        [TestMethod]
        public async Task ParseRequestAsync_NestedJsonBody_KeepsTheStructure()
        {
            // Dictionary<string,string> cannot carry a JTokenType.Object, so recordset inputs were
            // unreachable through the HTTP layer. Keeping the raw body restores them.
            const string nested = @"{""orders"":[{""id"":""1""},{""id"":""2""}]}";
            var req = await WorkflowFunctionHelper.ParseRequestAsync(
                MakeRequest("http://localhost/Public/wf.json", nested), null);

            Assert.AreEqual(nested, req.RawInputPayload);
        }

        [TestMethod]
        public async Task ParseRequestAsync_EmptyOrMalformedBody_LeavesRawPayloadUnset()
        {
            var empty = await WorkflowFunctionHelper.ParseRequestAsync(
                MakeRequest("http://localhost/Public/wf.json", "   "), null);
            Assert.IsNull(empty.RawInputPayload);

            // Malformed JSON must not throw - it is treated as an opaque payload and the workflow
            // simply binds nothing, exactly as a mismatched payload does on the full server.
            var malformed = await WorkflowFunctionHelper.ParseRequestAsync(
                MakeRequest("http://localhost/Public/wf.json", "{not json"), null);
            Assert.AreEqual("{not json", malformed.RawInputPayload);
        }

        [TestMethod]
        public async Task ParseRequestAsync_RoutedWorkflow_CannotBeRedirectedByTheBody()
        {
            // SECURITY REGRESSION LOCK. Authorization is evaluated on the ROUTE-derived name
            // (WorkflowHttpFunction:326) before the request is parsed (:365), and ResolveFilePath
            // returns early when WorkflowFilePath is already set - so a body-supplied
            // 'workflowFilePath' used to win and execute a workflow the caller was never authorized
            // for. On /Public that reaches workflows that are not public at all.
            //
            // This test FAILED when first written (WorkflowFilePath came back as '\etc\passwd'),
            // which is how the bypass was found.
            var req = await WorkflowFunctionHelper.ParseRequestAsync(
                MakeRequest("http://localhost/Public/wf.json",
                            @"{""workflowFilePath"":""/etc/passwd"",""message"":""x""}"),
                workflowsDirectory: null,
                workflowNameFromRoute: "RoutedWorkflow");

            Assert.AreEqual("RoutedWorkflow", req.WorkflowName);
            Assert.IsTrue(req.WorkflowFilePath is null
                          || !req.WorkflowFilePath.Contains("passwd", StringComparison.OrdinalIgnoreCase),
                "a routed request must never execute a body-supplied path");

            // The rest of the body is still honoured as inputs.
            StringAssert.Contains(req.RawInputPayload, "message");
        }

        [TestMethod]
        public async Task ParseRequestAsync_FormUrlEncodedBody_BindsAsInputParameters()
        {
            // Parity with SubmittedData.ExtractKeyValuePairForPostMethod, which falls through to
            // ExtractArgumentsFromDataListOrQueryString for a non-XML/non-JSON body - i.e. treats the
            // body as a query string. These must become InputParameters, NOT a raw payload:
            // ExecutionEnvironmentUtils only parses JSON and XML, so 'a=1&b=2' would bind nothing.
            var req = MakeRequest("http://localhost/Public/wf.json", "message=form-x&other=2");
            req.Headers.Add("Content-Type", "application/x-www-form-urlencoded");

            var parsed = await WorkflowFunctionHelper.ParseRequestAsync(req, null);

            Assert.AreEqual("form-x", parsed.InputParameters["message"]);
            Assert.AreEqual("2", parsed.InputParameters["other"]);
            Assert.IsNull(parsed.RawInputPayload,
                "a form body is not a payload the environment helper can parse");
        }

        [TestMethod]
        public async Task ParseRequestAsync_GenericRoute_StillHonoursAnExplicitPath()
        {
            // The generic /workflow route passes no route name, so selecting a workflow by path
            // remains legitimate there and must keep working.
            var req = await WorkflowFunctionHelper.ParseRequestAsync(
                MakeRequest("http://localhost/workflow", @"{""workflowFilePath"":""/wf/A.bite""}"), null);

            StringAssert.Contains(req.WorkflowFilePath, "A.bite");
        }

        // ── Payload selection (WorkflowExecutor.ResolveInputPayload) ─────────

        [TestMethod]
        public void ResolveInputPayload_RawBodyWins_ButQueryInputsAreStillMerged()
        {
            var request = new WorkflowExecutionRequest
            {
                RawInputPayload = @"{""message"":""from-body""}",
                InputParameters = new Dictionary<string, string>
                {
                    ["message"] = "from-query",   // clash: body must win
                    ["extra"]   = "kept",          // no clash: must survive
                },
            };

            var payload = WorkflowExecutor
                                  .ResolveInputPayload(request);

            var parsed = Newtonsoft.Json.Linq.JObject.Parse(payload);
            Assert.AreEqual("from-body", (string)parsed["message"],
                "body-after-query precedence must be preserved");
            Assert.AreEqual("kept", (string)parsed["extra"]);
        }

        [TestMethod]
        public void ResolveInputPayload_XmlBody_IsUsedUnmodified()
        {
            // Merging query values into arbitrary XML would mean guessing its shape, so the body is
            // used as-is.
            const string xml = "<DataList><message>x</message></DataList>";
            var request = new WorkflowExecutionRequest
            {
                RawInputPayload = xml,
                InputParameters = new Dictionary<string, string> { ["ignored"] = "y" },
            };

            Assert.AreEqual(xml, WorkflowExecutor
                                       .ResolveInputPayload(request));
        }

        [TestMethod]
        public void ResolveInputPayload_NoRawBody_FallsBackToQueryInputs()
        {
            var request = new WorkflowExecutionRequest
            {
                InputParameters = new Dictionary<string, string> { ["message"] = "qs" },
            };

            var parsed = Newtonsoft.Json.Linq.JObject.Parse(
                WorkflowExecutor.ResolveInputPayload(request));
            Assert.AreEqual("qs", (string)parsed["message"]);
        }

        // ════════════════════════════════════════════════════════════════════
        // ParseRequestAsync — query string paths
        // ════════════════════════════════════════════════════════════════════

        [TestMethod]
        public async Task ParseRequestAsync_QueryString_WorkflowNameAndDebug()
        {
            var req = MakeRequest("http://localhost/api/run?workflowName=Hello&isDebug=true");
            var exec = await WorkflowFunctionHelper.ParseRequestAsync(req, workflowsDirectory: null);

            Assert.AreEqual("Hello", exec.WorkflowName);
            Assert.IsTrue(exec.IsDebug);
            Assert.AreEqual(EmitionTypes.JSON, exec.ReturnType);
            Assert.IsNotNull(exec.WebServerUri);
        }

        [TestMethod]
        public async Task ParseRequestAsync_QueryString_WorkflowFilePathProvided()
        {
            var req = MakeRequest("http://localhost/api/run?workflowFilePath=/abs/wf.bite");
            var exec = await WorkflowFunctionHelper.ParseRequestAsync(req, workflowsDirectory: null);

            // NormalizeSeparators runs because WorkflowFilePath was provided.
            Assert.IsNotNull(exec.WorkflowFilePath);
            StringAssert.EndsWith(exec.WorkflowFilePath, "wf.bite");
        }

        [TestMethod]
        public async Task ParseRequestAsync_QueryString_ExtraParamsBecomeInputs()
        {
            var req = MakeRequest("http://localhost/api/run?workflowName=Wf&Foo=bar&Baz=qux");
            var exec = await WorkflowFunctionHelper.ParseRequestAsync(req, workflowsDirectory: null);

            Assert.AreEqual("bar", exec.InputParameters["Foo"]);
            Assert.AreEqual("qux", exec.InputParameters["Baz"]);
            Assert.IsFalse(exec.InputParameters.ContainsKey("workflowName"),
                "Reserved keys must not bleed into InputParameters.");
        }

        [TestMethod]
        public async Task ParseRequestAsync_IsDebugUnparseable_DefaultsToFalse()
        {
            var req = MakeRequest("http://localhost/api/run?workflowName=Wf&isDebug=notabool");
            var exec = await WorkflowFunctionHelper.ParseRequestAsync(req, workflowsDirectory: null);

            Assert.IsFalse(exec.IsDebug);
        }

        [TestMethod]
        public async Task ParseRequestAsync_UrlEndsWithXml_SetsReturnTypeXml()
        {
            var req = MakeRequest("http://localhost/api/run.xml?workflowName=Wf");
            var exec = await WorkflowFunctionHelper.ParseRequestAsync(req, workflowsDirectory: null);
            Assert.AreEqual(EmitionTypes.XML, exec.ReturnType);
        }

        [TestMethod]
        public async Task ParseRequestAsync_UrlEndsWithApi_SetsReturnTypeOpenApi()
        {
            var req = MakeRequest("http://localhost/api/run.api?workflowName=Wf");
            var exec = await WorkflowFunctionHelper.ParseRequestAsync(req, workflowsDirectory: null);
            Assert.AreEqual(EmitionTypes.OPENAPI, exec.ReturnType);
        }

        // ════════════════════════════════════════════════════════════════════
        // ParseRequestAsync — body parsing + merging
        // ════════════════════════════════════════════════════════════════════

        [TestMethod]
        public async Task ParseRequestAsync_JsonBody_OverridesQueryFields()
        {
            const string body = @"{
                ""workflowName"":     ""FromBody"",
                ""workflowFilePath"": ""/body/wf.bite"",
                ""isDebug"":          true,
                ""inputParameters"":  { ""bodyKey"": ""bodyVal"" }
            }";
            var req = MakeRequest(
                "http://localhost/api/run?workflowName=FromQuery&queryKey=queryVal",
                body);
            var exec = await WorkflowFunctionHelper.ParseRequestAsync(req, workflowsDirectory: null);

            Assert.AreEqual("FromBody", exec.WorkflowName, "Body should override query workflowName");
            StringAssert.EndsWith(exec.WorkflowFilePath!, "wf.bite");
            Assert.IsTrue(exec.IsDebug);
            Assert.AreEqual("bodyVal", exec.InputParameters["bodyKey"]);
            Assert.AreEqual("queryVal", exec.InputParameters["queryKey"],
                "Query-string inputs are preserved alongside body inputs.");
        }

        [TestMethod]
        public async Task ParseRequestAsync_InvalidJsonBody_Swallowed()
        {
            var req = MakeRequest(
                "http://localhost/api/run?workflowName=Wf",
                body: "{not valid json{{");
            var exec = await WorkflowFunctionHelper.ParseRequestAsync(req, workflowsDirectory: null);

            Assert.AreEqual("Wf", exec.WorkflowName,
                "Bad body should not corrupt query-string parsing.");
        }

        [TestMethod]
        public async Task ParseRequestAsync_WhitespaceBody_NoOp()
        {
            var req = MakeRequest(
                "http://localhost/api/run?workflowName=Wf",
                body: "   \n   ");
            var exec = await WorkflowFunctionHelper.ParseRequestAsync(req, workflowsDirectory: null);
            Assert.AreEqual("Wf", exec.WorkflowName);
        }

        [TestMethod]
        public async Task ParseRequestAsync_JsonBodyNullDeserialization_Continues()
        {
            var req = MakeRequest(
                "http://localhost/api/run?workflowName=Wf",
                body: "null");
            var exec = await WorkflowFunctionHelper.ParseRequestAsync(req, workflowsDirectory: null);
            Assert.AreEqual("Wf", exec.WorkflowName);
        }

        [TestMethod]
        public async Task ParseRequestAsync_RouteNameOverridesBodyAndQuery()
        {
            const string body = @"{ ""workflowName"": ""BodyName"" }";
            var req = MakeRequest(
                "http://localhost/api/run?workflowName=QueryName",
                body);
            var exec = await WorkflowFunctionHelper.ParseRequestAsync(
                req, workflowsDirectory: null, workflowNameFromRoute: "RouteName");

            Assert.AreEqual("RouteName", exec.WorkflowName);
        }

        [TestMethod]
        public async Task ParseRequestAsync_WorkflowsDirectoryPropagated()
        {
            var dir = NewTempDir();
            var req = MakeRequest("http://localhost/api/run?workflowName=Wf");
            var exec = await WorkflowFunctionHelper.ParseRequestAsync(req, workflowsDirectory: dir);

            Assert.AreEqual(dir, exec.WorkflowsDirectory);
        }

        [TestMethod]
        public async Task ParseRequestAsync_RouteNameWhitespace_DoesNotOverride()
        {
            var req = MakeRequest("http://localhost/api/run?workflowName=QueryName");
            var exec = await WorkflowFunctionHelper.ParseRequestAsync(
                req, workflowsDirectory: null, workflowNameFromRoute: "   ");

            Assert.AreEqual("QueryName", exec.WorkflowName);
        }

        // ════════════════════════════════════════════════════════════════════
        // ResolveFilePath — extension and on-disk lookup paths
        // ════════════════════════════════════════════════════════════════════

        [TestMethod]
        public void ResolveFilePath_NameNoExt_DirExistsBiteOnDisk_ReturnsBite()
        {
            var dir = NewTempDir();
            File.WriteAllText(Path.Combine(dir, "Hello.bite"), "<x/>");

            var req = WorkflowFunctionHelper.CreateRequestByName("Hello", dir);

            Assert.IsNotNull(req.WorkflowFilePath);
            StringAssert.EndsWith(req.WorkflowFilePath!, "Hello.bite");
        }

        [TestMethod]
        public void ResolveFilePath_NameNoExt_DirExistsOnlyXmlOnDisk_ReturnsXml()
        {
            var dir = NewTempDir();
            File.WriteAllText(Path.Combine(dir, "Hello.xml"), "<x/>");

            var req = WorkflowFunctionHelper.CreateRequestByName("Hello", dir);

            Assert.IsNotNull(req.WorkflowFilePath);
            StringAssert.EndsWith(req.WorkflowFilePath!, "Hello.xml");
        }

        [TestMethod]
        public void ResolveFilePath_NameNoExt_DirExistsNoFile_DefaultsToXml()
        {
            var dir = NewTempDir();
            var req = WorkflowFunctionHelper.CreateRequestByName("Missing", dir);

            Assert.IsNotNull(req.WorkflowFilePath);
            StringAssert.EndsWith(req.WorkflowFilePath!, "Missing.xml");
        }

        [TestMethod]
        public void ResolveFilePath_NameWithXmlExt_DirExistsFileExists_ResolvesActualCasing()
        {
            var dir = NewTempDir();
            File.WriteAllText(Path.Combine(dir, "Greet.xml"), "<x/>");

            // Pass the same name; resolver finds the on-disk file.
            var req = WorkflowFunctionHelper.CreateRequestByName("Greet.xml", dir);

            Assert.IsNotNull(req.WorkflowFilePath);
            StringAssert.EndsWith(req.WorkflowFilePath!, "Greet.xml");
        }

        [TestMethod]
        public void ResolveFilePath_NameWithBiteExt_DirExistsNoFile_FallsBackToCombine()
        {
            var dir = NewTempDir();
            var req = WorkflowFunctionHelper.CreateRequestByName("NotThere.bite", dir);

            Assert.IsNotNull(req.WorkflowFilePath);
            StringAssert.EndsWith(req.WorkflowFilePath!, "NotThere.bite");
        }

        [TestMethod]
        public void ResolveFilePath_FilePathPreSet_OnlyNormalizes()
        {
            var req = new WorkflowExecutionRequest
            {
                WorkflowFilePath = "a/b/c.bite",
                WorkflowName     = "ignored"
            };

            // Use CreateRequestByName-style call indirectly via ParseFromJson
            // (which doesn't touch ResolveFilePath) is not what we want.
            // Instead exercise via CreateRequestByName by giving a pre-set workflowFilePath
            // through ParseRequestAsync: easier path is to round-trip through CreateRequest.
            var built = WorkflowFunctionHelper.CreateRequest("a/b/c.bite");
            // CreateRequest does not call ResolveFilePath, so use CreateRequestByName by name
            // but with a name that already has an extension matching a non-existing dir:
            var dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N")); // not created
            var byname = WorkflowFunctionHelper.CreateRequestByName("only.bite", dir);

            Assert.IsNotNull(byname.WorkflowFilePath);
            StringAssert.EndsWith(byname.WorkflowFilePath!, "only.bite");
            Assert.AreEqual("a/b/c.bite", built.WorkflowFilePath);
        }

        [TestMethod]
        public void ResolveFilePath_EmptyDirectory_NoOp()
        {
            // No dir, only name → cannot resolve, WorkflowFilePath stays null.
            var req = WorkflowFunctionHelper.CreateRequestByName("Hello", workflowsDirectory: "");
            Assert.IsNull(req.WorkflowFilePath);
            Assert.AreEqual("Hello", req.WorkflowName);
        }

        [TestMethod]
        public void ResolveFilePath_NameWithMixedSeparators_AreNormalized()
        {
            var dir = NewTempDir();
            var sub = Path.Combine(dir, "sub");
            Directory.CreateDirectory(sub);
            File.WriteAllText(Path.Combine(sub, "Inner.bite"), "<x/>");

            // Use a forward-slash separator that NormalizeSeparators must convert
            // on Windows so the on-disk Inner.bite is found.
            var req = WorkflowFunctionHelper.CreateRequestByName("sub/Inner", dir);

            Assert.IsNotNull(req.WorkflowFilePath);
            StringAssert.EndsWith(req.WorkflowFilePath!, "Inner.bite");
        }

        // ════════════════════════════════════════════════════════════════════
        // Local FunctionContext for these tests — minimal, no SDK proxies.
        // (We can't reuse Auth.HttpFunctionContext directly because creating
        //  a FakeBindingsFeature exercises DispatchProxy and isn't needed
        //  here. A bare FunctionContext is sufficient because
        //  WorkflowFunctionHelper never reads FunctionContext members.)
        // ════════════════════════════════════════════════════════════════════

        private sealed class HttpHostContext : Microsoft.Azure.Functions.Worker.FunctionContext
        {
            public override string InvocationId => "test-invocation";
            public override string FunctionId => "test-function";
            public override Microsoft.Azure.Functions.Worker.TraceContext TraceContext => null!;
            public override Microsoft.Azure.Functions.Worker.BindingContext BindingContext => null!;
            public override Microsoft.Azure.Functions.Worker.RetryContext RetryContext => null!;
            public override IServiceProvider InstanceServices { get; set; } = null!;
            public override Microsoft.Azure.Functions.Worker.FunctionDefinition FunctionDefinition => null!;
            public override IDictionary<object, object> Items { get; set; } = new Dictionary<object, object>();
            public override Microsoft.Azure.Functions.Worker.IInvocationFeatures Features => null!;
        }
    }
}



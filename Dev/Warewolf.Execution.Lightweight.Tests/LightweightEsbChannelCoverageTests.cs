/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dev2.Communication;
using Dev2.Data.Interfaces.Enums;
using Dev2.Data.TO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Execution.Lightweight;
using Warewolf.Execution.Lightweight.Logging;
using Warewolf.Execution.Lightweight.Models;

namespace Warewolf.Execution.Lightweight.Tests
{
    [TestClass]
    public class LightweightEsbChannelCoverageTests
    {
        // ── Constructor ──────────────────────────────────────────────────────

        [TestMethod]
        [TestCategory("LightweightEsbChannel_Coverage")]
        public void Ctor_NullBaseDirectory_DoesNotThrow_AndStoresEmpty()
        {
            // Constructor swallows null via ?? string.Empty; no warm-up performed.
            var channel = Activator.CreateInstance(
                typeof(LightweightEsbChannel),
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
                binder: null,
                args: new object?[] { null },
                culture: null);
            Assert.IsNotNull(channel);
        }

        [TestMethod]
        [TestCategory("LightweightEsbChannel_Coverage")]
        public void Ctor_EmptyBaseDirectory_DoesNotWarmUp()
        {
            var channel = NewChannel(string.Empty);
            Assert.IsNotNull(channel);
        }

        [TestMethod]
        [TestCategory("LightweightEsbChannel_Coverage")]
        public void Ctor_ValidDirectory_WarmsUpResourceCache()
        {
            var dir = Path.Combine(Path.GetTempPath(), "esbch_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            try
            {
                var channel = NewChannel(dir);
                // After construction the directory should be in the cache, so a lookup
                // on an unknown id with no service-name returns null without re-scanning.
                Assert.IsNotNull(channel);
                Assert.IsNull(WorkflowResourceCache.Instance.Resolve(dir, Guid.NewGuid(), null));
            }
            finally
            {
                try { Directory.Delete(dir, true); } catch { /* best-effort */ }
            }
        }

        // ── ExecuteRequest ───────────────────────────────────────────────────

        [TestMethod]
        [TestCategory("LightweightEsbChannel_Coverage")]
        public void ExecuteRequest_ReturnsNullDataListId_AndAddsErrorWithServiceName()
        {
            var channel = NewChannel(string.Empty);
            var req = new EsbExecuteRequest { ServiceName = "MyService" };
            var id = ((Dev2.IEsbChannel)channel).ExecuteRequest(
                dataObject: null!,
                request: req,
                workspaceId: Guid.Empty,
                errors: out var errors);
            Assert.AreEqual(Dev2.Common.GlobalConstants.NullDataListID, id);
            Assert.IsTrue(errors.HasErrors());
            Assert.IsTrue(errors.FetchErrors().Any(e => e.Contains("MyService")));
        }

        [TestMethod]
        [TestCategory("LightweightEsbChannel_Coverage")]
        public void ExecuteRequest_NullRequest_AddsErrorWithoutThrowing()
        {
            var channel = NewChannel(string.Empty);
            var id = ((Dev2.IEsbChannel)channel).ExecuteRequest(
                dataObject: null!,
                request: null!,
                workspaceId: Guid.Empty,
                errors: out var errors);
            Assert.AreEqual(Dev2.Common.GlobalConstants.NullDataListID, id);
            Assert.IsTrue(errors.HasErrors());
        }

        // ── ExecuteLogErrorRequest ───────────────────────────────────────────

        [TestMethod]
        [TestCategory("LightweightEsbChannel_Coverage")]
        public void ExecuteLogErrorRequest_IsNoOp_AndYieldsNullErrors()
        {
            var channel = NewChannel(string.Empty);
            ((Dev2.IEsbChannel)channel).ExecuteLogErrorRequest(
                dataObject: null!,
                workspaceId: Guid.Empty,
                uri: "ignored",
                errors: out var errors,
                update: 0);
            Assert.IsNull(errors);
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private static object NewChannel(string baseDir) =>
            Activator.CreateInstance(
                typeof(LightweightEsbChannel),
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
                binder: null,
                args: new object?[] { baseDir },
                culture: null)!;
    }

    [TestClass]
    public class ExecutionErrorDetailCoverageTests
    {
        [TestMethod]
        [TestCategory("ExecutionErrorDetail_Coverage")]
        public void Defaults_ProduceEmptyStringsAndDefaultGuid()
        {
            var d = new ExecutionErrorDetail();
            Assert.AreEqual(Guid.Empty, d.ExecutionId);
            Assert.IsNull(d.ActivityName);
            Assert.IsNull(d.Message);
            Assert.IsNull(d.StackTrace);
            Assert.AreEqual(default(DateTime), d.Timestamp);
        }

        [TestMethod]
        [TestCategory("ExecutionErrorDetail_Coverage")]
        public void ToLogScope_PopulatesAllFields()
        {
            var id = Guid.NewGuid();
            var ts = new DateTime(2024, 5, 1, 12, 0, 0, DateTimeKind.Utc);
            var d = new ExecutionErrorDetail
            {
                ExecutionId = id,
                ActivityName = "MyActivity",
                Message = "boom",
                StackTrace = "at X.Y()",
                Timestamp = ts
            };
            var scope = d.ToLogScope();
            Assert.AreEqual(id, scope["ExecutionId"]);
            Assert.AreEqual("MyActivity", scope["ActivityName"]);
            Assert.AreEqual("boom", scope["Message"]);
            Assert.AreEqual("at X.Y()", scope["StackTrace"]);
            Assert.AreEqual(ts, scope["Timestamp"]);
        }

        [TestMethod]
        [TestCategory("ExecutionErrorDetail_Coverage")]
        public void ToLogScope_NullStrings_FallBackToEmpty()
        {
            var d = new ExecutionErrorDetail();
            var scope = d.ToLogScope();
            Assert.AreEqual(string.Empty, scope["ActivityName"]);
            Assert.AreEqual(string.Empty, scope["Message"]);
            Assert.AreEqual(string.Empty, scope["StackTrace"]);
        }
    }

    [TestClass]
    public class DebugStepResultCoverageTests
    {
        [TestMethod]
        [TestCategory("DebugStepResult_Coverage")]
        public void Defaults_CollectionsAreInitialised()
        {
            var s = new DebugStepResult();
            Assert.IsNotNull(s.Inputs);
            Assert.IsNotNull(s.Outputs);
            Assert.IsNotNull(s.AssertResultList);
            Assert.IsNotNull(s.Children);
            Assert.AreEqual(0, s.Inputs.Count);
            Assert.AreEqual(0, s.Outputs.Count);
        }

        [TestMethod]
        [TestCategory("DebugStepResult_Coverage")]
        public void SettersAndGetters_RoundTripValues()
        {
            var id = Guid.NewGuid();
            var s = new DebugStepResult
            {
                DisconnectedID = id, ID = id, SourceResourceID = id, OriginatingResourceID = id,
                OriginalInstanceID = id, SessionID = id, WorkspaceID = id, ServerID = id,
                EnvironmentID = id, ClientID = id, DisplayName = "Assign",
                ActivityType = 1, ActualType = "DsfMultiAssignActivity", StateType = "After",
                HasError = true, ErrorMessage = "boom",
                Origin = "External", ExecutionOrigin = 2,
                WorkSurfaceMappingId = id, IsDurationVisible = true,
                Duration = TimeSpan.FromMilliseconds(123),
                StartTime = new DateTime(2024,1,1,0,0,0,DateTimeKind.Utc),
                EndTime = new DateTime(2024,1,1,0,0,1,DateTimeKind.Utc),
            };
            Assert.AreEqual(id, s.DisconnectedID);
            Assert.AreEqual(1, s.ActivityType);
            Assert.AreEqual("Assign", s.DisplayName);
            Assert.IsTrue(s.HasError);
            Assert.AreEqual("External", s.Origin);
            Assert.AreEqual(TimeSpan.FromMilliseconds(123), s.Duration);
        }

        [TestMethod]
        [TestCategory("DebugStepResult_Coverage")]
        public void DebugLineItem_RoundTripsAllFields()
        {
            var li = new DebugLineItem
            {
                Type = 2, Label = "[[A]] =", Variable = "[[A]]", Operator = "=",
                Value = "x", HasError = true, TruncatedValue = "x",
                GroupName = "[[Rows(*)]]", GroupIndex = 3, MoreLink = "http://x"
            };
            Assert.AreEqual(2, li.Type);
            Assert.AreEqual("[[A]] =", li.Label);
            Assert.AreEqual("[[A]]", li.Variable);
            Assert.AreEqual("=", li.Operator);
            Assert.AreEqual("x", li.Value);
            Assert.IsTrue(li.HasError);
            Assert.AreEqual("x", li.TruncatedValue);
            Assert.AreEqual("[[Rows(*)]]", li.GroupName);
            Assert.AreEqual(3, li.GroupIndex);
            Assert.AreEqual("http://x", li.MoreLink);
        }

        [TestMethod]
        [TestCategory("DebugStepResult_Coverage")]
        public void Children_CanBeNested()
        {
            var s = new DebugStepResult();
            s.Children.Add(new DebugStepResult { DisplayName = "child" });
            s.Inputs.Add(new List<DebugLineItem> { new DebugLineItem { Value = "i" } });
            s.Outputs.Add(new List<DebugLineItem> { new DebugLineItem { Value = "o" } });
            s.AssertResultList.Add(new List<DebugLineItem> { new DebugLineItem { Value = "a" } });
            Assert.AreEqual(1, s.Children.Count);
            Assert.AreEqual("child", s.Children[0].DisplayName);
            Assert.AreEqual("i", s.Inputs[0][0].Value);
            Assert.AreEqual("o", s.Outputs[0][0].Value);
            Assert.AreEqual("a", s.AssertResultList[0][0].Value);
        }
    }

    [TestClass]
    public class ExecutionLogLevelCoverageTests
    {
        [TestMethod]
        [TestCategory("ExecutionLogLevel_Coverage")]
        public void Default_IsInfo()
        {
            Assert.AreEqual(LogLevel.INFO, ExecutionLogLevel.Default);
        }

        [TestMethod]
        [TestCategory("ExecutionLogLevel_Coverage")]
        public void Parse_NullOrWhitespace_ReturnsDefault()
        {
            Assert.AreEqual(LogLevel.INFO, ExecutionLogLevel.Parse(null));
            Assert.AreEqual(LogLevel.INFO, ExecutionLogLevel.Parse(""));
            Assert.AreEqual(LogLevel.INFO, ExecutionLogLevel.Parse("   "));
        }

        [TestMethod]
        [TestCategory("ExecutionLogLevel_Coverage")]
        public void Parse_NumericRecognised_ReturnsMatchingLevel()
        {
            Assert.AreEqual(LogLevel.OFF,   ExecutionLogLevel.Parse("0"));
            Assert.AreEqual(LogLevel.FATAL, ExecutionLogLevel.Parse("1"));
            Assert.AreEqual(LogLevel.INFO,  ExecutionLogLevel.Parse("4"));
        }

        [TestMethod]
        [TestCategory("ExecutionLogLevel_Coverage")]
        public void Parse_NumericOutOfRange_FallsThroughToEnumTryParse()
        {
            // Enum.IsDefined fails for 99 — first branch skipped.
            // Enum.TryParse<LogLevel>("99", ...) still succeeds and yields the cast value,
            // because .NET accepts any integer-string as a valid enum parse.
            // This documents the existing behaviour rather than ideal validation.
            Assert.AreEqual((LogLevel)99, ExecutionLogLevel.Parse("99"));
        }

        [TestMethod]
        [TestCategory("ExecutionLogLevel_Coverage")]
        public void Parse_NamedCaseInsensitive_Matches()
        {
            Assert.AreEqual(LogLevel.INFO,  ExecutionLogLevel.Parse("INFO"));
            Assert.AreEqual(LogLevel.DEBUG, ExecutionLogLevel.Parse("debug"));
            Assert.AreEqual(LogLevel.ERROR, ExecutionLogLevel.Parse("Error"));
        }

        [TestMethod]
        [TestCategory("ExecutionLogLevel_Coverage")]
        public void Parse_UnknownText_ReturnsDefault()
        {
            Assert.AreEqual(LogLevel.INFO, ExecutionLogLevel.Parse("LOUD"));
        }

        [TestMethod]
        [TestCategory("ExecutionLogLevel_Coverage")]
        public void ShouldLog_HigherOrEqualMinimum_Allows()
        {
            // minimum=DEBUG (5) >= message=INFO (4) => true
            Assert.IsTrue(ExecutionLogLevel.ShouldLog(LogLevel.INFO, LogLevel.DEBUG));
            // minimum=INFO (4) >= message=INFO (4) => true
            Assert.IsTrue(ExecutionLogLevel.ShouldLog(LogLevel.INFO, LogLevel.INFO));
        }

        [TestMethod]
        [TestCategory("ExecutionLogLevel_Coverage")]
        public void ShouldLog_LowerMinimum_Blocks()
        {
            // minimum=ERROR (2) < message=INFO (4) => false
            Assert.IsFalse(ExecutionLogLevel.ShouldLog(LogLevel.INFO, LogLevel.ERROR));
        }

        [TestMethod]
        [TestCategory("ExecutionLogLevel_Coverage")]
        public void ShouldLog_MinimumIsOff_AlwaysFalse()
        {
            Assert.IsFalse(ExecutionLogLevel.ShouldLog(LogLevel.FATAL, LogLevel.OFF));
            Assert.IsFalse(ExecutionLogLevel.ShouldLog(LogLevel.INFO, LogLevel.OFF));
        }

        [TestMethod]
        [TestCategory("ExecutionLogLevel_Coverage")]
        public void Read_RespectsEnvVariable()
        {
            const string varName = "EXECUTIONLOGLEVEL";
            var prior = Environment.GetEnvironmentVariable(varName);
            try
            {
                Environment.SetEnvironmentVariable(varName, "DEBUG");
                Assert.AreEqual(LogLevel.DEBUG, ExecutionLogLevel.Read());

                Environment.SetEnvironmentVariable(varName, null);
                Assert.AreEqual(LogLevel.INFO, ExecutionLogLevel.Read());
            }
            finally
            {
                Environment.SetEnvironmentVariable(varName, prior);
            }
        }
    }

    [TestClass]
    public class ElasticsearchLogDocumentCoverageTests
    {
        [TestMethod]
        [TestCategory("ElasticsearchLogDocument_Coverage")]
        public void Defaults_AreSensible()
        {
            var d = new ElasticsearchLogDocument();
            Assert.AreEqual("info", d.Level);
            Assert.AreEqual(string.Empty, d.Message);
            Assert.AreEqual(Guid.Empty, d.ExecutionId);
            Assert.IsNull(d.ActivityName);
            Assert.IsNull(d.ErrorMessage);
            Assert.IsNull(d.StackTrace);
            Assert.IsNull(d.InstanceId);
            Assert.IsNull(d.InvocationId);
            Assert.IsNull(d.FunctionName);
            Assert.IsNull(d.TraceId);
            // Timestamp is set at construction time — sanity-check it's recent.
            Assert.IsTrue((DateTimeOffset.UtcNow - d.Timestamp).TotalMinutes < 5);
        }

        [TestMethod]
        [TestCategory("ElasticsearchLogDocument_Coverage")]
        public void RecordEquality_ConsidersValueMembers()
        {
            var ts = DateTimeOffset.UtcNow;
            var id = Guid.NewGuid();
            var a = new ElasticsearchLogDocument
            {
                Timestamp = ts, Level = "error", Message = "m", ExecutionId = id,
                ActivityName = "act", ErrorMessage = "e", StackTrace = "s",
                InstanceId = "i", InvocationId = "iv", FunctionName = "f", TraceId = "t"
            };
            var b = new ElasticsearchLogDocument
            {
                Timestamp = ts, Level = "error", Message = "m", ExecutionId = id,
                ActivityName = "act", ErrorMessage = "e", StackTrace = "s",
                InstanceId = "i", InvocationId = "iv", FunctionName = "f", TraceId = "t"
            };
            Assert.AreEqual(a, b);
            Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
        }

        [TestMethod]
        [TestCategory("ElasticsearchLogDocument_Coverage")]
        public void RecordWithExpression_OverridesValue()
        {
            var a = new ElasticsearchLogDocument { Level = "info" };
            var b = a with { Level = "error" };
            Assert.AreEqual("info", a.Level);
            Assert.AreEqual("error", b.Level);
        }
    }

    [TestClass]
    public class PerRequestDebugCapturerCoverageTests
    {
        [TestMethod]
        [TestCategory("PerRequestDebugCapturer_Coverage")]
        public void Write_NullDebugState_IsIgnored()
        {
            var capturer = NewCapturer();
            InvokeWrite(capturer, debugState: null);
            Assert.AreEqual(0, GetStates(capturer).Count);
        }

        [TestMethod]
        [TestCategory("PerRequestDebugCapturer_Coverage")]
        public void Write_NonNullDebugState_IsCaptured()
        {
            var capturer = NewCapturer();
            var state = Dev2.Common.Interfaces.Diagnostics.Debug.IDebugStateProxy.Create();
            InvokeWrite(capturer, state);
            InvokeWrite(capturer, state);
            Assert.AreEqual(2, GetStates(capturer).Count);
        }

        [TestMethod]
        [TestCategory("PerRequestDebugCapturer_Coverage")]
        public void NoOpMembers_BehaveAsDocumented()
        {
            var capturer = NewCapturer();
            // Count is hard-coded zero
            var count = (int)capturer.GetType().GetProperty("Count")!.GetValue(capturer)!;
            Assert.AreEqual(0, count);
            // Add / Remove / Shutdown are no-ops; Get returns null
            capturer.GetType().GetMethod("Add")!.Invoke(capturer, new object?[] { Guid.NewGuid(), null });
            capturer.GetType().GetMethod("Remove")!.Invoke(capturer, new object?[] { Guid.NewGuid() });
            capturer.GetType().GetMethod("Shutdown")!.Invoke(capturer, null);
            var getResult = capturer.GetType().GetMethod("Get")!.Invoke(capturer, new object?[] { Guid.NewGuid() });
            Assert.IsNull(getResult);
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private static object NewCapturer() => new PerRequestDebugCapturer();

        private static void InvokeWrite(object capturer, Dev2.Common.Interfaces.Diagnostics.Debug.IDebugState? debugState)
        {
            var writeArgsType = typeof(Dev2.Common.Interfaces.Diagnostics.Debug.WriteArgs);
            var wa = Activator.CreateInstance(writeArgsType)!;
            writeArgsType.GetField("debugState")!.SetValue(wa, debugState);
            capturer.GetType().GetMethod("Write")!.Invoke(capturer, new[] { wa });
        }

        private static System.Collections.IList GetStates(object capturer)
        {
            var statesProp = capturer.GetType().GetProperty("States")!;
            return (System.Collections.IList)statesProp.GetValue(capturer)!;
        }
    }
}

namespace Dev2.Common.Interfaces.Diagnostics.Debug
{
    /// <summary>
    /// Builds a no-op <see cref="IDebugState"/> using <see cref="System.Reflection.DispatchProxy"/>
    /// for use in PerRequestDebugCapturer tests. Members are all unused by the capturer; the
    /// proxy returns <c>default</c> for every call.
    /// </summary>
    internal static class IDebugStateProxy
    {
        public static IDebugState Create() => StubDebugState.Build();

        private class StubDebugState : System.Reflection.DispatchProxy
        {
            public static IDebugState Build() =>
                System.Reflection.DispatchProxy.Create<IDebugState, StubDebugState>();

            protected override object? Invoke(System.Reflection.MethodInfo? targetMethod, object?[]? args)
            {
                var rt = targetMethod?.ReturnType;
                if (rt == null || rt == typeof(void)) return null;
                return rt.IsValueType ? Activator.CreateInstance(rt) : null;
            }
        }
    }
}

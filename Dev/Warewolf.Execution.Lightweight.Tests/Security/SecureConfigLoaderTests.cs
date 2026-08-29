/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  Unit tests for SecureConfigLoader.ReadConfig's failure paths: a corrupt/undecryptable
 *  secure.config previously fell back to AllowAll with zero trace — silently indistinguishable
 *  from "no config deployed" on a live instance (observed against warewolfserver-mcp: a
 *  create_workflow response with null httpEndpoints, moments later a Public/{name} invoke that
 *  got a real permission-denied response — two requests disagreeing on whether secure.config was
 *  effective). These tests pin that a read/decrypt failure is now logged via Dev2Logger.
 */

using Dev2.Common;
using Dev2.Common.Interfaces.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using Warewolf.Execution.Lightweight.Security;

namespace Warewolf.Execution.Lightweight.Tests.Security
{
    [TestClass]
    [DoNotParallelize] // mutates SecureConfigLoader + Dev2Logger.ExternalSink static state
    public class SecureConfigLoaderTests
    {
        private const string ConfigPathEnvVar = "WAREWOLF_SECURE_CONFIG";

        private string? _originalEnv;
        private ILogger? _originalSink;
        private string? _tempPath;

        [TestInitialize]
        public void Init()
        {
            _originalEnv = Environment.GetEnvironmentVariable(ConfigPathEnvVar);
            _originalSink = Dev2Logger.ExternalSink;
        }

        [TestCleanup]
        public void Cleanup()
        {
            Environment.SetEnvironmentVariable(ConfigPathEnvVar, _originalEnv);
            Dev2Logger.ExternalSink = _originalSink;

            if (_tempPath is not null && File.Exists(_tempPath))
                try { File.Delete(_tempPath); } catch { /* best effort */ }

            SecureConfigLoader.Reload();
        }

        private sealed class SpyLogger : ILogger
        {
            public List<(object Message, Exception? Exception)> Errors { get; } = new();

            public void Error(object message, string executionId) => Errors.Add((message, null));
            public void Error(object message, Exception exception, string executionId) => Errors.Add((message, exception));

            public void Trace(object message, string executionId) { }
            public void Trace(object message, Exception exception, string executionId) { }
            public void Debug(object message, string executionId) { }
            public void Debug(object message, Exception exception, string executionId) { }
            public void Warn(object message, string executionId) { }
            public void Warn(object message, Exception exception, string executionId) { }
            public void Fatal(object message, string executionId) { }
            public void Fatal(object message, Exception exception, string executionId) { }
            public void Info(object message, string executionId) { }
            public void Info(object message, Exception exception, string executionId) { }
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void ReadConfig_CorruptFile_LogsErrorAndFallsBackToAllowAll()
        {
            _tempPath = Path.GetTempFileName();
            File.WriteAllText(_tempPath, "this is neither encrypted data nor valid JSON");
            Environment.SetEnvironmentVariable(ConfigPathEnvVar, _tempPath);

            var spy = new SpyLogger();
            Dev2Logger.ExternalSink = spy;

            SecureConfigLoader.Reload();

            Assert.IsFalse(SecureConfigLoader.Config.IsLoaded,
                "a corrupt file must still fall back to AllowAll (IsLoaded:false) — this test guards the logging addition, not that fallback behaviour");
            Assert.AreEqual(1, spy.Errors.Count,
                "expected exactly one Dev2Logger.Error call for the corrupt-config read failure");
            Assert.IsNotNull(spy.Errors[0].Exception, "the logged error should carry the underlying exception");
            StringAssert.Contains(spy.Errors[0].Message.ToString(), "secure.config");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void ReadConfig_FileNotFound_DoesNotLogError()
        {
            // "No file at all" is the expected/normal open-access-by-default case (already
            // surfaced separately by WorkflowAuthPolicyLoader.Build's own warning) — must not be
            // conflated with a genuine read/decrypt failure.
            Environment.SetEnvironmentVariable(ConfigPathEnvVar, "nonexistent_path_does_not_exist.config");

            var spy = new SpyLogger();
            Dev2Logger.ExternalSink = spy;

            SecureConfigLoader.Reload();

            Assert.IsFalse(SecureConfigLoader.Config.IsLoaded);
            Assert.AreEqual(0, spy.Errors.Count);
        }
    }
}

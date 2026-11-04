/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Warewolf.Execution.Lightweight.SelfHost;

namespace Warewolf.Execution.Lightweight.Tests.SelfHost;

// `--run <file.bite>`: run one workflow and exit, with nothing listening (the npm package's `warewolf run`).
[TestClass]
public class RunOnceCommandTests
{
    [TestMethod]
    [TestCategory("UnitTest")]
    public void IsRequested_OnlyWhenRunIsAnArgument()
    {
        Assert.IsTrue(RunOnceCommand.IsRequested(new[] { "--RUN", "a.bite" }));
        Assert.IsFalse(RunOnceCommand.IsRequested(new[] { "--self-host" }));
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public void Parse_ReadsTheWorkflowAndEveryOption()
    {
        var options = RunOnceCommand.Parse(
            new[] { "--run", "a.bite", "--input", "{\"x\":1}", "--resources", "res", "--xml", "--debug" }, out var error);

        Assert.IsNull(error);
        Assert.AreEqual("a.bite", options!.WorkflowPath);
        Assert.AreEqual("{\"x\":1}", options.Input);
        Assert.AreEqual("res", options.ResourcesDirectory);
        Assert.IsTrue(options.Xml);
        Assert.IsTrue(options.Debug);
    }

    [DataTestMethod]
    [TestCategory("UnitTest")]
    [DataRow(new[] { "--run" }, "needs a value")]
    [DataRow(new[] { "--run", "a.bite", "--bogus" }, "Unknown option")]
    [DataRow(new[] { "--run", "a.bite", "--input", "{}", "--input-file", "f" }, "not both")]
    public void Parse_BadArguments_ReturnNullWithAnError(string[] args, string expected)
    {
        Assert.IsNull(RunOnceCommand.Parse(args, out var error));
        StringAssert.Contains(error, expected);
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public void ApplyDefaults_AreQuietAndFree_ButNeverOverrideWhatIsSet()
    {
        var env = new Dictionary<string, string> { ["EXECUTIONLOGLEVEL"] = "DEBUG" };

        RunOnceCommand.ApplyDefaults(name => env.GetValueOrDefault(name), (name, value) => env[name] = value);

        Assert.AreEqual("DEBUG", env["EXECUTIONLOGLEVEL"]);
        Assert.AreEqual("false", env["WAREWOLF_LICENSE_CHECK_ENABLED"]);
        Assert.AreEqual("{}", env["WAREWOLF_LOGGING_CONFIG"]);
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task RunAsync_MissingWorkflow_IsAUsageErrorOnStderr()
    {
        var stdout = new StringWriter();
        var stderr = new StringWriter();

        var exitCode = await RunOnceCommand.RunAsync(
            new[] { "--run", Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".bite") }, stdout, stderr, () => TextReader.Null);

        Assert.AreEqual(RunOnceCommand.UsageError, exitCode);
        Assert.AreEqual(string.Empty, stdout.ToString());
        StringAssert.Contains(stderr.ToString(), "Workflow not found");
    }
}

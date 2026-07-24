/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.Execution.EngineJobProcessor.Tests;

[TestClass]
[DoNotParallelize] // mutates process environment variables
public class ProcessorSettingsTests
{
    static readonly string[] Vars =
    {
        "ENGINE_RESUME_BASEURL", "ENGINE_RESUME_SCOPE", "ENGINE_RESUME_TIMEOUT_SECONDS",
        "ENGINE_RESUME_AUTH_DISABLED", "JOB_STALE_MINUTES",
    };

    readonly Dictionary<string, string?> _saved = new();

    [TestInitialize]
    public void SaveEnvironment()
    {
        foreach (var name in Vars)
        {
            _saved[name] = Environment.GetEnvironmentVariable(name);
            Environment.SetEnvironmentVariable(name, null);
        }
    }

    [TestCleanup]
    public void RestoreEnvironment()
    {
        foreach (var (name, value) in _saved)
        {
            Environment.SetEnvironmentVariable(name, value);
        }
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public void FromEnvironment_NoVariables_UsesDocumentedDefaults()
    {
        var settings = ProcessorSettings.FromEnvironment();

        Assert.IsNull(settings.EngineResumeBaseUrl);
        Assert.IsNull(settings.EngineResumeScope);
        Assert.AreEqual(15, settings.ResumeTimeoutSeconds);
        Assert.IsFalse(settings.AuthDisabled);
        Assert.AreEqual(15, settings.StaleMinutes);
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public void FromEnvironment_ParsesValues_AndTrimsBaseUrlTrailingSlash()
    {
        Environment.SetEnvironmentVariable("ENGINE_RESUME_BASEURL", "https://engine.azurewebsites.net/");
        Environment.SetEnvironmentVariable("ENGINE_RESUME_SCOPE", "api://app-id/.default");
        Environment.SetEnvironmentVariable("ENGINE_RESUME_TIMEOUT_SECONDS", "30");
        Environment.SetEnvironmentVariable("ENGINE_RESUME_AUTH_DISABLED", "TRUE");
        Environment.SetEnvironmentVariable("JOB_STALE_MINUTES", "45");

        var settings = ProcessorSettings.FromEnvironment();

        Assert.AreEqual("https://engine.azurewebsites.net", settings.EngineResumeBaseUrl,
            "Trailing slash must be trimmed so route concatenation yields a single '/'.");
        Assert.AreEqual("api://app-id/.default", settings.EngineResumeScope);
        Assert.AreEqual(30, settings.ResumeTimeoutSeconds);
        Assert.IsTrue(settings.AuthDisabled, "Flag parsing must be case-insensitive.");
        Assert.AreEqual(45, settings.StaleMinutes);
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public void FromEnvironment_InvalidNumbers_FallBackToDefaults()
    {
        Environment.SetEnvironmentVariable("ENGINE_RESUME_TIMEOUT_SECONDS", "not-a-number");
        Environment.SetEnvironmentVariable("JOB_STALE_MINUTES", "-5");

        var settings = ProcessorSettings.FromEnvironment();

        Assert.AreEqual(15, settings.ResumeTimeoutSeconds);
        Assert.AreEqual(15, settings.StaleMinutes, "Non-positive values are invalid and must fall back.");
    }
}

/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using Warewolf.Execution.Lightweight.Infrastructure;

namespace Warewolf.Execution.Lightweight.Tests.Infrastructure;

// A run-from-package engine's wwwroot is read-only, so create_workflow failed with
// FileNotFoundException (ashleyswarewolfserver, 2026-10-16). Workflows now live in a writable
// directory on the content share, seeded from the package.
[TestClass]
public class PackagedResourcesTests
{
    string _root = null!;
    string _package = null!;
    string _writable = null!;

    [TestInitialize]
    public void Setup()
    {
        _root = Path.Combine(Path.GetTempPath(), "packaged-resources-" + Guid.NewGuid().ToString("N"));
        _package = Path.Combine(_root, "wwwroot", "Resources");
        _writable = Path.Combine(_root, "home", "data", "Warewolf", "Resources");
        Directory.CreateDirectory(_package);
    }

    [TestCleanup]
    public void Cleanup()
    {
        if (Directory.Exists(_root)) Directory.Delete(_root, recursive: true);
    }

    void Package(string relative, string content)
    {
        var path = Path.Combine(_package, relative);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, content);
    }

    string? Writable(string relative)
    {
        var path = Path.Combine(_writable, relative);
        return File.Exists(path) ? File.ReadAllText(path) : null;
    }

    static Func<string, string?> Env(Dictionary<string, string?> values) => name => values.TryGetValue(name, out var v) ? v : null;

    [TestMethod]
    [TestCategory("UnitTest")]
    public void WritableDirectoryFor_RunFromPackageWithContentShare_IsUnderHomeData()
    {
        var env = Env(new() { ["WEBSITE_RUN_FROM_PACKAGE"] = "https://x/pkg.zip", ["HOME"] = @"C:\home", ["WEBSITE_CONTENTSHARE"] = "share" });
        Assert.AreEqual(Path.Combine(@"C:\home", "data", "Warewolf", "Resources"), PackagedResources.WritableDirectoryFor(env));
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public void WritableDirectoryFor_NoPackageOrNoContentShare_IsNull()
    {
        Assert.IsNull(PackagedResources.WritableDirectoryFor(Env(new() { ["HOME"] = @"C:\home", ["WEBSITE_CONTENTSHARE"] = "share" })));
        Assert.IsNull(PackagedResources.WritableDirectoryFor(Env(new() { ["WEBSITE_RUN_FROM_PACKAGE"] = "0", ["HOME"] = @"C:\home", ["WEBSITE_CONTENTSHARE"] = "share" })));
        Assert.IsNull(PackagedResources.WritableDirectoryFor(Env(new() { ["WEBSITE_RUN_FROM_PACKAGE"] = "1", ["HOME"] = "/home" })));
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public void ResolveWorkflowsDirectory_NotRunFromPackage_KeepsThePackageDirectory()
    {
        Assert.AreEqual(_package, PackagedResources.ResolveWorkflowsDirectory(_package, Env(new())));
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public void ResolveWorkflowsDirectory_RunFromPackage_SeedsAndReturnsTheWritableDirectory()
    {
        Package("Hello.bite", "hello");
        Package(Path.Combine("InfoBoet", "GetMovie.bite"), "movie");
        var env = Env(new() { ["WEBSITE_RUN_FROM_PACKAGE"] = "1", ["HOME"] = Path.Combine(_root, "home"), ["WEBSITE_CONTENTSHARE"] = "share" });

        Assert.AreEqual(_writable, PackagedResources.ResolveWorkflowsDirectory(_package, env));
        Assert.AreEqual("hello", Writable("Hello.bite"));
        Assert.AreEqual("movie", Writable(Path.Combine("InfoBoet", "GetMovie.bite")));
        Assert.IsTrue(File.Exists(Path.Combine(_root, "home", "data", "Warewolf", PackagedResources.SeedFileName)), "the seed record sits beside the directory");
        Assert.IsFalse(File.Exists(Path.Combine(_writable, PackagedResources.SeedFileName)), "not inside it, where it would be read as a resource");
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public void Seed_NewPackage_UpdatesUntouchedFilesOnly()
    {
        Package("Untouched.bite", "v1");
        Package("Edited.bite", "v1");
        PackagedResources.Seed(_package, _writable);
        File.WriteAllText(Path.Combine(_writable, "Edited.bite"), "mine");

        Package("Untouched.bite", "v2");
        Package("Edited.bite", "v2");
        PackagedResources.Seed(_package, _writable);

        Assert.AreEqual("v2", Writable("Untouched.bite"));
        Assert.AreEqual("mine", Writable("Edited.bite"));
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public void Seed_DeletedWorkflow_StaysDeleted_AndCreatedOnesAreKept()
    {
        Package("Gone.bite", "v1");
        PackagedResources.Seed(_package, _writable);
        File.Delete(Path.Combine(_writable, "Gone.bite"));
        File.WriteAllText(Path.Combine(_writable, "New.bite"), "created here");

        PackagedResources.Seed(_package, _writable);

        Assert.IsNull(Writable("Gone.bite"));
        Assert.AreEqual("created here", Writable("New.bite"));
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public void Seed_FileAlreadyThereButNeverSeeded_IsLeftAlone()
    {
        Directory.CreateDirectory(_writable);
        File.WriteAllText(Path.Combine(_writable, "Clash.bite"), "existing");
        Package("Clash.bite", "packaged");

        PackagedResources.Seed(_package, _writable);

        Assert.AreEqual("existing", Writable("Clash.bite"));
    }
}

/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  Unit tests for WorkflowNameResolver: resolution of an MCP `name` (an
 *  extension-free, forward-slash relative path) to an absolute file path,
 *  covering both the WorkflowIndex fast path and the on-disk fallback that
 *  WorkflowNameResolver itself implements for files added after the index
 *  was last warmed/cached for a directory.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using Warewolf.Execution.Lightweight.Mcp;

namespace Warewolf.Execution.Lightweight.Tests.Mcp
{
    [TestClass]
    public class WorkflowNameResolverTests
    {
        private string _root = string.Empty;

        [TestInitialize]
        public void Setup() => _root = Directory.CreateDirectory(
            Path.Combine(Path.GetTempPath(), "wnr-tests-" + Guid.NewGuid().ToString("N"))).FullName;

        [TestCleanup]
        public void Cleanup()
        {
            if (Directory.Exists(_root))
            {
                Directory.Delete(_root, recursive: true);
            }
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Resolve_ViaFreshWorkflowIndex_ReturnsAbsolutePath()
        {
            var fullPath = Path.Combine(_root, "Greeting.bite");
            File.WriteAllText(fullPath, "<Service />");

            // WorkflowIndex's own first-load disk scan for this never-before-seen
            // directory will find the file, so this exercises the fast path.
            var resolved = WorkflowNameResolver.Resolve(_root, "Greeting");

            Assert.AreEqual(fullPath, resolved);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Resolve_FileAddedAfterIndexWasWarmed_FallsBackToDiskScan()
        {
            // Warm (and cache) an index for this directory while it is still empty,
            // simulating a workflow added after startup's WorkflowIndex.WarmUp ran.
            WorkflowIndex.Instance.WarmUp(_root);

            var fullPath = Path.Combine(_root, "LateAddition.bite");
            File.WriteAllText(fullPath, "<Service />");

            var resolved = WorkflowNameResolver.Resolve(_root, "LateAddition");

            Assert.AreEqual(fullPath, resolved,
                "WorkflowNameResolver must fall back to an on-disk lookup when the cached WorkflowIndex misses.");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Resolve_FallbackIsCaseInsensitive()
        {
            WorkflowIndex.Instance.WarmUp(_root);

            var fullPath = Path.Combine(_root, "CaseSensitive.bite");
            File.WriteAllText(fullPath, "<Service />");

            var resolved = WorkflowNameResolver.Resolve(_root, "casesensitive");

            Assert.AreEqual(fullPath, resolved);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Resolve_FallbackPrefersBiteOverXml()
        {
            WorkflowIndex.Instance.WarmUp(_root);

            var bitePath = Path.Combine(_root, "BothFormats.bite");
            File.WriteAllText(bitePath, "<Service />");
            File.WriteAllText(Path.Combine(_root, "BothFormats.xml"), "<Service />");

            var resolved = WorkflowNameResolver.Resolve(_root, "BothFormats");

            Assert.AreEqual(bitePath, resolved);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Resolve_FallbackFindsXml_WhenNoBiteFileExists()
        {
            WorkflowIndex.Instance.WarmUp(_root);

            var xmlPath = Path.Combine(_root, "LegacyOnly.xml");
            File.WriteAllText(xmlPath, "<Service />");

            var resolved = WorkflowNameResolver.Resolve(_root, "LegacyOnly");

            Assert.AreEqual(xmlPath, resolved);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Resolve_NestedRelativePath_ResolvesWithinSubfolder()
        {
            var subDir = Directory.CreateDirectory(Path.Combine(_root, "Sub")).FullName;
            var fullPath = Path.Combine(subDir, "Nested.bite");
            File.WriteAllText(fullPath, "<Service />");

            var resolved = WorkflowNameResolver.Resolve(_root, "Sub/Nested");

            Assert.AreEqual(fullPath, resolved);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Resolve_NotFound_ReturnsNull()
        {
            WorkflowIndex.Instance.WarmUp(_root);

            var resolved = WorkflowNameResolver.Resolve(_root, "DoesNotExist");

            Assert.IsNull(resolved);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Resolve_MissingWorkflowsDirectory_ReturnsNull()
        {
            var missingDir = Path.Combine(_root, "does-not-exist");

            var resolved = WorkflowNameResolver.Resolve(missingDir, "Anything");

            Assert.IsNull(resolved);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Resolve_BlankName_ReturnsNull()
        {
            var resolved = WorkflowNameResolver.Resolve(_root, "   ");

            Assert.IsNull(resolved);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Resolve_BlankWorkflowsDirectory_ReturnsNull()
        {
            var resolved = WorkflowNameResolver.Resolve("   ", "Anything");

            Assert.IsNull(resolved);
        }
    }
}

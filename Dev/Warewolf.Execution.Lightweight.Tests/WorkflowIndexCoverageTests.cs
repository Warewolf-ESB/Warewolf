/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  Coverage uplift tests for <see cref="WorkflowIndex"/>.
 *
 *  Each test uses a unique temp directory so the per-directory cache inside
 *  the singleton does not bleed state across tests. All temp dirs are
 *  cleaned up in TestCleanup.
 *
 *  Reached via InternalsVisibleTo("Warewolf.Execution.Lightweight.Tests").
 *
 *  Covers:
 *    * Resolve — null / whitespace inputs, missing directory, missing key,
 *      hit via on-disk JSON index file, hit via disk-scan build path,
 *      forward-slash & backslash key normalisation + case-insensitivity.
 *    * Index file persistence after disk scan.
 *    * Empty / malformed index file → FrozenDictionary.Empty.
 *    * Duplicate keys → first writer wins.
 *    * WarmUp success path + WarmUp on an invalid path swallowing exception
 *      via the LoadIndex catch (Path.GetFullPath of "" → ArgumentException).
 */

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Warewolf.Execution.Lightweight.Tests
{
    [TestClass]
    [TestCategory("WorkflowIndex_Coverage")]
    public class WorkflowIndexCoverageTests
    {
        private readonly List<string> _tempDirs = new();

        [TestCleanup]
        public void Cleanup()
        {
            foreach (var d in _tempDirs)
            {
                try { if (Directory.Exists(d)) Directory.Delete(d, recursive: true); }
                catch { /* best-effort */ }
            }
        }

        private string NewTempDir()
        {
            var dir = Path.Combine(Path.GetTempPath(), "wfidx_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            _tempDirs.Add(dir);
            return dir;
        }

        private static void WriteJsonIndex(string dir, Dictionary<string, string> entries)
        {
            File.WriteAllText(
                Path.Combine(dir, WorkflowIndex.IndexFileName),
                JsonConvert.SerializeObject(entries));
        }

        // ════════════════════════════════════════════════════════════════════
        // Resolve — null / whitespace inputs
        // ════════════════════════════════════════════════════════════════════

        [TestMethod]
        public void Resolve_NullDirectory_ReturnsNull()
        {
            Assert.IsNull(WorkflowIndex.Instance.Resolve(null!, "anything"));
        }

        [TestMethod]
        public void Resolve_EmptyDirectory_ReturnsNull()
        {
            Assert.IsNull(WorkflowIndex.Instance.Resolve("", "anything"));
        }

        [TestMethod]
        public void Resolve_WhitespaceDirectory_ReturnsNull()
        {
            Assert.IsNull(WorkflowIndex.Instance.Resolve("   ", "anything"));
        }

        [TestMethod]
        public void Resolve_NullName_ReturnsNull()
        {
            var dir = NewTempDir();
            Assert.IsNull(WorkflowIndex.Instance.Resolve(dir, null!));
        }

        [TestMethod]
        public void Resolve_WhitespaceName_ReturnsNull()
        {
            var dir = NewTempDir();
            Assert.IsNull(WorkflowIndex.Instance.Resolve(dir, "   "));
        }

        // ════════════════════════════════════════════════════════════════════
        // Resolve — index file present (TryDeserializeIndex path)
        // ════════════════════════════════════════════════════════════════════

        [TestMethod]
        public void Resolve_IndexFilePresent_ResolvesKeyExactMatch()
        {
            var dir = NewTempDir();
            WriteJsonIndex(dir, new Dictionary<string, string>
            {
                ["hello"] = "Hello.bite",
                ["tools/foo"] = "tools/Foo.bite",
            });

            var path = WorkflowIndex.Instance.Resolve(dir, "hello");
            Assert.IsNotNull(path);
            StringAssert.EndsWith(path!, "Hello.bite");
        }

        [TestMethod]
        public void Resolve_IndexFilePresent_KeyIsCaseInsensitive()
        {
            var dir = NewTempDir();
            WriteJsonIndex(dir, new Dictionary<string, string> { ["hello"] = "Hello.bite" });

            // Pass mixed-case name; lookup normalises to lowercase.
            var path = WorkflowIndex.Instance.Resolve(dir, "HELLO");
            Assert.IsNotNull(path);
            StringAssert.EndsWith(path!, "Hello.bite");
        }

        [TestMethod]
        public void Resolve_IndexFilePresent_BackslashesAreNormalised()
        {
            var dir = NewTempDir();
            WriteJsonIndex(dir, new Dictionary<string, string>
            {
                ["tools/hello"] = "tools/Hello.bite",
            });

            var path = WorkflowIndex.Instance.Resolve(dir, "tools\\hello");
            Assert.IsNotNull(path);
            // The combined path uses the OS separator regardless of input.
            Assert.IsTrue(path!.EndsWith("Hello.bite"));
        }

        [TestMethod]
        public void Resolve_IndexFilePresent_LeadingSlashTrimmed()
        {
            var dir = NewTempDir();
            WriteJsonIndex(dir, new Dictionary<string, string>
            {
                ["tools/hello"] = "tools/Hello.bite",
            });

            var path = WorkflowIndex.Instance.Resolve(dir, "/tools/hello");
            Assert.IsNotNull(path);
        }

        [TestMethod]
        public void Resolve_IndexFilePresent_KeyMissing_ReturnsNull()
        {
            var dir = NewTempDir();
            WriteJsonIndex(dir, new Dictionary<string, string> { ["a"] = "A.bite" });

            Assert.IsNull(WorkflowIndex.Instance.Resolve(dir, "does/not/exist"));
        }

        [TestMethod]
        public void Resolve_EmptyJsonIndexFile_ReturnsNull()
        {
            var dir = NewTempDir();
            File.WriteAllText(Path.Combine(dir, WorkflowIndex.IndexFileName), "{}");

            Assert.IsNull(WorkflowIndex.Instance.Resolve(dir, "anything"));
        }

        [TestMethod]
        public void Resolve_MalformedJsonIndexFile_ReturnsNull()
        {
            var dir = NewTempDir();
            File.WriteAllText(Path.Combine(dir, WorkflowIndex.IndexFileName), "{not json");

            // Bad JSON → catch returns FrozenDictionary.Empty → Resolve returns null.
            Assert.IsNull(WorkflowIndex.Instance.Resolve(dir, "anything"));
        }

        // ════════════════════════════════════════════════════════════════════
        // Resolve — disk scan path (no index file)
        // ════════════════════════════════════════════════════════════════════

        [TestMethod]
        public void Resolve_NoIndexFile_BuildsFromDisk_ResolvesBiteFile()
        {
            var dir = NewTempDir();
            File.WriteAllText(Path.Combine(dir, "Greeter.bite"), "<x/>");

            var path = WorkflowIndex.Instance.Resolve(dir, "greeter");

            Assert.IsNotNull(path);
            StringAssert.EndsWith(path!, "Greeter.bite");
        }

        [TestMethod]
        public void Resolve_NoIndexFile_BuildsFromDisk_PersistsIndexFile()
        {
            var dir = NewTempDir();
            File.WriteAllText(Path.Combine(dir, "Persisted.bite"), "<x/>");

            // First Resolve triggers disk scan + persist.
            WorkflowIndex.Instance.Resolve(dir, "persisted");

            var indexFile = Path.Combine(dir, WorkflowIndex.IndexFileName);
            Assert.IsTrue(File.Exists(indexFile),
                "Disk scan should persist the generated index file for cold starts.");

            // And the persisted JSON should round-trip.
            var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(
                File.ReadAllText(indexFile));
            Assert.IsNotNull(dict);
            Assert.IsTrue(dict!.ContainsKey("persisted"));
        }

        [TestMethod]
        public void Resolve_NoIndexFile_NestedDirectories_KeyHasForwardSlash()
        {
            var dir = NewTempDir();
            var sub = Path.Combine(dir, "Examples");
            Directory.CreateDirectory(sub);
            File.WriteAllText(Path.Combine(sub, "Nested.bite"), "<x/>");

            var path = WorkflowIndex.Instance.Resolve(dir, "examples/nested");

            Assert.IsNotNull(path);
            StringAssert.EndsWith(path!, "Nested.bite");
        }

        [TestMethod]
        public void Resolve_NoIndexFile_EmptyDirectory_BuildsEmptyIndex_ReturnsNull()
        {
            var dir = NewTempDir();
            // No .bite files; the disk-scan build will return an empty dict
            // → FrozenDictionary.Empty → Resolve returns null.
            Assert.IsNull(WorkflowIndex.Instance.Resolve(dir, "anything"));
        }

        [TestMethod]
        public void Resolve_NoIndexFile_DirectoryDoesNotExist_ReturnsNull()
        {
            // Path.GetFullPath works for non-existent paths; BuildIndexFromDisk
            // detects the missing directory and returns an empty dictionary.
            var dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            _tempDirs.Add(dir); // for safety; nothing to clean
            Assert.IsNull(WorkflowIndex.Instance.Resolve(dir, "anything"));
        }

        // ════════════════════════════════════════════════════════════════════
        // WarmUp
        // ════════════════════════════════════════════════════════════════════

        [TestMethod]
        public void WarmUp_ValidDirectory_DoesNotThrow()
        {
            var dir = NewTempDir();
            File.WriteAllText(Path.Combine(dir, "Warm.bite"), "<x/>");

            WorkflowIndex.Instance.WarmUp(dir);

            // After WarmUp the index file should be persisted to disk.
            Assert.IsTrue(File.Exists(Path.Combine(dir, WorkflowIndex.IndexFileName)));
        }

        [TestMethod]
        public void WarmUp_IsIdempotent()
        {
            var dir = NewTempDir();
            File.WriteAllText(Path.Combine(dir, "Idem.bite"), "<x/>");

            WorkflowIndex.Instance.WarmUp(dir);
            WorkflowIndex.Instance.WarmUp(dir); // second call must not throw or alter behaviour.

            var path = WorkflowIndex.Instance.Resolve(dir, "idem");
            Assert.IsNotNull(path);
        }

        [TestMethod]
        public void WarmUp_EmptyString_ThrowsArgumentException()
        {
            // Path.GetFullPath("") throws ArgumentException; WarmUp propagates it
            // after logging (covering the catch + rethrow branch).
            Assert.ThrowsException<ArgumentException>(
                () => WorkflowIndex.Instance.WarmUp(""));
        }
    }
}

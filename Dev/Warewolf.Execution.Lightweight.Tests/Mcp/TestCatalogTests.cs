/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  Unit tests for TestCatalog: path resolution, save/overwrite/load round-trip, and the
 *  filename-legality rule that is new (not mirrored from any existing tool) — a test name
 *  is a single filename component, not a `/`-separated path like every other `name`-taking tool.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModelContextProtocol;
using System;
using System.IO;
using System.Linq;
using Warewolf.Execution.Lightweight.Mcp;

namespace Warewolf.Execution.Lightweight.Tests.Mcp
{
    [TestClass]
    public class TestCatalogTests
    {
        private string _root = string.Empty;

        [TestInitialize]
        public void Setup() => _root = Directory.CreateDirectory(
            Path.Combine(Path.GetTempPath(), "test-catalog-tests-" + Guid.NewGuid().ToString("N"))).FullName;

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
        public void TestsDirectoryFor_UsesDotTestsSuffix()
        {
            var directory = TestCatalog.TestsDirectoryFor(_root, "Sales/CalculateTotal");

            Assert.AreEqual(Path.Combine(_root, "Sales", "CalculateTotal.tests"), directory);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Exists_ReturnsFalse_WhenNoFileWritten()
        {
            Assert.IsFalse(TestCatalog.Exists(_root, "MyWorkflow", "OutputIs10"));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Save_Then_Exists_And_Load_RoundTrip()
        {
            TestCatalog.Save(_root, "MyWorkflow", "OutputIs10", "{\"testName\":\"OutputIs10\"}");

            Assert.IsTrue(TestCatalog.Exists(_root, "MyWorkflow", "OutputIs10"));
            Assert.AreEqual("{\"testName\":\"OutputIs10\"}", TestCatalog.Load(_root, "MyWorkflow", "OutputIs10"));
            Assert.IsTrue(File.Exists(Path.Combine(_root, "MyWorkflow.tests", "OutputIs10.test.json")));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Save_Overwrites_ExistingTest()
        {
            TestCatalog.Save(_root, "MyWorkflow", "OutputIs10", "{\"v\":1}");
            TestCatalog.Save(_root, "MyWorkflow", "OutputIs10", "{\"v\":2}");

            Assert.AreEqual("{\"v\":2}", TestCatalog.Load(_root, "MyWorkflow", "OutputIs10"));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Load_ReturnsNull_WhenTestDoesNotExist()
        {
            Assert.IsNull(TestCatalog.Load(_root, "MyWorkflow", "NoSuchTest"));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void List_IsEmpty_WhenTheWorkflowHasNoTestsDirectory()
        {
            Assert.AreEqual(0, TestCatalog.List(_root, "MyWorkflow").Count);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void List_ReturnsTestNamesSorted_IgnoringOtherFiles()
        {
            TestCatalog.Save(_root, "Sales/CalculateTotal", "b", "{}");
            TestCatalog.Save(_root, "Sales/CalculateTotal", "A", "{}");
            File.WriteAllText(Path.Combine(_root, "Sales", "CalculateTotal.tests", "notes.txt"), "");

            CollectionAssert.AreEqual(new[] { "A", "b" }, TestCatalog.List(_root, "Sales/CalculateTotal").ToArray());
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Delete_ReturnsFalse_WhenTestDoesNotExist()
        {
            Assert.IsFalse(TestCatalog.Delete(_root, "MyWorkflow", "NoSuchTest"));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Delete_KeepsTheTestsDirectory_WhileOtherFilesRemain()
        {
            TestCatalog.Save(_root, "MyWorkflow", "One", "{}");
            File.WriteAllText(Path.Combine(_root, "MyWorkflow.tests", "notes.txt"), "");

            Assert.IsTrue(TestCatalog.Delete(_root, "MyWorkflow", "One"));

            Assert.IsFalse(TestCatalog.Exists(_root, "MyWorkflow", "One"));
            Assert.IsTrue(Directory.Exists(TestCatalog.TestsDirectoryFor(_root, "MyWorkflow")));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [ExpectedException(typeof(McpException))]
        public void ValidateTestName_Blank_Throws() => TestCatalog.ValidateTestName("   ");

        [TestMethod]
        [TestCategory("UnitTest")]
        [ExpectedException(typeof(McpException))]
        public void ValidateTestName_ContainingForwardSlash_Throws() => TestCatalog.ValidateTestName("Sub/Test");

        [TestMethod]
        [TestCategory("UnitTest")]
        [ExpectedException(typeof(McpException))]
        public void ValidateTestName_ContainingBackslash_Throws() => TestCatalog.ValidateTestName(@"Sub\Test");

        [TestMethod]
        [TestCategory("UnitTest")]
        [ExpectedException(typeof(McpException))]
        public void ValidateTestName_ContainingDotDot_Throws() => TestCatalog.ValidateTestName("..EscapeAttempt");

        [TestMethod]
        [TestCategory("UnitTest")]
        [ExpectedException(typeof(McpException))]
        // NUL is the one Path.GetInvalidFileNameChars() entry (besides '/') common to every OS;
        // ':' is only invalid on Windows.
        public void ValidateTestName_ContainingInvalidFileNameChar_Throws() => TestCatalog.ValidateTestName("Bad\0Name");

        [TestMethod]
        [TestCategory("UnitTest")]
        public void ValidateTestName_PlainName_DoesNotThrow() => TestCatalog.ValidateTestName("OutputIs10");
    }
}

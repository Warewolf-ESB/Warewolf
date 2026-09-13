/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  Unit/smoke tests for XamlActivityBuilderLoader: validates the core,
 *  previously-unverified assumption that a plain ActivityXamlServices +
 *  XamlServices.Load pipeline (no custom Dev2XamlSchemaContext) can compile
 *  a real, committed workflow's XamlDefinition into an ActivityBuilder in
 *  this single-process .NET 8 isolated-worker host.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Text;
using Warewolf.Execution.Lightweight.Mcp;

namespace Warewolf.Execution.Lightweight.Tests.Mcp
{
    [TestClass]
    public class XamlActivityBuilderLoaderTests
    {
        /// <summary>
        /// Walks parents of the test assembly's base directory looking for a known committed
        /// workflow's <c>.bite</c> file, mirroring
        /// <c>WorkflowExecutorEndToEndTests.FindHelloWorldBite</c>'s ancestor-walk pattern.
        /// </summary>
        static string? FindBite(params string[] relativeSegments)
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            for (var hops = 0; dir != null && hops < 10; hops++, dir = dir.Parent)
            {
                var candidate = Path.Combine(new[] { dir.FullName }.Concat(relativeSegments).ToArray());
                if (File.Exists(candidate))
                {
                    return candidate;
                }

                var devCandidate = Path.Combine(new[] { dir.FullName, "Dev" }.Concat(relativeSegments).ToArray());
                if (File.Exists(devCandidate))
                {
                    return devCandidate;
                }
            }
            return null;
        }

        static StringBuilder ExtractXaml(string bitePath)
        {
            var fileContents = WorkflowExecutor.ReadWorkflowFile(bitePath);
            var (xamlDefinition, _, _) = WorkflowExecutor.ExtractWorkflowParts(fileContents);
            return xamlDefinition;
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Load_RealWorkflowXaml_AssignObject_ProducesNonNullActivityBuilder()
        {
            // "Assign Object" — committed with fidelity Status "Pass" — is the smallest known
            // real corpus sample for exercising this end-to-end.
            var bitePath = FindBite("Resources - ServerTests", "Resources", "Merge Acceptance Tests", "WorkFlowWithOneObject.bite");
            if (bitePath is null)
            {
                Assert.Inconclusive("WorkFlowWithOneObject.bite not found relative to the test assembly's base directory.");
                return;
            }

            var xaml = ExtractXaml(bitePath);
            Assert.IsTrue(xaml.Length > 0, "Extracted XamlDefinition must not be empty.");

            var builder = XamlActivityBuilderLoader.Load(xaml);

            Assert.IsNotNull(builder, "A real, previously round-tripped workflow's XAML must compile into a non-null ActivityBuilder.");
            Assert.IsNotNull(builder!.Implementation, "The ActivityBuilder must carry an Implementation activity tree.");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Load_RealWorkflowXaml_HelloWorld_ProducesNonNullActivityBuilder()
        {
            var bitePath = FindBite("Resources - Release", "Resources", "Hello World.bite");
            if (bitePath is null)
            {
                Assert.Inconclusive("Hello World.bite not found relative to the test assembly's base directory.");
                return;
            }

            var xaml = ExtractXaml(bitePath);

            var builder = XamlActivityBuilderLoader.Load(xaml);

            Assert.IsNotNull(builder);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Load_EmptyXaml_ReturnsNull()
        {
            var builder = XamlActivityBuilderLoader.Load(new StringBuilder());

            Assert.IsNull(builder);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Load_NullXaml_ReturnsNull()
        {
            var builder = XamlActivityBuilderLoader.Load(null!);

            Assert.IsNull(builder);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [ExpectedException(typeof(System.Xml.XmlException))]
        public void Load_MalformedXaml_Throws()
        {
            // Fails during Dev2XamlLoader.RemoveWindowsElements' own XmlDocument.LoadXml
            // pre-processing step, before the XAML reader is ever reached.
            XamlActivityBuilderLoader.Load(new StringBuilder("<NotValidXaml"));
        }
    }
}

/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Activities;
using System.Activities.Expressions;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using Dev2.Activities;
using Dev2.Common.Interfaces.DB;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Bite;

namespace Dev2.Tests.Activities.Parity
{
    [TestClass]
    public class ActivityGraphCanonicalizerTests
    {
        static readonly WfBiteReader Reader = new WfBiteReader();

        [TestMethod]
        [Timeout(60000)]
        [Owner("Ashley Lewis")]
        [TestCategory("WfParity")]
        public void ActivityGraphCanonicalizer_SameGraph_GivesIdenticalJson_InAnyCulture()
        {
            var start = Load(nameof(DsfSqlServerDatabaseActivity));
            var original = CultureInfo.CurrentCulture;

            string invariant, german;
            try
            {
                CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
                invariant = ActivityGraphCanonicalizer.ToJson(start);
                CultureInfo.CurrentCulture = new CultureInfo("de-DE");
                german = ActivityGraphCanonicalizer.ToJson(start);
            }
            finally
            {
                CultureInfo.CurrentCulture = original;
            }

            Assert.AreEqual(invariant, ActivityGraphCanonicalizer.ToJson(start), "Repeated serialisation differs.");
            Assert.AreEqual(invariant, german, "Serialisation depends on the current culture.");
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Ashley Lewis")]
        [TestCategory("WfParity")]
        public void ActivityGraphCanonicalizer_CapturesDecisionSwitchBodyAndChildEdges()
        {
            var decisionEdges = EdgeKinds(Load(nameof(DsfFlowDecisionActivity)));
            var switchEdges = EdgeKinds(Load(nameof(DsfFlowSwitchActivity)));
            var forEachEdges = EdgeKinds(Load(nameof(DsfForEachActivity)));
            var sequenceEdges = EdgeKinds(Load(nameof(DsfSequenceActivity)));

            CollectionAssert.IsSubsetOf(new[] { "TrueArm", "FalseArm" }, decisionEdges);
            CollectionAssert.IsSubsetOf(new[] { "Switches", "Default" }, switchEdges);
            CollectionAssert.IsSubsetOf(new[] { "DataFunc", "NextNodes" }, forEachEdges);
            CollectionAssert.IsSubsetOf(new[] { "Activities", "NextNodes" }, sequenceEdges);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Ashley Lewis")]
        [TestCategory("WfParity")]
        public void ActivityGraphCanonicalizer_InArgumentLiteral_SerialisesAsPlainValue()
        {
            var activity = new DsfActivity { UniqueID = "a", ActionName = new InArgument<string>(new Literal<string>("[[Action]]")) };

            var node = ((JsonObject)ActivityGraphCanonicalizer.Canonicalize(activity)["nodes"]!).Single().Value!;

            Assert.AreEqual("[[Action]]", node["ActionName"]!.GetValue<string>());
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Ashley Lewis")]
        [TestCategory("WfParity")]
        public void ActivityGraphDiff_OneNestedPropertyChanged_ReportsExactlyThatPath()
        {
            var expected = Load(nameof(DsfSqlServerDatabaseActivity));
            var actual = Load(nameof(DsfSqlServerDatabaseActivity));
            var inputs = ((DsfSqlServerDatabaseActivity)actual).Inputs.ToList();
            inputs[1].Name = "[[Changed]]";

            var differences = ActivityGraphDiff.Compare(expected, actual);

            // FullName is derived from Name, so it legitimately changes with it; nothing outside Inputs[1] may.
            var report = string.Join("\n", differences);
            Assert.IsTrue(differences.Count > 0, "The change was not detected.");
            Assert.IsTrue(differences.All(d => d.Path.Contains(".Inputs[1].")), report);
            var name = differences.SingleOrDefault(d => d.Path.EndsWith(".Inputs[1].Name"));
            Assert.IsNotNull(name, report);
            Assert.AreEqual("\"[[Changed]]\"", name.Actual);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Ashley Lewis")]
        [TestCategory("WfParity")]
        public void ActivityGraphCanonicalizer_BackEdgeAndSharedSuccessor_EachNodeAppearsOnce()
        {
            var first = new DsfCommentActivity { UniqueID = "1" };
            var second = new DsfCommentActivity { UniqueID = "2" };
            var shared = new DsfCommentActivity { UniqueID = "3" };
            first.NextNodes = new List<IDev2Activity> { second, shared };
            second.NextNodes = new List<IDev2Activity> { shared, first }; // back-edge to the start
            shared.NextNodes = new List<IDev2Activity>();

            var graph = ActivityGraphCanonicalizer.Canonicalize(first);

            var uniqueIds = ((JsonObject)graph["nodes"]!).Select(p => p.Value!["UniqueID"]!.GetValue<string>()).OrderBy(i => i).ToArray();
            CollectionAssert.AreEqual(new[] { "1", "2", "3" }, uniqueIds);
            Assert.AreEqual(4, ((JsonArray)graph["edges"]!).Count);
        }

        static IDev2Activity Load(string corpusEntry)
        {
            var assembly = typeof(ActivityGraphCanonicalizerTests).Assembly;
            var name = assembly.GetManifestResourceNames().Single(n => n.EndsWith($".WfCorpus.{corpusEntry}.bite", System.StringComparison.Ordinal));
            using (var reader = new StreamReader(assembly.GetManifestResourceStream(name)))
            {
                return Reader.Read(reader.ReadToEnd());
            }
        }

        static string[] EdgeKinds(IDev2Activity start) =>
            ((JsonArray)ActivityGraphCanonicalizer.Canonicalize(start)["edges"]!)
                .Select(e => e![1]!.GetValue<string>())
                .Distinct()
                .ToArray();
    }
}

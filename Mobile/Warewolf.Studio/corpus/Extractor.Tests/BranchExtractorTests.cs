using Newtonsoft.Json.Linq;

namespace Warewolf.Corpus.Extractor.Tests;

[TestClass]
public class BranchExtractorTests
{
    const string Assign = "dsfdotnetmultiassignactivity";
    const string Calculate = "dsfdotnetcalculateactivity";
    const string Decision = "flowdecision";
    const string Switch = "dsfflowswitchactivity";

    static readonly BranchExtractor Extractor = new(EngineToolResolver.Resolve);

    [TestMethod]
    public void Linear_workflow_gives_one_example_per_step_then_END()
    {
        var graph = Graph(
            [Node("s", "start"), Node("a", Assign), Node("c", Calculate)],
            [Edge("s", "a"), Edge("a", "c")]);

        var result = Extractor.Extract(graph);

        CollectionAssert.AreEqual(
            new[] { "[] -> Assign", "[Assign] -> Calculate", "[Assign,Calculate] -> END" },
            Describe(result));
    }

    [TestMethod]
    public void Decision_gives_a_True_and_a_False_branch()
    {
        var graph = Graph(
            [Node("s", "start"), Node("d", Decision), Node("a", Assign)],
            [Edge("s", "d"), Edge("d", "a", decisionArm: true)]);

        var result = Extractor.Extract(graph);

        CollectionAssert.AreEqual(
            new[] { "[] -> Decision", "[Decision] True -> Assign", "[Decision,Assign] -> END", "[Decision] False -> END" },
            Describe(result));
    }

    [TestMethod]
    public void Switch_gives_one_branch_per_case()
    {
        var graph = Graph(
            [Node("s", "start"), Node("w", Switch), Node("a", Assign), Node("c", Calculate)],
            [Edge("s", "w"), Edge("w", "a", label: "gold"), Edge("w", "c", label: "silver")]);

        var result = Extractor.Extract(graph);

        CollectionAssert.AreEqual(
            new[] { "[] -> Switch", "[Switch] gold -> Assign", "[Switch,Assign] -> END", "[Switch] silver -> Calculate", "[Switch,Calculate] -> END" },
            Describe(result));
    }

    [TestMethod]
    public void Unknown_activity_type_is_reported_and_kept_in_the_path()
    {
        var graph = Graph(
            [Node("s", "start"), Node("x", "notarealactivity"), Node("a", Assign)],
            [Edge("s", "x"), Edge("x", "a")]);

        var result = Extractor.Extract(graph);

        CollectionAssert.AreEqual(new[] { "notarealactivity" }, result.UnknownTypes.ToArray());
        CollectionAssert.AreEqual(
            new[] { "[] -> ?notarealactivity", "[?notarealactivity] -> Assign", "[?notarealactivity,Assign] -> END" },
            Describe(result));
    }

    [TestMethod]
    public void Legacy_resource_call_to_a_workflow_is_a_sub_workflow_step()
    {
        var graph = Graph(
            [Node("s", "start"), Node("r", LegacyType("Unlimited.Applications.BusinessDesignStudio.Activities.DsfActivity"), new JObject { ["IsWorkflow"] = "True" })],
            [Edge("s", "r")]);

        var result = Extractor.Extract(graph);

        CollectionAssert.AreEqual(new[] { "[] -> Service (sub-workflow)", "[Service (sub-workflow)] -> END" }, Describe(result));
        Assert.AreEqual(0, result.UnknownTypes.Count);
    }

    [TestMethod]
    public void Legacy_resource_call_to_anything_else_stays_unknown()
    {
        var type = LegacyType("Unlimited.Applications.BusinessDesignStudio.Activities.DsfActivity");
        var graph = Graph(
            [Node("s", "start"), Node("r", type, new JObject { ["IsWorkflow"] = "False" })],
            [Edge("s", "r")]);

        var result = Extractor.Extract(graph);

        CollectionAssert.AreEqual(new[] { type }, result.UnknownTypes.ToArray());
    }

    [TestMethod]
    [DataRow("Unlimited.Applications.BusinessDesignStudio.Activities.DsfMultiAssignActivity", "Assign")]
    [DataRow("Unlimited.Applications.BusinessDesignStudio.Activities.DsfMultiAssignObjectActivity", "Assign Object")]
    [DataRow("Dev2.Activities.DsfWebPostActivity", "POST Web Method")]
    [DataRow("Dev2.Activities.WebPostActivity", "POST Web Method")]
    [DataRow("Dev2.Activities.DsfWebGetRequestActivity", "Web Request")]
    [DataRow("Unlimited.Applications.BusinessDesignStudio.Activities.DsfCountRecordsetActivity", "Count Records")]
    [DataRow("Unlimited.Applications.BusinessDesignStudio.Activities.DsfRecordsetLengthActivity", "Length")]
    public void Legacy_activity_maps_to_its_catalog_replacement(string clrType, string tool)
    {
        var graph = Graph([Node("s", "start"), Node("a", LegacyType(clrType))], [Edge("s", "a")]);

        var result = Extractor.Extract(graph);

        Assert.AreEqual(tool, result.Examples[0].Next);
        Assert.AreEqual(0, result.UnknownTypes.Count);
    }

    [TestMethod]
    public void Duplicate_node_ids_are_rejected_with_a_clear_message()
    {
        var graph = Graph(
            [Node("s", "start"), Node("a", Assign), Node("a", Calculate)],
            [Edge("s", "a")]);

        var error = Assert.ThrowsException<InvalidDataException>(() => Extractor.Extract(graph));

        StringAssert.Contains(error.Message, "duplicate node id(s) in the X6 graph: a");
    }

    static string LegacyType(string clrType) => $"{clrType}, Dev2.Activities, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null";

    static string[] Describe(GraphExamples result) =>
        result.Examples.Select(e => $"[{string.Join(",", e.Path)}]{(e.Arm is null ? "" : " " + e.Arm)} -> {e.Next}").ToArray();

    static string Graph(JObject[] nodes, JObject[] edges) =>
        new JObject { ["nodes"] = new JArray(nodes), ["edges"] = new JArray(edges) }.ToString();

    static JObject Node(string id, string type, JObject? properties = null)
    {
        var data = new JObject { ["type"] = type };
        if (properties is not null)
        {
            data["properties"] = properties;
        }

        return new JObject { ["id"] = id, ["data"] = data };
    }

    static JObject Edge(string from, string to, string label = "", bool? decisionArm = null)
    {
        var data = new JObject { ["type"] = "sequence" };
        if (decisionArm is { } isTrue)
        {
            data["isDecisionArm"] = true;
            data["isTrue"] = isTrue;
        }

        return new JObject
        {
            ["source"] = new JObject { ["cell"] = from },
            ["target"] = new JObject { ["cell"] = to },
            ["label"] = label,
            ["data"] = data,
        };
    }
}

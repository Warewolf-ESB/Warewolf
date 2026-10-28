using Dev2.Common.X6;
using Newtonsoft.Json.Linq;

namespace Warewolf.Corpus.Extractor;

/// <summary>
/// Walks a workflow's X6 graph (as <c>BiteFormat.XamlToX6</c> produces it) from the start node the way the
/// generator's frontier loop builds one: one example per step on each branch, then END where the branch stops.
/// Decision arms come off the edges' <c>isDecisionArm</c>/<c>isTrue</c> flags, Switch cases off edge labels.
/// </summary>
public sealed class BranchExtractor(Func<string, JObject?, ResolvedTool?> resolve)
{
    public GraphExamples Extract(string x6Json)
    {
        var graph = JObject.Parse(x6Json);
        var nodes = (graph["nodes"] as JArray ?? []).OfType<JObject>().ToList();
        var edges = (graph["edges"] as JArray ?? []).OfType<JObject>().ToList();

        var nested = nodes.Where(n => n["data"]?[Constants.ISNESTED]?.Value<bool>() == true).Select(Id).ToHashSet();
        var duplicates = nodes.GroupBy(Id).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
        if (duplicates.Count > 0)
        {
            // Edges name nodes by id, so a graph with a repeated id cannot say which copy an edge belongs to.
            throw new InvalidDataException($"duplicate node id(s) in the X6 graph: {string.Join(", ", duplicates)}");
        }

        var topLevel = nodes.Where(n => !nested.Contains(Id(n))).ToDictionary(Id);
        var outgoing = edges
            .Where(e => topLevel.ContainsKey(Source(e)) && topLevel.ContainsKey(Target(e)))
            .ToLookup(Source);

        var walk = new Walk(resolve, topLevel, outgoing);
        var start = topLevel.Values.FirstOrDefault(n => DataType(n) == Constants.START);
        if (start is not null)
        {
            walk.FromStart(Id(start));
        }

        return new GraphExamples(walk.Examples, walk.Unknown.ToList(), nested.Count, walk.Merges);
    }

    sealed class Walk(Func<string, JObject?, ResolvedTool?> resolve, Dictionary<string, JObject> nodes, ILookup<string, JObject> outgoing)
    {
        readonly HashSet<string> _placed = [];
        public List<Example> Examples { get; } = [];
        public SortedSet<string> Unknown { get; } = new(StringComparer.Ordinal);
        public int Merges { get; private set; }

        public void FromStart(string startId) => Continue(outgoing[startId].FirstOrDefault(), [], arm: null);

        /// <summary>Records what follows <paramref name="path"/> along <paramref name="edge"/> (END when there is none), then descends.</summary>
        void Continue(JObject? edge, List<string> path, string? arm)
        {
            var targetId = edge is null ? null : Target(edge);
            if (targetId is null || !_placed.Add(targetId))
            {
                if (targetId is not null)
                {
                    Merges++;
                }

                Examples.Add(new Example(path, arm, Example.End));
                return;
            }

            var node = nodes[targetId];
            var dataType = DataType(node);
            var tool = resolve(dataType, node["data"]?["properties"] as JObject);
            if (tool is null)
            {
                Unknown.Add(dataType);
            }

            var name = tool?.Name ?? Example.UnknownPrefix + dataType;
            Examples.Add(new Example(path, arm, name));
            Place(targetId, [.. path, name], tool?.BranchKind ?? BranchKind.None);
        }

        void Place(string nodeId, List<string> path, BranchKind kind)
        {
            var edges = outgoing[nodeId].ToList();
            switch (kind)
            {
                case BranchKind.Decision:
                    Continue(edges.FirstOrDefault(e => IsDecisionArm(e, isTrue: true)), path, "True");
                    Continue(edges.FirstOrDefault(e => IsDecisionArm(e, isTrue: false)), path, "False");
                    break;
                case BranchKind.Switch:
                    // A Switch with no wired cases closes its branch like any other dead end.
                    if (edges.Count == 0)
                    {
                        Examples.Add(new Example(path, null, Example.End));
                    }

                    foreach (var edge in edges)
                    {
                        Continue(edge, path, edge["label"]?.Value<string>() ?? "");
                    }

                    break;
                default:
                    Continue(edges.FirstOrDefault(), path, arm: null);
                    break;
            }
        }
    }

    static string Id(JObject cell) => cell["id"]?.Value<string>() ?? "";
    static string Source(JObject edge) => edge["source"]?["cell"]?.Value<string>() ?? "";
    static string Target(JObject edge) => edge["target"]?["cell"]?.Value<string>() ?? "";
    static string DataType(JObject node) => node["data"]?[Constants.TYPE]?.Value<string>() ?? "";

    static bool IsDecisionArm(JObject edge, bool isTrue) =>
        edge["data"]?[Constants.ISDECISIONARM]?.Value<bool>() == true &&
        edge["data"]?[Constants.ISTRUEARM]?.Value<bool>() == isTrue;
}

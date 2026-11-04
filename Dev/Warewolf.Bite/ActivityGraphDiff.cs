/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using Dev2;

namespace Warewolf.Bite
{
    /// <summary>One difference between two canonical graphs. Values are canonical JSON text; null when absent.</summary>
    public sealed record GraphDifference(string Path, string? Expected, string? Actual)
    {
        public override string ToString() => (Expected, Actual) switch
        {
            (null, _) => $"{Path}: unexpected in actual ({Actual})",
            (_, null) => $"{Path}: missing in actual ({Expected})",
            _ => $"{Path}: {Expected} != {Actual}",
        };
    }

    /// <summary>
    /// Compares two activity graphs through their canonical form and reports each difference as a path,
    /// e.g. <c>nodes.n0003.Inputs[1].Name: "[[a]]" != "[[b]]"</c>.
    /// </summary>
    public static class ActivityGraphDiff
    {
        public static IReadOnlyList<GraphDifference> Compare(IDev2Activity? expected, IDev2Activity? actual) =>
            Compare(ActivityGraphCanonicalizer.Canonicalize(expected), ActivityGraphCanonicalizer.Canonicalize(actual));

        public static IReadOnlyList<GraphDifference> Compare(JsonNode? expected, JsonNode? actual)
        {
            var differences = new List<GraphDifference>();
            Walk("", expected, actual, differences);
            return differences;
        }

        static void Walk(string path, JsonNode? expected, JsonNode? actual, List<GraphDifference> differences)
        {
            switch (expected)
            {
                case JsonObject expectedObject when actual is JsonObject actualObject:
                    foreach (var key in expectedObject.Select(p => p.Key).Union(actualObject.Select(p => p.Key)).OrderBy(k => k, StringComparer.Ordinal))
                    {
                        var childPath = path.Length == 0 ? key : $"{path}.{key}";
                        var inExpected = expectedObject.TryGetPropertyValue(key, out var e);
                        var inActual = actualObject.TryGetPropertyValue(key, out var a);
                        if (inExpected != inActual)
                        {
                            differences.Add(new GraphDifference(childPath, inExpected ? Text(e) : null, inActual ? Text(a) : null));
                            continue;
                        }
                        Walk(childPath, e, a, differences);
                    }
                    return;
                case JsonArray expectedArray when actual is JsonArray actualArray:
                    if (expectedArray.Count != actualArray.Count)
                    {
                        differences.Add(new GraphDifference($"{path}.Count", expectedArray.Count.ToString(), actualArray.Count.ToString()));
                    }
                    for (var i = 0; i < Math.Min(expectedArray.Count, actualArray.Count); i++)
                    {
                        Walk($"{path}[{i}]", expectedArray[i], actualArray[i], differences);
                    }
                    return;
                default:
                    var expectedText = Text(expected);
                    var actualText = Text(actual);
                    if (!string.Equals(expectedText, actualText, StringComparison.Ordinal))
                    {
                        differences.Add(new GraphDifference(path, expectedText, actualText));
                    }
                    return;
            }
        }

        static string Text(JsonNode? node) => node?.ToJsonString() ?? "null";
    }
}

using System.Globalization;
using System.Xml.Linq;

namespace Warewolf.Corpus.Extractor;

/// <summary>Reads workflow resources from .bite envelopes and keeps one copy per Service ID.</summary>
public static class CorpusReader
{
    /// <summary>The workflow in <paramref name="filePath"/>, or <c>null</c> when it is a source or another non-workflow resource.</summary>
    public static WorkflowFile? Read(string set, string filePath)
    {
        var service = XDocument.Load(filePath).Root;
        if (service is null || (string?)service.Attribute("ResourceType") != "WorkflowService")
        {
            return null;
        }

        var version = service.Element("VersionInfo");
        return new WorkflowFile(
            set,
            filePath,
            (string?)service.Attribute("ID") ?? "",
            (string?)service.Element("DisplayName") ?? (string?)service.Attribute("Name") ?? Path.GetFileNameWithoutExtension(filePath),
            (string?)service.Element("Comment") ?? "",
            int.TryParse((string?)version?.Attribute("VersionNumber"), out var number) ? number : 0,
            DateTimeOffset.TryParse((string?)version?.Attribute("DateTimeStamp"), CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var stamp)
                ? stamp
                : DateTimeOffset.MinValue,
            (string?)service.Element("Action")?.Element("XamlDefinition") ?? "");
    }

    /// <summary>
    /// One workflow per Service ID: the highest version number, then the latest save; ties keep the first
    /// one read, so set order on the command line decides between identical copies.
    /// </summary>
    public static IReadOnlyList<WorkflowFile> Deduplicate(IEnumerable<WorkflowFile> workflows) =>
        workflows
            .Select((workflow, order) => (workflow, order))
            .GroupBy(x => x.workflow.Id, StringComparer.OrdinalIgnoreCase)
            .Select(group => group
                .OrderByDescending(x => x.workflow.VersionNumber)
                .ThenByDescending(x => x.workflow.VersionTimestamp)
                .ThenBy(x => x.order)
                .First())
            .OrderBy(x => x.order)
            .Select(x => x.workflow)
            .ToList();
}

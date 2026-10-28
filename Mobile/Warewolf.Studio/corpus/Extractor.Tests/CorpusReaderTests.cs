namespace Warewolf.Corpus.Extractor.Tests;

[TestClass]
public class CorpusReaderTests
{
    string _folder = "";

    [TestInitialize]
    public void CreateFolder() => _folder = Directory.CreateTempSubdirectory("corpus-reader-").FullName;

    [TestCleanup]
    public void DeleteFolder() => Directory.Delete(_folder, recursive: true);

    [TestMethod]
    public void Duplicate_service_id_keeps_the_newest_version()
    {
        const string id = "6f1d9c64-1111-4c3a-9a51-6f2f5d0c7e01";
        var older = Read("a", Bite("older.bite", id, version: 3, stamp: "2026-01-01T00:00:00Z"));
        var newer = Read("b", Bite("newer.bite", id, version: 5, stamp: "2025-06-01T00:00:00Z"));
        var other = Read("a", Bite("other.bite", "6f1d9c64-2222-4c3a-9a51-6f2f5d0c7e02", version: 1, stamp: "2026-01-01T00:00:00Z"));

        var kept = CorpusReader.Deduplicate([older, other, newer]);

        CollectionAssert.AreEquivalent(new[] { newer, other }, kept.ToArray());
    }

    WorkflowFile Read(string set, string path) =>
        CorpusReader.Read(set, path) ?? throw new AssertFailedException($"{path} was not read as a workflow");

    string Bite(string fileName, string id, int version, string stamp)
    {
        var path = Path.Combine(_folder, fileName);
        File.WriteAllText(path, $"""
            <Service ID="{id}" Version="1.0" Name="{fileName}" ResourceType="WorkflowService">
              <DisplayName>{fileName}</DisplayName>
              <Comment>test workflow</Comment>
              <Action Name="InvokeWorkflow" Type="Workflow"><XamlDefinition>&lt;Activity /&gt;</XamlDefinition></Action>
              <VersionInfo DateTimeStamp="{stamp}" VersionNumber="{version}" ResourceId="{id}" />
            </Service>
            """);
        return path;
    }
}

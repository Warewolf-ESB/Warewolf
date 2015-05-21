namespace Dev2.Common.Interfaces.DB
{
    public interface IServiceOutputMapping
    {
        string Name { get; set; }
        string OutputName { get; set; }
        string RecordSetName { get; set; }
        string CompleteName { get; }
    }
}
namespace Dev2.Common.Interfaces.DB
{
    public interface IServiceOutputMapping
    {
        string MappedFrom { get; set; }
        string MappedTo { get; set; }
        string RecordSetName { get; set; }
    }
}

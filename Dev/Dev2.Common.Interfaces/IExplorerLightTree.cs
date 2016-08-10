namespace Dev2.Common.Interfaces
{
    public interface IExplorerLightTree
    {
        string IconPath { get; set; }
        string ParentId { get; set; }
        string ResourceId { get; set; }
        string ResourceName { get; set; }
    }
}
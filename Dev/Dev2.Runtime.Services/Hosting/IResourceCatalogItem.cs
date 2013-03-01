using Dev2.Common.ServiceModel;

namespace Dev2.Runtime.Hosting
{
    public interface IResourceCatalogItem
    {
        ResourceCatalogItemKey Key { get; set; }

        string Name { get; set; }

        ResourceType ResourceType { get; set; }

        string FilePath { get; set; }

        string Xml { get; set; }
    }
}
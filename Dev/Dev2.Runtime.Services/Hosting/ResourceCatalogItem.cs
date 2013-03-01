using Dev2.Common.ServiceModel;

namespace Dev2.Runtime.Hosting
{
    public class ResourceCatalogItem : IResourceCatalogItem
    {
        public ResourceCatalogItemKey Key { get; set; }

        public string Name { get; set; }

        public ResourceType ResourceType { get; set; }

        public string FilePath { get; set; }

        public string Xml { get; set; }
    }
}

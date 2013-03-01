using System;

namespace Dev2.Runtime.Hosting
{
    public interface IResourceCatalogItemKey : IEquatable<ResourceCatalogItemKey>
    {
        Guid ID { get; set; }

        string Version { get; set; }
    }
}
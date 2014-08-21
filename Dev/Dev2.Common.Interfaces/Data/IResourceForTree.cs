using System;

namespace Dev2.Common.Interfaces.Data
{
    public interface IResourceForTree
    {
        // ReSharper disable InconsistentNaming
        Guid UniqueID { get; set; }
        Guid ResourceID { get; set; }
        // ReSharper restore InconsistentNaming
        String ResourceName { get; set; }
        ResourceType ResourceType { get; set; }
    }
}
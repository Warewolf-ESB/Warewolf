using System;
using System.Collections.Generic;

namespace Dev2.Common.Interfaces
{
    public interface ILightExplorerItem
    {
        List<ILightExplorerItem> Children { get; set; }
        bool IsFolder { get; set; }
        bool IsService { get; set; }
        bool IsSource { get; set; }
        Guid ResourceId { get; set; }
        string ResourceName { get; set; }
        string ResourcePath { get; set; }
        string ResourceType { get; set; }
        string Category { get; set; }
    }
}
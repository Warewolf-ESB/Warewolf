using Dev2.Data.ServiceModel;
using Dev2.Services.Security;
using System;
using System.Collections.Generic;

namespace Dev2.Interfaces
{
    public interface IExplorerItem
    {
        string DisplayName { get; set; }
        Guid ServerId { get; set; }
        Guid ResourceId { get; set; }
        ResourceType ResourceType { get; set; }
        IList<IExplorerItem> Children { get; set; }
        Permissions Permissions { get; set; }
        string ResourcePath { get; set; }
        IExplorerItem Parent { get; set; }
        string WebserverUri { get; set; }
    }
}
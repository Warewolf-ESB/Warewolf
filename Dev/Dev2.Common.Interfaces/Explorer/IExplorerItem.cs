using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Security;
using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces.Versioning;

namespace Dev2.Common.Interfaces.Explorer
{
    public interface IExplorerItem
    {
        string DisplayName { get; set; }
        Guid ServerId { get; set; }
        Guid ResourceId { get; set; }
        ResourceType ResourceType { get; set; }
        IList<IExplorerItem> Children { get; set; }
        Permissions Permissions { get; set; }
        IVersionInfo VersionInfo { get; set; }
        string ResourcePath { get; set; }
        IExplorerItem Parent { get; set; }
        string WebserverUri { get; set; }
    }
}
#pragma warning disable
using System;
using Dev2.Common.Interfaces.Versioning;

namespace Dev2.Interfaces
{
    public interface IResourceCriteria
    {
        string AuthorRoles { get; set; }
        bool FetchAll { get; set; }
        string FilePath { get; set; }
        string GuidCsv { get; set; }
        bool IsNewResource { get; set; }
        bool IsUpgraded { get; set; }
        Guid ResourceID { get; set; }
        string ResourceName { get; set; }
        string ResourcePath { get; set; }
        string ResourceType { get; set; }
        IVersionInfo VersionInfo { get; set; }
        Guid WorkspaceId { get; set; }
    }
}
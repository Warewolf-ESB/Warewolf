using System;
using Dev2.Common.Interfaces.Versioning;
using Dev2.Interfaces;

namespace Dev2.Common
{
    [Serializable]
    public class ResourceCriteria : IResourceCriteria
    {
       
        public Guid ResourceID { get; set; }
        public Guid WorkspaceId { get; set; }
        public IVersionInfo VersionInfo { get; set; }
        public string ResourceName { get; set; }
        public string ResourceType { get; set; }
        public string ResourcePath { get; set; }
        public string FilePath { get; set; }
        public string AuthorRoles { get; set; }
        public bool IsUpgraded { get; set; }
        public bool IsNewResource { get; set; }
        public bool FetchAll { get; set; }
        public string GuidCsv { get; set; }
    }
}

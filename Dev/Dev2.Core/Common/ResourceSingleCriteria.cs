using System;

namespace Dev2.Common
{
    [Serializable]
    public class ResourceSingleCriteria
    {
        public Guid ResourceID { get; set; }
        public Guid WorkspaceId { get; set; }
        public string ResourceName { get; set; }
        public string ResourcePath { get; set; }
    }
}
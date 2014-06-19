using System;
using System.Collections.Generic;
using Dev2.Data.ServiceModel;
using Dev2.Interfaces;
using Dev2.Services.Security;

namespace Dev2.Explorer
{
    public class ServerExplorerItem : IExplorerItem
    {
        public ServerExplorerItem()
        {
            Children = new List<IExplorerItem>();
        }
        public ServerExplorerItem(string displayName, Guid resourceId, ResourceType resourceType,
                                  IList<IExplorerItem> children, Permissions permissions, string resourcePath)
        {
            DisplayName = displayName;
            ResourceId = resourceId;
            ResourceType = resourceType;
            Children = children;
            Permissions = permissions;
            ResourcePath = resourcePath;
        }
        public string DisplayName { get; set; }
        public Guid ResourceId { get; set; }
        public Guid ServerId { get; set; }
        public string WebserverUri { get; set; }
        public ResourceType ResourceType { get; set; }

        public IList<IExplorerItem> Children { get; set; }
        public Permissions Permissions { get; set; }
        public string ResourcePath { get; set; }
        public IExplorerItem Parent { get; set; }

        public override bool Equals(object obj)
        {
            if(ReferenceEquals(null, obj)) return false;
            if(ReferenceEquals(this, obj)) return true;
            if(obj.GetType() != GetType()) return false;
            return Equals((ServerExplorerItem)obj);
        }


        protected bool Equals(ServerExplorerItem other)
        {
            return ResourceId.Equals(other.ResourceId);
        }

        public override int GetHashCode()
        {
            return ResourceId.GetHashCode();
        }
    }
}

using System.Collections.Generic;
using Dev2.Common.Interfaces.Data;
using Dev2.Studio.TO;

namespace Warewolf.Studio.Models
{
    public class ResouceTypeAndPathPredicate:IDeployPredicate
    {
        public ResouceTypeAndPathPredicate(string name,  IList<string> paths, ResourceType resourceType)
        {
            ResourceType = resourceType;
            Name = name;
            Paths = paths;
        }

        #region Implementation of IDeployPredicate

        public bool Predicate(IResource resource, IList<IResource> selectedSourceResources, IList<IResource> destinationResources)
        {
            return (ResourceType.HasFlag(resource.ResourceType));
        }

        public ResourceType ResourceType { get;  private set; }

        public string Name { get; private set; }
        public IList<string> Paths { get; private set; }

        #endregion
    }
}

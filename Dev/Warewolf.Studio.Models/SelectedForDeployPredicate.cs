using System.Collections.Generic;
using Dev2.Common.Interfaces.Data;
using Dev2.Studio.TO;

namespace Warewolf.Studio.Models
{
    public class SelectedForDeployPredicate : IDeployPredicate 
    {
        public SelectedForDeployPredicate( IList<ResourceType> excludedTypes)
        {
            ExcludedTypes = excludedTypes;
            Name = "Selected";
        }

        #region Implementation of IDeployPredicate

        public bool Predicate(IResource resource, IList<IResource> selectedSourceResources, IList<IResource> destinationResources)
        {
            return resource.IsSelected && !ExcludedTypes.Contains(resource.ResourceType);
        }

        public IList<ResourceType> ExcludedTypes { get; private set; }

        public string Name { get; private set; }

        #endregion
    }
}
using System.Collections.Generic;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Studio.ViewModels;
using Dev2.Studio.TO;

namespace Warewolf.Studio.Models.Deploy
{
    public class SelectedForDeployPredicate : IDeployPredicate 
    {
        public SelectedForDeployPredicate( IList<ResourceType> excludedTypes)
        {
            ExcludedTypes = excludedTypes;
            Name = "Selected";
        }

        #region Implementation of IDeployPredicate

        public bool Predicate(IExplorerItemViewModel resource, ICollection<IExplorerItemViewModel> selectedSourceResources, ICollection<IExplorerItemViewModel> destinationResources)
        {
            return resource.Checked && !ExcludedTypes.Contains(resource.ResourceType);
        }

        public IList<ResourceType> ExcludedTypes { get; private set; }

        public string Name { get; private set; }

        #endregion
    }
}
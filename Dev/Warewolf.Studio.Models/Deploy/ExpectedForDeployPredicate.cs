using System.Collections.Generic;
using System.Linq;
using Dev2.Common.Interfaces.Studio.ViewModels;
using Dev2.Studio.TO;

namespace Warewolf.Studio.Models.Deploy
{
    public class ExistingForDeployPredicate : IDeployPredicate
    {
        public ExistingForDeployPredicate()
        {

            Name = "Existing Resources";
        }

        #region Implementation of IDeployPredicate

        public bool Predicate(IExplorerItemViewModel resource, ICollection<IExplorerItemViewModel> selectedSourceResources, ICollection<IExplorerItemViewModel> destinationResources)
        {
            return resource.Checked && destinationResources.Any(a => a.ResourceId == resource.ResourceId);
        }



        public string Name { get; private set; }

        #endregion
    }
}
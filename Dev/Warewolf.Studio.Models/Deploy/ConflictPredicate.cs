using System.Collections.Generic;
using System.Linq;
using Dev2.Common.Interfaces.Studio.ViewModels;
using Dev2.Studio.TO;

namespace Warewolf.Studio.Models.Deploy
{
    public class ConflictPredicate:IDeployPredicate
    {
        public ConflictPredicate(string name)
        {
            Name = name;
        }

        #region Implementation of IDeployPredicate

        public bool Predicate(IExplorerItemViewModel resource, ICollection<IExplorerItemViewModel> selectedSourceResources, ICollection<IExplorerItemViewModel> destinationResources)
        {
            return  destinationResources.Any(a => a.ResourceId == resource.ResourceId);
        }

        public string Name { get; private set; }

        #endregion
    }
}
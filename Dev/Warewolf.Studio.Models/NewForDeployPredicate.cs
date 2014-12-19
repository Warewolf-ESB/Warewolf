using System.Collections.Generic;
using System.Linq;
using Dev2.Common.Interfaces.Data;
using Dev2.Studio.TO;

namespace Warewolf.Studio.Models
{
    public class NewForDeployPredicate : IDeployPredicate
    {
        public NewForDeployPredicate()
        {

            Name = "New Resources";
        }

        #region Implementation of IDeployPredicate

        public bool Predicate(IResource resource, IList<IResource> selectedSourceResources, IList<IResource> destinationResources)
        {
            return resource.IsSelected && !destinationResources.Any(a=>a.ResourceID==resource.ResourceID);
        }



        public string Name { get; private set; }

        #endregion
    }
}
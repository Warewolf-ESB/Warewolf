using System.Collections.Generic;
using System.Linq;
using Dev2.Common.Interfaces.Data;
using Dev2.Studio.TO;

namespace Warewolf.Studio.Models
{
    public class ConflictPredicate:IDeployPredicate
    {
        public ConflictPredicate(string name)
        {
            Name = name;
        }

        #region Implementation of IDeployPredicate

        public bool Predicate(IResource resource, IList<IResource> selectedSourceResources, IList<IResource> destinationResources)
        {
            return  destinationResources.Any(a => a.ResourceID == resource.ResourceID);
        }

        public string Name { get; private set; }

        #endregion
    }
}
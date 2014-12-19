using System.Collections.Generic;
using Dev2.Common.Interfaces.Data;

namespace Dev2.Studio.TO
{
    public interface IDeployPredicate
    {
        bool Predicate(IResource resource, IList<IResource> selectedSourceResources,
            IList<IResource> destinationResources);
        string Name{get;}
    }
}
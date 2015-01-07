using System.Collections.Generic;
using System.Linq;
using Dev2.Common.Interfaces.Deploy;
using Dev2.Common.Interfaces.Studio.ViewModels;
using Dev2.Studio.TO;

namespace Warewolf.Studio.ViewModels
{
    public class ConflictHandlerViewModel : IConflictHandlerViewModel
    {
        public ConflictHandlerViewModel(IDeployPredicate conflictPredicate)
        {
            ConflictPredicate = conflictPredicate;
        }

        #region Implementation of IConflictHandlerViewModel

        /// <summary>
        /// Handles conflicts
        /// </summary>
        /// <returns></returns>
        public bool HandleConflicts(IEnvironmentViewModel source, IEnvironmentViewModel destination)
        {
            // ReSharper disable MaximumChainedReferences
           Conflicts = source.ExplorerItemViewModels.Intersect(destination.ExplorerItemViewModels)
               .Select(a=>new Conflict(a.ResourceId,a.ResourceName,a.ResourceName) as IConflict).ToList();
            // ReSharper restore MaximumChainedReferences
           if (Conflicts.Count == 0)
               return true;
            //todo:show popup here
            return false;
        }
        public  IDeployPredicate ConflictPredicate { get; private set; }
        
        public IList<IConflict> Conflicts { get; set; }

        #endregion
    }
}

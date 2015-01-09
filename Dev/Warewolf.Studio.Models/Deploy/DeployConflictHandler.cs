using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Common.Interfaces.Deploy;
using Dev2.Common.Interfaces.Studio.ViewModels;
using Dev2.Studio.TO;

namespace Warewolf.Studio.Models.Deploy
{
    public class DeployConflictHandlerViewModel : IConflictHandlerViewModel
    {
        public DeployConflictHandlerViewModel(IDeployPredicate predicate)
        {
            Predicate = predicate;
        }

        #region Implementation of IConflictHandlerViewModel

        /// <summary>
        /// Handles conflicts
        /// </summary>
        /// <returns></returns>
        public bool HandleConflicts(IEnvironmentViewModel source, IEnvironmentViewModel destination)
        {
            var conflicts = new List<IConflict>();
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach(var sourceItem in source.AsList().Where(a=>a.Checked))
            {
               if( Predicate.Predicate(sourceItem,source.ExplorerItemViewModels,destination.ExplorerItemViewModels))
                   conflicts.Add(new Conflict(sourceItem.ResourceId, sourceItem.ResourceName, destination.AsList().First(a=>a.ResourceId==sourceItem.ResourceId).ResourceName));
            }
            if(conflicts.Any()){
                Conflicts = conflicts;
                if(ShowConflictsWindow())
                {
                    
                }
                //todo:show conflict Window
                return true;
            }
            return true;
        }

        bool ShowConflictsWindow()
        {
            return false;
        }

        public IList<IConflict> Conflicts { get; set; }


        public IDeployPredicate Predicate { get; private set; }
        #endregion
    }

    public class Conflict : IConflict {
        public Conflict(Guid id, string sourceName, string destinationName)
        {
            DestinationName = destinationName;
            SourceName = sourceName;
            Id = id;
        }

        #region Implementation of IConflict

        public Guid Id { get; private set; }
        public string SourceName { get; private set; }
        public string DestinationName { get; private set; }

        #endregion
    }
}

using System.Collections.Generic;
using Dev2.Common.Interfaces.Studio.ViewModels;

namespace Dev2.Common.Interfaces.Deploy
{
    /// <summary>
    /// Handles conflicts when a user selects deploy
    /// </summary>
    public interface IConflictHandlerViewModel
    {
        /// <summary>
        /// Handles conflicts
        /// </summary>
        /// <returns></returns>
        bool HandleConflicts(IEnvironmentViewModel source, IEnvironmentViewModel destination);

        IList<IConflict> Conflicts { get; set; }
    }

}
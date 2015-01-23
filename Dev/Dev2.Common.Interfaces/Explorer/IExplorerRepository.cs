using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces.Studio.ViewModels;
using Dev2.Common.Interfaces.Versioning;

namespace Dev2.Common.Interfaces.Explorer
{
    public interface IExplorerRepository
    {
        bool Rename(IExplorerItemViewModel vm, string newName);

        bool Move(IExplorerItemViewModel explorerItemViewModel, IExplorerItemViewModel destination);

        bool Delete(IExplorerItemViewModel explorerItemViewModel);

        ICollection<IVersionInfo> GetVersions(Guid id);
    }
}

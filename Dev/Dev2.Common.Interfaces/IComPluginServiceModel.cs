using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Dev2.Common.Interfaces
{
    public interface IComPluginServiceModel
    {
        ObservableCollection<IComPluginSource> RetrieveSources();
        ICollection<IPluginAction> GetActions(IComPluginSource source, INamespaceItem value);
        ICollection<INamespaceItem> GetNameSpaces(IComPluginSource source);
        void CreateNewSource();
        void EditSource(IComPluginSource selectedSource);
        string TestService(IComPluginService inputValues);

        IStudioUpdateManager UpdateRepository { get; }
    }
}
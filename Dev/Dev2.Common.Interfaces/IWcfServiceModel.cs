using System.Collections.Generic;
using System.Collections.ObjectModel;
using Dev2.Common.Interfaces.DB;

namespace Dev2.Common.Interfaces
{
    public interface IWcfServiceModel
    {
        ObservableCollection<IWcfServerSource> RetrieveSources();
        ICollection<IWcfAction> GetActions(IWcfServerSource source);
        void CreateNewSource();
        void EditSource(IWcfServerSource selectedSource);
        string TestService(IWcfService inputValues);
        IEnumerable<IServiceOutputMapping> GetPluginOutputMappings(IWcfAction action);
        void SaveService(IWcfService model);
        IStudioUpdateManager UpdateRepository { get; }
    }
}

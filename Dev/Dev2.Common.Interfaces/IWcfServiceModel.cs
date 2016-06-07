using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Dev2.Common.Interfaces
{
    public interface IWcfServiceModel
    {
        ObservableCollection<IWcfServerSource> RetrieveSources();
        ICollection<IWcfAction> GetActions(IWcfServerSource source);
        void CreateNewSource();
        void EditSource(IWcfServerSource selectedSource);
        string TestService(IWcfService inputValues);
        IStudioUpdateManager UpdateRepository { get; }
    }
}

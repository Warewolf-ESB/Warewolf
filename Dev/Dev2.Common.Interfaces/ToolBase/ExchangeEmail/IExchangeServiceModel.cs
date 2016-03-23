using System.Collections.Generic;
using System.Collections.ObjectModel;
using Dev2.Common.Interfaces.DB;

namespace Dev2.Common.Interfaces.ToolBase.ExchangeEmail
{
    public interface IExchangeServiceModel
    {
        ObservableCollection<IExchangeSource> RetrieveSources();
        void CreateNewSource();
        void EditSource(IExchangeSource selectedSource);
        string TestService(IExchangeSource inputValues);
        IEnumerable<IServiceOutputMapping> GetPluginOutputMappings(IExchangeSource action);
        void SaveService(IExchangeSource model);

        IStudioUpdateManager UpdateRepository { get; }
    }
}

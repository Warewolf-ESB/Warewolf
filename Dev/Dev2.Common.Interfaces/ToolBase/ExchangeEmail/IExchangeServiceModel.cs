using System.Collections.Generic;
using System.Collections.ObjectModel;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.Exchange;

namespace Dev2.Common.Interfaces.ToolBase.ExchangeEmail
{
    public interface IExchangeServiceModel
    {
        ObservableCollection<IExchangeSource> RetrieveSources();
        void CreateNewSource();
        void EditSource(IExchangeSource selectedSource);
        string TestService(IExchangeService inputValues);
        IEnumerable<IServiceOutputMapping> GetPluginOutputMappings(IExchangeSource source);
        void SaveService(IExchangeService model);

        IStudioUpdateManager UpdateRepository { get; }
    }
}

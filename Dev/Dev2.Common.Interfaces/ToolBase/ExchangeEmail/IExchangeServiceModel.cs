using System.Collections.ObjectModel;

namespace Dev2.Common.Interfaces.ToolBase.ExchangeEmail
{
    public interface IExchangeServiceModel
    {
        ObservableCollection<IExchangeSource> RetrieveSources();
        void CreateNewSource();
        void EditSource(IExchangeServiceSource selectedSource);
        void SaveService(IExchangeServiceSource model);
        IStudioUpdateManager UpdateRepository { get; }
    }
}

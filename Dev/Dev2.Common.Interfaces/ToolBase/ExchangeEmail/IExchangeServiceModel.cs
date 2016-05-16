using System.Collections.ObjectModel;

namespace Dev2.Common.Interfaces.ToolBase.ExchangeEmail
{
    public interface IExchangeServiceModel
    {
        ObservableCollection<IExchangeSource> RetrieveSources();
        void CreateNewSource();
        void EditSource(IExchangeSource selectedSource);
        void SaveService(IExchangeSource model);
        IStudioUpdateManager UpdateRepository { get; }
    }
}

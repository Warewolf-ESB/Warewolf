using System.Collections.ObjectModel;

namespace Dev2.Common.Interfaces.ToolBase.ExchangeEmail
{
    public interface IExchangeServiceModel
    {
        ObservableCollection<IExchange> RetrieveSources();
        void CreateNewSource();
        void EditSource(IExchange selectedSource);
        IStudioUpdateManager UpdateRepository { get; }
    }
}

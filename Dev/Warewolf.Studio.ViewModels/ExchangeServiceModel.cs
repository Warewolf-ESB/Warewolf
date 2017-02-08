using System.Collections.ObjectModel;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.ToolBase.ExchangeEmail;

namespace Warewolf.Studio.ViewModels
{
    public class ExchangeServiceModel : IExchangeServiceModel
    {
        readonly IStudioUpdateManager _updateRepository;
        readonly IQueryManager _queryProxy;
        readonly IShellViewModel _shell;

        public ExchangeServiceModel(IStudioUpdateManager updateRepository, IQueryManager queryProxy, IShellViewModel shell, IServer server)
        {
            _updateRepository = updateRepository;
            _queryProxy = queryProxy;
            _shell = shell;
            shell.SetActiveServer(server);
        }
        public ObservableCollection<IExchange> RetrieveSources()
        {
            return new ObservableCollection<IExchange>(_queryProxy.FetchExchangeSources());
        }

        public void CreateNewSource()
        {
            _shell.NewExchangeSource(string.Empty);
        }

        public void EditSource(IExchange selectedSource)
        {
            var sourceDef = new ExchangeSourceDefinition(selectedSource);
            _shell.EditResource(sourceDef);
        }
        

        public IStudioUpdateManager UpdateRepository => _updateRepository;
    }
}

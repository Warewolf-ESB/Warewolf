using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Dev2;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.DB;
using Warewolf.Core;

namespace Warewolf.Studio.ViewModels
{
    public class ManageWcfServiceModel : IWcfServiceModel
    {
        readonly IStudioUpdateManager _updateRepository;
        readonly IQueryManager _queryProxy;
        readonly IShellViewModel _shell;

        public ManageWcfServiceModel(IStudioUpdateManager updateRepository, IQueryManager queryProxy, IShellViewModel shell, IServer server)
        {
            VerifyArgument.AreNotNull(new Dictionary<string, object>
            {
                { "updateRepository", updateRepository },
                { "queryProxy", queryProxy },
                { "shell", shell } ,
                {"server",server}
            });
            _updateRepository = updateRepository;
            _queryProxy = queryProxy;
            _shell = shell;
            shell.SetActiveServer(server);
        }

        public IStudioUpdateManager UpdateRepository => _updateRepository;

        public ObservableCollection<IWcfServerSource> RetrieveSources()
        {
            return new ObservableCollection<IWcfServerSource>(_queryProxy.FetchWcfSources());
        }

        public ICollection<IWcfAction> GetActions(IWcfServerSource source)
        {
            return _queryProxy.WcfActions(source).ToArray();
        }

        public void CreateNewSource()
        {
            _shell.NewWcfSource(string.Empty);
        }


        public void EditSource(IWcfServerSource selectedSource)
        {
            _shell.EditResource(selectedSource);
        }

        public string TestService(IWcfService inputValues)
        {
            return _updateRepository.TestWcfService(inputValues);
        }

        public IEnumerable<IServiceOutputMapping> GetPluginOutputMappings(IWcfAction action)
        {
            return new List<IServiceOutputMapping> { new ServiceOutputMapping("bob", "The", ""), new ServiceOutputMapping("dora", "The", ""), new ServiceOutputMapping("Tree", "The", "") };
        }

        public void SaveService(IWcfService toModel)
        {
            _updateRepository.Save(toModel);
        }
    }
}

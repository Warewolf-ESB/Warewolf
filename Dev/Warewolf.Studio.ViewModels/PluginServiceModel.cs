using System;
using System.Collections.Generic;
using Dev2;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.PluginService;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Runtime.ServiceModel.Data;
using Warewolf.Core;

namespace Warewolf.Studio.ViewModels
{
    public class PluginServiceModel:IPluginServiceModel
    {
        readonly IStudioUpdateManager _updateRepository;
        readonly IQueryManager _queryProxy;
        readonly IShellViewModel _shell;
        readonly string _serverName;

        #region Implementation of IDbServiceModel

        public PluginServiceModel(IStudioUpdateManager updateRepository, IQueryManager queryProxy, IShellViewModel shell, string serverName)
        {
            VerifyArgument.AreNotNull(new Dictionary<string, object> { { "updateRepository", updateRepository }, { "queryProxy", queryProxy }, { "shell", shell } ,{"serverName",serverName} });
            _updateRepository = updateRepository;
            _queryProxy = queryProxy;
            _shell = shell;
            _serverName = serverName;

        }

        public ICollection<IPluginSource> RetrieveSources()
        {

            return _queryProxy.FetchPluginSources();

        }

        public ICollection<IPluginAction> GetActions(IPluginSource source, INamespaceItem ns)
        {
            return _queryProxy.PluginActions(source,ns);
        }

        public ICollection<INamespaceItem> GetNameSpaces(IPluginSource source)
        {
            return _queryProxy.FetchNamespaces(source);
        }

        public void CreateNewSource()
        {
            _shell.NewResource(ResourceType.PluginSource, Guid.NewGuid());
        }

        public void EditSource(IPluginSource selectedSource)
        {
            _shell.EditResource(selectedSource);

        }

        public  string TestService(IPluginService inputValues)
        {
           return _updateRepository.TestPluginService(inputValues);          
        }

        public IEnumerable<IServiceOutputMapping> GetPluginOutputMappings(IPluginAction action)
        {
            return new List<IServiceOutputMapping> { new ServiceOutputMapping("bob", "The"), new ServiceOutputMapping("dora", "The"), new ServiceOutputMapping("Tree", "The") }; 
        }

        public void SaveService(IPluginService toModel)
        {
      
            _updateRepository.Save(toModel);
        }

        #endregion

    }
}

﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Dev2;
using Dev2.Common.Interfaces;

namespace Warewolf.Studio.ViewModels
{
    public class ManageComPluginServiceModel : IComPluginServiceModel
    {
        readonly IStudioUpdateManager _updateRepository;
        readonly IQueryManager _queryProxy;
        readonly IShellViewModel _shell;

        #region Implementation of IDbServiceModel

        public ManageComPluginServiceModel(IStudioUpdateManager updateRepository, IQueryManager queryProxy, IShellViewModel shell, IServer server)
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

        public ObservableCollection<IComPluginSource> RetrieveSources()
        {
            return new ObservableCollection<IComPluginSource>(_queryProxy.FetchComPluginSources());
        }

        public ICollection<IPluginAction> GetActions(IComPluginSource source, INamespaceItem ns)
        {
            return _queryProxy.PluginActions(source, ns).Where(a => a.Method != "GetType").ToList();
        }

        public ICollection<INamespaceItem> GetNameSpaces(IComPluginSource source)
        {
            return _queryProxy.FetchNamespaces(source);
        }

        public void CreateNewSource()
        {
            _shell.NewComPluginSource(string.Empty);
        }

        public void EditSource(IComPluginSource selectedSource)
        {
            _shell.EditResource(selectedSource);
        }

        public string TestService(IComPluginService inputValues)
        {
            return _updateRepository.TestPluginService(inputValues);
        }

        #endregion
    }
}
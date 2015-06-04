using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.WebService;
using Dev2.Common.Interfaces.WebServices;

namespace Warewolf.Studio.ViewModels
{
    public class WebServiceModel : IWebServiceModel {
        public IStudioUpdateManager UpdateRepository { get; private set; }
        public IQueryManager QueryProxy { get; private set; }
        public ObservableCollection<IWebServiceSource> Sources
        {
            get
            {
                if(_sources == null)
                {
                    _sources = new ObservableCollection<IWebServiceSource>(QueryProxy.FetchWebServiceSources());
                }
                return _sources;
            }
            
        }



        public string HandlePasteResponse(string current)
        {
            return _shell.OpenPasteWindow(current);
        }

        readonly IShellViewModel _shell;
        readonly string _serverName;
        ObservableCollection<IWebServiceSource> _sources;

        public WebServiceModel(IStudioUpdateManager updateRepository, IQueryManager queryProxy, IShellViewModel shell, string serverName)
        {
            UpdateRepository = updateRepository;
            QueryProxy = queryProxy;
            _serverName = serverName;
            _shell = shell;
            updateRepository.WebServiceSourceSaved += UpdateSourcesCollection;
        }

        public WebServiceModel()
        {
        }

        #region Implementation of IWebServiceModel

        void UpdateSourcesCollection(IWebServiceSource serviceSource)
        {
            var webServiceSource = Sources.FirstOrDefault(source => source.Id == serviceSource.Id);
            if(webServiceSource != null)
            {
                Sources.Remove(webServiceSource);
            }
            Sources.Add(serviceSource);
        }
        public ICollection<IWebServiceSource> RetrieveSources()
        {
            return new List<IWebServiceSource>(QueryProxy.FetchWebServiceSources());
        }

        public void CreateNewSource()
        {
            _shell.NewResource(ResourceType.WebSource, Guid.NewGuid());
        }

        public void EditSource(IWebServiceSource selectedSource)
        {
            _shell.EditResource(selectedSource);
        }

        public string TestService(IWebService inputValues)
        {
            if(UpdateRepository != null)
            {
                return UpdateRepository.TestWebService(inputValues);
            }
            return "Error";
        }

        public void SaveService(IWebService toModel)
        {
            UpdateRepository.Save(toModel);
        }

        #endregion
    }
}
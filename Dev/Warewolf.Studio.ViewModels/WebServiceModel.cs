using System.Collections.Generic;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.WebService;
using Dev2.Common.Interfaces.WebServices;

namespace Warewolf.Studio.ViewModels
{
    public class WebServiceModel : IWebServiceModel {


        readonly IStudioUpdateManager _updateRepository;
        readonly IQueryManager _queryProxy;
        readonly IShellViewModel _shell;
        readonly string _serverName;

        public WebServiceModel(IStudioUpdateManager updateRepository, IQueryManager queryProxy, string serverName)
        {
            _updateRepository = updateRepository;
            _queryProxy = queryProxy;
            _serverName = serverName;
        }

        public WebServiceModel()
        {
        }

        #region Implementation of IWebServiceModel

        public ICollection<IWebServiceSource> RetrieveSources()
        {
            return new List<IWebServiceSource>(_queryProxy.FetchWebServiceSources());
        }

        public void CreateNewSource()
        {
        }

        public void EditSource(IWebServiceSource selectedSource)
        {
        }

        public string TestService(IWebService inputValues)
        {
            if(_updateRepository != null)
            {
                return _updateRepository.TestWebService(inputValues);
            }
            return "Error";
        }

        public void SaveService(IWebService toModel)
        {
            _updateRepository.Save(toModel);
        }

        #endregion
    }
}
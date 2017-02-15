using System;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Core;

namespace Warewolf.Studio.ViewModels
{
    public class ManageWebServiceSourceModel : IManageWebServiceSourceModel
    {
        readonly IStudioUpdateManager _updateRepository;
        readonly IQueryManager _queryProxy;

        public ManageWebServiceSourceModel(IStudioUpdateManager updateRepository, IQueryManager queryProxy, string serverName)
        {
            _updateRepository = updateRepository;
            _queryProxy = queryProxy;

            ServerName = serverName;
            if (ServerName.Contains("("))
            {
                ServerName = serverName.Substring(0, serverName.IndexOf("(", StringComparison.Ordinal));
            }

        }

        #region Implementation of IManageWebServiceSourceModel

        public void TestConnection(IWebServiceSource resource)
        {
            _updateRepository.TestConnection(resource);
        }

        public void Save(IWebServiceSource resource)
        {
            _updateRepository.Save(resource);
        }

        public IWebServiceSource FetchSource(Guid resourceID)
        {
            var xaml = _queryProxy.FetchResourceXaml(resourceID);
            var db = new WebSource(xaml.ToXElement());

            var def = new WebServiceSourceDefinition(db);
            return def;
        }

        public string ServerName { get; set; }

        #endregion
    }
}

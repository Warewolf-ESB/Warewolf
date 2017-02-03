using Dev2.Common.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.ToolBase.ExchangeEmail;
using Dev2.Runtime.ServiceModel.Data;
using System;

namespace Warewolf.Studio.ViewModels
{
    public class ManageExchangeSourceModel : IManageExchangeSourceModel
    {
        readonly IStudioUpdateManager _updateRepository;
        // ReSharper disable once NotAccessedField.Local
        readonly IQueryManager _queryProxy;

        public ManageExchangeSourceModel(IStudioUpdateManager updateRepository, IQueryManager queryProxy, string serverName)
        {
            _updateRepository = updateRepository;
            _queryProxy = queryProxy;

            ServerName = serverName;
            if (ServerName.Contains("("))
            {
                ServerName = serverName.Substring(0, serverName.IndexOf("(", System.StringComparison.Ordinal));
            }
        }

        #region Implementation of IManageDatabaseSourceModel

        public string TestConnection(IExchangeSource resource)
        {
            return _updateRepository.TestConnection(resource);
        }

        public void Save(IExchangeSource toDbSource)
        {
            _updateRepository.Save(toDbSource);
        }

        public IExchangeSource FetchSource(Guid exchangeSourceResourceID)
        {
            var xaml = _queryProxy.FetchResourceXaml(exchangeSourceResourceID);
            var db = new ExchangeSource(xaml.ToXElement());

            var def = new ExchangeSourceDefinition(db);
            return def;
        }

        public string ServerName { get; private set; }

        #endregion
    }
}

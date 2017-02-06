using System;
using Dev2.Common.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core;
using Dev2.Runtime.ServiceModel.Data;

namespace Warewolf.Studio.ViewModels
{
    public class ManageEmailSourceModel: IManageEmailSourceModel
    {
        readonly IStudioUpdateManager _updateRepository;
        readonly IQueryManager _queryProxy;

        public ManageEmailSourceModel(IStudioUpdateManager updateRepository, IQueryManager queryProxy, string serverName)
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
        public IEmailServiceSource FetchSource(Guid resourceID)
        {
            var xaml = _queryProxy.FetchResourceXaml(resourceID);
            var db = new EmailSource(xaml.ToXElement());

            var def = new EmailServiceSourceDefinition(db);
            return def;
        }


        public string TestConnection(IEmailServiceSource resource)
        {
            return _updateRepository.TestConnection(resource);
        }

        public void Save(IEmailServiceSource toDbSource)
        {
            _updateRepository.Save(toDbSource);
        }

        public string ServerName { get; private set; }

        #endregion
    }
}
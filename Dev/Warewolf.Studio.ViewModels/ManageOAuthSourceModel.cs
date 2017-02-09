using System;
using Dev2.Common.Common;
using Dev2.Common.Interfaces;
using Dev2.Data.ServiceModel;

namespace Warewolf.Studio.ViewModels
{
    public class ManageOAuthSourceModel : IManageOAuthSourceModel
    {
        readonly IStudioUpdateManager _updateRepository;
        readonly IQueryManager _queryProxy;

        public ManageOAuthSourceModel(IStudioUpdateManager updateRepository, IQueryManager queryProxy, string serverName)
        {
            _updateRepository = updateRepository;
            _queryProxy = queryProxy;

            ServerName = serverName;
            if (ServerName.Contains("("))
            {
                ServerName = serverName.Substring(0, serverName.IndexOf("(", StringComparison.Ordinal));
            }

        }

        #region Implementation of IManageDatabaseSourceModel

        
        public void Save(IOAuthSource toDbSource)
        {
            _updateRepository.Save(toDbSource);
        }

        public string ServerName { get; private set; }
    

        public IOAuthSource FetchSource(Guid resourceID)
        {
            var xaml = _queryProxy.FetchResourceXaml(resourceID);
            var db = new DropBoxSource(xaml.ToXElement());
            return db;
        }

        #endregion
    }
}
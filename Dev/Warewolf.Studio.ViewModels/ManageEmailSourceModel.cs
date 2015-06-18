using Dev2.Common.Interfaces.Email;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.Studio.ViewModels.Dialogues;

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
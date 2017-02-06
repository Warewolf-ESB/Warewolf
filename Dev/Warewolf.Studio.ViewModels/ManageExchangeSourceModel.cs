using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.ToolBase.ExchangeEmail;

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

        public string ServerName { get; private set; }

        #endregion
    }
}

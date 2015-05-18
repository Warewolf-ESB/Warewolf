using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.Studio.ViewModels.Dialogues;

namespace Warewolf.Studio.ViewModels
{
    public class ManageWebServiceSourceModel : IManageWebServiceSourceModel
    {
        readonly IStudioUpdateManager _updateRepository;
        readonly IQueryManager _queryProxy;

        public ManageWebServiceSourceModel(IStudioUpdateManager updateRepository, IQueryManager queryProxy,string serverName)
        {
            _updateRepository = updateRepository;
            _queryProxy = queryProxy;
           
            
        }

        #region Implementation of IManageWebServiceSourceModel

        public void TestConnection(IWebServiceSource resource)
        {
        }

        public void Save(IWebServiceSource toDbSource)
        {
        }


        #endregion
    }
}
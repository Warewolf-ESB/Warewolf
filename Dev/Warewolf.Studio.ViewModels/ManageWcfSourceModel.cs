using Dev2.Common.Interfaces;

namespace Warewolf.Studio.ViewModels
{
    public class ManageWcfSourceModel : IWcfSourceModel
    {
        readonly IStudioUpdateManager _updateRepository;

        // ReSharper disable once UnusedParameter.Local
        public ManageWcfSourceModel(IStudioUpdateManager updateRepository, string serverName)
        {
            _updateRepository = updateRepository;
        }

        public void TestConnection(IWcfServerSource resource)
        {
            _updateRepository.TestConnection(resource);
        }

        public void Save(IWcfServerSource resource)
        {
            _updateRepository.Save(resource);
        }

        public string ServerName { get; set; }
    }
}

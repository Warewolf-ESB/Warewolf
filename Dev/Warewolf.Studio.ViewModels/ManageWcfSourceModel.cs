using Dev2.Common.Interfaces;

namespace Warewolf.Studio.ViewModels
{
    public class ManageWcfSourceModel : IWcfSourceModel
    {
        public IStudioUpdateManager UpdateRepository { get; }

        // ReSharper disable once UnusedParameter.Local
        public ManageWcfSourceModel(IStudioUpdateManager updateRepository, string serverName)
        {
            UpdateRepository = updateRepository;
        }

        public void TestConnection(IWcfServerSource resource)
        {
            UpdateRepository.TestConnection(resource);
        }

        public void Save(IWcfServerSource resource)
        {
            UpdateRepository.Save(resource);
        }

        public string ServerName { get; set; }
    }
}

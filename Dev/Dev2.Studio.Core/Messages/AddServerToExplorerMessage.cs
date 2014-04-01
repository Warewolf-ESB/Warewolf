using Dev2.Studio.Core.Interfaces;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.Messages
{
    public class AddServerToExplorerMessage : IMessage
    {
        public AddServerToExplorerMessage(IEnvironmentModel environmentModel, bool forceConnect = false)
        {
            EnvironmentModel = environmentModel;
            ForceConnect = forceConnect;
        }

        public IEnvironmentModel EnvironmentModel { get; set; }
        public bool ForceConnect { get; set; }
    }
}
using Dev2.Studio.Core.Interfaces;

namespace Dev2.Studio.Core.Messages
{
    public class RemoveServerFromExplorerMessage:IMessage
    {
        public IEnvironmentModel EnvironmentModel { get; set; }

        public RemoveServerFromExplorerMessage(IEnvironmentModel environmentModel)
        {
            EnvironmentModel = environmentModel;
        }
    }
}
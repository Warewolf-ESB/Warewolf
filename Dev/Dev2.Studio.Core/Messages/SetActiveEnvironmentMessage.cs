using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Interfaces.Messages;

namespace Dev2.Studio.Core.Messages
{
    public class SetActiveEnvironmentMessage : IMessage
    {
        public IEnvironmentModel EnvironmentModel { get; set; }

        public SetActiveEnvironmentMessage(IEnvironmentModel environmentModel)
        {
            EnvironmentModel = environmentModel;
        }
    }
}
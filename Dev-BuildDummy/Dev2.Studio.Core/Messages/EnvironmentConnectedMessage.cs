using Dev2.Studio.Core.Interfaces;

namespace Dev2.Studio.Core.Messages
{
    public class EnvironmentConnectedMessage : AbstractEnvironmentMessage
    {
        public EnvironmentConnectedMessage(IEnvironmentModel environmentModel) : base(environmentModel)
        {
        }
    }
}

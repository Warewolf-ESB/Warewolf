using Dev2.Studio.Core.Interfaces;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.Messages
{
    public class EnvironmentDisconnectedMessage : AbstractEnvironmentMessage
    {
        public EnvironmentDisconnectedMessage(IEnvironmentModel environmentModel)
            : base(environmentModel)
        {
        }
    }
}

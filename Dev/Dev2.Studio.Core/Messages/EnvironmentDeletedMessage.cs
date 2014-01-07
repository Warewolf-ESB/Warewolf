using Dev2.Studio.Core.Interfaces;

namespace Dev2.Studio.Core.Messages
{
    public class EnvironmentDeletedMessage : AbstractEnvironmentMessage
    {
        public EnvironmentDeletedMessage(IEnvironmentModel environmentModel)
            : base(environmentModel)
        {
        }
    }
}

using Dev2.Studio.Core.Interfaces;

namespace Dev2.Studio.Core.Messages
{
    public abstract class AbstractEnvironmentMessage : IEnvironmentMessage
    {
        protected AbstractEnvironmentMessage(IEnvironmentModel environmentModel)
        {
            EnvironmentModel = environmentModel;
        }

        public IEnvironmentModel EnvironmentModel { get; set; }
    }
}

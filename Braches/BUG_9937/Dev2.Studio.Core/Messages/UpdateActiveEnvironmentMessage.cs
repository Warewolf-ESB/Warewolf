using Dev2.Studio.Core.Interfaces;

namespace Dev2.Studio.Core.Messages
{
    public class UpdateActiveEnvironmentMessage : IMessage
    {
        public IEnvironmentModel EnvironmentModel { get; set; }

        public UpdateActiveEnvironmentMessage(IEnvironmentModel environmentModel)
        {
            EnvironmentModel = environmentModel;
        }
    }
}
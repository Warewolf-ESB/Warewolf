using Dev2.Studio.Core.Interfaces;

namespace Dev2.Studio.Core.Messages
{
    public class RemoveEnvironmentMessage : IMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public RemoveEnvironmentMessage(IEnvironmentModel environmentModel)
        {
            EnvironmentModel = environmentModel;
        }

        public IEnvironmentModel EnvironmentModel { get; set; }
    }
}
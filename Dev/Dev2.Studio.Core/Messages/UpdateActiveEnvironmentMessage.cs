using Dev2.Studio.Core.Interfaces;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.Messages
{
    public class UpdateActiveEnvironmentMessage : IMessage
    {
        public IEnvironmentModel EnvironmentModel { get; set; }

        public bool SetFromConnectControl { get; set; }

        public UpdateActiveEnvironmentMessage(IEnvironmentModel environmentModel, bool setFromConnectControl = false)
        {
            EnvironmentModel = environmentModel;
            SetFromConnectControl = setFromConnectControl;
        }
    }
}
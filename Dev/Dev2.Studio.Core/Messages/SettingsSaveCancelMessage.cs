
using Dev2.Studio.Core.Interfaces;

namespace Dev2.Studio.Core.Messages
{
    public class SettingsSaveCancelMessage : IMessage
    {
        public IEnvironmentModel Environment { get; set; }

        public SettingsSaveCancelMessage(IEnvironmentModel environment)
        {
            Environment = environment;
        }
    }
}

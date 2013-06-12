using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Interfaces.Messages;

namespace Dev2.Studio.Core.Messages
{
    public interface IEnvironmentMessage : IMessage
    {
        IEnvironmentModel EnvironmentModel { get; set; }
    }
}

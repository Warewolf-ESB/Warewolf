using Dev2.Studio.Core.Interfaces;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.Messages
{
    public interface IEnvironmentMessage : IMessage
    {
        IEnvironmentModel EnvironmentModel { get; set; }
    }
}


// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.Network
{
    public interface IServiceEndpointGenerationStrategyProvider
    {
        void RegisterEndpointGenerationStrategies(IServiceLocator serviceLocator);
    }
}

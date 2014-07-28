using Dev2.Studio.Core.Interfaces;

namespace Dev2.ConnectionHelpers
{
    public interface IConnectControlEnvironment
    {
        IEnvironmentModel EnvironmentModel { get; set; }
        bool IsConnected { get; set; }
        string ConnectedText { get; set; }
        string DisplayName { get; set; }
        bool AllowEdit { get; set; }
    }
}
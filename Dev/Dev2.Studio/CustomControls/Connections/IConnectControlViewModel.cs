using Dev2.Studio.Core.Interfaces;

namespace Dev2.CustomControls.Connections
{
    public interface IConnectControlViewModel
    {
        void SetTargetEnvironment();
        void UpdateActiveEnvironment(IEnvironmentModel environmentModel, bool isSetFromConnectControl);
    }
}
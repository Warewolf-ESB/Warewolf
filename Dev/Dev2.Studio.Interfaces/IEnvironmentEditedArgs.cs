using Dev2.Studio.Interfaces;

namespace Dev2.Studio.Core
{
    public interface IEnvironmentEditedArgs
    {
        IEnvironmentModel Environment { get; set; }
        bool IsConnected { get; set; }
    }
}
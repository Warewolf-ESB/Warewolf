using Dev2.Studio.Interfaces;

namespace Dev2.Studio.Core
{
    public interface IEnvironmentEditedArgs
    {
        IServer Environment { get; set; }
        bool IsConnected { get; set; }
    }
}
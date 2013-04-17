
namespace Dev2.Studio.Core.Services
{
    public interface IWindowsServiceManager
    {
        bool Exists();
        bool IsRunning();
        bool Start();
        bool Stop();
    }
}

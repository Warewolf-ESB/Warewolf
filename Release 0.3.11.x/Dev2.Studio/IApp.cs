using System.Windows;

namespace Dev2.Studio
{
    public interface IApp
    {
        void Shutdown();
        bool ShouldRestart { get; set; }
        Window MainWindow { get; set; }
    }
}

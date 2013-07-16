using System.Windows;
using Dev2.Studio;

namespace Dev2.Core.Tests.Diagnostics
{
    public class MockApp : IApp
    {
        public virtual void Shutdown(){}
        public virtual bool ShouldRestart { get; set; }
        public Window MainWindow { get; set; }
    }
}

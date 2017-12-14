using System;
using System.Collections;
using System.Reflection;
using System.Windows;
using Dev2.Studio.Core.Interfaces;

namespace Dev2.Studio.Core
{
    public class ApplicationAdaptor : IApplicationAdaptor
    {
        private readonly Application _realApp;

        public ApplicationAdaptor(Application realApp)
        {
            _realApp = realApp;
        }
        public Assembly ResourceAssembly
        {
            get => Application.ResourceAssembly;
            set => Application.ResourceAssembly = value;
        }

        public Application Current => _realApp;

        public WindowCollection Windows => _realApp.Windows;

        public Window MainWindow
        {
            get => _realApp.MainWindow;
            set => _realApp.MainWindow = value;
        }

        public ShutdownMode ShutdownMode
        {
            get => _realApp.ShutdownMode;
            set => _realApp.ShutdownMode = value;
        }

        public ResourceDictionary Resources
        {
            get => _realApp.Resources;
            set => _realApp.Resources = value;
        }

        public Uri StartupUri
        {
            get => _realApp.StartupUri;
            set => _realApp.StartupUri = value;
        }

        public IDictionary Properties => _realApp.Properties;

        public object FindResource(object resourceKey)
        {
            return _realApp.FindResource(resourceKey);
        }

        public int Run(Window window)
        {
            return _realApp.Run(window);
        }

        public int Run()
        {
            return _realApp.Run();
        }

        public void Shutdown()
        {
            _realApp.Shutdown();
        }

        public void Shutdown(int exitCode)
        {
            _realApp.Shutdown(exitCode);
        }

        public object TryFindResource(object resourceKey)
        {
            return _realApp.TryFindResource(resourceKey);
        }
    }
}

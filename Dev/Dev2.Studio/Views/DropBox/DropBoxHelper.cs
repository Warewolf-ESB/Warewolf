using System.Windows.Controls;
using Dev2.CustomControls;
using Dev2.Studio.Core.Interfaces;

namespace Dev2.Views.DropBox
{
    public class DropBoxHelper : IDropBoxHelper
    {
        readonly IEnvironmentModel _activeEnvironment;
        readonly string _resourceType;
        readonly string _resourcePath;

        public DropBoxHelper(DropBoxViewWindow dropBoxViewWindow)
        {
            CircularProgressBar = dropBoxViewWindow.CircularProgressBar;
            WebBrowser = dropBoxViewWindow.WebBrowserHost;
            DropBoxViewWindow = dropBoxViewWindow;

        }

        public DropBoxHelper(DropBoxViewWindow dropBoxViewWindow, IEnvironmentModel activeEnvironment, string resourceType, string resourcePath)
        {
            _activeEnvironment = activeEnvironment;
            _resourceType = resourceType;
            _resourcePath = resourcePath;
            DropBoxViewWindow = dropBoxViewWindow;
            WebBrowser = dropBoxViewWindow.WebBrowserHost;
            CircularProgressBar = dropBoxViewWindow.CircularProgressBar;
        }

        #region Implementation of IDropBoxHelper

        public DropBoxViewWindow DropBoxViewWindow { get; private set; }
        public WebBrowser WebBrowser { get; private set; }
        public CircularProgressBar CircularProgressBar { get; private set; }

        public IEnvironmentModel ActiveEnvironment
        {
            get
            {
                return _activeEnvironment;
            }
            set
            {
            }
        }
        public string ResourceType
        {
            get
            {
                return _resourceType;
            }
            set
            {
            }
        }
        public string ResourcePath
        {
            get
            {
                return _resourcePath;
            }
            set
            {
            }
        }

        public void Navigate(string uri)
        {
            DropBoxViewWindow.WebBrowserHost.Navigate(uri);
        }

        public void CloseAndSave(DropBoxSourceViewModel dropBoxSourceViewModel)
        {
            DropBoxViewWindow.DialogResult = true;
            DropBoxViewWindow.Close();
        }

        #endregion
    }
}
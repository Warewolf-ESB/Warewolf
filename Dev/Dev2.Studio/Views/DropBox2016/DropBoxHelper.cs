using System.Collections.Generic;
using System.Windows.Controls;
using Dev2.CustomControls;
using Dev2.Studio.Core.Interfaces;
using Warewolf.Studio.ViewModels;

namespace Dev2.Views.DropBox2016
{
    public class DropBoxHelper : IDropBoxHelper
    {
        public DropBoxHelper(DropBoxViewWindow dropBoxViewWindow)
        {
            VerifyArgument.IsNotNull("dropBoxViewWindow",dropBoxViewWindow);
            DropBoxViewWindow = dropBoxViewWindow;

        }

        // ReSharper disable once TooManyDependencies
        public DropBoxHelper(DropBoxViewWindow dropBoxViewWindow, IEnvironmentModel activeEnvironment, string resourceType, string resourcePath)
        {
            VerifyArgument.AreNotNull(new Dictionary<string, object>{{"dropBoxViewWindow",dropBoxViewWindow},{"activeEnvironment",activeEnvironment},{"resourceType",resourceType},{"resourcePath",resourcePath}});
            ActiveEnvironment = activeEnvironment;
            ResourceType = resourceType;
            ResourcePath = resourcePath;
            DropBoxViewWindow = dropBoxViewWindow;

        }

        #region Implementation of IDropBoxHelper

        public DropBoxViewWindow DropBoxViewWindow { get; private set; }
        public WebBrowser WebBrowser => DropBoxViewWindow.WebBrowserHost;
        public CircularProgressBar CircularProgressBar => DropBoxViewWindow.CircularProgressBar;

        public IEnvironmentModel ActiveEnvironment { get; set; }
        public string ResourceType { get; set; }
        public string ResourcePath { get; set; }

        public void Navigate(string uri)
        {
            DropBoxViewWindow.WebBrowserHost.Navigate(uri);
        }

        public void CloseAndSave(ManageOAuthSourceViewModel manageOAuthSourceViewModel)
        {
            DropBoxViewWindow.DialogResult = true;
            DropBoxViewWindow.Close();
        }

        #endregion
    }
}
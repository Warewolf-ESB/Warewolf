using System.Windows;
using Dev2.Studio.Core.AppResources.Browsers;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Webs;

namespace Dev2.Webs
{
    public static class WebSites
    {
        #region CreateWebPageDialog

        public static Window CreateWebPageDialog(this IEnvironmentModel environment, string website, string relativeUriString, IPropertyEditorWizard callbackHandler, double width = 800, double height = 600, string leftTitle = "", string rightTitle = "")
        {
            var uriString = string.Format("{0}{1}/{2}", environment.Connection.WebServerUri, website.Trim('/'), relativeUriString.Trim('/'));
            return CreateWebPageDialog(uriString, callbackHandler, width, height, leftTitle, rightTitle);
        }

        public static Window CreateWebPageDialog(string absoluteUriString, IPropertyEditorWizard callbackHandler, double width = 800, double height = 600, string leftTitle = "", string rightTitle = "")
        {
            Browser.CallbackHandler.PropertyEditorViewModel = callbackHandler;

            var window = new WebBrowserWindow { Width = width, Height = height };

            var viewModel = new WebBrowserViewModel { RightTitle = rightTitle, LeftTitle = leftTitle };
            window.DataContext = viewModel;

            callbackHandler.NavigateRequested += uri => window.Browser.LoadSafe(uri);
            callbackHandler.Owner = window;

            window.Browser.LoadSafe(absoluteUriString);

            return window;
        }

        #endregion

        #region ShowWebPageDialog(this IEnvironmentModel environment)

        public static void ShowWebPageDialog(this IEnvironmentModel environment, string website, string relativeUriString, IPropertyEditorWizard callbackHandler, double width, double height, string leftTitle = "", string rightTitle = "")
        {
            var window = CreateWebPageDialog(environment, website, relativeUriString, callbackHandler, width, height, leftTitle, rightTitle);
            window.ShowDialog();
        }

        public static void ShowWebPageDialog(string absoluteUriString, IPropertyEditorWizard callbackHandler, double width = 800, double height = 600, string leftTitle = "", string rightTitle = "")
        {
            var window = CreateWebPageDialog(absoluteUriString, callbackHandler, width, height, leftTitle, rightTitle);
            window.ShowDialog();
        }

        #endregion

    }
}

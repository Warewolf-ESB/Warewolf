using System.Windows;
using Dev2.Studio.Core.AppResources.Browsers;
using Dev2.Studio.Core.Interfaces;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Webs
{
    public static class WebSites
    {
        #region CreateWebPageDialog

        public static Window CreateWebPageDialog(this IEnvironmentModel environment, string website, string relativeUriString, IPropertyEditorWizard callbackHandler, double width = 800, double height = 600)
        {
            var uriString = string.Format("{0}{1}/{2}", environment.Connection.WebServerUri, website.Trim('/'), relativeUriString.Trim('/'));
            return CreateWebPageDialog(uriString, callbackHandler, width, height);
        }

        public static Window CreateWebPageDialog(string absoluteUriString, IPropertyEditorWizard callbackHandler, double width = 800, double height = 600)
        {
            Browser.CallbackHandler.PropertyEditorViewModel = callbackHandler;

            var window = new WebBrowserWindow { Width = width, Height = height };

            callbackHandler.NavigateRequested += uri => window.Browser.LoadSafe(uri);
            callbackHandler.Owner = window;

            window.Browser.LoadSafe(absoluteUriString);

            return window;
        }

        #endregion

        #region ShowWebPageDialog(this IEnvironmentModel environment)

        public static void ShowWebPageDialog(this IEnvironmentModel environment, string website, string relativeUriString, IPropertyEditorWizard callbackHandler, double width, double height)
        {
            var window = CreateWebPageDialog(environment, website, relativeUriString, callbackHandler, width, height);
            window.ShowDialog();
        }

        public static void ShowWebPageDialog(string absoluteUriString, IPropertyEditorWizard callbackHandler, double width = 800, double height = 600)
        {
            var window = CreateWebPageDialog(absoluteUriString, callbackHandler, width, height);
            window.ShowDialog();
        }

        #endregion

    }
}

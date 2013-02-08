using Dev2.Studio.Core.AppResources.Browsers;
using Dev2.Studio.Core.Interfaces;
using System.Windows;
using Unlimited.Applications.BusinessDesignStudio.Views;

namespace Dev2.Studio.Webs
{
    public static class WebSites
    {
        #region CreateWebPageDialog

        public static Window CreateWebPageDialog(this IEnvironmentModel environment, string website, string relativeUriString, IPropertyEditorWizard callbackHandler, double width = 800, double height = 600)
        {
            Browser.CallbackHandler.PropertyEditorViewModel = callbackHandler;

            var uriString = string.Format("{0}{1}/{2}", environment.WebServerAddress, website.Trim('/'), relativeUriString.Trim('/'));
            var window = new WebPropertyEditorWindow { Width = width, Height = height };

            callbackHandler.NavigateRequested += uri => window.Browser.LoadSafe(uri);
            callbackHandler.Owner = window;

            window.Browser.LoadSafe(uriString);

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
            Browser.CallbackHandler.PropertyEditorViewModel = callbackHandler;

            var window = new WebPropertyEditorWindow { Width = width, Height = height };

            callbackHandler.NavigateRequested += uri => window.Browser.LoadSafe(uri);
            callbackHandler.Owner = window;

            window.Browser.LoadSafe(absoluteUriString);
            window.ShowDialog();
        }

        #endregion

    }
}

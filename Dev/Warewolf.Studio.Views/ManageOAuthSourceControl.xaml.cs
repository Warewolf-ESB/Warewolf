using System;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Navigation;
using Dev2.Common.Interfaces;
using mshtml;
using Microsoft.Practices.Prism.Mvvm;
using Warewolf.Studio.Core;
using Warewolf.Studio.ViewModels;

namespace Warewolf.Studio.Views
{
    /// <summary>
    /// Interaction logic for ManageOAuthSourceControl.xaml
    /// </summary>
    public partial class ManageOAuthSourceControl : IView, ICheckControlEnabledView, IWebBrowser
    {
        public ManageOAuthSourceControl()
        {
            InitializeComponent();
            HideScriptErrors(WebBrowserHost, true);

            DataContextChanged += (sender, args) =>
              {
                  ViewModel = args.NewValue as ManageOAuthSourceViewModel;
                  if (ViewModel != null)
                  {
                      ViewModel.WebBrowser = this;
                  }
              };

            WebBrowserHost.Navigated += (sender, args) =>
            {
                Navigated?.Invoke(args.Uri);
            };
        }

        public void Navigate(Uri uri)
        {
            WebBrowserHost.Navigate(uri);
        }

        public event Action<Uri> Navigated;

        private ManageOAuthSourceViewModel ViewModel { get; set; }

        private void HideScriptErrors(WebBrowser wb, bool hide)
        {
            FieldInfo fiComWebBrowser = typeof(WebBrowser).GetField("_axIWebBrowser2", BindingFlags.Instance | BindingFlags.NonPublic);

            object objComWebBrowser = fiComWebBrowser?.GetValue(wb);

            objComWebBrowser?.GetType().InvokeMember("Silent", BindingFlags.SetProperty, null, objComWebBrowser, new object[] { hide });
        }

        private void WebBrowserHost_OnLoadCompleted(object sender, NavigationEventArgs e)
        {
            var browser = sender as WebBrowser;

            if (browser?.Document == null)
            {
                return;
            }

            dynamic document = browser.Document;

            if (document.readyState != "complete")
            {
                return;
            }

            var title = ((HTMLDocument)WebBrowserHost.Document).title;
            if (title == "Dropbox - 400")
            {
                ViewModel.TestMessage = "";
                ViewModel.TestPassed = false;
                ViewModel.TestFailed = true;
                ViewModel.Testing = false;
            }

            dynamic script = document.createElement("script");
            script.type = @"text/javascript";
            script.text = @"window.onerror = function(msg,url,line){return true;}";
            document.head.appendChild(script);
        }

        #region Implementation of ICheckControlEnabledView

        public bool GetControlEnabled(string controlName)
        {
            return false;
        }

        #endregion
    }
}

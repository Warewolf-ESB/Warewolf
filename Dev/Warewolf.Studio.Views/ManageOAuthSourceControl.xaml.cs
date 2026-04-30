#pragma warning disable
using System;
using Dev2.Common.Interfaces;
using mshtml;
using Warewolf.Studio.Core;
using Warewolf.Studio.ViewModels;
#if !NETFRAMEWORK
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.Rendering;
#else
using Prism.Mvvm;
#endif

namespace Warewolf.Studio.Views
{
    public partial class ManageOAuthSourceControl : IView, ICheckControlEnabledView, IWebBrowser
    {
        public ManageOAuthSourceControl()
        {
            InitializeComponent();

            DataContextChanged += (sender, args) =>
            {
                ViewModel = args.NewValue as ManageOAuthSourceViewModel;
                if (ViewModel != null)
                {
                    ViewModel.WebBrowser = this;
                }
            };

            WebBrowserHost.NavigationCompleted += WebBrowserHost_NavigationCompleted;
        }

        public void Navigate(Uri uri) => WebBrowserHost.Source = uri;

        public event Action<Uri> Navigated;

        ManageOAuthSourceViewModel ViewModel { get; set; }

        void WebBrowserHost_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            var core = WebBrowserHost.CoreWebView2;
            if (core == null)
            {
                return;
            }

            if (ViewModel != null && core.DocumentTitle == "Dropbox - 400")
            {
                ViewModel.TestMessage = "";
                ViewModel.TestPassed = false;
                ViewModel.TestFailed = true;
                ViewModel.Testing = false;
            }

            if (Uri.TryCreate(core.Source, UriKind.Absolute, out var uri))
            {
                Navigated?.Invoke(uri);
            }
        }

        #region Implementation of ICheckControlEnabledView

        public bool GetControlEnabled(string controlName) => false;

#if !NETFRAMEWORK
		public Task RenderAsync(ViewContext context)
		{
			throw new NotImplementedException();
		}
#endif

		#endregion
	}
}

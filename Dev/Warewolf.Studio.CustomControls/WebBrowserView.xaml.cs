/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Web.WebView2.Core;
using Warewolf.Studio.Core;

namespace Warewolf.Studio.CustomControls
{
    [SuppressMessage("ReSharper", "CC0091")]
    public partial class WebBrowserView : IView
    {
        private readonly Grid _blackoutGrid = new Grid();

        public WebBrowserView(string licenseType)
        {
            InitializeComponent();
            PopupViewManageEffects.AddBlackOutEffect(_blackoutGrid);
            InitializeWebView(licenseType);
        }

        private void InitializeWebView(string licenseType)
        {           
            var pathFolder = Environment.Is64BitOperatingSystem ? "WebView\\x64" : "WebView\\x86";
            var path = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), pathFolder);

            webView.CreationProperties =
                  new Microsoft.Web.WebView2.Wpf.CoreWebView2CreationProperties { BrowserExecutableFolder = path };
            webView.Source = ScriptManager.GetSourceUri(licenseType);
        }

        private void webView_NavigationStarting(object sender, CoreWebView2NavigationStartingEventArgs e)
        {
            var helper = new ScriptManager(this);

            webView.EnsureCoreWebView2Async().Wait();
            webView.CoreWebView2.AddHostObjectToScript(nameof(helper), helper);
        }

        private void webView_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {

        }

        private void WebBrowserView_OnClosing(object sender, CancelEventArgs e)
        {
            PopupViewManageEffects.RemoveBlackOutEffect(_blackoutGrid);
        }
    }
}
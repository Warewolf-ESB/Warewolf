/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
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
        private string _licenseType = "";

        public WebBrowserView(string licenseType)
        {
            InitializeComponent();
            PopupViewManageEffects.AddBlackOutEffect(_blackoutGrid);
            _licenseType = licenseType;
        }

        public async System.Threading.Tasks.Task Initialize()
        {
            var helper = new ScriptManager(this);
            const string installPath = "Microsoft.WebView2.FixedVersionRuntime.93.0.961.52.x86";

            webView.CreationProperties = new Microsoft.Web.WebView2.Wpf.CoreWebView2CreationProperties { BrowserExecutableFolder = installPath };
            var webView2Environment = await Microsoft.Web.WebView2.Core.CoreWebView2Environment.CreateAsync(installPath);

            await webView.EnsureCoreWebView2Async(webView2Environment);
            webView.CoreWebView2.AddHostObjectToScript(nameof(helper), helper);

            webView.Source = ScriptManager.GetSourceUri(_licenseType);
        }

        private void WebBrowserOnNavigated(object sender, NavigationEventArgs e)
        {
            
        }

        private void WebBrowser_OnNavigating(object sender, NavigatingCancelEventArgs e)
        {
            var browser = sender as WebBrowser;

            if(browser?.Document is null)
            {
                return;
            }

            dynamic document = browser.Document;

            if(document.readyState != "complete")
                return;

            var script = document.createElement("script");
            script.type = @"text/javascript";
            script.text = @"window.onerror = function(msg,url,line){return true;}";
            document.head.appendChild(script);
        }

        private void WebBrowserView_OnClosing(object sender, CancelEventArgs e)
        {
            PopupViewManageEffects.RemoveBlackOutEffect(_blackoutGrid);
        }

        private void WebBrowser_OnNavigating(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs e)
        {

        }

        private void webView_NavigationStarting(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs e)
        {
            //var helper = new ScriptManager(this);


            //var installPath = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory() , "WebView");  //@"C:\Program Files (x86)\WebView2Runtime\Microsoft.WebView2.FixedVersionRuntime.88.0.705.81.x64\";
            //var webView2Environment = Microsoft.Web.WebView2.Core.CoreWebView2Environment.CreateAsync(installPath).Result;

            //webView.EnsureCoreWebView2Async(webView2Environment).Wait();
            //webView.CoreWebView2.AddHostObjectToScript(nameof(helper), helper);
        }

        private void webView_NavigationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            
        }
    }
}
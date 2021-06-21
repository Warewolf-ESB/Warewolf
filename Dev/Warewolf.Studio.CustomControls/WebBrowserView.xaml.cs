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

            var helper = new ScriptManager(this);
            webBrowser.ObjectForScripting = helper;
            webBrowser.AllowDrop = false;
            webBrowser.Source = ScriptManager.GetSourceUri(licenseType);
        }

        private void WebBrowserOnNavigated(object sender, NavigationEventArgs e)
        {
            ScriptManager.SetSilent(webBrowser, true); // make it silent
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
    }
}
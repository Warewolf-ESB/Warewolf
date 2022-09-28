/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Net;
using System.Windows;
using Dev2.CustomControls;
using Dev2.Studio.Views.Help;
using Microsoft.Web.WebView2.Wpf;

namespace Dev2.ViewModels.Help
{
    public class HelpViewWrapper : IHelpViewWrapper
    {
        public HelpViewWrapper(HelpView view)
        {
            HelpView = view;
        }

        public HelpView HelpView { get; private set; }

        public WebView2 WebBrowser => HelpView.webView;

        public CircularProgressBar CircularProgressBar => HelpView.CircularProgressBar;

        public Visibility WebBrowserVisibility  
        {
            get
            {
                return WebBrowser.Visibility;
            }
            set
            {
                WebBrowser.Visibility = value;
            }
        }

         public Visibility CircularProgressBarVisibility  
        {
            get
            {
                return CircularProgressBar.Visibility;
            }
            set
            {
                CircularProgressBar.Visibility = value;
            }
        }

        public void Navigate(string uri)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3;
            HelpView.webView.Source = new Uri(uri, UriKind.Absolute);
        }
    }
}

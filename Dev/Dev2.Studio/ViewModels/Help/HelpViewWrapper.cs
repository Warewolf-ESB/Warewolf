
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Windows;
using System.Windows.Controls;
using Dev2.CustomControls;
using Dev2.Studio.Views.Help;

namespace Dev2.ViewModels.Help
{
    public class HelpViewWrapper : IHelpViewWrapper
    {
        public HelpViewWrapper(HelpView view)
        {
            HelpView = view;
        }

        public HelpView HelpView { get; private set; }

        public WebBrowser WebBrowser
        {
            get
            {
                return HelpView.WebBrowserHost;
            }
        }

        public CircularProgressBar CircularProgressBar
        {
            get
            {
                return HelpView.CircularProgressBar;
            }
        }

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
            HelpView.WebBrowserHost.Navigate(uri);
        }
    }
}

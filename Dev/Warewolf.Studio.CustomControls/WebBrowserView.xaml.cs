using System;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Web;
using System.Windows;
using System.Windows.Navigation;

namespace Warewolf.Studio.CustomControls
{
    [SuppressMessage("ReSharper", "CC0091")]
    public partial class WebBrowserView : Window
    {
        public WebBrowserView()
        {
            InitializeComponent();
        }

        private void TestButton_OnClick(object sender, RoutedEventArgs e)
        {
            var helper = new ScriptHelper(this);
            this.webBrowser.ObjectForScripting = helper;

            var curDir = Directory.GetCurrentDirectory();
            var url = new Uri($"file:///{curDir}/ChargeBee.html");

            webBrowser.Source = url;
            //
            // webBrowser.InvokeScript("function");
        }

        private void WebBrowser_OnNavigating(object sender, NavigatingCancelEventArgs e)
        {
            var urlParameters = HttpUtility.ParseQueryString(e.Uri.ToString());
            // extract the post data from the url.
            var postdata = urlParameters["http://localhost/AlertDisplay.aspx?txtSomething"];
            if (postdata != null)
            {
                MessageBox.Show(postdata);
            }
        }
    }
}
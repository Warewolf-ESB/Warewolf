/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace Dev2.Views.Dialogs
{
    /// <summary>
    /// Interaction logic for WebLatestVersionDialog.xaml
    /// </summary>
    public partial class WebLatestVersionDialog
    {
        public WebLatestVersionDialog()
        {
            InitializeComponent();
            Browser.Navigated += Navigated;
            Browser.Navigate(new Uri("http://warewolf.io/start_new.php"));
        }

        void Navigated(object sender, NavigationEventArgs e)
        {
            SetSilent(Browser,true);
        }

        void WbLoadCompleted(object sender, NavigationEventArgs e)
        {
            Browser.Width = Browser.ActualWidth + 64;
            Browser.Height = Browser.ActualHeight + 32;
        }


        private static void SetSilent(WebBrowser browser, bool silent)
        {
            if (browser == null)
                throw new ArgumentNullException("browser");

            // get an IWebBrowser2 from the document
            IOleServiceProvider sp = browser.Document as IOleServiceProvider;
            if (sp != null)
            {
                Guid iidIWebBrowserApp = new Guid("0002DF05-0000-0000-C000-000000000046");
                Guid iidIWebBrowser2 = new Guid("D30C1661-CDAF-11d0-8A3E-00C04FC9E26E");

                object webBrowser;
                sp.QueryService(ref iidIWebBrowserApp, ref iidIWebBrowser2, out webBrowser);
                if (webBrowser != null)
                {
                    webBrowser.GetType().InvokeMember("Silent", BindingFlags.Instance | BindingFlags.Public | BindingFlags.PutDispProperty, null, webBrowser, new object[] { silent });
                }
            }
        }
    }

    [ComImport, Guid("6D5140C1-7436-11CE-8034-00AA006009FA"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IOleServiceProvider
    {
        [PreserveSig]
        int QueryService([In] ref Guid guidService, [In] ref Guid riid, [MarshalAs(UnmanagedType.IDispatch)] out object ppvObject);
    }

}

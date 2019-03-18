#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
﻿using System;
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

        public void Navigate(Uri uri) => WebBrowserHost.Navigate(uri);

        public event Action<Uri> Navigated;

        ManageOAuthSourceViewModel ViewModel { get; set; }

        void HideScriptErrors(WebBrowser wb, bool hide)
        {
            var fiComWebBrowser = typeof(WebBrowser).GetField("_axIWebBrowser2", BindingFlags.Instance | BindingFlags.NonPublic);

            var objComWebBrowser = fiComWebBrowser?.GetValue(wb);

            objComWebBrowser?.GetType().InvokeMember("Silent", BindingFlags.SetProperty, null, objComWebBrowser, new object[] { hide });
        }

        void WebBrowserHost_OnLoadCompleted(object sender, NavigationEventArgs e)
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

            var script = document.createElement("script");
            script.type = @"text/javascript";
            script.text = @"window.onerror = function(msg,url,line){return true;}";
            document.head.appendChild(script);
        }

        #region Implementation of ICheckControlEnabledView

        public bool GetControlEnabled(string controlName) => false;

        #endregion
    }
}

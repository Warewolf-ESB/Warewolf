
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Windows;
using System.Windows.Input;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Webs
{
    public partial class WebBrowserWindow
    {
        //readonly IPropertyEditorWizard _layoutObjectModel;

        #region CTOR

        // DO NOT USE THIS CONSTRUCTOR DIRECTLY! 
        // USE WebSites.ShowWebPageDialog() INSTEAD!
        public WebBrowserWindow()
        {
            InitializeComponent();
            Owner = Application.Current.MainWindow;
            Closed += OnClosed;
            Loaded += (s, e) => Browser.Focus();
        }

        //[Obsolete("use WebSites.ShowWebPageDialog() or WebSites.CreateWebPageDialog() instead")]
        //public WebBrowserWindow(IPropertyEditorWizard layoutObject, string homeUrl = null)
        //    : this()
        //{
        //    Browser.Initialize(homeUrl, layoutObject);

        //    _layoutObjectModel = layoutObject;
        //    _layoutObjectModel.NavigateRequested += NavigateRequested;
        //}

        #endregion

        #region OnClosed

        private void OnClosed(object sender, EventArgs eventArgs)
        {
            Browser.Dispose();
        }

        #endregion

        void WebBrowserWindow_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}

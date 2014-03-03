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

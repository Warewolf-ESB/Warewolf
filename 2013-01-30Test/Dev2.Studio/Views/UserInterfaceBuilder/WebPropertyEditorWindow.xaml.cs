using Dev2.Studio.Core.AppResources.Browsers;
using Dev2.Studio.Core.Interfaces;
using System;

namespace Unlimited.Applications.BusinessDesignStudio.Views
{
    /// <summary>
    /// Interaction logic for WebPropertyEditorWindow.xaml
    /// </summary>
    public partial class WebPropertyEditorWindow
    {
        readonly IPropertyEditorWizard _layoutObjectModel;


        #region CTOR

        // DO NOT USE THIS CONSTRUCTOR DIRECTLY! 
        // USE WebSites.ShowWebPageDialog() INSTEAD!
        public WebPropertyEditorWindow()
        {
            InitializeComponent();
        }

        [Obsolete("use WebSites.ShowWebPageDialog() or WebSites.CreateWebPageDialog() instead")]
        public WebPropertyEditorWindow(IPropertyEditorWizard layoutObject, string homeUrl = null)
        {
            InitializeComponent();

            Browser.Initialize(homeUrl, layoutObject);

            _layoutObjectModel = layoutObject;
            _layoutObjectModel.NavigateRequested += NavigateRequested;
        }

        #endregion

        protected void NavigateRequested(string uri)
        {
            _layoutObjectModel.NavigateRequested -= NavigateRequested;
            Browser.LoadSafe(uri);
        }
    }
}

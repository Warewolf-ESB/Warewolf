using System;
using System.Windows;
using Dev2.Studio.StartupResources;

namespace Dev2.Studio.Views
{
    public partial class MainView
    {
        #region Constructor

        public MainView()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        #endregion Constructor

        #region Overrides of Window

        /// <summary>
        /// Called when the <see cref="P:System.Windows.Controls.ContentControl.Content"/> property changes.
        /// </summary>
        /// <param name="oldContent">A reference to the root of the old content tree.</param><param name="newContent">A reference to the root of the new content tree.</param>
        protected override void OnContentChanged(object oldContent, object newContent)
        {
            base.OnContentChanged(oldContent, newContent);
        }

        #endregion

        void OnLoaded(object sender, RoutedEventArgs e)
        {
            Dev2SplashScreen.Close(TimeSpan.FromSeconds(0.3));
        }
    }
}
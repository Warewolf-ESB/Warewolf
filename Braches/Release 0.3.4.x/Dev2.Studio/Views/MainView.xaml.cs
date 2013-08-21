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

        void OnLoaded(object sender, RoutedEventArgs e)
        {
            Dev2SplashScreen.Close(TimeSpan.FromSeconds(0.3));
        }
    }
}
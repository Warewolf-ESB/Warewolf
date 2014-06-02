using System.ComponentModel;
using System;
using System.Windows;
using System.Windows.Interop;
using Dev2.Studio.StartupResources;
using Dev2.Studio.ViewModels;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Views
{
    public partial class MainView : System.Windows.Forms.IWin32Window
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

        #region Implementation of IWin32Window

        public IntPtr Handle
        {
            get
            {
                var interopHelper = new WindowInteropHelper(this);
                return interopHelper.Handle;
            }
        }

        #endregion

        void MainView_OnClosing(object sender, CancelEventArgs e)
        {
            MainViewModel mainViewModel = DataContext as MainViewModel;
            if(mainViewModel != null)
            {
                if(!mainViewModel.OnStudioClosing())
                {
                    e.Cancel = true;
                }

                if (mainViewModel.IsDownloading())
                {
                    e.Cancel = true;
                }
            }
        }
    }
}
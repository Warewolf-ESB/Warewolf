using Dev2.Studio.StartupResources;
using System;
using System.Windows;
using System.Windows.Interop;

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
    }
}
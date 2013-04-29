using System;
using System.Windows;
using System.Windows.Input;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.StartupResources;
using Dev2.Studio.ViewModels;

namespace Dev2.Studio.Views
{
    public partial class MainView
    {
        #region Constructor

        public MainView()
        {
            InitializeComponent();
            this.Loaded += MainView_Loaded;
        }

        void MainView_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            Dev2SplashScreen.Close(TimeSpan.FromSeconds(0.3));
        }

        public override void EndInit()
        {
            base.EndInit();
        }

        #endregion Constructor

        public void Variables_OnKeyboardLostFocus(object sender, RoutedEventArgs routedEventArgs)
        {
            var vm = this.DataContext as IMainViewModel;
            if(vm != null)
            {
                vm.AddMissingAndFindUnusedVariableForActiveWorkflow();
            }
        }
    }
}
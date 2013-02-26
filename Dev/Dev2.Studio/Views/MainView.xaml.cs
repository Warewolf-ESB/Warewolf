using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;
using Caliburn.Micro;
using Dev2.Composition;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.ViewModels;
using Dev2.Studio.ViewModels.Diagnostics;
using Dev2.Studio.Views.Diagnostics;
using Infragistics.Windows.DockManager.Events;

namespace Dev2.Studio.Views
{
    public partial class MainView
    {
        #region Constructor

        public MainView()
        {
            InitializeComponent();
            ImportService.SatisfyImports(this);
            InitializeDebugOutputWindow();
        }

        #endregion Constructor

        #region Properties

        [Import]
        public IEventAggregator EventAggregator { get; set; }

        #endregion Properties

        #region Private Methods

        private void InitializeDebugOutputWindow()
        {
            var debugOutputWindow = new DebugOutputView();
            var debugTreeViewModel = new DebugOutputViewModel();
            debugOutputWindow.DataContext = debugTreeViewModel;
            OutputPane.Content = debugOutputWindow;
        }

        #endregion Private Methods

        #region Event Handlers

        private void RibbonWindowLoaded(object sender, RoutedEventArgs e)
        {
            var dataContext = DataContext as IMainViewModel;

            if (dataContext == null) return;

            //Push up dependencies to the ViewModel
            //dataContext.UserInterfaceLayoutProvider.Manager = dockManager;
            dataContext.UserInterfaceLayoutProvider.Value.Manager = TabManager;
            dataContext.UserInterfaceLayoutProvider.Value.OutputPane = OutputPane;
            dataContext.UserInterfaceLayoutProvider.Value.NavigationPane = Explorer;
            // TODO PBI 8291
            //dataContext.UserInterfaceLayoutProvider.Value.PropertyPane = Properties;
            //dataContext.UserInterfaceLayoutProvider.Value.DataMappingPane = Mapping;
            dataContext.UserInterfaceLayoutProvider.Value.DataListPane = Variables;
            dataContext.LoadExplorerPage();
            dataContext.AddStartTabs();
        }

        private void RibbonWindow_Closing(object sender, CancelEventArgs e)
        {
            var data = DataContext as MainViewModel;
            if (data != null)
            {
                data.UserInterfaceLayoutProvider.Value.PersistTabs(TabManager.Items);
            }
            else
            {
                throw new Exception("Error - data was null in MainView.xaml.cs!");
            }
            Application.Current.Shutdown();
        }

        private void ContentPane_Closing(object sender, PaneClosingEventArgs e)
        {
            var documentToClose = sender as ContentControl;
            if (documentToClose != null)
            {
                var data = DataContext as MainViewModel;
                if (data != null)
                {
                    e.Cancel = (!data.UserInterfaceLayoutProvider.Value.RemoveDocument(documentToClose.DataContext));
                }
                else
                {
                    throw new Exception("Error - data was null in MainView.xaml.cs!");
                }
            }

            if (documentToClose != null)
            {
                EventAggregator.Publish(new TabClosedMessage(documentToClose.Content));
            }
            else
            {
                throw new Exception("Error - documentToClose was null in MainView.xaml.cs!");
            }
        }

        #endregion Event Handlers
    }
}
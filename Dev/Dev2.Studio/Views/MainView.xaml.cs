
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
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using Dev2.Studio.StartupResources;
using Dev2.Studio.ViewModels;
using Dev2.Studio.ViewModels.Workflow;
using Infragistics.Windows.Controls;
using Infragistics.Windows.DockManager;
using Infragistics.Windows.DockManager.Events;

// ReSharper disable CheckNamespace
namespace Dev2.Studio.Views
{
    public partial class MainView : System.Windows.Forms.IWin32Window
    {
        ContentPane _contentPane;

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

        public void ClearToolboxSelection()
        {
            if(Toolboxcontrol != null)
            {
                Toolboxcontrol.ClearSelection();
            }
        }

        public void ClearToolboxSearch()
        {
            if(Toolboxcontrol != null)
            {
                Toolboxcontrol.ClearSearch();
            }
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

                if(mainViewModel.IsDownloading())
                {
                    e.Cancel = true;
                }
            }
        }

        public void DockManager_OnPaneDragEnded_(object sender, PaneDragEndedEventArgs e)
        {
            _contentPane = e.Panes[0];
            UpdatePane(_contentPane);
        }

        public void UpdatePane(ContentPane contentPane)
        {
            if(contentPane == null)
            {
                throw new ArgumentNullException("contentPane");
            }
            contentPane.AllowDockingInTabGroup = true;
            contentPane.AllowDockingFloating = true;
            contentPane.AllowDockingBottom = false;
            contentPane.AllowDockingLeft = false;
            contentPane.AllowDockingRight = false;
            contentPane.AllowDockingTop = false;
            contentPane.AllowInDocumentHost = true;
            contentPane.AllowClose = true;
            contentPane.AllowDrop = true;

            contentPane.AllowDocking = true;
            contentPane.CloseButtonVisibility = Visibility.Visible;

            var windows = Application.Current.Windows;
            foreach(var window in windows)
            {
                var actuallWindow = window as Window;
                if(actuallWindow != null)
                {
                    actuallWindow.Activated -=ActuallWindowOnActivated;                     
                    actuallWindow.Activated +=ActuallWindowOnActivated;                     
                }
            }
        }

        void ActuallWindowOnActivated(object sender, EventArgs eventArgs)
        {
            MainViewModel mainViewModel = DataContext as MainViewModel;
            if (mainViewModel != null)
            {
                if(_contentPane != null)
                {
                    WorkflowDesignerViewModel workflowDesignerViewModel = _contentPane.TabHeader as WorkflowDesignerViewModel;
                    if (workflowDesignerViewModel != null)
                    {
                        mainViewModel.AddWorkSurfaceContext(workflowDesignerViewModel.ResourceModel);
                    }
                }
            }
        }

        void DockManager_OnToolWindowLoaded(object sender, PaneToolWindowEventArgs e)
        {
            Style style = new Style(typeof(TabGroupPane));
            Setter setter = new Setter(TabControl.TabStripPlacementProperty, System.Windows.Controls.Dock.Top);
            style.Setters.Add(setter);
            setter = new Setter(TemplateProperty, Resources["NewButtonTabGroupPaneTemplate"]);
            style.Setters.Add(setter);
            var res = e.Window.Resources;
            res.Add(typeof(TabGroupPane), style);
            res.Add(typeof(PaneTabItem), Resources[typeof(PaneTabItem)]);
            ControlTemplate ct2 = (ControlTemplate)Resources["MyDocumentTabItemTemplateKey"];
            res.Add(PaneTabItem.DockableTabItemTemplateKey, ct2);
        }


        private void ContentControl_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {

            
//                MainViewModel mainViewModel = DataContext as MainViewModel;
//                if (mainViewModel != null)
//                {
//                    var contentControl = sender as ContentControl;
//                    if (contentControl != null && contentControl.Content != null)
//                    {
//                        WorkflowDesignerViewModel workflowDesignerViewModel = (contentControl.DataContext as WorkflowDesignerViewModel);
//                        if (workflowDesignerViewModel != null)
//                        {
//                            mainViewModel.AddWorkSurfaceContext(workflowDesignerViewModel.ResourceModel);
//                        }
//                    }
//
//
//                }
        }


    }
}


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
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using Dev2.Studio.StartupResources;
using Dev2.Studio.ViewModels;
using Dev2.Studio.ViewModels.Workflow;
using Infragistics.Windows.DockManager;
using Infragistics.Windows.DockManager.Dragging;
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
            if (Toolboxcontrol != null)
            {
                Toolboxcontrol.ClearSelection();
            }
        }

        public void ClearToolboxSearch()
        {
            if (Toolboxcontrol != null)
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
            if (mainViewModel != null)
            {
                if (!mainViewModel.OnStudioClosing())
                {
                    e.Cancel = true;
                }

                if (mainViewModel.IsDownloading())
                {
                    e.Cancel = true;
                }
            }
        }



        public void DockManager_OnPaneDragEnded_(object sender, PaneDragEndedEventArgs e)
        {
            var contentPane = e.Panes[0];
            _contentPane = contentPane;
            UpdatePane(contentPane);
            var windows = Application.Current.Windows;
            foreach (var window in windows)
            {
                var actuallWindow = window as Window;
                if (actuallWindow != null)
                {
                    var windowType = actuallWindow.GetType();
                    if (windowType.FullName == "Infragistics.Windows.Controls.ToolWindowHostWindow")
                    {
                       actuallWindow.Activated -= ActuallWindowOnActivated;
                       actuallWindow.Activated += ActuallWindowOnActivated;
                    }
                }
            }
        }

        public void UpdatePane(ContentPane contentPane)
        {
            if(contentPane == null)
                throw new ArgumentNullException("contentPane");


            WorkflowDesignerViewModel workflowDesignerViewModel = contentPane.TabHeader as WorkflowDesignerViewModel;
            if (workflowDesignerViewModel != null && contentPane.ContentVisibility == Visibility.Visible)
                        {

                            contentPane.CloseButtonVisibility = Visibility.Visible;
                        }


            
        }

        void ContentPaneOnPreviewDragEnter(object sender, DragEventArgs dragEventArgs)
        {
            dragEventArgs.Handled = true;
        }

        void ActuallWindowOnActivated(object sender, EventArgs eventArgs)
        {
            MainViewModel mainViewModel = DataContext as MainViewModel;
            if (mainViewModel != null && _contentPane != null)
            {
                WorkflowDesignerViewModel workflowDesignerViewModel = _contentPane.TabHeader as WorkflowDesignerViewModel;
                if (workflowDesignerViewModel != null && _contentPane.ContentVisibility == Visibility.Visible)
                {
                    mainViewModel.AddWorkSurfaceContext(workflowDesignerViewModel.ResourceModel);
                }

            }

        }



        void DockManager_OnPaneDragOver(object sender, PaneDragOverEventArgs e)
        {
            if (e.DragAction.GetType() != typeof(MoveWindowAction))
            {
                var contentPane = e.Panes[0];
                MainViewModel mainViewModel = DataContext as MainViewModel;
                if (mainViewModel != null && contentPane != null)
                {
                    var windows = Application.Current.Windows;
                    foreach (var window in windows)
                    {
                        var actuallWindow = window as Window;
                        if (actuallWindow != null)
                        {
                            var windowType = actuallWindow.GetType();
                            if (windowType.FullName == "Infragistics.Windows.Controls.ToolWindowHostWindow")
                            {
                                WorkflowDesignerViewModel workflowDesignerViewModel = contentPane.TabHeader as WorkflowDesignerViewModel;
                                if (workflowDesignerViewModel != null && contentPane.ContentVisibility == Visibility.Visible)
                                {


                                    PaneDragAction paneDragAction = e.DragAction;
                                    if (paneDragAction is AddToGroupAction || paneDragAction is NewSplitPaneAction || paneDragAction is NewTabGroupAction)
                                    {
                                        e.IsValidDragAction = false;
                                        e.Cursor = Cursors.No;
                                    }
                                }

                            }
                        }
                    }
                }
            }
        }

        private void ContentControl_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {

        }

        void DockManager_OnPaneDragStarting(object sender, PaneDragStartingEventArgs e)
        {
            _contentPane = e.Panes[0];
        }
    }
}

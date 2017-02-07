/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Xml;
using Dev2.Common;
using Dev2.Settings.Scheduler;
using Dev2.Studio.ViewModels;
using Dev2.Views;
using FontAwesome.WPF;
using Infragistics.Windows.DockManager.Events;
using WinInterop = System.Windows.Interop;
using Dev2.Studio.Core;
using Dev2.Studio.ViewModels.Workflow;
using Dev2.Studio.ViewModels.WorkSurface;
using Dev2.ViewModels;
using Dev2.Workspaces;
using Infragistics.Windows.DockManager;

// ReSharper disable CheckNamespace
namespace Dev2.Studio.Views
{
    public partial class MainView : IWin32Window
    {
        private static bool _isSuperMaximising;
        private bool _isLocked;
        readonly string _savedLayout;
        private static MainView _this;

        #region Constructor

        public static MainView GetInstance()
        {
            return _this;
        }

        public MainView()
        {
            InitializeComponent();
            _isSuperMaximising = false;
            _isLocked = true;
            HideFullScreenPanel.IsHitTestVisible = false;
            ShowFullScreenPanel.IsHitTestVisible = false;
            Loaded += OnLoaded;
            KeyDown += Shell_KeyDown;
            SourceInitialized += WinSourceInitialized;

            if (File.Exists(FilePath))
            {
                GetFilePath();
                using (FileStream fs = new FileStream(FilePath, FileMode.Open, FileAccess.Read))
                {
                    var streamReader = new StreamReader(fs);
                    _savedLayout = streamReader.ReadToEnd();
                }
                if (!string.IsNullOrEmpty(_savedLayout))
                {
                    try
                    {
                        DockManager.LoadLayout(_savedLayout);
                    }
                    catch (Exception err)
                    {
                        _savedLayout = null;
                        File.Delete(FilePath);
                        Dev2Logger.Error("Unable to load layout");
                        Dev2Logger.Error(err);
                    }
                }
            }

            _this = this;
        }

        private string FilePath => Path.Combine(new[]
        {
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            StringResources.App_Data_Directory,
            StringResources.User_Interface_Layouts_Directory,
            "WorkspaceLayout.xml"
        });

        private void GetFilePath()
        {
            if (!File.Exists(FilePath))
            {
                FileInfo fileInfo = new FileInfo(FilePath);
                if (fileInfo.Directory != null)
                {
                    string finalDirectoryPath = fileInfo.Directory.FullName;

                    if (!Directory.Exists(finalDirectoryPath))
                    {
                        Directory.CreateDirectory(finalDirectoryPath);
                    }
                }
            }
        }

        #endregion Constructor

        void WinSourceInitialized(object sender, EventArgs e)
        {
            Maximise();
        }

        private void Maximise()
        {
            var handle = new WinInterop.WindowInteropHelper(this).Handle;
            var handleSource = WinInterop.HwndSource.FromHwnd(handle);
            handleSource?.AddHook(WindowProc);
        }

        private static IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case 0x0024:/* WM_GETMINMAXINFO */
                    if (!_isSuperMaximising)
                    {
                        WmGetMinMaxInfo(hwnd, lParam);
                        handled = true;
                    }
                    break;
            }
            return (IntPtr)0;
        }

        private static void WmGetMinMaxInfo(IntPtr hwnd, IntPtr lParam)
        {
            var mmi = (MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(MINMAXINFO));

            // Adjust the maximized size and position to fit the work area of the correct monitor
            var currentScreen = Screen.FromHandle(hwnd);
            var workArea = currentScreen.WorkingArea;
            var monitorArea = currentScreen.Bounds;
            mmi.ptMaxPosition.x = Math.Abs(workArea.Left - monitorArea.Left);
            mmi.ptMaxPosition.y = Math.Abs(workArea.Top - monitorArea.Top);
            mmi.ptMaxSize.x = Math.Abs(workArea.Right - workArea.Left);
            mmi.ptMaxSize.y = Math.Abs(workArea.Bottom - workArea.Top);

            Marshal.StructureToPtr(mmi, lParam, true);
        }

        [StructLayout(LayoutKind.Sequential)]
        // ReSharper disable InconsistentNaming
        public struct MINMAXINFO
        {
            public POINT ptReserved;
            public POINT ptMaxSize;
            public POINT ptMaxPosition;
            public POINT ptMinTrackSize;
            public POINT ptMaxTrackSize;
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            /// <summary>
            /// x coordinate of point.
            /// </summary>
            public int x;

            /// <summary>
            /// y coordinate of point.
            /// </summary>
            public int y;

            /// <summary>
            /// Construct a point of coordinates (x,y).
            /// </summary>
            public POINT(int x, int y)
            {
                this.x = x;
                this.y = y;
            }
        }

        private void Shell_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if ((Keyboard.Modifiers == (ModifierKeys.Alt | ModifierKeys.Control)) && (e.Key == Key.F4))
            {
                ResetToStartupView();
            }
            if (e.Key == Key.Home && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                var imageWindow = new ImageWindow();
                imageWindow.Show();
            }
            if (e.Key == Key.G && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {

            }
            if (e.Key == Key.I && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {

            }
            if (e.Key == Key.F1)
            {
                Process.Start(Warewolf.Studio.Resources.Languages.HelpText.WarewolfHelpURL);
            }
            if (e.Key == Key.F11 && _isLocked)
            {
                if (_isSuperMaximising)
                {
                    HideFullScreenPanel.IsHitTestVisible = false;
                    ShowFullScreenPanel.IsHitTestVisible = false;
                    ExitSuperMaximisedMode();
                }
                else
                {
                    HideFullScreenPanel.IsHitTestVisible = true;
                    ShowFullScreenPanel.IsHitTestVisible = true;
                    EnterSuperMaximisedMode();
                }
            }
        }

        public void ResetToStartupView()
        {
            var windowCollection = System.Windows.Application.Current.Windows;
            var mainViewModel = DataContext as MainViewModel;

            foreach (var window in windowCollection)
            {
                var window1 = window as Window;

                if (window1 != null && window1.Name != "MainViewWindow")
                {
                    if (window1.GetType().Name == "ToolWindowHostWindow")
                    {
                        var contentPane = window1.Content as PaneToolWindow;

                        var splitPane = contentPane?.Pane;

                        if (splitPane != null)
                            foreach (var item in splitPane.Panes)
                            {
                                var pane = item as ContentPane;

                                RemoveWorkspaceItems(pane, mainViewModel);
                            }
                    }
                    window1.Close();
                }
            }

            for (int i = TabManager.Items.Count - 1; i>= 0; i--)
            {
                var item = TabManager.Items[i];
                var contentPane = item as ContentPane;
                RemoveWorkspaceItems(contentPane, mainViewModel);
            }
            
            TabManager.Items.Clear();
            
            if (mainViewModel != null)
            {
                mainViewModel.ExplorerViewModel.SearchText = string.Empty;
                mainViewModel.ToolboxViewModel.SearchTerm = string.Empty;

                if(mainViewModel.LocalhostServer.IsConnected)
                {
                    if (mainViewModel.ActiveServer != mainViewModel.LocalhostServer) {
                        mainViewModel.SetActiveEnvironment(mainViewModel.LocalhostServer.EnvironmentID);
                        mainViewModel.SetActiveServer(mainViewModel.LocalhostServer);
                    }

                    foreach (var server in mainViewModel.ExplorerViewModel.ConnectControlViewModel.Servers)
                    {
                        if (server.DisplayName != mainViewModel.LocalhostServer.DisplayName && server.IsConnected)
                        {
                            server.Disconnect();
                        }
                    }

                    for (var i = 0;  i < mainViewModel.ExplorerViewModel.Environments.Count-1; i++)
                    {
                        var remoteEnvironment = mainViewModel.ExplorerViewModel.Environments.FirstOrDefault(model => model.ResourceId != Guid.Empty);
                        mainViewModel.ExplorerViewModel.Environments.Remove(remoteEnvironment);
                    }
                }
            }
        }

        private static void RemoveWorkspaceItems(ContentPane pane, MainViewModel mainViewModel)
        {
            var item1 = pane?.Content as WorkflowDesignerViewModel;
            if (item1?.ResourceModel != null)
                WorkspaceItemRepository.Instance.ClearWorkspaceItems(item1.ResourceModel);
            item1?.RemoveAllWorkflowName(item1.DisplayName);

            var workSurfaceContextViewModel = pane?.DataContext as WorkSurfaceContextViewModel;
            mainViewModel?.Items.Remove(workSurfaceContextViewModel);
        }

        void OnLoaded(object sender, RoutedEventArgs e)
         {
            var xmlDocument = new XmlDocument();
            if (_savedLayout != null)
            {
                xmlDocument.LoadXml(_savedLayout);
            }
            MainViewModel mainViewModel = DataContext as MainViewModel;
            if (mainViewModel != null)
            {
                SetMenuExpanded(xmlDocument, mainViewModel);
                SetMenuPanelOpen(xmlDocument, mainViewModel);
                SetMenuPanelLockedOpen(xmlDocument, mainViewModel);
            }
            Toolbox.Activate();
        }

        private static void SetMenuExpanded(XmlDocument xmlDocument, MainViewModel mainViewModel)
        {
            var elementsByTagNameMenuExpanded = xmlDocument.GetElementsByTagName("MenuExpanded");
            if(elementsByTagNameMenuExpanded.Count > 0)
            {
                var menuExpandedString = elementsByTagNameMenuExpanded[0].InnerXml;

                bool menuExpanded;
                if(bool.TryParse(menuExpandedString, out menuExpanded))
                {
                    mainViewModel.MenuExpanded = menuExpanded;
                }
            }
            else
                mainViewModel.MenuExpanded = true;
        }

        private static void SetMenuPanelOpen(XmlDocument xmlDocument, MainViewModel mainViewModel)
        {
            var elementsByTagNameMenuPanelOpen = xmlDocument.GetElementsByTagName("MenuPanelOpen");
            if (elementsByTagNameMenuPanelOpen.Count > 0)
            {
                var menuPanelOpenString = elementsByTagNameMenuPanelOpen[0].InnerXml;

                bool panelOpen;
                if (bool.TryParse(menuPanelOpenString, out panelOpen))
                {
                    mainViewModel.MenuViewModel.IsPanelOpen = panelOpen;
                }
            }
        }

        private static void SetMenuPanelLockedOpen(XmlDocument xmlDocument, MainViewModel mainViewModel)
        {
            var elementsByTagNameMenuPanelLockedOpen = xmlDocument.GetElementsByTagName("MenuPanelLockedOpen");
            if (elementsByTagNameMenuPanelLockedOpen.Count > 0)
            {
                var menuPanelLockedOpenString = elementsByTagNameMenuPanelLockedOpen[0].InnerXml;

                bool panelLockedOpen;
                if (bool.TryParse(menuPanelLockedOpenString, out panelLockedOpen))
                {
                    mainViewModel.MenuViewModel.IsPanelLockedOpen = panelLockedOpen;
                }
            }
            else
                mainViewModel.MenuViewModel.IsPanelLockedOpen = false;
        }

        #region Implementation of IWin32Window

        public IntPtr Handle
        {
            get
            {
                var interopHelper = new WinInterop.WindowInteropHelper(this);
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

            GetFilePath();
            SaveLayout(mainViewModel);
        }

        private void SaveLayout(MainViewModel mainViewModel)
        {
            var dockManagerLayout = DockManager.SaveLayout();
            XmlDocument document = new XmlDocument();
            document.LoadXml(dockManagerLayout);
            var menuExpandedNode = document.CreateNode(XmlNodeType.Element, "MenuExpanded", document.NamespaceURI);
            menuExpandedNode.InnerXml = (mainViewModel != null && mainViewModel.MenuExpanded).ToString();

            var menuPanelOpenNode = document.CreateNode(XmlNodeType.Element, "MenuPanelOpen", document.NamespaceURI);
            menuPanelOpenNode.InnerXml = (mainViewModel != null && mainViewModel.MenuViewModel.IsPanelOpen).ToString();

            var menuPanelLockedOpenNode = document.CreateNode(XmlNodeType.Element, "MenuPanelLockedOpen", document.NamespaceURI);
            menuPanelLockedOpenNode.InnerXml =
                (mainViewModel != null && mainViewModel.MenuViewModel.IsPanelLockedOpen).ToString();

            if (document.DocumentElement != null)
            {
                document.DocumentElement.AppendChild(menuExpandedNode);
                document.DocumentElement.AppendChild(menuPanelOpenNode);
                document.DocumentElement.AppendChild(menuPanelLockedOpenNode);
            }
            using (FileStream fs = new FileStream(FilePath, FileMode.Create, FileAccess.Write))
            {
                document.Save(fs);
            }
        }

        private void SlidingMenuPane_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            var vm = DataContext as MainViewModel;
            if (vm != null)
            {
                vm.MenuPanelWidth = e.NewSize.Width;
            }
        }

        private void DockManager_OnToolWindowLoaded(object sender, PaneToolWindowEventArgs e)
        {
            try
            {
                var window = e.Window;
                var resourceDictionary = System.Windows.Application.Current.Resources;
                var style = resourceDictionary["WarewolfToolWindow"] as Style;
                if (style != null)
                {
                    
                    window.UseOSNonClientArea = false;
                    window.Style = style;

                    window.PreviewMouseLeftButtonUp += WindowOnPreviewMouseDown;
                }

                if (e.Source.GetType() == typeof(XamDockManager))
                {
                    var binding = Infragistics.Windows.Utilities.CreateBindingObject(DataContextProperty, BindingMode.OneWay, sender as XamDockManager);
                    e.Window.SetBinding(DataContextProperty, binding);

                    MainViewModel mainViewModel = DataContext as MainViewModel;
                    PaneToolWindow = window;

                    if (PaneToolWindow.Pane.Panes != null && PaneToolWindow.Pane.Panes.Count > 0)
                    {
                        var workSurfaceContextViewModel = PaneToolWindow.Pane.Panes[0].DataContext as WorkSurfaceContextViewModel;
                        mainViewModel?.ActivateItem(workSurfaceContextViewModel);
                        PaneToolWindow.Name = "FloatingWindow";
                        if (string.IsNullOrWhiteSpace(e.Window.Title))
                        {
                            PaneToolWindow.Title = Title;
                        }
                        else
                        {
                            var dockManager = sender as XamDockManager;
                            if (dockManager?.DataContext.GetType() == typeof (WorkflowDesignerViewModel))
                            {
                                var workflowDesignerViewModel = dockManager?.DataContext as WorkflowDesignerViewModel;
                                var title = PaneToolWindow.Title;
                                var newTitle = " - " + workflowDesignerViewModel?.DisplayName.Replace("*", "").TrimEnd();
                                if (!title.Contains(newTitle))
                                {
                                    if (!string.IsNullOrWhiteSpace(workflowDesignerViewModel?.DisplayName))
                                    {
                                        PaneToolWindow.Title = PaneToolWindow.Title + " - " + workflowDesignerViewModel?.DisplayName;
                                    }
                                }
                            }
                            else if (dockManager?.DataContext.GetType() == typeof(StudioTestViewModel))
                            {
                                var studioTestViewModel = dockManager?.DataContext as StudioTestViewModel;
                                var title = PaneToolWindow.Title;
                                var newTitle = " - " + studioTestViewModel?.DisplayName.Replace("*", "").TrimEnd();
                                if (!title.Contains(newTitle))
                                {
                                    if (!string.IsNullOrWhiteSpace(studioTestViewModel?.DisplayName))
                                    {
                                        PaneToolWindow.Title = PaneToolWindow.Title + " - " + studioTestViewModel?.DisplayName;
                                    }
                                }
                            }
                            else if (dockManager?.DataContext.GetType() == typeof(SchedulerViewModel))
                            {
                                var schedulerViewModel = dockManager?.DataContext as SchedulerViewModel;
                                var title = PaneToolWindow.Title;
                                var newTitle = " - " + schedulerViewModel?.DisplayName.Replace("*", "").TrimEnd();
                                if (!title.Contains(newTitle))
                                {
                                    if (!string.IsNullOrWhiteSpace(schedulerViewModel?.DisplayName))
                                    {
                                        PaneToolWindow.Title = PaneToolWindow.Title + " - " + schedulerViewModel?.DisplayName;
                                    }
                                }
                            }
                        }
                        if (workSurfaceContextViewModel?.ContextualResourceModel != null)
                        {
                            PaneToolWindow.ToolTip = "Floating window for - " + workSurfaceContextViewModel?.ContextualResourceModel.DisplayName;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Dev2Logger.Error(ex);
            }
        }

        public PaneToolWindow PaneToolWindow { get; set; }

        private void WindowOnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                MainViewModel mainViewModel = DataContext as MainViewModel;
                if (mainViewModel != null)
                {
                    var paneToolWindow = sender as PaneToolWindow;
                    if (paneToolWindow?.Pane?.Panes.Count > 0)
                    {
                        var contentPane = paneToolWindow.Pane.Panes[0] as ContentPane;
                        if (contentPane != null)
                        {
                            var workSurfaceContextViewModel = contentPane.DataContext as WorkSurfaceContextViewModel;
                            mainViewModel.ActivateItem(workSurfaceContextViewModel);
                        }
                        else
                        {
                            var tabGroupPane = paneToolWindow.Pane.Panes[0] as TabGroupPane;
                            if (tabGroupPane?.Items.Count >= 1)
                            {
                                var selectedContent = tabGroupPane.SelectedContent as ContentPane;
                                var workSurfaceContextViewModel = selectedContent?.DataContext as WorkSurfaceContextViewModel;
                                mainViewModel.ActivateItem(workSurfaceContextViewModel);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Dev2Logger.Error(ex);
            }
        }

        private void EnterSuperMaximisedMode()
        {
            _isSuperMaximising = true;
            var dependencyObject = GetTemplateChild("PART_TITLEBAR");
            if (dependencyObject != null)
            {
                dependencyObject.SetValue(VisibilityProperty, Visibility.Collapsed);
                WindowState = WindowState.Normal;
                WindowState = WindowState.Maximized;
            }
        }

        private void CloseSuperMaximised(object sender, RoutedEventArgs e)
        {
            ExitSuperMaximisedMode();
        }

        private void ExitSuperMaximisedMode()
        {
            DoCloseExitFullScreenPanelAnimation();
            _isSuperMaximising = false;
            var dependencyObject = GetTemplateChild("PART_TITLEBAR");
            if (dependencyObject != null)
            {
                dependencyObject.SetValue(VisibilityProperty, Visibility.Visible);
                WindowState = WindowState.Normal;
                WindowState = WindowState.Maximized;
            }
        }

        private void DoCloseExitFullScreenPanelAnimation()
        {
            var storyboard = Resources["AnimateExitFullScreenPanelClose"] as Storyboard;
            storyboard?.Begin();
        }

        private void ShowFullScreenPanel_OnMouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (_isSuperMaximising)
            {
                var storyboard = Resources["AnimateExitFullScreenPanelOpen"] as Storyboard;
                storyboard?.Begin();
            }
            if (!_isLocked)
            {
                DoAnimateOpenTitleBar();
            }
        }

        private void DoAnimateOpenTitleBar()
        {
            var storyboard = Resources["AnimateOpenTitleBorder"] as Storyboard;
            if (storyboard != null)
            {
                var titleBar = GetTemplateChild("PART_TITLEBAR");
                storyboard.SetValue(Storyboard.TargetProperty, titleBar);
                storyboard.Begin();
            }
        }

        private void HideFullScreenPanel_OnMouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (_isSuperMaximising)
            {
                DoCloseExitFullScreenPanelAnimation();
            }
            if (!_isLocked)
            {
                DoAnimateCloseTitle();
            }
        }

        private void DoAnimateCloseTitle()
        {
            var storyboard = Resources["AnimateCloseTitleBorder"] as Storyboard;
            if (storyboard != null)
            {
                var titleBar = GetTemplateChild("PART_TITLEBAR");
                storyboard.SetValue(Storyboard.TargetProperty, titleBar);
                storyboard.Begin();
            }
        }

        bool restoreIfMove;
        bool allowMaximizeState;

        private void PART_TITLEBAR_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                SwitchState();
                ResizeMode = WindowState == WindowState.Normal ? ResizeMode.CanResize : ResizeMode.CanMinimize;
            }
            else
            {
                if (WindowState == WindowState.Maximized)
                {
                    restoreIfMove = true;
                }
                DragMove();
            }
        }

        private void SwitchState()
        {
            switch (WindowState)
            {
                case WindowState.Normal:
                    {
                        WindowState = WindowState.Maximized;
                        break;
                    }
                case WindowState.Maximized:
                    {
                        WindowState = WindowState.Normal;
                        break;
                    }
            }
        }

        private void PART_CLOSE_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void PART_MAXIMIZE_RESTORE_Click(object sender, RoutedEventArgs e)
        {
            ToggleWindowState();
        }

        private void ToggleWindowState()
        {
            WindowState = WindowState == WindowState.Normal ? WindowState.Maximized : WindowState.Normal;
            ResizeMode = WindowState == WindowState.Normal ? ResizeMode.CanResize : ResizeMode.CanMinimize;
        }

        private void PART_MINIMIZE_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void PART_SUPER_MAXIMIZE_RESTORE_Click(object sender, RoutedEventArgs e)
        {
            EnterSuperMaximisedMode();
        }

        private void PART_LOCK_Click(object sender, RoutedEventArgs e)
        {
            var dependencyObject = GetTemplateChild("PART_LOCK");
            if (dependencyObject != null)
            {
                var fontAwesome = new FontAwesome.WPF.FontAwesome();
                if (_isLocked)
                {
                    HideFullScreenPanel.IsHitTestVisible = true;
                    ShowFullScreenPanel.IsHitTestVisible = true;
                    fontAwesome.Icon = FontAwesomeIcon.Unlock;
                    DoAnimateCloseTitle();
                }
                else
                {
                    fontAwesome.Icon = FontAwesomeIcon.Lock;
                }
                dependencyObject.SetValue(ContentProperty, fontAwesome);
                _isLocked = !_isLocked;
            }
        }

        void PART_TITLEBAR_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            restoreIfMove = false;
            allowMaximizeState = true;
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetCursorPos(out POINT lpPoint);
        void PART_TITLEBAR_OnMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            try
            {
                if (restoreIfMove)
                {
                    restoreIfMove = false;

                    double percentHorizontal = e.GetPosition(this).X / ActualWidth;
                    double targetHorizontal = RestoreBounds.Width * percentHorizontal;

                    double percentVertical = e.GetPosition(this).Y / ActualHeight;
                    double targetVertical = RestoreBounds.Height * percentVertical;

                    WindowState = WindowState.Normal;
                    ResizeMode = WindowState == WindowState.Normal ? ResizeMode.CanResize : ResizeMode.CanMinimize;

                    POINT lMousePosition;
                    GetCursorPos(out lMousePosition);

                    Left = lMousePosition.x - targetHorizontal;
                    Top = lMousePosition.y - targetVertical;

                    DragMove();
                    allowMaximizeState = true;
                }
                if (allowMaximizeState)
                {
                    POINT lMousePosition;
                    GetCursorPos(out lMousePosition);

                    if (lMousePosition.y <= 0)
                    {
                        WindowState = WindowState.Maximized;
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private void ContentDockManager_OnPaneDragEnded(object sender, PaneDragEndedEventArgs e)
        {
            if (e.Panes != null)
            {
                var tabGroupPane = e.Panes[0].Parent as TabGroupPane;
                var splitPane = tabGroupPane?.Parent as SplitPane;
                var paneToolWindow = splitPane?.Parent as PaneToolWindow;
                if (paneToolWindow != null)
                {
                    if (string.IsNullOrWhiteSpace(paneToolWindow.Title))
                    {
                        paneToolWindow.Title = Title;
                    }
                }
                
            }
        }
        
    }
}
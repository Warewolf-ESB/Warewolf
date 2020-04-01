#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Triggers.Scheduler;
using Dev2.Studio.ViewModels;
using FontAwesome.WPF;
using Infragistics.Windows.DockManager.Events;
using WinInterop = System.Windows.Interop;
using Dev2.Studio.Core;
using Dev2.Studio.ViewModels.Workflow;
using Dev2.Studio.ViewModels.WorkSurface;
using Dev2.ViewModels;
using Dev2.Workspaces;
using Infragistics.Windows.DockManager;
using Dev2.Triggers.Scheduler;

namespace Dev2.Studio.Views
{
    public partial class ShellView : IWin32Window
    {
        static bool _isSuperMaximising;
        bool _isLocked;
        readonly string _savedLayout;
        static ShellView _this;

        #region Constructor

        public static ShellView GetInstance() => _this;

        public ShellView()
        {
            //InitializeComponent();
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
                        Dev2Logger.Error("Unable to load layout", GlobalConstants.WarewolfError);
                        Dev2Logger.Error(err, GlobalConstants.WarewolfError);
                    }
                }
            }

#pragma warning disable S3010 // For testing (Studio reset shortcut)
            _this = this;
#pragma warning restore S3010
        }

        string FilePath => Path.Combine(new[]
        {
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            StringResources.App_Data_Directory,
            StringResources.User_Interface_Layouts_Directory,
            "WorkspaceLayout.xml"
        });

        void GetFilePath()
        {
            if (!File.Exists(FilePath))
            {
                var fileInfo = new FileInfo(FilePath);
                if (fileInfo.Directory != null)
                {
                    var finalDirectoryPath = fileInfo.Directory.FullName;

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

        void Maximise()
        {
            var handle = new WinInterop.WindowInteropHelper(this).Handle;
            var handleSource = WinInterop.HwndSource.FromHwnd(handle);
            handleSource?.AddHook(WindowProc);
        }

        static IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == 0x0024 && !_isSuperMaximising)
            {
                WmGetMinMaxInfo(hwnd, lParam);
                handled = true;
            }
            return (IntPtr)0;
        }

        static void WmGetMinMaxInfo(IntPtr hwnd, IntPtr lParam)
        {
            var mmi = (Minmaxinfo)Marshal.PtrToStructure(lParam, typeof(Minmaxinfo));

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

        public struct Minmaxinfo
        {
            public Point ptReserved;
            public Point ptMaxSize;
            public Point ptMaxPosition;
            public Point ptMinTrackSize;
            public Point ptMaxTrackSize;
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct Point
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
            public Point(int x, int y)
            {
                this.x = x;
                this.y = y;
            }
        }

        void Shell_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if ((Keyboard.Modifiers == (ModifierKeys.Alt | ModifierKeys.Control)) && (e.Key == Key.F4))
            {
                ResetToStartupView();
            }
            if (e.Key == Key.Home && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                var shellViewModel = DataContext as ShellViewModel;
                shellViewModel?.MergeCommand.Execute(null);
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
            if (DataContext is ShellViewModel mainViewModel)
            {
                ClearWindowCollection(mainViewModel);
                ClearTabItems(mainViewModel);

                var localhostServer = mainViewModel.LocalhostServer;
                if (localhostServer.IsConnected && !Equals(mainViewModel.ActiveServer, localhostServer))
                {
                    mainViewModel.SetActiveServer(localhostServer.EnvironmentID);
                    mainViewModel.SetActiveServer(localhostServer);
                }

                var explorerViewModel = mainViewModel.ExplorerViewModel;
                if (explorerViewModel != null)
                {
                    DisconnectAllServers(localhostServer, explorerViewModel);
                }

                if (mainViewModel.ToolboxViewModel != null)
                {
                    mainViewModel.ToolboxViewModel.SearchTerm = string.Empty;
                    Toolbox.Activate();
                    Toolboxcontrol.Focus();
                }
            }
        }

        static void DisconnectAllServers(Interfaces.IServer localhostServer, Interfaces.IExplorerViewModel explorerViewModel)
        {
            explorerViewModel.SearchText = string.Empty;

            DisconnectServers(localhostServer, explorerViewModel);

            var environmentViewModels = explorerViewModel.Environments;
            if (environmentViewModels?.Count > 1)
            {
                for (var i = 0; i < environmentViewModels.Count - 1; i++)
                {
                    var remoteEnvironment = environmentViewModels.FirstOrDefault(model => model.ResourceId != Guid.Empty);
                    environmentViewModels.Remove(remoteEnvironment);
                }
            }
        }

        static void DisconnectServers(Interfaces.IServer localhostServer, Interfaces.IExplorerViewModel explorerViewModel)
        {
            if (explorerViewModel.ConnectControlViewModel != null)
            {
                foreach (var server in explorerViewModel.ConnectControlViewModel.Servers)
                {
                    if (server != null && server.DisplayName != localhostServer.DisplayName && server.IsConnected)
                    {
                        server.Disconnect();
                    }
                }
            }
        }

        void ClearTabItems(ShellViewModel mainViewModel)
        {
            for (int i = TabManager.Items.Count - 1; i >= 0; i--)
            {
                var item = TabManager.Items[i];
                var contentPane = item as ContentPane;
                RemoveWorkspaceItems(contentPane, mainViewModel);
            }
            TabManager.Items.Clear();
        }

        static void ClearWindowCollection(ShellViewModel mainViewModel)
        {
            var windowCollection = System.Windows.Application.Current.Windows;
            foreach (var window in windowCollection)
            {
                if (window is Window window1 && window1.Name != "MainViewWindow")
                {
                    if (window1.GetType().Name == "ToolWindowHostWindow")
                    {
                        ClearWindowCollection(mainViewModel, window1);
                    }
                    window1.Close();
                }
            }
        }

        static void ClearWindowCollection(ShellViewModel mainViewModel, Window window1)
        {
            var contentPane = window1.Content as PaneToolWindow;
            foreach (var item in contentPane?.Pane?.Panes)
            {
                var pane = item as ContentPane;
                RemoveWorkspaceItems(pane, mainViewModel);
            }
        }

        static void RemoveWorkspaceItems(ContentPane pane, ShellViewModel shellViewModel)
        {
            var item1 = pane?.Content as WorkflowDesignerViewModel;
            if (item1?.ResourceModel != null)
            {
                WorkspaceItemRepository.Instance.ClearWorkspaceItems(item1.ResourceModel);
            }
            item1?.RemoveAllWorkflowName(item1.DisplayName);

            var workSurfaceContextViewModel = pane?.DataContext as WorkSurfaceContextViewModel;
            shellViewModel?.Items.Remove(workSurfaceContextViewModel);
        }

        void OnLoaded(object sender, RoutedEventArgs e)
        {
            var xmlDocument = new XmlDocument();
            if (_savedLayout != null)
            {
                try
                {
                    xmlDocument.LoadXml(_savedLayout);
                }
                catch (Exception err)
                {
                    File.Delete(FilePath);
                    Dev2Logger.Error("Unable to load layout", GlobalConstants.WarewolfError);
                    Dev2Logger.Error(err, GlobalConstants.WarewolfError);
                }
            }
            if (DataContext is ShellViewModel shellViewModel)
            {
                SetMenuExpanded(xmlDocument, shellViewModel);
                SetMenuPanelOpen(xmlDocument, shellViewModel);
                SetMenuPanelLockedOpen(xmlDocument, shellViewModel);
            }
            Toolbox.Activate();
            Toolboxcontrol.Focus();
        }

        static void SetMenuExpanded(XmlDocument xmlDocument, ShellViewModel shellViewModel)
        {
            var elementsByTagNameMenuExpanded = xmlDocument.GetElementsByTagName("MenuExpanded");
            if (elementsByTagNameMenuExpanded.Count > 0)
            {
                var menuExpandedString = elementsByTagNameMenuExpanded[0].InnerXml;

                if (bool.TryParse(menuExpandedString, out bool menuExpanded))
                {
                    shellViewModel.MenuExpanded = menuExpanded;
                }
            }
            else
            {
                shellViewModel.MenuExpanded = true;
            }
        }

        static void SetMenuPanelOpen(XmlDocument xmlDocument, ShellViewModel shellViewModel)
        {
            var elementsByTagNameMenuPanelOpen = xmlDocument.GetElementsByTagName("MenuPanelOpen");
            if (elementsByTagNameMenuPanelOpen.Count > 0)
            {
                var menuPanelOpenString = elementsByTagNameMenuPanelOpen[0].InnerXml;

                if (bool.TryParse(menuPanelOpenString, out bool panelOpen))
                {
                    shellViewModel.MenuViewModel.IsPanelOpen = panelOpen;
                }
            }
        }

        static void SetMenuPanelLockedOpen(XmlDocument xmlDocument, ShellViewModel shellViewModel)
        {
            var elementsByTagNameMenuPanelLockedOpen = xmlDocument.GetElementsByTagName("MenuPanelLockedOpen");
            if (elementsByTagNameMenuPanelLockedOpen.Count > 0)
            {
                var menuPanelLockedOpenString = elementsByTagNameMenuPanelLockedOpen[0].InnerXml;

                if (bool.TryParse(menuPanelLockedOpenString, out bool panelLockedOpen))
                {
                    shellViewModel.MenuViewModel.IsPanelLockedOpen = panelLockedOpen;
                }
            }
            else
            {
                shellViewModel.MenuViewModel.IsPanelLockedOpen = false;
            }
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
            var shellViewModel = DataContext as ShellViewModel;
            if (shellViewModel != null)
            {
                if (!shellViewModel.OnStudioClosing())
                {
                    e.Cancel = true;
                }

                if (ShellViewModel.IsDownloading())
                {
                    e.Cancel = true;
                }
            }
            GetFilePath();
            SaveLayout(shellViewModel);
        }

        void SaveLayout(ShellViewModel shellViewModel)
        {
            var dockManagerLayout = DockManager.SaveLayout();
            var document = new XmlDocument();
            document.LoadXml(dockManagerLayout);
            var menuExpandedNode = document.CreateNode(XmlNodeType.Element, "MenuExpanded", document.NamespaceURI);
            menuExpandedNode.InnerXml = (shellViewModel != null && shellViewModel.MenuExpanded).ToString();

            var menuPanelOpenNode = document.CreateNode(XmlNodeType.Element, "MenuPanelOpen", document.NamespaceURI);
            menuPanelOpenNode.InnerXml = (shellViewModel != null && shellViewModel.MenuViewModel.IsPanelOpen).ToString();

            var menuPanelLockedOpenNode = document.CreateNode(XmlNodeType.Element, "MenuPanelLockedOpen", document.NamespaceURI);
            menuPanelLockedOpenNode.InnerXml =
                (shellViewModel != null && shellViewModel.MenuViewModel.IsPanelLockedOpen).ToString();

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

        void SlidingMenuPane_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (DataContext is ShellViewModel vm)
            {
                vm.MenuPanelWidth = e.NewSize.Width;
            }
        }

        void TryDockManager_OnToolWindowLoaded(object sender, PaneToolWindowEventArgs e)
        {
            try
            {
                DockManager_OnToolWindowLoaded(sender, e);
            }
            catch (Exception ex)
            {
                Dev2Logger.Error(ex, GlobalConstants.WarewolfError);
            }
        }

        void DockManager_OnToolWindowLoaded(object sender, PaneToolWindowEventArgs e)
        {
            var window = e.Window;
            var resourceDictionary = System.Windows.Application.Current.Resources;
            if (resourceDictionary["WarewolfToolWindow"] is Style style)
            {
                window.UseOSNonClientArea = false;
                window.Style = style;
                window.PreviewMouseLeftButtonUp += WindowOnPreviewMouseDown;
            }

            if (e.Source.GetType() == typeof(XamDockManager))
            {
                var binding = Infragistics.Windows.Utilities.CreateBindingObject(DataContextProperty, BindingMode.OneWay, sender as XamDockManager);
                e.Window.SetBinding(DataContextProperty, binding);

                var shellViewModel = DataContext as ShellViewModel;
                PaneToolWindow = window;

                if (PaneToolWindow.Pane.Panes != null && PaneToolWindow.Pane.Panes.Count > 0)
                {
                    var workSurfaceContextViewModel = PaneToolWindow.Pane.Panes[0].DataContext as WorkSurfaceContextViewModel;
                    shellViewModel?.ActivateItem(workSurfaceContextViewModel);
                    PaneToolWindow.Name = "FloatingWindow";
                    if (string.IsNullOrWhiteSpace(e.Window.Title))
                    {
                        PaneToolWindow.Title = Title;
                    }
                    else
                    {
                        UpdatePaneToolWindow(sender);
                    }
                    if (workSurfaceContextViewModel?.ContextualResourceModel != null)
                    {
                        PaneToolWindow.ToolTip = "Floating window for - " + workSurfaceContextViewModel.ContextualResourceModel.DisplayName;
                    }
                }
            }
        }

        void UpdatePaneToolWindow(object sender)
        {
            var dockManager = sender as XamDockManager;
            var displayName = string.Empty;
            if (dockManager?.DataContext.GetType() == typeof(WorkflowDesignerViewModel))
            {
                var workflowDesignerViewModel = dockManager.DataContext as WorkflowDesignerViewModel;
                displayName = workflowDesignerViewModel?.DisplayName;
            }
            else if (dockManager?.DataContext.GetType() == typeof(StudioTestViewModel))
            {
                var studioTestViewModel = dockManager.DataContext as StudioTestViewModel;
                displayName = studioTestViewModel?.DisplayName;
            }
            else
            {
                if (dockManager?.DataContext.GetType() == typeof(SchedulerViewModel))
                {
                    var schedulerViewModel = dockManager.DataContext as SchedulerViewModel;
                    displayName = schedulerViewModel?.DisplayName;
                }
            }
            SetPaneToolWindowTitle(displayName);
        }

        void SetPaneToolWindowTitle(string displayName)
        {
            var title = PaneToolWindow.Title;
            var newTitle = " - " + displayName?.Replace("*", "").TrimEnd();
            if (!title.Contains(newTitle) && !string.IsNullOrWhiteSpace(displayName))
            {
                PaneToolWindow.Title = PaneToolWindow.Title + " - " + displayName;
            }
        }

        public PaneToolWindow PaneToolWindow { get; set; }

        void WindowOnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (DataContext is ShellViewModel shellViewModel)
                {
                    WindowOnPreviewMouseDown(sender, shellViewModel);
                }
            }
            catch (Exception ex)
            {
                Dev2Logger.Error(ex, GlobalConstants.WarewolfError);
            }
        }

        static void WindowOnPreviewMouseDown(object sender, ShellViewModel shellViewModel)
        {
            var paneToolWindow = sender as PaneToolWindow;
            if (paneToolWindow?.Pane?.Panes.Count > 0)
            {
                if (paneToolWindow.Pane.Panes[0] is ContentPane contentPane)
                {
                    var workSurfaceContextViewModel = contentPane.DataContext as WorkSurfaceContextViewModel;
                    shellViewModel.ActivateItem(workSurfaceContextViewModel);
                }
                else
                {
                    var tabGroupPane = paneToolWindow.Pane.Panes[0] as TabGroupPane;
                    if (tabGroupPane?.Items.Count >= 1)
                    {
                        var selectedContent = tabGroupPane.SelectedContent as ContentPane;
                        var workSurfaceContextViewModel = selectedContent?.DataContext as WorkSurfaceContextViewModel;
                        shellViewModel.ActivateItem(workSurfaceContextViewModel);
                    }
                }
            }
        }

        void EnterSuperMaximisedMode()
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

        void CloseSuperMaximised(object sender, RoutedEventArgs e)
        {
            ExitSuperMaximisedMode();
        }

        void ExitSuperMaximisedMode()
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

        void DoCloseExitFullScreenPanelAnimation()
        {
            var storyboard = Resources["AnimateExitFullScreenPanelClose"] as Storyboard;
            storyboard?.Begin();
        }

        void ShowFullScreenPanel_OnMouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
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

        void DoAnimateOpenTitleBar()
        {
            if (Resources["AnimateOpenTitleBorder"] is Storyboard storyboard)
            {
                var titleBar = GetTemplateChild("PART_TITLEBAR");
                storyboard.SetValue(Storyboard.TargetProperty, titleBar);
                storyboard.Begin();
            }
        }

        void HideFullScreenPanel_OnMouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
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

        void DoAnimateCloseTitle()
        {
            if (Resources["AnimateCloseTitleBorder"] is Storyboard storyboard)
            {
                var titleBar = GetTemplateChild("PART_TITLEBAR");
                storyboard.SetValue(Storyboard.TargetProperty, titleBar);
                storyboard.Begin();
            }
        }

        bool restoreIfMove;
        bool allowMaximizeState;

        void PART_TITLEBAR_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
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

        void SwitchState()
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

                case WindowState.Minimized:
                    break;
                default:
                    WindowState = WindowState.Normal;
                    break;
            }
        }

        void PART_CLOSE_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        void PART_MAXIMIZE_RESTORE_Click(object sender, RoutedEventArgs e)
        {
            ToggleWindowState();
        }

        void ToggleWindowState()
        {
            WindowState = WindowState == WindowState.Normal ? WindowState.Maximized : WindowState.Normal;
            ResizeMode = WindowState == WindowState.Normal ? ResizeMode.CanResize : ResizeMode.CanMinimize;
        }

        void PART_MINIMIZE_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        void PART_SUPER_MAXIMIZE_RESTORE_Click(object sender, RoutedEventArgs e)
        {
            EnterSuperMaximisedMode();
        }

        void PART_LOCK_Click(object sender, RoutedEventArgs e)
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
        static extern bool GetCursorPos(out Point lpPoint);
        void PART_TITLEBAR_OnMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            try
            {
                if (restoreIfMove)
                {
                    restoreIfMove = false;

                    var percentHorizontal = e.GetPosition(this).X / ActualWidth;
                    var targetHorizontal = RestoreBounds.Width * percentHorizontal;

                    var percentVertical = e.GetPosition(this).Y / ActualHeight;
                    var targetVertical = RestoreBounds.Height * percentVertical;

                    WindowState = WindowState.Normal;
                    ResizeMode = WindowState == WindowState.Normal ? ResizeMode.CanResize : ResizeMode.CanMinimize;

                    GetCursorPos(out Point lMousePosition);

                    Left = lMousePosition.x - targetHorizontal;
                    Top = lMousePosition.y - targetVertical;

                    DragMove();
                    allowMaximizeState = true;
                }
                if (allowMaximizeState)
                {
                    GetCursorPos(out Point lMousePosition);

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

        void ContentDockManager_OnPaneDragEnded(object sender, PaneDragEndedEventArgs e)
        {
            if (e.Panes != null)
            {
                var tabGroupPane = e.Panes[0].Parent as TabGroupPane;
                var splitPane = tabGroupPane?.Parent as SplitPane;
                if (splitPane?.Parent is PaneToolWindow paneToolWindow && string.IsNullOrWhiteSpace(paneToolWindow.Title))
                {
                    paneToolWindow.Title = Title;
                }

            }
        }

        void MainViewWindow_Closed(object sender, EventArgs e)
        {
            foreach (Process proc in Process.GetProcessesByName("Warewolf Studio"))
            {
                Dev2Logger.Warn(proc.ProcessName + " still running in the background.", "Warewolf Warn");
            }
        }
    }
}
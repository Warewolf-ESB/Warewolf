using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Dev2.Common.Interfaces;
using FontAwesome.WPF;
using Infragistics.Windows;
using Infragistics.Windows.Controls;
using Infragistics.Windows.DockManager;
using Infragistics.Windows.DockManager.Events;
using Warewolf.Studio.ViewModels;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using WinInterop = System.Windows.Interop;

namespace Warewolf.Studio
{
    /// <summary>
    ///     Interaction logic for Shell.xaml
    /// </summary>
    public partial class Shell
    {
        private static bool _isSuperMaximising;
        private bool _isLocked;
        Point _pointInToolWindow;

        public Shell(IShellViewModel shellViewModel)
        {
        
            InitializeComponent();
            DataContext = shellViewModel;
            _isSuperMaximising = false;
            _isLocked = true;
            Loaded += OnLoaded;
            KeyDown += Shell_KeyDown;
            SourceInitialized += WinSourceInitialized;
        }

        void WinSourceInitialized(object sender, EventArgs e)
        {
            Maximise();
        }

        private void Maximise()
        {
            var handle = (new WinInterop.WindowInteropHelper(this)).Handle;
            var handleSource = WinInterop.HwndSource.FromHwnd(handle);
            if (handleSource == null)
                return;
            handleSource.AddHook(WindowProc);
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

        private void Shell_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.X && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                var testingWindow = new ControlStyleTestingWindow();
                testingWindow.Show();
            }
            if (e.Key == Key.G && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                var testingWindow = new Graph();
                testingWindow.Show();
            }
            if (e.Key == Key.F11)
            {
                if (_isSuperMaximising)
                {
                    ExitSuperMaximisedMode();
                }
                else
                {
                    EnterSuperMaximisedMode();
                }
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            LoadShellViewModel();
        }

        protected virtual void LoadShellViewModel()
        {
            var viewModel = DataContext as ShellViewModel;
            if (viewModel != null)
            {
                viewModel.Initialize();
            }
        }

        private void SlidingMenuPane_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            var vm = DataContext as ShellViewModel;
            if (vm != null)
            {
                vm.MenuPanelWidth = e.NewSize.Width;
            }
        }

        private void PART_TITLEBAR_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
            if (e.ClickCount == 2)
            {
                ToggleWindowState();
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
            if (WindowState == WindowState.Normal)
            {
                WindowState = WindowState.Maximized;
            }
            else
            {
                WindowState = WindowState.Normal;
            }
        }

        private void PART_MINIMIZE_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }



        /// <summary>
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public class MONITORINFO
        {
            /// <summary>
            /// </summary>
            public int cbSize = Marshal.SizeOf(typeof(MONITORINFO));

            /// <summary>
            /// </summary>
            public RECT rcMonitor = new RECT();

            /// <summary>
            /// </summary>
            public RECT rcWork = new RECT();

            /// <summary>
            /// </summary>
            public int dwFlags = 0;
        }

        /// <summary> Win32 </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 0)]
        public struct RECT
        {
            /// <summary> Win32 </summary>
            public int left;

            /// <summary> Win32 </summary>
            public int top;

            /// <summary> Win32 </summary>
            public int right;

            /// <summary> Win32 </summary>
            public int bottom;

            /// <summary> Win32 </summary>
            public static readonly RECT Empty;

            /// <summary> Win32 </summary>
            public int Width
            {
                get { return Math.Abs(right - left); } // Abs needed for BIDI OS
            }

            /// <summary> Win32 </summary>
            public int Height
            {
                get { return bottom - top; }
            }

            /// <summary> Win32 </summary>
            public RECT(int left, int top, int right, int bottom)
            {
                this.left = left;
                this.top = top;
                this.right = right;
                this.bottom = bottom;
            }


            /// <summary> Win32 </summary>
            public RECT(RECT rcSrc)
            {
                left = rcSrc.left;
                top = rcSrc.top;
                right = rcSrc.right;
                bottom = rcSrc.bottom;
            }

            /// <summary> Win32 </summary>
            public bool IsEmpty
            {
                get
                {
                    // BUGBUG : On Bidi OS (hebrew arabic) left > right
                    return left >= right || top >= bottom;
                }
            }

            /// <summary> Return a user friendly representation of this struct </summary>
            public override string ToString()
            {
                if (this == Empty)
                {
                    return "RECT {Empty}";
                }
                return "RECT { left : " + left + " / top : " + top + " / right : " + right + " / bottom : " + bottom +
                       " }";
            }

            /// <summary> Determine if 2 RECT are equal (deep compare) </summary>
            public override bool Equals(object obj)
            {
                if (!(obj is Rect))
                {
                    return false;
                }
                // ReSharper disable PossibleInvalidCastException
                return (this == (RECT)obj);
            }

            /// <summary>Return the HashCode for this struct (not garanteed to be unique)</summary>
            public override int GetHashCode()
            {
                // ReSharper disable NonReadonlyMemberInGetHashCode
                return left.GetHashCode() + top.GetHashCode() + right.GetHashCode() + bottom.GetHashCode();
            }


            /// <summary> Determine if 2 RECT are equal (deep compare)</summary>
            public static bool operator ==(RECT rect1, RECT rect2)
            {
                return (rect1.left == rect2.left && rect1.top == rect2.top && rect1.right == rect2.right &&
                        rect1.bottom == rect2.bottom);
            }

            /// <summary> Determine if 2 RECT are different(deep compare)</summary>
            public static bool operator !=(RECT rect1, RECT rect2)
            {
                return !(rect1 == rect2);
            }
        }


        private void PART_SUPER_MAXIMIZE_RESTORE_Click(object sender, RoutedEventArgs e)
        {
            EnterSuperMaximisedMode();
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
            if (storyboard != null)
            {
                storyboard.Begin();
            }
        }

        private void ShowFullScreenPanel_OnMouseEnter(object sender, MouseEventArgs e)
        {
            if (_isSuperMaximising)
            {
                var storyboard = Resources["AnimateExitFullScreenPanelOpen"] as Storyboard;
                if (storyboard != null)
                {
                    storyboard.Begin();
                }
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

        private void HideFullScreenPanel_OnMouseEnter(object sender, MouseEventArgs e)
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
        
        private void PART_LOCK_Click(object sender, RoutedEventArgs e)
        {
            var dependencyObject = GetTemplateChild("PART_LOCK");
            if (dependencyObject != null)
            {
                var fontAwesome = new FontAwesome.WPF.FontAwesome();
                if (_isLocked)
                {
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

        private void DockManager_OnToolWindowLoaded(object sender, PaneToolWindowEventArgs e)
        {
            var style = Resources["WarewolfToolWindow"] as Style;
            if (style != null)
            {
                var window = e.Window;
                window.UseOSNonClientArea = false;
                window.Style = style;
            }
        }
    }
}
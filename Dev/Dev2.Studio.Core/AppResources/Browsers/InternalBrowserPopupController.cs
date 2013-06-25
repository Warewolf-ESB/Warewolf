using System;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;

namespace Dev2.Studio.Core.AppResources.Browsers
{
    // BUG 9798 - 2013.06.25 - TWR : modified to handle internal
    public class InternalBrowserPopupController : BrowserPopupControllerAbstract
    {
        #region User32 Imports

        [DllImport("user32.dll")]
        static extern IntPtr FindWindow(string lclassName, string windowTitle);

        [DllImport("user32.dll")]
        static extern bool SetWindowText(IntPtr hWnd, String strNewWindowName);

        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern uint SetClassLong(IntPtr hWnd, int nIndex, uint dwNewLong);

        #endregion

        // ReSharper disable InconsistentNaming
        const int GCL_HICON = -14;
        // ReSharper restore InconsistentNaming

        #region CTOR

        public InternalBrowserPopupController()
        {
            if(Application.Current != null && Application.Current.MainWindow != null)
            {
                PopupTitle = Application.Current.MainWindow.Title;
            }
        }

        #endregion

        public string PopupTitle { get; private set; }

        public override bool ShowPopup(string url)
        {
            return false;
        }

        public override IntPtr FindPopup()
        {
            return FindWindow("CefBrowserWindow", null);
        }

        public override void SetPopupTitle(IntPtr hwnd)
        {
            SetWindowText(hwnd, PopupTitle);
        }

        public override void SetPopupForeground(IntPtr hwnd)
        {
            SetForegroundWindow(hwnd);
        }

        public override void SetPopupIcon(IntPtr hwnd)
        {
            var iconPath = Assembly.GetEntryAssembly().Location;
            var icon = Icon.ExtractAssociatedIcon(iconPath);
            if(icon != null)
            {
                SetClassLong(hwnd, GCL_HICON, (uint)icon.Handle);
                icon.Dispose();
            }
        }
    }
}
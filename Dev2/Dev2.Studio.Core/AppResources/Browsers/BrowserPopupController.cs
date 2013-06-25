using System;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;

namespace Dev2.Studio.Core.AppResources.Browsers
{
    public class BrowserPopupController : BrowserPopupControllerAbstract
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

        public BrowserPopupController()
            : base(Application.Current == null ? string.Empty : Application.Current.MainWindow.Title)
        {
        }

        #endregion

        protected override IntPtr FindPopup()
        {
            return FindWindow("CefBrowserWindow", null);
        }

        protected override void SetPopupTitle(IntPtr hwnd)
        {
            SetWindowText(hwnd, PopupTitle);
        }

        protected override void SetPopupForeground(IntPtr hwnd)
        {
            SetForegroundWindow(hwnd);
        }

        protected override void SetPopupIcon(IntPtr hwnd)
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
using System;
using System.Windows;
using System.Windows.Threading;

namespace Dev2.Studio.Core.AppResources.Browsers
{
    public class BrowserPopupController : IBrowserPopupController
    {
        #region CTOR

        // DO NOT use in tests!
        public BrowserPopupController()
        {
            // Null check so that we can test InvokeOnDispatcherThread setting
            MainWindow = Application.Current == null ? null : Application.Current.MainWindow;
            InvokeOnDispatcherThread = true;
        }

        // For testing only!!
        public BrowserPopupController(Window mainWindow, bool invokeOnDispatcherThread)
        {
            if(mainWindow == null)
            {
                throw new ArgumentNullException("mainWindow");
            }

            MainWindow = mainWindow;
            InvokeOnDispatcherThread = invokeOnDispatcherThread;
        }

        #endregion

        public bool InvokeOnDispatcherThread { get; private set; }
        public Window MainWindow { get; private set; }

        public void Show(string url, int width, int height)
        {
            if(string.IsNullOrEmpty(url))
            {
                return;
            }

            if(InvokeOnDispatcherThread)
            {
                Application.Current.Dispatcher.BeginInvoke(() => ShowImpl(url, width, height));
            }
            else
            {
                ShowImpl(url, width, height);
            }
        }

        protected virtual void ShowDialog(BrowserPopup popup)
        {
            popup.ShowDialog();
        }

        #region ShowImpl

        void ShowImpl(string url, int width, int height)
        {
            IBrowserPopupController controller = this;
            var popup = new BrowserPopup
            {
                Icon = MainWindow.Icon,
                Title = MainWindow.Title,

                // Check if window is visible otherwise testing gives this error: 
                // Cannot set Owner property to a Window that has not been shown previously
                Owner = MainWindow.IsVisible ? MainWindow : null,

                _webView =
                {
                    Address = url,
                    LifeSpanHandler = new BrowserLifeSpanHandler(controller)
                },
            };

            if(width > 0 && height > 0)
            {
                popup.Width = width;
                popup.Height = height;
            }
            ShowDialog(popup);
        }

        #endregion

    }
}
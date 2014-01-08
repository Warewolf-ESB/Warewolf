using System;
using System.ComponentModel;
using CefSharp;
using CefSharp.Wpf;
using Dev2.Studio.Core.Interfaces;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.AppResources.Browsers
{
    public static class Browser
    {
        // TODO: Change CallbackObjectName to Dev2Callback
        const string CallbackObjectName = "Dev2Awesomium";

        #region CallbackHandler - Singleton Instance

        //
        // Multi-threaded implementation - see http://msdn.microsoft.com/en-us/library/ff650316.aspx
        //
        // This approach ensures that only one instance is created and only when the instance is needed. 
        // Also, the variable is declared to be volatile to ensure that assignment to the instance variable
        // completes before the instance variable can be accessed. Lastly, this approach uses a sync root 
        // instance to lock on, rather than locking on the type itself, to avoid deadlocks.
        //
        static volatile WebPropertyEditorScriptableClass _theCallbackHandler;
        static readonly object TheCallbackHandlerSyncRoot = new Object();

        public static WebPropertyEditorScriptableClass CallbackHandler
        {
            get
            {
                if(_theCallbackHandler == null)
                {
                    lock(TheCallbackHandlerSyncRoot)
                    {
                        if(_theCallbackHandler == null)
                        {
                            _theCallbackHandler = new WebPropertyEditorScriptableClass();
                        }
                    }
                }
                return _theCallbackHandler;
            }
        }

        #endregion

        #region Startup/Shutdown

        public static void Startup()
        {
            var settings = new Settings();

            if(CEF.Initialize(settings))
            {
                CEF.RegisterJsObject(CallbackObjectName, CallbackHandler);
            }
        }

        public static void Shutdown()
        {
            CEF.Shutdown();
        }

        #endregion

        #region Initialize

        public static void Initialize(this WebView browser, string homeUrl = null, IPropertyEditorWizard propertyEditorViewModel = null)
        {
            //browser.ShowDevTools(); // Remove
            CallbackHandler.PropertyEditorViewModel = propertyEditorViewModel;
            browser.LoadSafe(homeUrl);
        }

        #endregion

        #region LoadSafe

        public static void LoadSafe(this WebView browser, string url)
        {
            if(string.IsNullOrEmpty(url))
            {
                return;
            }

            // PBI 9512 - 2013.06.07 - TWR: added
            // PBI 9644 - 2013.06.21 - TWR: added            
            if(browser.LoadHandler == null)
            {
                var browserHandler = new BrowserHandler();
                browser.LoadHandler = browserHandler;
                browser.LifeSpanHandler = browserHandler;
                browser.RequestHandler = browserHandler;
            }

            if(browser.IsBrowserInitialized)
            {
                browser.Load(url);
                return;
            }

            PropertyChangedEventHandler handler = null;
            handler = (s, e) =>
            {
                if(e.PropertyName == "IsBrowserInitialized" && browser.IsBrowserInitialized)
                {
                    browser.PropertyChanged -= handler;
                    browser.Load(url);
                }
            };

            browser.PropertyChanged += handler;
        }

        #endregion

        #region FormatUrl

        [Obsolete("use FormatUrl(string uriString, string args)")]
        public static string FormatUrl(string uriString, Guid dataListID)
        {
            if(dataListID == Guid.Empty)
            {
                return uriString;
            }
            uriString += (uriString.IndexOf('?') == -1 ? "?" : "&") + "dlid=" + dataListID;
            return uriString;
        }

        //09.03.2013: Ashley Lewis - Bug 9198 New FormatUrl method takes all args to avoid using UploadToDataList(Args)
        public static string FormatUrl(string uriString, string args)
        {
            if(string.IsNullOrEmpty(args))
            {
                return uriString;
            }
            uriString += (uriString.IndexOf('?') == -1 ? "?" : "&") + args;
            return uriString;
        }

        #endregion
    }
}

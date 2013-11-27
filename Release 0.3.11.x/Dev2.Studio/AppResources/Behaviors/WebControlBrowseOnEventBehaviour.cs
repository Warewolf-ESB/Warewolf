
using CefSharp.Wpf;
using Dev2.DataList.Contract;
using Dev2.Studio.Core.AppResources.Browsers;
using Dev2.Studio.Core.Interfaces;
using System;
using System.Reflection;
using System.Windows;
using System.Windows.Interactivity;

namespace Dev2.Studio.AppResources.Behaviors
{
    public class WebControlBrowseOnEventBehaviour : Behavior<WebView>, IDisposable
    {
        #region Class Members

        private Delegate eventHandlerDelegate;
        //private MethodInfo eventHandlerMethodInfo;

        #endregion Class Members

        #region Constructor

        public WebControlBrowseOnEventBehaviour()
        {
            eventHandlerDelegate = Delegate.CreateDelegate(typeof(EventHandler<BrowseRequestedEventArgs>), this, "EventHandler");
        }

        #endregion Constructor

        #region Override Methods

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.DataContextChanged -= AssociatedObject_DataContextChanged;
            AssociatedObject.DataContextChanged += AssociatedObject_DataContextChanged;
            AssociatedObject.Unloaded += AssociatedObjectOnUnloaded;
        }

        void AssociatedObjectOnUnloaded(object sender, RoutedEventArgs routedEventArgs)
        {
            AssociatedObject.Unloaded -= AssociatedObjectOnUnloaded;
            AssociatedObject.DataContextChanged -= AssociatedObject_DataContextChanged;
            routedEventArgs.Handled = true;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            AssociatedObject.Unloaded -= AssociatedObjectOnUnloaded;
            AssociatedObject.DataContextChanged -= AssociatedObject_DataContextChanged;
        }

        #endregion Override Methods

        #region Dependency Properties

        #region BrowseRequestedEventName

        public string BrowseRequestedEventName
        {
            get { return (string)GetValue(BrowseRequestedEventNameProperty); }
            set { SetValue(BrowseRequestedEventNameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BrowseRequestedEventName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BrowseRequestedEventNameProperty =
            DependencyProperty.Register("BrowseRequestedEventName", typeof(string), typeof(WebControlBrowseOnEventBehaviour), new PropertyMetadata(BrowseRequestedEventNameChanged));

        private static void BrowseRequestedEventNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            WebControlBrowseOnEventBehaviour webControlBrowseOnEventBehaviour = d as WebControlBrowseOnEventBehaviour;

            if(webControlBrowseOnEventBehaviour != null)
            {
                if(e.OldValue != null)
                {
                    webControlBrowseOnEventBehaviour.UnsubscribeFromEvent(e.OldValue.ToString(), webControlBrowseOnEventBehaviour.AssociatedObject.DataContext);
                }

                if(e.OldValue != null)
                {
                    webControlBrowseOnEventBehaviour.SubscribeToEvent(e.NewValue.ToString(), webControlBrowseOnEventBehaviour.AssociatedObject.DataContext);
                }
            }
        }

        #endregion BrowseRequested

        #endregion Dependency Properties

        #region Private Methods

        private void EventHandler(object sender, BrowseRequestedEventArgs e)
        {
            ErrorResultTO errors;
            AssociatedObject.Post(e.Url, e.EnvironmentModel, e.Payload, out errors);
        }

        private void AssociatedObject_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            UnsubscribeFromEvent(BrowseRequestedEventName, e.OldValue);
            SubscribeToEvent(BrowseRequestedEventName, e.NewValue);
        }

        private void SubscribeToEvent(string eventName, object datacontext)
        {
            if(datacontext == null || eventHandlerDelegate == null) return;

            EventInfo ei = datacontext.GetType().GetEvent(eventName);

            if(ei == null) return;

            ei.RemoveEventHandler(datacontext, eventHandlerDelegate);
            ei.AddEventHandler(datacontext, eventHandlerDelegate);
        }

        private void UnsubscribeFromEvent(string eventName, object datacontext)
        {
            if(datacontext == null || eventHandlerDelegate == null) return;

            EventInfo ei = datacontext.GetType().GetEvent(eventName);

            if(ei == null) return;

            ei.RemoveEventHandler(datacontext, eventHandlerDelegate);
        }

        #endregion Private Methods

        #region Methods

        public void Dispose()
        {
            AssociatedObject.Unloaded -= AssociatedObjectOnUnloaded;
            AssociatedObject.DataContextChanged -= AssociatedObject_DataContextChanged;
            UnsubscribeFromEvent(BrowseRequestedEventName, AssociatedObject.DataContext);
        }

        #endregion Methods
    }

    public class BrowseRequestedEventArgs : EventArgs
    {
        public BrowseRequestedEventArgs()
            : this(string.Empty)
        {
        }

        public BrowseRequestedEventArgs(string url)
            : this(url, string.Empty, null)
        {
        }

        public BrowseRequestedEventArgs(string url, string payload, IEnvironmentModel environmentModel)
        {
            Url = url;
            Payload = payload;
            EnvironmentModel = environmentModel;
        }

        public string Url { get; set; }
        public string Payload { get; set; }
        public IEnvironmentModel EnvironmentModel { get; set; }
    }
}

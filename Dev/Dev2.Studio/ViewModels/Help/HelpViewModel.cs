using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using Caliburn.Micro;
using Dev2.Common;
using Dev2.Providers.Logs;
using Dev2.Services.Events;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Helpers;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.ViewModels.WorkSurface;
using Dev2.Studio.Views.Help;
using Dev2.ViewModels.Help;
using Dev2.Webs.Callbacks;

// ReSharper disable CheckNamespace
namespace Dev2.Studio.ViewModels.Help
// ReSharper restore CheckNamespace
{
    public class HelpViewModel : BaseWorkSurfaceViewModel,
        IHandle<TabClosedMessage>
    {
        readonly INetworkHelper _network;

        public IHelpViewWrapper HelpViewWrapper { get; private set; }
        public string Uri { get; private set; }
        public string ResourcePath { get; private set; }
        public bool HelpViewDisposed { get; private set; }
        public bool IsViewAvailable { get; private set; }


        public HelpViewModel(INetworkHelper network, IHelpViewWrapper helpViewWrapper, bool isViewAvailable)
            : base(EventPublishers.Aggregator)
        {
            _network = network;
            HelpViewWrapper = helpViewWrapper;
            IsViewAvailable = isViewAvailable;
        }

        public HelpViewModel()
            : this(EventPublishers.Aggregator)
        {
        }

        public HelpViewModel(IEventAggregator eventPublisher)
            : base(eventPublisher)
        {
            _network = new NetworkHelper();
            IsViewAvailable = true;
        }

        public override WorkSurfaceContext WorkSurfaceContext
        {
            get { return WorkSurfaceContext.Help; }
        }
       
        protected override void OnViewLoaded(object view)
        {
            base.OnViewLoaded(view);    
            var helpView = view as HelpView;
            if(helpView == null)
            {
                return;
            }
            HelpViewWrapper = HelpViewWrapper ?? new HelpViewWrapper(helpView);
            OnViewisLoaded(HelpViewWrapper);
        }

        public void OnViewisLoaded(IHelpViewWrapper viewWrapper)
        {
            HelpViewWrapper = viewWrapper;
            if(!IsViewAvailable)
            {
                LoadBrowserUri(Uri);
            }
        }

        public void LoadBrowserUri(string uri)
        {
            Uri = uri;

            if(HelpViewWrapper == null)
            {
                IsViewAvailable = false;
            }
            else
            {
                IsViewAvailable = true;

                if(_network.HasConnection(uri))
                {
                    HelpViewWrapper.Navigate(Uri);
                    HelpViewWrapper.WebBrowser.Navigated += (sender, args) => SuppressJavaScriptsErrors(HelpViewWrapper.WebBrowser);
                    HelpViewWrapper.WebBrowser.LoadCompleted += (sender, args) =>
                    {
                        HelpViewWrapper.CircularProgressBarVisibility = Visibility.Collapsed;
                        HelpViewWrapper.WebBrowserVisibility = Visibility.Visible;
                    };
                }
                else
                {
                    ResourcePath = FileHelper.GetFullPath(StringResources.Uri_Studio_PageNotAvailable);
                    HelpViewWrapper.Navigate(ResourcePath);
                    HelpViewWrapper.CircularProgressBarVisibility = Visibility.Collapsed;
                    HelpViewWrapper.WebBrowserVisibility = Visibility.Visible;
                }
            }
        }

        public void SuppressJavaScriptsErrors(WebBrowser webBrowser)
        {
            FieldInfo fiComWebBrowser = typeof(WebBrowser).GetField("_axIWebBrowser2", BindingFlags.Instance | BindingFlags.NonPublic);
            if(fiComWebBrowser == null)
            {
                return; 
            }

            object objComWebBrowser = fiComWebBrowser.GetValue(webBrowser);
            if(objComWebBrowser == null)
            {
                return;
            }

            objComWebBrowser.GetType().InvokeMember("Silent", BindingFlags.SetProperty, null, objComWebBrowser, new object[] { true });
        }
        
        public void Handle(TabClosedMessage message)
        {   
            Dev2Logger.Log.Info(message.GetType().Name);
            if(!message.Context.Equals(this)) return;

            EventPublisher.Unsubscribe(this);
            HelpViewWrapper.WebBrowser.Dispose();
            HelpViewDisposed = true;
        }
    }
}

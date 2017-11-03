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
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using Dev2.Common;
using Dev2.Services.Events;
using Dev2.Studio.Core.Helpers;
using Dev2.Studio.ViewModels.WorkSurface;
using Dev2.Studio.Views.Help;
using Dev2.ViewModels.Help;
using Dev2.Studio.Core;
using Dev2.Studio.Interfaces.Enums;




namespace Dev2.Studio.ViewModels.Help

{
    public class HelpViewModel : BaseWorkSurfaceViewModel
    {
        public IHelpViewWrapper HelpViewWrapper { get; private set; }
        public string Uri { get; private set; }
        public string ResourcePath { get; private set; }
        public bool HelpViewDisposed { get; private set; }
        public bool IsViewAvailable { get; private set; }


        public HelpViewModel(IHelpViewWrapper helpViewWrapper, bool isViewAvailable)
            : base(EventPublishers.Aggregator)
        {
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
            IsViewAvailable = true;
        }

        public override WorkSurfaceContext WorkSurfaceContext => WorkSurfaceContext.Help;

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

        public async void OnViewisLoaded(IHelpViewWrapper viewWrapper)
        {
            HelpViewWrapper = viewWrapper;
            if(!IsViewAvailable)
            {
                try
                {
                    await LoadBrowserUri(Uri);
                }
                catch(Exception e)
                {
                    Dev2Logger.Error(e.Message,e, "Warewolf Error");
                }
            }
        }

        public Task LoadBrowserUri(string uri)
        {
            Uri = uri;

            if (HelpViewWrapper == null)
            {
                IsViewAvailable = false;
            }
            else
            {
                IsViewAvailable = true;
                SetupFailedNavigationEvent();
                SetupLoadCompletedEvent();
                SetupNavigationCompletdEvent();
                Execute.OnUIThread(() => { HelpViewWrapper.Navigate(Uri); });
            }
            return Task.FromResult(true);
        }

        void SetupLoadCompletedEvent()
        {
            HelpViewWrapper.WebBrowser.LoadCompleted += (sender, args) => Execute.OnUIThread(() =>
            {
                HelpViewWrapper.CircularProgressBarVisibility = Visibility.Collapsed;
                HelpViewWrapper.WebBrowserVisibility = Visibility.Visible;
            });
        }

        void SetupNavigationCompletdEvent()
        {
            HelpViewWrapper.WebBrowser.Navigated += (sender, args) =>
            {
                var navService = HelpViewWrapper.WebBrowser.NavigationService;
                dynamic browser = navService.GetType().GetField("_webBrowser", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(navService);
                dynamic iWebBrowser2 = browser.GetType().GetField("_axIWebBrowser2", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(browser);
                iWebBrowser2.Silent = true;               
            };
        }
        void SetupFailedNavigationEvent()
        {
            HelpViewWrapper.WebBrowser.NavigationService.NavigationFailed += (sender, args) =>
            {
                ResourcePath = FileHelper.GetFullPath(StringResources.Uri_Studio_PageNotAvailable);
                Execute.OnUIThread(() =>
                {
                    HelpViewWrapper.Navigate(ResourcePath);
                    HelpViewWrapper.CircularProgressBarVisibility = Visibility.Collapsed;
                    HelpViewWrapper.WebBrowserVisibility = Visibility.Visible;
                });
            };
        }

        #region Overrides of Screen

        /// <summary>Called when deactivating.</summary>
        /// <param name="close">Inidicates whether this instance will be closed.</param>
        protected override void OnDeactivate(bool close)
        {
            if (close)
            {
                EventPublisher.Unsubscribe(this);
                HelpViewDisposed = true;
            }
            base.OnDeactivate(close);
        }

        #endregion

        public string ResourceType => "StartPage";
    }
}

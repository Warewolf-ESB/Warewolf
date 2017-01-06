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
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Helpers;
using Dev2.Studio.ViewModels.WorkSurface;
using Dev2.Studio.Views.Help;
using Dev2.ViewModels.Help;
using Dev2.Webs.Callbacks;
using Dev2.Studio.Core;
// ReSharper disable UnusedAutoPropertyAccessor.Local

// ReSharper disable CheckNamespace
namespace Dev2.Studio.ViewModels.Help
// ReSharper restore CheckNamespace
{
    public class HelpViewModel : BaseWorkSurfaceViewModel
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
                    Dev2Logger.Error(e.Message,e);
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
                HelpViewWrapper.WebBrowser.LoadCompleted += (sender, args) => Execute.OnUIThread(() =>
                {
                    HelpViewWrapper.CircularProgressBarVisibility = Visibility.Collapsed;
                    HelpViewWrapper.WebBrowserVisibility = Visibility.Visible;
                });
                Execute.OnUIThread(() => { HelpViewWrapper.Navigate(Uri); });
            }
            return Task.FromResult(true);
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

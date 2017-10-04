using System;
using System.Collections.Concurrent;
using Dev2.Studio.Core;
using Microsoft.Practices.Prism.Mvvm;
using Warewolf.Studio.Views;

namespace Dev2
{
    internal class ViewFactory : IViewFactory
    {
        private readonly ConcurrentDictionary<string, Func<IView>> _viewMap =
            new ConcurrentDictionary<string, Func<IView>>();

        public ViewFactory()
        {
            _viewMap.TryAdd("Server", () => new ManageServerControl());
            _viewMap.TryAdd("Dev2Server", () => new ManageServerControl());
            _viewMap.TryAdd("ServerSource", () => new ManageServerControl());

            _viewMap.TryAdd("RabbitMQSource", () => new ManageRabbitMQSourceControl());
            _viewMap.TryAdd("OauthSource", () => new ManageOAuthSourceControl());
            _viewMap.TryAdd("SharepointServerSource", () => new SharepointServerSource());
            _viewMap.TryAdd("DropBoxSource", () => new ManageOAuthSourceControl());
            _viewMap.TryAdd("ExchangeSource", () => new ManageExchangeSourceControl());
            _viewMap.TryAdd("EmailSource", () => new ManageEmailSourceControl());
            _viewMap.TryAdd("WcfSource", () => new ManageWcfSourceControl());
            _viewMap.TryAdd("ComPluginSource", () => new ManageComPluginSourceControl());
            _viewMap.TryAdd("PluginSource", () => new ManagePluginSourceControl());
            _viewMap.TryAdd("WebSource", () => new ManageWebserviceSourceControl());
            _viewMap.TryAdd("MySqlDatabase", () => new ManageDatabaseSourceControl());
            _viewMap.TryAdd("PostgreSQL", () => new ManageDatabaseSourceControl());
            _viewMap.TryAdd("Oracle", () => new ManageDatabaseSourceControl());
            _viewMap.TryAdd("ODBC", () => new ManageDatabaseSourceControl());
            _viewMap.TryAdd("SqlDatabase", () => new ManageDatabaseSourceControl());
        }

        public IView GetViewGivenServerResourceType(string resourceModel)
        {
            if (_viewMap.TryGetValue(resourceModel, out Func<IView> funcView))
            {
                return funcView.Invoke();
            }
            return default(IView);
        }
    }
}
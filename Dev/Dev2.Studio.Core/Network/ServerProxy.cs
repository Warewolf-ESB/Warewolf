using System;
using System.Net;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.Communication;
using Dev2.ExtMethods;
using Dev2.Providers.Logs;
using Microsoft.AspNet.SignalR.Client;

namespace Dev2.Network
{
    public class ServerProxy
    {
        HubConnection _hubConnection;
        IHubProxy _resourcesProxy;

        public void Connect()
        {
            _hubConnection = new HubConnection("http://localhost:8080")
            {
                Credentials = CredentialCache.DefaultCredentials,
                TraceLevel = TraceLevels.All,
                TraceWriter = Console.Out
            };
            _hubConnection.Error += OnHubConnectionError;

            _resourcesProxy = _hubConnection.CreateHubProxy("resources");
            //_esbProxy = hubConnection.CreateHubProxy("esb");
            _resourcesProxy.On<DesignValidationMemo>("SendMemo", OnMemoReceived);

            _hubConnection.Start().Wait();
        }

        void OnHubConnectionError(Exception exception)
        {
            this.LogError(exception.Message);
        }

        void OnMemoReceived(DesignValidationMemo obj)
        {

            // DO NOT use publish as memo is of type object 
            // and hence won't find the correct subscriptions
            this.TraceInfo("Publish message of type - " + typeof(Memo));
            //_serverEventPublisher.PublishObject(memo);
        }

        public string ExecuteCommand(string payload, Guid workspaceID, Guid dataListID)
        {
            
            var resourceXml = XElement.Parse(payload).ElementSafe("ResourceXml");

            var t = _resourcesProxy.Invoke<string>("Save", resourceXml, workspaceID, dataListID);

            t.WaitWithPumping(GlobalConstants.NetworkTimeOut);
            return t.Result;
        }
    }
}

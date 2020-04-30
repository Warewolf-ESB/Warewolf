/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Dev2.Common;
using Dev2.SignalR.Wrappers;
using Dev2.SignalR.Wrappers.New;
using Warewolf.Client;
using Warewolf.Data;
using Warewolf.Esb;
using Warewolf.Service;

namespace Warewolf.ClientConsole
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            var result = CommandLine.Parser.Default.ParseArguments<Args>(args);
            return result.MapResult(
                options => new Implementation(options).Run(),
                _ => 1);
        }
    }
    internal class Implementation
    {
        private readonly Args _options;
        private readonly ManualResetEvent _canExit = new ManualResetEvent(false);
        
        public Implementation(Args options)
        {
            this._options = options;
        } 
        public int Run()
        {
            try
            {
                var context = new Context(_options);
                var esbProxy = context.EsbProxy;
                RegisterForEventsOnServerConnection(esbProxy, _canExit);
                var connectedTask = context.EnsureConnected();
                connectedTask.Wait();

                esbProxy.Connection.Closed += () => { Console.WriteLine("connection closed"); _canExit.Set(); };

                var joinResponse = RequestRegistrationForChangeNotifications(esbProxy);
                joinResponse.Wait();
                var response = joinResponse.Result;

                _canExit.WaitOne(-1);


                // wait and watch the changes


                Console.WriteLine("press enter to exit");
                Console.ReadLine();
            }
            catch (Exception e)
            {
                WriteExceptionToConsole(e);
            }

            return 0;
        }

        private static void WriteExceptionToConsole(Exception t1Exception)
        {
            if (t1Exception is null)
            {
                return;
            }
            WriteExceptionToConsole(t1Exception.InnerException);
            Console.WriteLine(t1Exception.Message);
        }


        private static Task<ClusterJoinResponse> RequestRegistrationForChangeNotifications(IConnectedHubProxyWrapper esbProxy)
        {
            var joinRequest = new ClusterJoinRequest(Config.Cluster.Key);
            //return esbProxy.ExecReq3<ClusterJoinResponse>(joinRequest, 3);
            return joinRequest.Execute(esbProxy, 3);
        }
        private static void RegisterForEventsOnServerConnection(IConnectedHubProxyWrapper esbProxy, ManualResetEvent canExit)
        {
            var t = esbProxy.Watch<ChangeNotification>();
            t.Received += changeNotification =>
            {
                // expect an InitialChangeNotification to confirm that we are now following the leader
                // should timeout

                Console.WriteLine("Change notification received");
                // TODO: trigger a git sync in c:\ProgramData\Warewolf if there is an active git source
                // or?
                // alternatively we could let this program exit and then use the process monitor in warewolf server
                // start the sync, that way we could use a singleton inside the server to ensure no other processes try to
                // do the same thing. This would need some way to know that there was in fact a change notification
                canExit.Set();
            };


            var list = new ObservableDistributedListClient<ServerFollower>(esbProxy.Connection, DistributedLists.ClusterFollowers);
            list.CollectionChanged += (sender, args) =>
            {
                Console.WriteLine("current items");
                foreach (var item in list)
                {
                    Console.WriteLine("\t- "+ item);
                }
            };
        }
    }

    internal class Context
    {
        private IConnectedHubProxyWrapper _hubProxy;
        private readonly HubConnectionWrapper _hubConnection;

        public Context(IArgs args)
        {
            ServicePointManager.ServerCertificateValidationCallback = ValidateServerCertificate;

            _hubConnection = new HubConnectionWrapper(args.ServerEndpoint.ToString()) { Credentials = System.Net.CredentialCache.DefaultNetworkCredentials};
        }
        private static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslpolicyerrors) => true;

        public IConnectedHubProxyWrapper EsbProxy => _hubProxy ?? (_hubProxy = new ConnectedHubProxy { Connection = _hubConnection, Proxy = _hubConnection.CreateHubProxy("esb") });

        public ICatalogRequest NewResourceRequest<T>(Guid serverWorkspaceId, Guid resourceId) where T : class, new()
        {
            if (_hubConnection.State == ConnectionStateWrapped.Disconnected)
            {
                _hubConnection.Start();
            }
            return new ResourceRequest<T>(serverWorkspaceId, resourceId);
        }

        public ICatalogSubscribeRequest NewWatcher<T>(Guid serverWorkspaceId)
        {
            return new EventRequest<T>(serverWorkspaceId);
        }

        public Task EnsureConnected()
        {
            return _hubConnection.EnsureConnected(-1);
        }
    }
}

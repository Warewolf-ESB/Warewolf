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
using CommandLine;
using Dev2.Common;
using Dev2.Common.Interfaces.Data;
using Dev2.SignalR.Wrappers;
using Dev2.SignalR.Wrappers.New;
using Warewolf.Client;
using Warewolf.Esb;

namespace Warewolf.ClientConsole
{
    class Program
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

        public Implementation(Args options)
        {
            this._options = options;
        } 
        public int Run()
        {
            try
            {
                var context = new Context(_options);
                var esb = context.EsbProxy;

                var req = context.NewResourceRequest<IResource>(GlobalConstants.ServerWorkspaceID, Guid.Empty);
                var t = esb.ExecReq3<IResource>(req, 3);
                t.ContinueWith(t1 =>
                {
                    if (t1.IsFaulted)
                    {
                        Console.WriteLine("Error:");
                        WriteExceptionToConsole(t1.Exception);
                    }
                    else
                    {
                        Console.WriteLine("woot");
                    }
                });
                t.Wait();
            }
            catch (Exception e)
            {
                
            }

            return 0;
        }

        private void WriteExceptionToConsole(Exception t1Exception)
        {
            if (t1Exception is null)
            {
                return;
            }
            WriteExceptionToConsole(t1Exception.InnerException);
            Console.WriteLine(t1Exception.Message);
        }
    }

    internal class Context
    {
        private IConnectedHubProxy _hubProxy;
        private HubConnectionWrapper _hubConnection;

        public Context(Args args)
        {
            _hubConnection = new HubConnectionWrapper(args.ServerEndpoint.ToString()) { Credentials = System.Net.CredentialCache.DefaultNetworkCredentials};
        }

        public IConnectedHubProxy EsbProxy
        {
            get => _hubProxy ?? (_hubProxy = new ConnectedHubProxy { Connection = _hubConnection, Proxy = _hubConnection.CreateHubProxy("esb") });
            set => _hubProxy = value;
        }

        public ICatalogRequest NewResourceRequest<T>(Guid serverWorkspaceId, Guid resourceId)
        {
            if (_hubConnection.State == ConnectionStateWrapped.Disconnected)
            {
                _hubConnection.Start();
            }
            return new ResourceRequest<T>(serverWorkspaceId, resourceId);
        }
    }
}

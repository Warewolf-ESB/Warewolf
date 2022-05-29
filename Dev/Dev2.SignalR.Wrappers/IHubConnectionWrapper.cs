/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/
using System;
using System.Net;
using System.Threading.Tasks;

namespace Dev2.SignalR.Wrappers
{
    public enum ConnState
    {
        Disconnected = 0,
        Connected = 1,
    }

    public interface IStateController
    {
        ConnState Current { get; }
        Task<bool> MoveToState(ConnState state, TimeSpan timeout);
        Task<bool> MoveToState(ConnState state, int milliSecondsTimeout);

    }
    public interface IHubConnectionWrapper 
    {        
        IHubProxyWrapper CreateHubProxy(string hubName);
#if NETFRAMEWORK
        event Action<Exception> Error;
        event Action Closed;
        ICredentials Credentials { get;  }
        event Action<IStateChangeWrapped> StateChanged;
#else
        event Func<Exception, Task> Closed;
#endif
        ConnectionStateWrapped State { get;  }
        IStateController StateController { get; }
        Task Start();
        void Stop(TimeSpan timeSpan);
        Task EnsureConnected(TimeSpan timeout);
    }

    public interface IStateChangeWrapped
    {
         ConnectionStateWrapped OldState { get;  }        
         ConnectionStateWrapped NewState { get; }
    }
}
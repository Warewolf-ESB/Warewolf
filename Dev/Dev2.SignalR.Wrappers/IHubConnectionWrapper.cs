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
        event Action<Exception> Error;
		event Action<Exception> LegacyError;
		event Func<Exception, Task> Closed;
		event Action LegacyClosed;
		event Action<IStateChangeWrapped> StateChanged;
		event Action<IStateChangeWrapped> LegacyStateChanged;
		ConnectionStateWrapped State { get; }
		ConnectionStateWrapped LegacyState { get; }
		ICredentials Credentials { get; }
		ICredentials LegacyCredentials { get; }
		IStateController StateController { get; }
		bool UsingLegacy { get; }

		Task Start();
		Task LegacyStart();
		void Stop(TimeSpan timeSpan);
		void LegacyStop(TimeSpan timeSpan);
		Task EnsureConnected(TimeSpan timeout);
		Task LegacyEnsureConnected(TimeSpan timeout);
	}

    public interface IStateChangeWrapped
    {
         ConnectionStateWrapped OldState { get;  }        
         ConnectionStateWrapped NewState { get; }
    }
}
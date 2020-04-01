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
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNet.SignalR.Client.Transports;
using Warewolf;

namespace Dev2.SignalR.Wrappers.New
{
    public class ConnectedHubProxy : IConnectedHubProxy
    {
        public IHubConnectionWrapper Connection { get; set; }
        public IHubProxyWrapper Proxy { get; set; }
    }
    
    public class HubConnectionWrapper : IHubConnectionWrapper, IDisposable
    {
        readonly HubConnection _wrapped;
        private readonly ManualResetEvent _connectNotify = new ManualResetEvent(false);
        private readonly IStateController _stateController;

        public IStateController StateController => _stateController;

        #region Implementation of IHubConnectionWrapper

        HubConnectionWrapper(HubConnection wrapped)
        {
            _wrapped = wrapped;
            _wrapped.DeadlockErrorTimeout = TimeSpan.FromSeconds(30);
            _wrapped.StateChanged += change =>
            {
                switch (change.NewState)
                {
                    case ConnectionState.Connected:
                        _connectNotify.Set();
                        break;
                    case ConnectionState.Connecting:
                    case ConnectionState.Reconnecting:
                    case ConnectionState.Disconnected:
                        _connectNotify.Reset();
                        break;
                    default:
                        throw new Exception("unknown ConnectionState");
                }
            };

            _stateController = new StateController(this);
        }

        public HubConnectionWrapper(string uriString)
            : this(new HubConnection(uriString))
        {
        }

        public Task EnsureConnected(TimeSpan timeout)
        {
            return EnsureConnected((int)Math.Floor(timeout.TotalMilliseconds));
        }

        public Task EnsureConnected(int milliSecondsTimeout)
        {
            _stateController.MoveToState(ConnState.Connected, milliSecondsTimeout);

            /// when this function returns we should have waited for a connect or reconnect to have completed
            var startTask = Task.Factory.StartNew(() =>
            {
                if (State != ConnectionStateWrapped.Connected)
                {
                    _connectNotify.WaitOne(milliSecondsTimeout);
                }
            });

            var timerTask = Task.Delay(milliSecondsTimeout);
            return Task.WhenAny(startTask, timerTask).ContinueWith((task) =>
            {
                if (task == timerTask || State != ConnectionStateWrapped.Connected)
                {
                    throw new Exception("unexpected timeout while connecting to warewolf server");
                }

                return task.Result;
            });
        }


        public IHubProxyWrapper CreateHubProxy(string hubName) => new HubProxyWrapper(_wrapped.CreateHubProxy(hubName));

        public event Action<Exception> Error
        {
            add
            {
                _wrapped.Error += value;
            }
            remove
            {
                _wrapped.Error -= value;
            }
        }

        public event Action Closed
        {
            add
            {
                _wrapped.Closed += value;
            }
            remove
            {
                _wrapped.Closed -= value;
            }
        }

        public event Action<IStateChangeWrapped> StateChanged
        {
            add
            {
                _wrapped.StateChanged += change => value?.Invoke(new StateChangeWrapped(change));
            }
            remove
            {
                throw new NotImplementedException();
            }
        }

        public ConnectionStateWrapped State => (ConnectionStateWrapped)_wrapped.State;

        public Task Start()
        {
            var serverSentEventsTransport = new ServerSentEventsTransport();
            return _wrapped.Start(serverSentEventsTransport);
        }

        public void Stop(TimeSpan timeSpan)
        {
            _wrapped.Stop(timeSpan);
        }

        public ICredentials Credentials
        {
            get
            {
                return _wrapped.Credentials;
            }

            set
            {
                _wrapped.Credentials=value;
            }
        }

        #endregion

        public void Dispose()
        {
            _connectNotify.Dispose();
        }
    }

    public class StateController : BindableBase, IStateController
    {

        public StateController(IHubConnectionWrapper hubConnection)
        {
            _watcher = new Watcher(hubConnection);
            PropertyChanged += (sender, e) => _watcher.NotifyStateChanged(Current, ExpectedState);
            hubConnection.StateChanged += (state) => {
                switch (state.NewState)
                {
                    case ConnectionStateWrapped.Connected:
                        Current = ConnState.Connected;
                        break;
                    case ConnectionStateWrapped.Disconnected:
                        Current = ConnState.Disconnected;
                        break;
                    case ConnectionStateWrapped.Connecting:
                        break;
                    case ConnectionStateWrapped.Reconnecting:
                        break;
                    default:
                        throw new Exception("unknown ConnectionStateWrapped value");
                }
            };
        }

        readonly object _currentStateLock = new object();
        ConnState _current;
        public ConnState Current {
            get { lock (_currentStateLock) { return _current; } }
            private set
            {
                lock (_currentStateLock)
                {
                    SetProperty(ref _current, value);
                }
            }
        }
        readonly ReaderWriterLock _expectedStateLock = new ReaderWriterLock();
        ConnState _expectedState;
        public ConnState ExpectedState {
            get { try { _expectedStateLock.AcquireReaderLock(-1); return _expectedState; } finally { _expectedStateLock.ReleaseReaderLock(); } }
            private set
            {
                try
                {
                    _expectedStateLock.AcquireWriterLock(-1);
                    SetProperty(ref _expectedState, value);
                } finally
                {
                    _expectedStateLock.ReleaseWriterLock();
                }
            }
        }

        Watcher _watcher;

        public Task<bool> MoveToState(ConnState state, TimeSpan timeout)
        {
            return MoveToState(state, (int)Math.Floor(timeout.TotalMilliseconds));
        }
        public async Task<bool> MoveToState(ConnState state, int milliSecondsTimeout)
        {
            ExpectedState = state;
            //if (state == State.Connected)
            //{
            //    _watcher.Start();
            //} else if (state == State.Disconnected)
            //{
            //    _watcher.Suspend();
            //}
            var t = new Task(() =>
            {
                while (Current != ExpectedState)
                {
                    Task.Yield();
                    Task.Delay(TimeSpan.FromSeconds(1)).Wait();
                }
            });
            t.Start();
            var delayTask = Task.Delay(milliSecondsTimeout);
            var result = await Task.WhenAny(delayTask, t);
            if (result == delayTask)
            {
                throw new Exception("timeout");
            }
            if (Current != ExpectedState)
            {
                return false;
            }
            return true;
        }

        class Watcher
        {
            Thread _threadWorker;
            IHubConnectionWrapper HubConnection;
            public Watcher(IHubConnectionWrapper hubConnection)
            {
                HubConnection = hubConnection;
                InitializeThreadWorker();
            }
            private void InitializeThreadWorker()
            {
                _threadWorker = new Thread(KeepConnected);
                _threadWorker.IsBackground = true;
            }

            internal void NotifyStateChanged(ConnState current, ConnState expectedState)
            {
                if (current != expectedState)
                {
                    if (expectedState == ConnState.Disconnected)
                    {
                        Suspend();
                    } else if (expectedState == ConnState.Connected)
                    {
                        Start();
                    }
                }
            }

            private void KeepConnected()
            {
                // Create new hubconnection

                // attach event listeners

                // monitor for connection failure and reconnect if necessary

                const int initialDelay = 1000;
                const int multiplier = 2;
                const int maxDelay = 30000;
                var delay = 50;
                bool stopped = HubConnection.State != ConnectionStateWrapped.Connected;

                HubConnection.StateChanged += (stateChange) =>
                {
                    if (stateChange.NewState == ConnectionStateWrapped.Disconnected)
                    {
                        stopped = true;
                    }
                };
                while (true)
                {
                    if (stopped)
                    {
                        stopped = false;
                        delay *= multiplier;
                        if (delay > maxDelay)
                        {
                            delay = initialDelay;
                        }
                        HubConnection.Start();
                    }
                    if (HubConnection.State != ConnectionStateWrapped.Connected)
                    {
                        Thread.Sleep(delay);
                    } else
                    {
                        Thread.Sleep(1000);
                    }
                }
            }

            internal void Start()
            {
                if ((_threadWorker.ThreadState & ThreadState.Suspended) != 0)
                {
                    _threadWorker.Resume();
                } else if ((_threadWorker.ThreadState & ThreadState.Unstarted) != 0)
                {
                    _threadWorker.Start();
                }
                else if (!_threadWorker.IsReallyRunning())
                {
                    _threadWorker.Abort();
                    InitializeThreadWorker();
                    _threadWorker.Start();
                }/* else
                {
                    // Threadworker is running and will make sure connection is re-established
                }*/
            }
            internal void Suspend()
            {
                HubConnection.Stop(TimeSpan.Zero);
                _threadWorker.Suspend();
            }
        }
    }
    static class ThreadExtensionMethods
    {
        public static bool IsReallyRunning(this Thread thread)
        {
            var isWaiting = (thread.ThreadState & ThreadState.WaitSleepJoin) == 0;
            var isAborting = (thread.ThreadState & ThreadState.AbortRequested) == 0;
            var isStopping = (thread.ThreadState & ThreadState.StopRequested) == 0;
            return !isWaiting && !isAborting && !isStopping;
        }
    }
}

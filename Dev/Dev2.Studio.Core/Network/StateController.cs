//
// /*
// *  Warewolf - Once bitten, there's no going back
// *  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
// *  Licensed under GNU Affero General Public License 3.0 or later.
// *  Some rights reserved.
// *  Visit our website for more information <http://warewolf.io/>
// *  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
// *  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
// */

using System;
using System.Threading;
using System.Threading.Tasks;
using Dev2.SignalR.Wrappers;
using Warewolf.Data;

namespace Dev2.Network
{
    public enum State
    {
        Disconnected = 0,
        Connected = 1,
    }
    public interface IStateController
    {
        State Current { get; }
        Task<bool> MoveToState(State state, TimeSpan timeout);
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
                        Current = State.Connected;
                        break;
                    case ConnectionStateWrapped.Disconnected:
                        Current = State.Disconnected;
                        break;
                }
            };
        }

        object _currentStateLock = new object();
        State _current;
        public State Current {
            get { lock (_currentStateLock) { return _current; } }
            private set
            {
                lock (_currentStateLock)
                {
                    SetProperty(ref _current, value);
                }
            }
        }
        ReaderWriterLock _expectedStateLock = new ReaderWriterLock();
        State _expectedState;
        public State ExpectedState {
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
        public async Task<bool> MoveToState(State state, TimeSpan timeout)
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
            var delayTask = Task.Delay(timeout);
            var result = await Task.WhenAny(delayTask, t);
            if (result == delayTask)
            {
                throw new Exception(nameof(timeout));
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

            internal void NotifyStateChanged(State current, State expectedState)
            {
                if (current != expectedState)
                {
                    if (expectedState == State.Disconnected)
                    {
                        Suspend();
                    } else if (expectedState == State.Connected)
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
                else if ((_threadWorker.ThreadState & ThreadState.Running) == 0)
                {
                    _threadWorker.Abort();
                    InitializeThreadWorker();
                    _threadWorker.Start();
                }
            }
            internal void Suspend()
            {
                HubConnection.Stop(TimeSpan.Zero);
                _threadWorker.Suspend();
            }

        }
    }
}
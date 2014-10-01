
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Threading
{
    public sealed class MessageDispatcher : IDisposable, IMessageContext
    {
        #region Constants
        private const int UNSTARTED = 0;
        private const int STARTED = 1;
        #endregion

        #region Instance Fields
        private object _ssLock;
        private volatile bool _disposed;
        private volatile int _state;
        private CancellationTokenSource _dispatcherCancellation;
        private BlockingCollection<IMessage> _pending;
        private Thread _dispatcher;
        private SynchronizationContext _context;
        private SendOrPostCallback _callback;
        #endregion

        #region Public Properties
        public bool Disposed { get { return _disposed; } }
        public bool Running { get { return _state != UNSTARTED; } }
        #endregion

        #region Constructor
        public MessageDispatcher(SynchronizationContext context)
        {
            _ssLock = new object();
            _pending = new BlockingCollection<IMessage>();
            if ((_context = context) != null) _callback = new SendOrPostCallback(Send);
        }
        #endregion

        #region [Start/Stop] Handling
        public void Start()
        {
            if (_disposed) throw new ObjectDisposedException("MessageDispatcher is disposed.");
            if (_state != UNSTARTED) return;

            lock (_ssLock)
            {
                if (_state != UNSTARTED) return;

                _dispatcherCancellation = new CancellationTokenSource();
                _dispatcher = new Thread(new ParameterizedThreadStart(DispatchEvents));
                _dispatcher.IsBackground = true;

                _state = STARTED;
                _dispatcher.Start(_pending);
            }
        }

        public void Stop(bool disgardPending)
        {
            if (_disposed) throw new ObjectDisposedException("MessageDispatcher is disposed.");
            if (_state == UNSTARTED) return;

            lock (_ssLock)
            {
                if (_state == UNSTARTED) return;
                BlockingCollection<IMessage> pending = Interlocked.Exchange(ref _pending, new BlockingCollection<IMessage>());

                try { _dispatcherCancellation.Cancel(); }
                catch { }
                try { _dispatcher.Join(); }
                catch { }

                _dispatcherCancellation = null;
                _dispatcher = null;

                if (pending != null)
                {
                    if (!disgardPending)
                    {
                        IMessage item = null;
                        while (pending.TryTake(out item)) Dispatch(item);
                    }

                    pending.Dispose();
                }
                
                _state = UNSTARTED;
            }
        }
        #endregion

        #region Post Handling
        public void Post(IMessage message)
        {
            if (_disposed) throw new ObjectDisposedException("MessageDispatcher is disposed.");
            _pending.Add(message);
        }
        #endregion

        #region Dispatch Handling
        public void DispatchEvents()
        {
            if (_disposed) throw new ObjectDisposedException("MessageDispatcher is disposed.");

            lock (_ssLock)
            {
                if (_state != UNSTARTED) throw new InvalidOperationException();
                BlockingCollection<IMessage> pending = Interlocked.Exchange(ref _pending, new BlockingCollection<IMessage>());

                if (pending != null)
                {
                    IMessage item = null;
                    while (pending.TryTake(out item)) Dispatch(item);
                    pending.Dispose();
                }
            }
        }

        private void DispatchEvents(object args)
        {
            BlockingCollection<IMessage> pending = null;
            lock (_ssLock) pending = (BlockingCollection<IMessage>)args;
            IMessage current = null;

            try
            {
                while (!_dispatcherCancellation.IsCancellationRequested)
                {
                    try
                    {
                        current = pending.Take(_dispatcherCancellation.Token);
                        Dispatch(current);
                    }
                    catch (ThreadAbortException)
                    {
                        if (!Environment.HasShutdownStarted && !AppDomain.CurrentDomain.IsFinalizingForUnload()) 
                            Thread.ResetAbort();
                    }
                }
            }
            catch (OperationCanceledException) { }
        }

        private void Dispatch(IMessage item)
        {
            if (_context == null) item.Execute();
            else _context.Send(_callback, item);
        }

        private void Send(object item)
        {
            ((IMessage)item).Execute();
        }
        #endregion

        #region Disposal Handling
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            lock (_ssLock)
            {
                BlockingCollection<IMessage> pending = Interlocked.Exchange(ref _pending, null);

                if (_state != UNSTARTED)
                {
                    try { _dispatcherCancellation.Cancel(); } catch { }
                    try { _dispatcher.Join(); } catch { }
                    _dispatcherCancellation = null;
                    _dispatcher = null;
                    _state = UNSTARTED;
                }

                if (pending != null) pending.Dispose();
            }
        }
        #endregion
    }
}

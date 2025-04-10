// Decompiled with JetBrains decompiler
// Type: System.Threading.MessageDispatcher
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Collections.Concurrent;

namespace System.Threading
{
  public sealed class MessageDispatcher : IDisposable, IMessageContext
  {
    private const int UNSTARTED = 0;
    private const int STARTED = 1;
    private object _ssLock;
    private volatile bool _disposed;
    private volatile int _state;
    private CancellationTokenSource _dispatcherCancellation;
    private BlockingCollection<IMessage> _pending;
    private Thread _dispatcher;
    private SynchronizationContext _context;
    private SendOrPostCallback _callback;

    public bool Disposed => this._disposed;

    public bool Running => this._state != 0;

    public MessageDispatcher(SynchronizationContext context)
    {
      this._ssLock = new object();
      this._pending = new BlockingCollection<IMessage>();
      if ((this._context = context) == null)
        return;
      this._callback = new SendOrPostCallback(this.Send);
    }

    public void Start()
    {
      if (this._disposed)
        throw new ObjectDisposedException("MessageDispatcher is disposed.");
      if (this._state != 0)
        return;
      lock (this._ssLock)
      {
        if (this._state != 0)
          return;
        this._dispatcherCancellation = new CancellationTokenSource();
        this._dispatcher = new Thread(new ParameterizedThreadStart(this.DispatchEvents));
        this._dispatcher.IsBackground = true;
        this._state = 1;
        this._dispatcher.Start((object) this._pending);
      }
    }

    public void Stop(bool disgardPending)
    {
      if (this._disposed)
        throw new ObjectDisposedException("MessageDispatcher is disposed.");
      if (this._state == 0)
        return;
      lock (this._ssLock)
      {
        if (this._state == 0)
          return;
        BlockingCollection<IMessage> blockingCollection = Interlocked.Exchange<BlockingCollection<IMessage>>(ref this._pending, new BlockingCollection<IMessage>());
        try
        {
          this._dispatcherCancellation.Cancel();
        }
        catch
        {
        }
        try
        {
          this._dispatcher.Join();
        }
        catch
        {
        }
        this._dispatcherCancellation = (CancellationTokenSource) null;
        this._dispatcher = (Thread) null;
        if (blockingCollection != null)
        {
          if (!disgardPending)
          {
            IMessage message = (IMessage) null;
            while (blockingCollection.TryTake(out message))
              this.Dispatch(message);
          }
          blockingCollection.Dispose();
        }
        this._state = 0;
      }
    }

    public void Post(IMessage message)
    {
      if (this._disposed)
        throw new ObjectDisposedException("MessageDispatcher is disposed.");
      this._pending.Add(message);
    }

    public void DispatchEvents()
    {
      if (this._disposed)
        throw new ObjectDisposedException("MessageDispatcher is disposed.");
      lock (this._ssLock)
      {
        if (this._state != 0)
          throw new InvalidOperationException();
        BlockingCollection<IMessage> blockingCollection = Interlocked.Exchange<BlockingCollection<IMessage>>(ref this._pending, new BlockingCollection<IMessage>());
        if (blockingCollection == null)
          return;
        IMessage message = (IMessage) null;
        while (blockingCollection.TryTake(out message))
          this.Dispatch(message);
        blockingCollection.Dispose();
      }
    }

    private void DispatchEvents(object args)
    {
      BlockingCollection<IMessage> blockingCollection = (BlockingCollection<IMessage>) null;
      lock (this._ssLock)
        blockingCollection = (BlockingCollection<IMessage>) args;
      try
      {
        while (!this._dispatcherCancellation.IsCancellationRequested)
        {
          try
          {
            this.Dispatch(blockingCollection.Take(this._dispatcherCancellation.Token));
          }
          catch (ThreadAbortException ex)
          {
            if (!Environment.HasShutdownStarted)
            {
              if (!AppDomain.CurrentDomain.IsFinalizingForUnload())
                Thread.ResetAbort();
            }
          }
        }
      }
      catch (OperationCanceledException ex)
      {
      }
    }

    private void Dispatch(IMessage item)
    {
      if (this._context == null)
        item.Execute();
      else
        this._context.Send(this._callback, (object) item);
    }

    private void Send(object item) => ((IMessage) item).Execute();

    public void Dispose()
    {
      if (this._disposed)
        return;
      this._disposed = true;
      lock (this._ssLock)
      {
        BlockingCollection<IMessage> blockingCollection = Interlocked.Exchange<BlockingCollection<IMessage>>(ref this._pending, (BlockingCollection<IMessage>) null);
        if (this._state != 0)
        {
          try
          {
            this._dispatcherCancellation.Cancel();
          }
          catch
          {
          }
          try
          {
            this._dispatcher.Join();
          }
          catch
          {
          }
          this._dispatcherCancellation = (CancellationTokenSource) null;
          this._dispatcher = (Thread) null;
          this._state = 0;
        }
        blockingCollection?.Dispose();
      }
    }
  }
}

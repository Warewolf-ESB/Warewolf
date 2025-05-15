// Decompiled with JetBrains decompiler
// Type: System.Threading.WriteLock
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

namespace System.Threading
{
  internal struct WriteLock : IDisposable
  {
    private readonly Lock _lock;
    private int _disposed;

    public WriteLock(Lock @lock)
    {
      this._disposed = 0;
      this._lock = @lock;
      this._lock.EnterWriteLock();
    }

    public void Dispose()
    {
      if (Interlocked.CompareExchange(ref this._disposed, 1, 0) != 0)
        return;
      this._lock.ExitWriteLock();
    }
  }
}

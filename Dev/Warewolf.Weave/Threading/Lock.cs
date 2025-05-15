// Decompiled with JetBrains decompiler
// Type: System.Threading.Lock
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

namespace System.Threading
{
  internal sealed class Lock : IDisposable
  {
    private ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
    private int _disposed;

    public void EnterReadLock() => this._lock.EnterReadLock();

    public void EnterWriteLock() => this._lock.EnterWriteLock();

    public void ExitReadLock() => this._lock.ExitReadLock();

    public void ExitWriteLock() => this._lock.ExitWriteLock();

    public void Dispose()
    {
      if (Interlocked.CompareExchange(ref this._disposed, 1, 0) != 0)
        return;
      this._lock.Dispose();
    }
  }
}

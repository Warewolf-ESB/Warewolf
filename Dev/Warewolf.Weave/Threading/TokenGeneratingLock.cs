// Decompiled with JetBrains decompiler
// Type: System.Threading.TokenGeneratingLock
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

namespace System.Threading
{
  public sealed class TokenGeneratingLock : IDisposable
  {
    private int _disposed;
    private bool _isThreadSafe;
    private readonly Lock _lock;

    public bool IsThreadSafe => this._isThreadSafe;

    public TokenGeneratingLock(bool isThreadSafe)
    {
      if (!(this._isThreadSafe = isThreadSafe))
        return;
      this._lock = new Lock();
    }

    public IDisposable LockStateForRead() => this._isThreadSafe ? (IDisposable) new ReadLock(this._lock) : WeaveUtility.EmptyReferenceDisposable;

    public IDisposable LockStateForWrite() => this._isThreadSafe ? (IDisposable) new WriteLock(this._lock) : WeaveUtility.EmptyReferenceDisposable;

    public void Dispose()
    {
      if (!this._isThreadSafe || Interlocked.CompareExchange(ref this._disposed, 1, 0) != 0)
        return;
      this._lock.Dispose();
    }
  }
}

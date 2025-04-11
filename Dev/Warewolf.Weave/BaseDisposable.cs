// Decompiled with JetBrains decompiler
// Type: System.BaseDisposable
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

namespace System
{
  public abstract class BaseDisposable : IDisposable
  {
    private bool _isDisposed;
    private bool _isDisposing;

    protected bool IsDisposing => this._isDisposing;

    public bool IsDisposed => this._isDisposed;

    public void Dispose()
    {
      if (this._isDisposed)
        return;
      this._isDisposed = true;
      try
      {
        this._isDisposing = true;
        this.OnDisposing();
        this.OnDispose();
      }
      finally
      {
        this._isDisposing = false;
        GC.SuppressFinalize((object) this);
      }
    }

    protected virtual void OnDisposing()
    {
    }

    protected virtual void OnDispose()
    {
    }
  }
}

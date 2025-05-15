// Decompiled with JetBrains decompiler
// Type: System.Network.Envoy
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Net;

namespace System.Network
{
  public sealed class Envoy : IDisposable
  {
    private IAsyncResult _currentOperation;
    private string _hostNameOrAddress;
    private int _port;

    public string HostNameOrAddress => this._hostNameOrAddress;

    public int Port => this._port;

    public event EnvoyReturnedEventHandler EnvoyReturned;

    public Envoy(string hostNameOrAddress) => this._hostNameOrAddress = hostNameOrAddress;

    public Envoy(string hostNameOrAddress, int port)
    {
      this._hostNameOrAddress = hostNameOrAddress;
      this._port = port;
    }

    public Exception BeginResolution()
    {
      Exception exception = (Exception) null;
      try
      {
        this._currentOperation = Dns.BeginGetHostEntry(this._hostNameOrAddress, new AsyncCallback(this.EndResolution), (object) null);
      }
      catch (Exception ex)
      {
        exception = ex;
      }
      return exception;
    }

    private void EndResolution(IAsyncResult result)
    {
      bool successful = true;
      IPHostEntry resolvedHostEntry = (IPHostEntry) null;
      try
      {
        resolvedHostEntry = Dns.EndGetHostEntry(result);
      }
      catch
      {
        successful = false;
      }
      if (successful)
        successful = resolvedHostEntry != null;
      this._currentOperation = (IAsyncResult) null;
      if (this.EnvoyReturned == null)
        return;
      this.EnvoyReturned(this, successful, this._hostNameOrAddress, resolvedHostEntry);
    }

    public void Dispose()
    {
      if (this._currentOperation != null)
      {
        try
        {
          Dns.EndGetHostEntry(this._currentOperation);
        }
        catch
        {
        }
        if (this.EnvoyReturned != null)
          this.EnvoyReturned(this, false, this._hostNameOrAddress, (IPHostEntry) null);
        this._currentOperation = (IAsyncResult) null;
      }
      this.EnvoyReturned = (EnvoyReturnedEventHandler) null;
    }
  }
}

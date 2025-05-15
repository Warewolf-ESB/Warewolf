// Decompiled with JetBrains decompiler
// Type: System.Network.BFPSetup
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

namespace System.Network
{
  public sealed class BFPSetup : SerializableEntity, ICloneable
  {
    private uint _accountActivationThreshold;
    private uint _addressActivationThreshold;
    private TimeSpan _accountMonitorDuration;
    private TimeSpan _accountDuration;
    private TimeSpan _accountPenalty;
    private TimeSpan _addressMonitorDuration;
    private TimeSpan _addressDuration;
    private TimeSpan _addressPenalty;

    public uint AccountActivationThreshold
    {
      get => this._accountActivationThreshold;
      set => this._accountActivationThreshold = value;
    }

    public uint AddressActivationThreshold
    {
      get => this._addressActivationThreshold;
      set => this._addressActivationThreshold = value;
    }

    public TimeSpan AccountMonitorDuration
    {
      get => this._accountMonitorDuration;
      set => this._accountMonitorDuration = value;
    }

    public TimeSpan AccountDuration
    {
      get => this._accountDuration;
      set => this._accountDuration = value;
    }

    public TimeSpan AccountPenalty
    {
      get => this._accountPenalty;
      set => this._accountPenalty = value;
    }

    public TimeSpan AddressMonitorDuration
    {
      get => this._addressMonitorDuration;
      set => this._addressMonitorDuration = value;
    }

    public TimeSpan AddressDuration
    {
      get => this._addressDuration;
      set => this._addressDuration = value;
    }

    public TimeSpan AddressPenalty
    {
      get => this._addressPenalty;
      set => this._addressPenalty = value;
    }

    private BFPSetup(BFPSetup original)
    {
      this._accountActivationThreshold = original._accountActivationThreshold;
      this._addressActivationThreshold = original._addressActivationThreshold;
      this._accountMonitorDuration = original._accountMonitorDuration;
      this._accountDuration = original._accountDuration;
      this._accountPenalty = original._accountPenalty;
      this._addressMonitorDuration = original._addressMonitorDuration;
      this._addressDuration = original._addressDuration;
      this._addressPenalty = original._addressPenalty;
    }

    public BFPSetup()
    {
      this._accountActivationThreshold = 10U;
      this._addressActivationThreshold = 6U;
      this._accountMonitorDuration = TimeSpan.FromMinutes(5.0);
      this._accountDuration = TimeSpan.FromMinutes(10.0);
      this._accountPenalty = TimeSpan.FromMinutes(2.0);
      this._addressMonitorDuration = TimeSpan.FromMinutes(5.0);
      this._addressDuration = TimeSpan.FromMinutes(15.0);
      this._addressPenalty = TimeSpan.FromMinutes(4.0);
    }

    object ICloneable.Clone() => (object) this.Clone();

    public BFPSetup Clone() => new BFPSetup(this);

    public BFPSetup(IByteReaderBase reader)
      : base(reader)
    {
      this._accountActivationThreshold = reader.ReadUInt32();
      this._addressActivationThreshold = reader.ReadUInt32();
      this._accountMonitorDuration = reader.ReadTimeSpan();
      this._accountDuration = reader.ReadTimeSpan();
      this._accountPenalty = reader.ReadTimeSpan();
      this._addressMonitorDuration = reader.ReadTimeSpan();
      this._addressDuration = reader.ReadTimeSpan();
      this._addressPenalty = reader.ReadTimeSpan();
    }

    protected override void Serialize(IByteWriterBase writer)
    {
      writer.Write(this._accountActivationThreshold);
      writer.Write(this._addressActivationThreshold);
      writer.Write(this._accountMonitorDuration);
      writer.Write(this._accountDuration);
      writer.Write(this._accountPenalty);
      writer.Write(this._addressMonitorDuration);
      writer.Write(this._addressDuration);
      writer.Write(this._addressPenalty);
    }
  }
}

// Decompiled with JetBrains decompiler
// Type: System.Cryptography.HashDataBroker
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

namespace System.Cryptography
{
  public sealed class HashDataBroker
  {
    internal byte[] RawData;

    internal int Length => this.RawData.Length;

    public HashDataBroker(byte[] data) => this.RawData = data;

    public static implicit operator HashDataBroker(byte[] data) => new HashDataBroker(data);

    public static implicit operator HashDataBroker(string str) => new HashDataBroker(CryptUtility.Encoding.GetBytes(str));

    public static implicit operator HashDataBroker(BigInteger integer) => new HashDataBroker(integer.GetBytes());

    public static implicit operator HashDataBroker(uint integer) => new HashDataBroker(new BigInteger((long) integer).GetBytes());
  }
}

// Decompiled with JetBrains decompiler
// Type: System.IByteReaderBase
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

namespace System
{
  public interface IByteReaderBase : IDisposable
  {
    bool ReadBoolean();

    char ReadChar();

    string ReadString();

    byte[] ReadBytes(int amount);

    int ReadBytes(byte[] buffer, int offset, int count);

    char[] ReadChars(int amount);

    int ReadChars(char[] buffer, int offset, int count);

    sbyte ReadSByte();

    short ReadInt16();

    int ReadInt32();

    long ReadInt64();

    Decimal ReadDecimal();

    byte ReadByte();

    ushort ReadUInt16();

    uint ReadUInt32();

    ulong ReadUInt64();

    float ReadSingle();

    double ReadDouble();

    Version ReadVersion();

    DateTime ReadDateTime();

    TimeSpan ReadTimeSpan();

    Guid ReadGuid();

    TwoOctetUnion ReadTwoOctet();

    FourOctetUnion ReadFourOctet();

    EightOctetUnion ReadEightOctet();
  }
}

// Decompiled with JetBrains decompiler
// Type: System.IByteWriterBase
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

namespace System
{
  public interface IByteWriterBase : IDisposable
  {
    void Write(bool value);

    void Write(char value);

    void Write(string value);

    void Write(byte[] value);

    void Write(byte[] value, int offset, int count);

    void Write(char[] value);

    void Write(char[] value, int offset, int count);

    void Write(sbyte value);

    void Write(short value);

    void Write(int value);

    void Write(long value);

    void Write(Decimal value);

    void Write(byte value);

    void Write(ushort value);

    void Write(uint value);

    void Write(ulong value);

    void Write(float value);

    void Write(double value);

    void Write(Version value);

    void Write(DateTime value);

    void Write(TimeSpan value);

    void Write(Guid value);

    void Write(TwoOctetUnion value);

    void Write(FourOctetUnion value);

    void Write(EightOctetUnion value);
  }
}

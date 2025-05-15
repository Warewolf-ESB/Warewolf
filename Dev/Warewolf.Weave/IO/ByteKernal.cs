// Decompiled with JetBrains decompiler
// Type: System.IO.ByteKernal
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

namespace System.IO
{
  public sealed class ByteKernal
  {
    public byte[] Buffer;
    public int Length;
    public int Capacity;
    public int Position;

    public void SetCapacity(int value)
    {
      if (value == this.Capacity)
        return;
      if (value > 0)
      {
        byte[] dst = new byte[value];
        if (this.Length > 0)
          System.Buffer.BlockCopy((Array) this.Buffer, 0, (Array) dst, 0, this.Length);
        this.Buffer = dst;
      }
      else
        this.Buffer = (byte[]) null;
      this.Capacity = value;
    }

    public void SetLength(int value)
    {
      if (!this.EnsureCapacity(value) && value > this.Length)
        Array.Clear((Array) this.Buffer, this.Length, value - this.Length);
      this.Length = value;
      if (this.Position <= value)
        return;
      this.Position = value;
    }

    public bool EnsureRead(int amount) => this.Position + amount <= this.Length;

    public void EnsureWrite(int end)
    {
      bool flag = this.Position > this.Length;
      if (end > this.Capacity && this.EnsureCapacity(end))
        flag = false;
      if (flag)
        Array.Clear((Array) this.Buffer, this.Length, end - this.Length);
      this.Length = end;
    }

    public bool EnsureCapacity(int value)
    {
      if (value <= this.Capacity)
        return false;
      if (value < 32)
        value = 32;
      if (value < this.Capacity * 2)
        value = this.Capacity * 2;
      this.SetCapacity(value);
      return true;
    }
  }
}

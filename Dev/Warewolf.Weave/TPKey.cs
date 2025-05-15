// Decompiled with JetBrains decompiler
// Type: System.TPKey
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Runtime.InteropServices;

namespace System
{
  [StructLayout(LayoutKind.Explicit, Size = 4)]
  internal struct TPKey : IEquatable<TPKey>, IComparable<TPKey>
  {
    public const int SizeInBytes = 4;
    public static readonly TPKey Invalid;
    [FieldOffset(0)]
    internal int Value;

    public bool Valid => this.Value != 0;

    internal TPKey(int value) => this.Value = value;

    public override string ToString() => "TPKey " + (this.Valid ? "[Valid]" : "[Invalid]");

    public override int GetHashCode() => this.Value;

    public override bool Equals(object obj) => obj != null && obj is TPKey tpKey && this.Value == tpKey.Value;

    public bool Equals(TPKey other) => this.Value == other.Value;

    public int CompareTo(TPKey comparand) => this.Value.CompareTo(comparand.Value);

    public static bool operator ==(TPKey l, TPKey r) => l.Value == r.Value;

    public static bool operator !=(TPKey l, TPKey r) => l.Value != r.Value;
  }
}

// Decompiled with JetBrains decompiler
// Type: System.IO.__Error
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

namespace System.IO
{
  internal static class __Error
  {
    internal static void EndOfFile() => throw new EndOfStreamException(WeaveUtility.GetResourceString("IO.EOF_ReadBeyondEOF"));

    internal static void FileNotOpen() => throw new ObjectDisposedException((string) null, WeaveUtility.GetResourceString("ObjectDisposed_FileClosed"));
  }
}

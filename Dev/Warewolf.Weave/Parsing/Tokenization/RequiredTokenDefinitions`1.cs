// Decompiled with JetBrains decompiler
// Type: System.Parsing.Tokenization.RequiredTokenDefinitions`1
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

namespace System.Parsing.Tokenization
{
  public sealed class RequiredTokenDefinitions<K> where K : TokenDefinition
  {
    public readonly K Whitespace;
    public readonly K LineBreak;
    public readonly K Unknown;
    public readonly K EndOfFile;

    public RequiredTokenDefinitions(K whitespace, K lineBreak, K unknown, K endOfFile)
    {
      if ((object) (this.Whitespace = whitespace) == null)
        throw new ArgumentNullException(nameof (whitespace));
      if ((object) (this.LineBreak = lineBreak) == null)
        throw new ArgumentNullException(nameof (lineBreak));
      if ((object) (this.Unknown = unknown) == null)
        throw new ArgumentNullException(nameof (unknown));
      if ((object) (this.EndOfFile = endOfFile) == null)
        throw new ArgumentNullException(nameof (endOfFile));
    }
  }
}

// Decompiled with JetBrains decompiler
// Type: System.Parsing.Intellisense.Token
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Parsing.Tokenization;

namespace System.Parsing.Intellisense
{
  public sealed class Token : Token<Token, TokenKind>
  {
    public override string ToString() => this.Definition != null ? this.Definition.ToString() + " (( " + this.Content : this.Content;
  }
}

// Decompiled with JetBrains decompiler
// Type: System.Parsing.Intellisense.LiteralNode
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

namespace System.Parsing.Intellisense
{
  public sealed class LiteralNode : Node
  {
    public TokenClassification Kind => this.Declaration.Start != null ? this.Declaration.Start.Definition.Kind : TokenClassification.Invalid;

    public override bool IsTerminal => true;

    public LiteralNode()
    {
    }

    public LiteralNode(Token token)
      : base(token)
    {
    }

    public override string GetEvaluatedValue() => this.GetRepresentationForEvaluation();

    public override string GetRepresentationForEvaluation()
    {
      if (!this.Declaration.Start.Definition.IsStringLiteral)
        return this.Declaration.Content;
      int startIndex = this.Declaration.Start.SourceIndex + 1;
      int length = this.Declaration.Start.SourceLength - 2;
      return this.Declaration.Start.Definition == TokenKind.VerbatimString ? "\"" + StringUtility.GetUnescapedVerbatimString(this.Declaration.Start.Source.Substring(startIndex + 1, length - 1)) + "\"" : "\"" + StringUtility.GetUnescapedRegularString(this.Declaration.Start.Source.Substring(startIndex, length)) + "\"";
    }
  }
}

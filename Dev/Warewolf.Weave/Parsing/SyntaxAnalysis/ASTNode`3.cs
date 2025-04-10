// Decompiled with JetBrains decompiler
// Type: System.Parsing.SyntaxAnalysis.ASTNode`3
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Parsing.Tokenization;

namespace System.Parsing.SyntaxAnalysis
{
  public abstract class ASTNode<Token, TokenKind, ConcreteNode>
    where Token : System.Parsing.Tokenization.Token<Token, TokenKind>, new()
    where TokenKind : TokenDefinition
    where ConcreteNode : ASTNode<Token, TokenKind, ConcreteNode>
  {
    public abstract Token Start { get; }

    public abstract Token End { get; }

    public virtual bool IsTerminal => true;

    public void Visit(
      VisitPurpose purpose,
      AbstractSyntaxTreeBuilder<Token, TokenKind, ConcreteNode> builder)
    {
      this.OnVisit(purpose, builder);
    }

    protected virtual void OnVisit(
      VisitPurpose purpose,
      AbstractSyntaxTreeBuilder<Token, TokenKind, ConcreteNode> builder)
    {
    }
  }
}

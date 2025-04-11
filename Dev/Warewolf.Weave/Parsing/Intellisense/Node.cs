// Decompiled with JetBrains decompiler
// Type: System.Parsing.Intellisense.Node
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Collections.Generic;
using System.Parsing.SyntaxAnalysis;
using System.Text;

namespace System.Parsing.Intellisense
{
  public abstract class Node : ASTNode<Token, TokenKind, Node>
  {
    public string EvaluatedValue;
    internal Node TransientReference;
    public TokenPair Declaration;
    public TokenPair Identifier;

    public override sealed Token Start => this.Declaration.Start;

    public override sealed Token End => this.Declaration.End;

    public Node() => this.TransientReference = this;

    public Node(Token token)
    {
      this.TransientReference = this;
      this.Declaration = this.Identifier = (TokenPair) token;
    }

    internal virtual Node GetLeftMostNode() => this;

    internal virtual Node GetRightMostNode() => this;

    public virtual Node GetNodeAt(int sourceIndex) => this;

    public virtual void CollectNodes(ICollection<Node> nodes) => nodes.Add(this);

    public virtual string GetEvaluatedValue() => this.EvaluatedValue;

    public virtual string GetRepresentationForEvaluation() => this.Declaration.Content;

    public virtual void AppendText(StringBuilder text, int indent)
    {
      string str = new string('\t', indent);
      text.AppendLine(str.Substring(1) + this.GetType().Name);
      text.AppendLine(str + this.Identifier.Content);
      text.AppendLine();
    }
  }
}

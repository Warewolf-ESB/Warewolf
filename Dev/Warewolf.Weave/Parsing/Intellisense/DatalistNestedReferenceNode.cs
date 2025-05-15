// Decompiled with JetBrains decompiler
// Type: System.Parsing.Intellisense.DatalistNestedReferenceNode
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Collections.Generic;
using System.Parsing.SyntaxAnalysis;
using System.Text;

namespace System.Parsing.Intellisense
{
  public class DatalistNestedReferenceNode : DatalistReferenceNode
  {
    public Node NestedIdentifier;

    public DatalistNestedReferenceNode()
    {
    }

    public DatalistNestedReferenceNode(Token token)
      : base(token)
    {
    }

    public override string GetRepresentationForEvaluation() => TokenKind.OpenDL.Identifier + this.NestedIdentifier.GetEvaluatedValue() + TokenKind.CloseDL.Identifier;

    public override Node GetNodeAt(int sourceIndex) => this.NestedIdentifier != null && this.NestedIdentifier.Declaration.Contains(sourceIndex) ? this.NestedIdentifier.GetNodeAt(sourceIndex) : (Node) this;

    public override void CollectNodes(ICollection<Node> nodes)
    {
      base.CollectNodes(nodes);
      if (this.NestedIdentifier == null)
        return;
      this.NestedIdentifier.CollectNodes(nodes);
    }

    protected override void OnVisit(
      VisitPurpose purpose,
      AbstractSyntaxTreeBuilder<Token, TokenKind, Node> builder)
    {
      base.OnVisit(purpose, builder);
      if (purpose != VisitPurpose.BuildNodeDeclarations || this.NestedIdentifier == null)
        return;
      this.NestedIdentifier.Visit(purpose, builder);
    }

    public override void AppendText(StringBuilder text, int indent)
    {
      base.AppendText(text, indent);
      if (this.NestedIdentifier == null)
        return;
      this.NestedIdentifier.AppendText(text, ++indent);
    }
  }
}

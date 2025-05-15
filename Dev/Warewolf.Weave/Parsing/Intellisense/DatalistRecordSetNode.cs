// Decompiled with JetBrains decompiler
// Type: System.Parsing.Intellisense.DatalistRecordSetNode
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Collections.Generic;
using System.Parsing.SyntaxAnalysis;
using System.Text;

namespace System.Parsing.Intellisense
{
  public class DatalistRecordSetNode : DatalistReferenceNode
  {
    public Node NestedIdentifier;
    public ParenthesisGroupNode Parameter;

    public DatalistRecordSetNode()
    {
    }

    public DatalistRecordSetNode(Token token)
      : base(token)
    {
    }

    public override string GetRepresentationForEvaluation() => this.NestedIdentifier != null ? TokenKind.OpenDL.Identifier + this.NestedIdentifier.GetEvaluatedValue() + this.Parameter.GetRepresentationForEvaluation() + TokenKind.CloseDL.Identifier : TokenKind.OpenDL.Identifier + this.Identifier.Content + this.Parameter.GetRepresentationForEvaluation() + TokenKind.CloseDL.Identifier;

    public override Node GetNodeAt(int sourceIndex)
    {
      if (this.NestedIdentifier != null && this.NestedIdentifier.Declaration.Contains(sourceIndex))
        return this.NestedIdentifier.GetNodeAt(sourceIndex);
      return this.Parameter != null && this.Parameter.Declaration.Contains(sourceIndex) ? this.Parameter.GetNodeAt(sourceIndex) : (Node) this;
    }

    public override void CollectNodes(ICollection<Node> nodes)
    {
      base.CollectNodes(nodes);
      if (this.NestedIdentifier != null)
        this.NestedIdentifier.CollectNodes(nodes);
      if (this.Parameter == null)
        return;
      this.Parameter.CollectNodes(nodes);
    }

    protected override void OnVisit(
      VisitPurpose purpose,
      AbstractSyntaxTreeBuilder<Token, TokenKind, Node> builder)
    {
      base.OnVisit(purpose, builder);
      if (purpose != VisitPurpose.BuildNodeDeclarations)
        return;
      if (this.NestedIdentifier != null)
        this.NestedIdentifier.Visit(purpose, builder);
      if (this.Parameter == null)
        return;
      this.Parameter.Visit(purpose, builder);
    }

    public override void AppendText(StringBuilder text, int indent)
    {
      base.AppendText(text, indent);
      ++indent;
      if (this.NestedIdentifier != null)
        this.NestedIdentifier.AppendText(text, indent);
      if (this.Parameter == null)
        return;
      this.Parameter.AppendText(text, indent);
    }
  }
}

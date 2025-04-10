// Decompiled with JetBrains decompiler
// Type: System.Parsing.Intellisense.DatalistRecordSetFieldNode
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Collections.Generic;
using System.Parsing.SyntaxAnalysis;
using System.Text;

namespace System.Parsing.Intellisense
{
  public class DatalistRecordSetFieldNode : DatalistReferenceNode
  {
    public DatalistRecordSetNode RecordSet;
    public Node Field;

    public DatalistRecordSetFieldNode()
    {
    }

    public DatalistRecordSetFieldNode(Token token)
      : base(token)
    {
    }

    public override string GetRepresentationForEvaluation() => this.Field != null ? (this.RecordSet.NestedIdentifier != null ? TokenKind.OpenDL.Identifier + this.RecordSet.NestedIdentifier.GetEvaluatedValue() + this.RecordSet.Parameter.GetRepresentationForEvaluation() + TokenKind.FullStop.Identifier + this.Field.GetEvaluatedValue() + TokenKind.CloseDL.Identifier : TokenKind.OpenDL.Identifier + this.RecordSet.Identifier.Content + this.RecordSet.Parameter.GetRepresentationForEvaluation() + TokenKind.FullStop.Identifier + this.Field.GetEvaluatedValue() + TokenKind.CloseDL.Identifier) : (this.RecordSet.NestedIdentifier != null ? TokenKind.OpenDL.Identifier + this.RecordSet.NestedIdentifier.GetEvaluatedValue() + this.RecordSet.Parameter.GetRepresentationForEvaluation() + TokenKind.FullStop.Identifier + this.Identifier.Content + TokenKind.CloseDL.Identifier : TokenKind.OpenDL.Identifier + this.RecordSet.Identifier.Content + this.RecordSet.Parameter.GetRepresentationForEvaluation() + TokenKind.FullStop.Identifier + this.Identifier.Content + TokenKind.CloseDL.Identifier);

    public override Node GetNodeAt(int sourceIndex)
    {
      if (this.RecordSet != null && this.RecordSet.Declaration.Contains(sourceIndex))
        return this.RecordSet.GetNodeAt(sourceIndex);
      return this.Field != null && this.Field.Declaration.Contains(sourceIndex) ? this.Field.GetNodeAt(sourceIndex) : (Node) this;
    }

    public override void CollectNodes(ICollection<Node> nodes)
    {
      base.CollectNodes(nodes);
      if (this.RecordSet != null)
        this.RecordSet.CollectNodes(nodes);
      if (this.Field == null)
        return;
      this.Field.CollectNodes(nodes);
    }

    protected override void OnVisit(
      VisitPurpose purpose,
      AbstractSyntaxTreeBuilder<Token, TokenKind, Node> builder)
    {
      base.OnVisit(purpose, builder);
      if (purpose != VisitPurpose.BuildNodeDeclarations)
        return;
      if (this.RecordSet != null)
        this.RecordSet.Visit(purpose, builder);
      if (this.Field == null)
        return;
      this.Field.Visit(purpose, builder);
    }

    public override void AppendText(StringBuilder text, int indent)
    {
      base.AppendText(text, indent);
      ++indent;
      if (this.RecordSet != null)
        this.RecordSet.AppendText(text, indent);
      if (this.Field == null)
        return;
      this.Field.AppendText(text, indent);
    }
  }
}

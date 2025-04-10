// Decompiled with JetBrains decompiler
// Type: System.Parsing.Intellisense.ParameterNode
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Collections.Generic;
using System.Parsing.SyntaxAnalysis;
using System.Text;

namespace System.Parsing.Intellisense
{
  public class ParameterNode : Node
  {
    public Node Statement;

    public override string GetEvaluatedValue() => this.Statement.GetEvaluatedValue();

    public override string GetRepresentationForEvaluation() => this.Statement.GetEvaluatedValue();

    public override Node GetNodeAt(int sourceIndex) => this.Statement != null && this.Statement.Declaration.Contains(sourceIndex) ? this.Statement.GetNodeAt(sourceIndex) : (Node) this;

    public override void CollectNodes(ICollection<Node> nodes)
    {
      base.CollectNodes(nodes);
      if (this.Statement == null)
        return;
      this.Statement.CollectNodes(nodes);
    }

    protected override void OnVisit(
      VisitPurpose purpose,
      AbstractSyntaxTreeBuilder<Token, TokenKind, Node> builder)
    {
      base.OnVisit(purpose, builder);
      if (this.Statement == null)
        return;
      this.Statement.Visit(purpose, builder);
    }

    public override void AppendText(StringBuilder text, int indent)
    {
      base.AppendText(text, indent);
      if (this.Statement == null)
        return;
      this.Statement.AppendText(text, indent + 1);
    }
  }
}

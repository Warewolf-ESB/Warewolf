// Decompiled with JetBrains decompiler
// Type: System.Parsing.Intellisense.MethodInvocationNode
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Collections.Generic;
using System.Parsing.SyntaxAnalysis;
using System.Text;

namespace System.Parsing.Intellisense
{
  public class MethodInvocationNode : Node
  {
    public ParameterCollectionNode Parameters;

    public MethodInvocationNode()
    {
    }

    public MethodInvocationNode(Token token)
      : base(token)
    {
    }

    public override string GetEvaluatedValue() => this.GetRepresentationForEvaluation();

    public override string GetRepresentationForEvaluation() => this.Identifier.Content + this.Parameters.GetEvaluatedValue();

    public override Node GetNodeAt(int sourceIndex) => this.Parameters != null && this.Parameters.Declaration.Contains(sourceIndex) ? this.Parameters.GetNodeAt(sourceIndex) : (Node) this;

    public override void CollectNodes(ICollection<Node> nodes)
    {
      base.CollectNodes(nodes);
      if (this.Parameters == null)
        return;
      this.Parameters.CollectNodes(nodes);
    }

    protected override void OnVisit(
      VisitPurpose purpose,
      AbstractSyntaxTreeBuilder<Token, TokenKind, Node> builder)
    {
      base.OnVisit(purpose, builder);
      if (this.Parameters == null)
        return;
      this.Parameters.Visit(purpose, builder);
    }

    public override void AppendText(StringBuilder text, int indent)
    {
      base.AppendText(text, indent);
      if (this.Parameters == null)
        return;
      this.Parameters.AppendText(text, indent + 1);
    }
  }
}

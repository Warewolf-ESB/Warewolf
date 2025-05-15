// Decompiled with JetBrains decompiler
// Type: System.Parsing.Intellisense.BinaryOperatorNode
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Collections.Generic;
using System.Parsing.SyntaxAnalysis;
using System.Text;

namespace System.Parsing.Intellisense
{
  public class BinaryOperatorNode : Node
  {
    public Node Left;
    public Node Right;

    public override bool IsTerminal => true;

    public BinaryOperatorNode()
    {
    }

    public BinaryOperatorNode(Node placeholderOperator, Node left, Node right)
    {
      this.Identifier = placeholderOperator.Identifier;
      this.Left = left;
      this.Right = right;
      this.Declaration = new TokenPair(this.Left.Declaration.Start, this.Right.End);
    }

    public override string GetEvaluatedValue() => this.EvaluatedValue == null ? this.GetRepresentationForEvaluation() : this.EvaluatedValue;

    public override string GetRepresentationForEvaluation() => this.Left.GetEvaluatedValue() + this.Identifier.Content + this.Right.GetEvaluatedValue();

    public override Node GetNodeAt(int sourceIndex)
    {
      if (this.Left != null && this.Left.Declaration.Contains(sourceIndex))
        return this.Left.GetNodeAt(sourceIndex);
      return this.Right != null && this.Right.Declaration.Contains(sourceIndex) ? this.Right.GetNodeAt(sourceIndex) : (Node) this;
    }

    internal override Node GetLeftMostNode() => this.Left.GetLeftMostNode();

    internal override Node GetRightMostNode() => this.Right.GetRightMostNode();

    public override void CollectNodes(ICollection<Node> nodes)
    {
      base.CollectNodes(nodes);
      if (this.Left != null)
        this.Left.CollectNodes(nodes);
      if (this.Right == null)
        return;
      this.Right.CollectNodes(nodes);
    }

    protected override void OnVisit(
      VisitPurpose purpose,
      AbstractSyntaxTreeBuilder<Token, TokenKind, Node> builder)
    {
      base.OnVisit(purpose, builder);
      if (this.Left != null)
        this.Left.Visit(purpose, builder);
      if (this.Right == null)
        return;
      this.Right.Visit(purpose, builder);
    }

    public override void AppendText(StringBuilder text, int indent)
    {
      base.AppendText(text, indent);
      if (this.Left != null)
        this.Left.AppendText(text, indent + 1);
      if (this.Right == null)
        return;
      this.Right.AppendText(text, indent + 1);
    }
  }
}

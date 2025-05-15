// Decompiled with JetBrains decompiler
// Type: System.Parsing.Intellisense.ParenthesisGroupNode
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Collections.Generic;
using System.Parsing.SyntaxAnalysis;
using System.Text;

namespace System.Parsing.Intellisense
{
  public class ParenthesisGroupNode : CollectionNode
  {
    public Node Statement;

    public override Node GetNodeAt(int sourceIndex)
    {
      if (this.Statement == null)
        return base.GetNodeAt(sourceIndex);
      return this.Statement.Declaration.Contains(sourceIndex) ? this.Statement.GetNodeAt(sourceIndex) : (Node) this;
    }

    public override string GetEvaluatedValue() => this.GetRepresentationForEvaluation();

    public override string GetRepresentationForEvaluation()
    {
      if (this.EvaluatedValue != null)
        return TokenKind.LeftParenthesis.Identifier + this.EvaluatedValue + TokenKind.RightParenthesis.Identifier;
      if (this.Statement != null)
        return TokenKind.LeftParenthesis.Identifier + this.Statement.GetEvaluatedValue() + TokenKind.RightParenthesis.Identifier;
      if (this.Items == null)
        return TokenKind.LeftParenthesis.Identifier + TokenKind.RightParenthesis.Identifier;
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.Append(TokenKind.LeftParenthesis.Identifier);
      for (int index = 0; index < this.Items.Length; ++index)
        stringBuilder.Append(this.Items[0].GetEvaluatedValue());
      stringBuilder.Append(TokenKind.RightParenthesis.Identifier);
      return stringBuilder.ToString();
    }

    public override void CollectNodes(ICollection<Node> nodes)
    {
      if (this.Statement != null)
      {
        nodes.Add((Node) this);
        this.Statement.CollectNodes(nodes);
      }
      else
        base.CollectNodes(nodes);
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

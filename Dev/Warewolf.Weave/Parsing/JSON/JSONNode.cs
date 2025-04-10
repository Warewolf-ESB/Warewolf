// Decompiled with JetBrains decompiler
// Type: System.Parsing.JSON.JSONNode
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Collections.Generic;
using System.Parsing.SyntaxAnalysis;
using System.Text;

namespace System.Parsing.JSON
{
  public abstract class JSONNode : ASTNode<Token, TokenKind, JSONNode>
  {
    public string EvaluatedValue;
    internal JSONNode TransientReference;
    public TokenPair Declaration;
    public TokenPair Identifier;

    public override sealed Token Start => this.Declaration.Start;

    public override sealed Token End => this.Declaration.End;

    public JSONNode() => this.TransientReference = this;

    public JSONNode(Token token)
    {
      this.TransientReference = this;
      this.Declaration = this.Identifier = (TokenPair) token;
    }

    internal virtual JSONNode GetLeftMostNode() => this;

    internal virtual JSONNode GetRightMostNode() => this;

    public virtual JSONNode GetNodeAt(int sourceIndex) => this;

    public virtual void CollectNodes(ICollection<JSONNode> nodes) => nodes.Add(this);

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

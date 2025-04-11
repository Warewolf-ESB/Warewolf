// Decompiled with JetBrains decompiler
// Type: System.Parsing.Intellisense.IterationNode
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

namespace System.Parsing.Intellisense
{
  public class IterationNode : Node
  {
    public INodeValueSource ValueSource;

    public IterationNode(Token token)
      : base(token)
    {
    }

    public override string GetEvaluatedValue() => this.ValueSource != null ? this.ValueSource.GetEvaluatedValue((Node) this) : this.GetRepresentationForEvaluation();

    public override string GetRepresentationForEvaluation() => this.ValueSource != null ? this.ValueSource.GetRepresentationForEvaluation((Node) this) : this.Declaration.Content;
  }
}

// Decompiled with JetBrains decompiler
// Type: System.Parsing.JSON.CollectionNode
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Collections.Generic;
using System.Parsing.SyntaxAnalysis;
using System.Text;

namespace System.Parsing.JSON
{
  public abstract class CollectionNode : JSONNode
  {
    public JSONNode[] Items;

    public CollectionNode()
    {
    }

    public CollectionNode(Token token)
      : base(token)
    {
    }

    public override JSONNode GetNodeAt(int sourceIndex)
    {
      if (this.Items != null)
      {
        for (int index = 0; index < this.Items.Length; ++index)
        {
          if (this.Items[index].Declaration.Contains(sourceIndex))
            return this.Items[index].GetNodeAt(sourceIndex);
        }
      }
      return (JSONNode) this;
    }

    public override void CollectNodes(ICollection<JSONNode> nodes)
    {
      base.CollectNodes(nodes);
      if (this.Items == null)
        return;
      for (int index = 0; index < this.Items.Length; ++index)
      {
        if (this.Items[0] != null)
          this.Items[index].CollectNodes(nodes);
      }
    }

    protected override void OnVisit(
      VisitPurpose purpose,
      AbstractSyntaxTreeBuilder<Token, TokenKind, JSONNode> builder)
    {
      base.OnVisit(purpose, builder);
      if (purpose != VisitPurpose.BuildNodeDeclarations)
        return;
      this.Items = builder.BuildNodes((JSONNode) this, this.Declaration.Start.Next, this.Declaration.End.Previous);
    }

    public override void AppendText(StringBuilder text, int indent)
    {
      base.AppendText(text, indent);
      if (this.Items == null || this.Items.Length == 0)
        return;
      ++indent;
      for (int index = 0; index < this.Items.Length; ++index)
        this.Items[index].AppendText(text, indent);
    }
  }
}

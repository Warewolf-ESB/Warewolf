// Decompiled with JetBrains decompiler
// Type: System.Parsing.Intellisense.CompositeStringLiteralNode
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Text;

namespace System.Parsing.Intellisense
{
  public class CompositeStringLiteralNode : CollectionNode
  {
    public override string GetEvaluatedValue() => this.GetRepresentationForEvaluation();

    public override string GetRepresentationForEvaluation()
    {
      bool flag = this.Declaration.Start.Definition == TokenKind.CompositeVerbatimString;
      if (this.Items != null)
      {
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.Append("\"");
        int startIndex = this.Declaration.Start.SourceIndex + this.Declaration.Start.SourceLength;
        for (int index = 0; index < this.Items.Length; ++index)
        {
          Node node = this.Items[index];
          int sourceIndex = node.Declaration.Start.SourceIndex;
          if (sourceIndex > startIndex)
          {
            if (flag)
              stringBuilder.Append(StringUtility.GetUnescapedVerbatimString(this.Declaration.Start.Source.Substring(startIndex, sourceIndex - startIndex)));
            else
              stringBuilder.Append(StringUtility.GetUnescapedRegularString(this.Declaration.Start.Source.Substring(startIndex, sourceIndex - startIndex)));
          }
          stringBuilder.Append(this.Items[index].GetEvaluatedValue());
          startIndex = node.Declaration.End.SourceIndex + node.Declaration.End.SourceLength;
        }
        int sourceIndex1 = this.Declaration.End.SourceIndex;
        if (sourceIndex1 > startIndex)
        {
          if (flag)
            stringBuilder.Append(StringUtility.GetUnescapedVerbatimString(this.Declaration.Start.Source.Substring(startIndex, sourceIndex1 - startIndex)));
          else
            stringBuilder.Append(StringUtility.GetUnescapedRegularString(this.Declaration.Start.Source.Substring(startIndex, sourceIndex1 - startIndex)));
        }
        stringBuilder.Append("\"");
        return stringBuilder.ToString();
      }
      return flag ? "\"" + StringUtility.GetUnescapedVerbatimString(this.Declaration.Start.Source.Substring(this.Declaration.Start.SourceIndex + this.Declaration.Start.SourceLength, this.Declaration.End.SourceIndex - (this.Declaration.Start.SourceIndex + this.Declaration.Start.SourceLength))) + "\"" : "\"" + StringUtility.GetUnescapedRegularString(this.Declaration.Start.Source.Substring(this.Declaration.Start.SourceIndex + this.Declaration.Start.SourceLength, this.Declaration.End.SourceIndex - (this.Declaration.Start.SourceIndex + this.Declaration.Start.SourceLength))) + "\"";
    }
  }
}

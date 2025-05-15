// Decompiled with JetBrains decompiler
// Type: System.Parsing.Intellisense.ParameterCollectionNode
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Text;

namespace System.Parsing.Intellisense
{
  public class ParameterCollectionNode : CollectionNode
  {
    public override string GetEvaluatedValue() => this.GetRepresentationForEvaluation();

    public override string GetRepresentationForEvaluation()
    {
      if (this.Items == null)
        return TokenKind.LeftParenthesis.Identifier + TokenKind.RightParenthesis.Identifier;
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.Append(TokenKind.LeftParenthesis.Identifier);
      for (int index = 0; index < this.Items.Length; ++index)
      {
        if (index > 0)
          stringBuilder.Append(TokenKind.Comma.Identifier);
        stringBuilder.Append(this.Items[index].GetEvaluatedValue());
      }
      stringBuilder.Append(TokenKind.RightParenthesis.Identifier);
      return stringBuilder.ToString();
    }
  }
}

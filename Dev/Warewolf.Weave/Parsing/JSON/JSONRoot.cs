// Decompiled with JetBrains decompiler
// Type: System.Parsing.JSON.JSONRoot
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

namespace System.Parsing.JSON
{
  public class JSONRoot : CollectionNode
  {
    public JSONRoot(TokenPair declaration, JSONNode[] items)
    {
      this.Declaration = declaration;
      this.Identifier = new TokenPair(declaration.Start);
      this.Items = items;
    }
  }
}

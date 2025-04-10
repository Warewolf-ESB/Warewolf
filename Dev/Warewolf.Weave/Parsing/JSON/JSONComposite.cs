// Decompiled with JetBrains decompiler
// Type: System.Parsing.JSON.JSONComposite
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

namespace System.Parsing.JSON
{
  public class JSONComposite : CollectionNode
  {
    public JSONComposite(Token identifier, Token end)
    {
      this.Declaration = new TokenPair(identifier, end);
      this.Identifier = new TokenPair(identifier);
    }
  }
}

// Decompiled with JetBrains decompiler
// Type: System.Parsing.JSON.JSONScalar
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

namespace System.Parsing.JSON
{
  public class JSONScalar : JSONNode
  {
    private Token _value;

    public Token Value => this._value;

    public JSONScalar(Token identifier, Token value)
    {
      this.Declaration = new TokenPair(identifier, value);
      this.Identifier = new TokenPair(identifier);
      this._value = value;
    }
  }
}

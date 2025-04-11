// Decompiled with JetBrains decompiler
// Type: System.Parsing.Tokenization.TokenDefinition
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Collections.Generic;

namespace System.Parsing.Tokenization
{
  public abstract class TokenDefinition
  {
    private static Dictionary<Type, int> _typeSerialCounters = new Dictionary<Type, int>();
    private string _name;
    private string _identifier;
    protected int _serial;

    public string Name => this._name;

    public string Identifier => this._identifier;

    public int Serial => this._serial;

    public virtual bool IsWhitespace => false;

    public virtual bool IsUnknown => false;

    public virtual bool IsEndOfFile => false;

    public virtual bool IsKeyword => false;

    private static int AcquireSerial(Type type)
    {
      int num1;
      if (!TokenDefinition._typeSerialCounters.TryGetValue(type, out num1))
        num1 = 0;
      int num2 = num1;
      int num3 = num2 + 1;
      int num4 = num2;
      TokenDefinition._typeSerialCounters[type] = num3;
      return num4;
    }

    public static int GetTotalDefinitionsOfType(Type type)
    {
      if (type == (Type) null)
        throw new ArgumentNullException(nameof (type));
      int num;
      return !TokenDefinition._typeSerialCounters.TryGetValue(type, out num) ? 0 : num;
    }

    public TokenDefinition(string name, string identifier)
    {
      this._name = name;
      this._identifier = identifier;
      this._serial = TokenDefinition.AcquireSerial(this.GetType());
    }

    public override string ToString() => this._name;
  }
}

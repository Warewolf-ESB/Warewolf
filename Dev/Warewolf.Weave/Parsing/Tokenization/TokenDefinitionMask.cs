// Decompiled with JetBrains decompiler
// Type: System.Parsing.Tokenization.TokenDefinitionMask
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

namespace System.Parsing.Tokenization
{
  internal sealed class TokenDefinitionMask
  {
    private int[] _buckets;
    private int _offset;
    private int _length;

    public bool this[TokenDefinition index]
    {
      get => this.GetValue(index);
      set => this.SetValue(index, value);
    }

    public TokenDefinitionMask(params TokenDefinition[] definitions)
    {
      int num1 = 0;
      if (definitions != null && definitions.Length != 0)
      {
        int num2 = int.MaxValue;
        int num3 = int.MinValue;
        foreach (TokenDefinition definition in definitions)
        {
          if (num2 > definition.Serial)
            num2 = definition.Serial;
          if (num3 < definition.Serial)
            num3 = definition.Serial;
        }
        if (num3 != int.MinValue && num2 != int.MaxValue)
        {
          this._offset = num2 >> 5 << 5;
          num1 = num3 - this._offset + 1;
        }
      }
      this._buckets = new int[num1 + 31 >> 5];
      this._length = num1;
      if (definitions == null || definitions.Length == 0)
        return;
      for (int index1 = 0; index1 < definitions.Length; ++index1)
      {
        int num4 = definitions[index1].Serial - this._offset;
        int index2 = num4 >> 5;
        this._buckets[index2] |= 1 << num4 - (index2 << 5);
      }
    }

    private bool GetValue(TokenDefinition def)
    {
      if (def == null)
        throw new ArgumentNullException("index");
      int num = def.Serial - this._offset;
      if (num < 0 || num >= this._length)
        return false;
      int index = num >> 5;
      return (this._buckets[index] & 1 << num - (index << 5)) != 0;
    }

    private void SetValue(TokenDefinition def, bool value)
    {
      if (def == null)
        throw new ArgumentNullException("index");
      int num1 = def.Serial - this._offset;
      if (num1 < 0)
      {
        int num2 = def.Serial >> 5;
        int num3 = num2 << 5;
        int num4 = this._length - 1 + this._offset - num3 + 1;
        int num5 = (this._offset >> 5) - num2;
        int[] numArray = new int[num4 + 31 >> 5];
        for (int index = 0; index < this._buckets.Length; ++index)
          numArray[index + num5] = this._buckets[index];
        this._offset = num3;
        this._buckets = numArray;
        this._length = num4;
        num1 = def.Serial - this._offset;
      }
      else if (num1 >= this._length)
      {
        int num6 = num1 + 1;
        int[] destinationArray = new int[num6 + 31 >> 5];
        Array.Copy((Array) this._buckets, 0, (Array) destinationArray, 0, this._buckets.Length);
        this._buckets = destinationArray;
        this._length = num6;
      }
      int index1 = num1 >> 5;
      if (value)
        this._buckets[index1] |= 1 << num1 - (index1 << 5);
      else
        this._buckets[index1] &= ~(1 << num1 - (index1 << 5));
    }
  }
}

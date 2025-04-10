// Decompiled with JetBrains decompiler
// Type: System.Parsing.Tokenization.TokenizationHandlerExecutionStore
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Collections.Generic;

namespace System.Parsing.Tokenization
{
  public class TokenizationHandlerExecutionStore
  {
    private BitVector _state;

    public BitVector State
    {
      get => this._state;
      set => this._state = value;
    }

    public bool this[int index]
    {
      get => this._state[index];
      set => this._state[index] = value;
    }
  }
}

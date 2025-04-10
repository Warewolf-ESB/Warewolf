// Decompiled with JetBrains decompiler
// Type: System.Emission.DesignatingScope
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Collections.Generic;

namespace System.Emission
{
  internal sealed class DesignatingScope : IDesignatingScope
  {
    private string _name;
    private IDesignatingScope _parentScope;
    private Dictionary<string, int> _suggestionHistory;

    public string Name => this._name;

    public IDesignatingScope ParentScope => this._parentScope;

    public DesignatingScope() => this._suggestionHistory = new Dictionary<string, int>();

    private DesignatingScope(string name, IDesignatingScope parentScope)
    {
      this._name = name;
      this._parentScope = parentScope;
      this._suggestionHistory = new Dictionary<string, int>();
    }

    public string GenerateDesignation(string suggestedDesignation)
    {
      if (string.IsNullOrEmpty(suggestedDesignation))
        throw new ArgumentException("Suggested designation cannot be a null or empty string.", nameof (suggestedDesignation));
      int num;
      if (this._suggestionHistory.TryGetValue(suggestedDesignation, out num))
        return suggestedDesignation + "_" + (this._suggestionHistory[suggestedDesignation] = num + 1).ToString();
      this._suggestionHistory.Add(suggestedDesignation, 0);
      return suggestedDesignation;
    }

    public IDesignatingScope GenerateChildScope(string suggestedDesignation) => (IDesignatingScope) new DesignatingScope(this.GenerateDesignation(suggestedDesignation), (IDesignatingScope) this);
  }
}

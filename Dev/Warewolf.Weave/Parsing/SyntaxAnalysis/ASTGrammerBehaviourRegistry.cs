// Decompiled with JetBrains decompiler
// Type: System.Parsing.SyntaxAnalysis.ASTGrammerBehaviourRegistry
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Collections.Generic;
using System.Data;
using System.Parsing.Tokenization;

namespace System.Parsing.SyntaxAnalysis
{
  public sealed class ASTGrammerBehaviourRegistry
  {
    private List<int>[] _definitionTriggers;
    private int _length;
    private int _grammerRegistryIndex;
    private HashSet<TokenDefinition> _unaryDefinitions;
    private HashSet<TokenDefinition> _keywordDefinitions;
    private Dictionary<Type, List<int>> _nodeTriggers;
    private Type _baseNodeType;

    internal List<int>[] DefinitionTriggers => this._definitionTriggers;

    internal Dictionary<Type, List<int>> NodeTriggers => this._nodeTriggers;

    internal int GrammerRegistryIndex
    {
      get => this._grammerRegistryIndex;
      set => this._grammerRegistryIndex = value;
    }

    internal HashSet<TokenDefinition> UnaryDefinitions => this._unaryDefinitions;

    internal HashSet<TokenDefinition> KeywordDefinitions => this._keywordDefinitions;

    internal ASTGrammerBehaviourRegistry(int maxUniqueDefinitions, Type baseNodeType)
    {
      this._grammerRegistryIndex = -1;
      this._definitionTriggers = new List<int>[this._length = maxUniqueDefinitions];
      this._unaryDefinitions = new HashSet<TokenDefinition>();
      this._keywordDefinitions = new HashSet<TokenDefinition>();
      this._baseNodeType = baseNodeType;
      this._nodeTriggers = new Dictionary<Type, List<int>>();
    }

    public void Register(TokenDefinition triggeringDefinition)
    {
      if (triggeringDefinition == null)
        throw new ArgumentNullException(nameof (triggeringDefinition));
      if (this._grammerRegistryIndex == -1)
        throw new ReadOnlyException("Triggers may only be registered from within AbstractSyntaxTreeGrammer.OnRegisterTriggers");
      int serial = triggeringDefinition.Serial;
      (this._definitionTriggers[serial] ?? (this._definitionTriggers[serial] = new List<int>())).Add(this._grammerRegistryIndex);
      if (triggeringDefinition.IsUnknown || triggeringDefinition.IsWhitespace || triggeringDefinition.IsEndOfFile || string.IsNullOrEmpty(triggeringDefinition.Identifier))
        return;
      if (triggeringDefinition.IsKeyword)
        this._keywordDefinitions.Add(triggeringDefinition);
      else
        this._unaryDefinitions.Add(triggeringDefinition);
    }

    public void Register(Type triggeringNodeType)
    {
      if (triggeringNodeType == (Type) null)
        throw new ArgumentNullException(nameof (triggeringNodeType));
      if (!this._baseNodeType.IsAssignableFrom(triggeringNodeType))
        throw new ArgumentException("triggeringNodeType must be a descendant of \"" + this._baseNodeType.Name + "\"");
      if (this._grammerRegistryIndex == -1)
        throw new ReadOnlyException("Triggers may only be registered from within AbstractSyntaxTreeGrammer.OnRegisterTriggers");
      List<int> intList;
      if (!this._nodeTriggers.TryGetValue(triggeringNodeType, out intList))
        this._nodeTriggers.Add(triggeringNodeType, intList = new List<int>());
      intList.Add(this._grammerRegistryIndex);
    }
  }
}

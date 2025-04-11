// Decompiled with JetBrains decompiler
// Type: System.Emission.EmissionModule
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;

namespace System.Emission
{
  public sealed class EmissionModule : BaseDisposable
  {
    private ModuleBuilder _builder;
    private IDesignatingScope _designatingScope;
    private Dictionary<EmissionCacheKey, Type> _typeCache;
    private ReaderWriterLockSlim _cacheLock;
    private string _dynamicAssemblyName;

    internal ReaderWriterLockSlim CacheLock => this._cacheLock;

    internal string DynamicAssemblyName => this._dynamicAssemblyName;

    public IDesignatingScope DesignatingScope => this._designatingScope;

    internal EmissionModule(
      ModuleBuilder builder,
      IDesignatingScope designatingScope,
      string dynamicAssemblyName)
    {
      this._builder = builder;
      this._designatingScope = designatingScope;
      this._dynamicAssemblyName = dynamicAssemblyName;
      this._typeCache = new Dictionary<EmissionCacheKey, Type>();
      this._cacheLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
    }

    internal TypeBuilder DefineType(string name, TypeAttributes flags) => this._builder.DefineType(name, flags);

    internal Type GetFromCache(EmissionCacheKey key)
    {
      Type type;
      return !this._typeCache.TryGetValue(key, out type) ? (Type) null : type;
    }

    internal void RegisterInCache(EmissionCacheKey key, Type type) => this._typeCache[key] = type;

    protected override void OnDispose()
    {
      this._builder = (ModuleBuilder) null;
      this._designatingScope = (IDesignatingScope) null;
    }
  }
}

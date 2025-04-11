// Decompiled with JetBrains decompiler
// Type: System.Collections.Generic.KeyedValueKeyCollectionDebugView`2
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Diagnostics;

namespace System.Collections.Generic
{
  internal sealed class KeyedValueKeyCollectionDebugView<K, T>
  {
    private ICollection<K> _collection;

    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    public K[] Items
    {
      get
      {
        K[] array = new K[this._collection.Count];
        this._collection.CopyTo(array, 0);
        return array;
      }
    }

    public KeyedValueKeyCollectionDebugView(ICollection<K> collection) => this._collection = collection != null ? collection : throw new ArgumentNullException(nameof (collection));
  }
}

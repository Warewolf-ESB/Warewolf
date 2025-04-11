// Decompiled with JetBrains decompiler
// Type: System.Collections.Generic.OneDimensionCollectionDebugView`1
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Diagnostics;

namespace System.Collections.Generic
{
  internal sealed class OneDimensionCollectionDebugView<T>
  {
    private ICollection<T> _collection;

    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    public T[] Items
    {
      get
      {
        T[] array = new T[this._collection.Count];
        this._collection.CopyTo(array, 0);
        return array;
      }
    }

    public OneDimensionCollectionDebugView(ICollection<T> collection) => this._collection = collection != null ? collection : throw new ArgumentNullException(nameof (collection));
  }
}

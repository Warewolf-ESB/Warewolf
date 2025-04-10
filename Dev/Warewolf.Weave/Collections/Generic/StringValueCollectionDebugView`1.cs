// Decompiled with JetBrains decompiler
// Type: System.Collections.Generic.StringValueCollectionDebugView`1
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Diagnostics;

namespace System.Collections.Generic
{
  internal sealed class StringValueCollectionDebugView<V>
  {
    private IDictionary<string, V> _dictionary;

    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    public KeyValuePair<string, V>[] Items
    {
      get
      {
        KeyValuePair<string, V>[] array = new KeyValuePair<string, V>[this._dictionary.Count];
        this._dictionary.CopyTo(array, 0);
        return array;
      }
    }

    public StringValueCollectionDebugView(IDictionary<string, V> dictionary) => this._dictionary = dictionary != null ? dictionary : throw new ArgumentNullException(nameof (dictionary));
  }
}

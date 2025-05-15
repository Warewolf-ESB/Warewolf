// Decompiled with JetBrains decompiler
// Type: System.Emission.Meta.TypeElementCollection`1
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Collections;
using System.Collections.Generic;

namespace System.Emission.Meta
{
  internal sealed class TypeElementCollection<TElement> : 
    ICollection<TElement>,
    IEnumerable<TElement>,
    IEnumerable
    where TElement : MetaTypeElement, IEquatable<TElement>
  {
    private readonly ICollection<TElement> _items = (ICollection<TElement>) new List<TElement>();

    public int Count => this._items.Count;

    bool ICollection<TElement>.IsReadOnly => false;

    public void Add(TElement item)
    {
      if (!item.CanBeImplementedExplicitly)
      {
        this._items.Add(item);
      }
      else
      {
        if (this.Contains(item))
        {
          item.SwitchToExplicitImplementation();
          if (this.Contains(item))
            throw new InvalidOperationException("Duplicate element: " + item?.ToString());
        }
        this._items.Add(item);
      }
    }

    public bool Contains(TElement item)
    {
      foreach (TElement element in (IEnumerable<TElement>) this._items)
      {
        if (element.Equals(item))
          return true;
      }
      return false;
    }

    public IEnumerator<TElement> GetEnumerator() => this._items.GetEnumerator();

    void ICollection<TElement>.Clear() => throw new NotSupportedException();

    void ICollection<TElement>.CopyTo(TElement[] array, int arrayIndex) => throw new NotSupportedException();

    bool ICollection<TElement>.Remove(TElement item) => throw new NotSupportedException();

    IEnumerator IEnumerable.GetEnumerator() => (IEnumerator) this.GetEnumerator();
  }
}

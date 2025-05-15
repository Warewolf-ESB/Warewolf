// Decompiled with JetBrains decompiler
// Type: System.Collections.Generic.StringValueInt32CollectionImplementation
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Runtime.CompilerServices;

namespace System.Collections.Generic
{
  internal sealed class StringValueInt32CollectionImplementation : 
    StringValueCollectionImplementation
  {
    private static StringValueInt32CollectionImplementation.StringTier[] _emptyArray = new StringValueInt32CollectionImplementation.StringTier[0];
    private StringValueInt32CollectionImplementation.StringTier _firstTier;
    private StringValueInt32CollectionImplementation.StringTier[] _tiers;
    private int _released;
    private int _capacity;
    private int _size;

    internal override int Size => this._tiers.Length;

    internal override int MaxCapacity => 2147483639;

    public StringValueInt32CollectionImplementation() => this._tiers = StringValueInt32CollectionImplementation._emptyArray;

    public StringValueInt32CollectionImplementation(int capacity) => this._tiers = new StringValueInt32CollectionImplementation.StringTier[this._capacity = capacity];

    public StringValueInt32CollectionImplementation(StringValueInt16CollectionImplementation from)
    {
      this._released = from._released;
      this._capacity = from._capacity;
      this._size = from._size;
      if (from._firstTier != null)
        this._firstTier = new StringValueInt32CollectionImplementation.StringTier(from._firstTier);
      if (from._tiers != null && from._tiers.Length != 0)
      {
        this._tiers = new StringValueInt32CollectionImplementation.StringTier[from._tiers.Length];
        for (int index = 0; index < this._tiers.Length; ++index)
        {
          if (from._tiers[index] != null)
            this._tiers[index] = from._tiers[index] != from._firstTier ? new StringValueInt32CollectionImplementation.StringTier(from._tiers[index]) : this._firstTier;
        }
      }
      else
        this._tiers = StringValueInt32CollectionImplementation._emptyArray;
    }

    internal override string GetEntryKey(int index, int tier, short entryIndex)
    {
      int num = 1024;
      char[] charStorage = StringValueCollectionImplementation.CharStorage;
      string empty = string.Empty;
      StringValueInt32CollectionImplementation.StringTier tier1 = this._tiers[tier];
      index = num;
      charStorage[--index] = (char) entryIndex;
      int parentIndex = tier1.ParentIndex;
      while (tier1.Parent != -1)
      {
        charStorage[--index] = (char) parentIndex;
        if (index == 0)
        {
          empty += new string(charStorage);
          index = num;
        }
        tier1 = this._tiers[tier1.Parent];
        parentIndex = tier1.ParentIndex;
      }
      if (index != num)
        empty += new string(charStorage, index, num - index);
      return empty;
    }

    private void SetCapacity(int value)
    {
      if (value == this._capacity)
        return;
      if (value < this._size)
        throw new ArgumentOutOfRangeException(nameof (value));
      if (value > 0)
      {
        this._capacity = value;
        StringValueInt32CollectionImplementation.StringTier[] destinationArray = new StringValueInt32CollectionImplementation.StringTier[value];
        if (this._size > 0)
          Array.Copy((Array) this._tiers, 0, (Array) destinationArray, 0, this._size);
        this._tiers = destinationArray;
      }
      else
      {
        this._tiers = StringValueInt32CollectionImplementation._emptyArray;
        this._capacity = 0;
      }
    }

    internal override unsafe bool Add(
      string key,
      bool overwrite,
      int itemCount,
      out int resultTier,
      out int resultIndex)
    {
      if (this._firstTier == null)
      {
        if (this._size == this._capacity)
          this.EnsureCapacity(this._size + 1);
        this._tiers[this._size] = this._firstTier = new StringValueInt32CollectionImplementation.StringTier(-1, -1, this._size);
        ++this._size;
      }
      StringValueInt32CollectionImplementation.StringTier stringTier1 = this._firstTier;
      StringValueInt32CollectionImplementation.StringTier stringTier2 = (StringValueInt32CollectionImplementation.StringTier) null;
      int index1 = key.Length - 1;
      int index2 = 0;
      IntPtr num1;
      if (key == null)
      {
        num1 = IntPtr.Zero;
      }
      else
      {
        fixed (char* chPtr = key)
          num1 = (IntPtr) chPtr;
      }
      char* chPtr1 = (char*) num1;
      for (int index3 = 0; index3 < index1; ++index3)
      {
        int num2 = (int) chPtr1[index3];
        int index4;
        if ((index4 = stringTier1[num2]) == 0)
        {
          index2 = this.EnsureSlot(index2);
          index4 = stringTier1[num2] = index2;
          this._tiers[index2] = new StringValueInt32CollectionImplementation.StringTier(stringTier1.Index, num2, index2);
        }
        else if (index4 < 0)
        {
          int num3 = index4;
          index2 = this.EnsureSlot(index2);
          index4 = stringTier1[num2] = index2;
          (this._tiers[index2] = new StringValueInt32CollectionImplementation.StringTier(stringTier1.Index, num2, index2)).Item = num3;
        }
        stringTier1 = this._tiers[index4];
      }
      int index5;
      int index6;
      if ((index6 = stringTier1[index5 = (int) chPtr1[index1]]) != 0)
      {
        if (index6 < 0 || (stringTier2 = this._tiers[index6]).Item != 0)
        {
          if (overwrite)
          {
            resultIndex = (index6 < 0 ? index6 : stringTier2.Item) * -1 - 1;
            resultTier = -1;
            return true;
          }
          resultIndex = 0;
          resultTier = -1;
          return false;
        }
        stringTier1[index5] = index6;
        stringTier2.Item = -(itemCount + 1);
      }
      else
        stringTier1[index5] = -(itemCount + 1);
      resultIndex = index5;
      resultTier = stringTier1.Index;
      return true;
    }

    internal override int BeginRemove(string key, int entryTier, short entryIndex)
    {
      StringValueInt32CollectionImplementation.StringTier tier1 = this._tiers[entryTier];
      int index = tier1[(int) entryIndex];
      int num;
      if (index < 0)
      {
        num = index * -1 - 1;
        tier1[(int) entryIndex] = 0;
        StringValueInt32CollectionImplementation.StringTier tier2;
        for (StringValueInt32CollectionImplementation.StringTier stringTier = tier1; stringTier.Parent != -1 && stringTier.Routes == 0; stringTier = tier2)
        {
          tier2 = this._tiers[stringTier.Parent];
          if (stringTier.Item != 0)
          {
            tier2[stringTier.ParentIndex] = stringTier.Item;
            stringTier.Item = 0;
          }
          else
            tier2[stringTier.ParentIndex] = 0;
          this._tiers[stringTier.Index] = (StringValueInt32CollectionImplementation.StringTier) null;
          ++this._released;
        }
      }
      else
      {
        StringValueInt32CollectionImplementation.StringTier tier3 = this._tiers[index];
        num = tier3.Item * -1 - 1;
        tier3.Item = 0;
      }
      return num;
    }

    internal override void EndRemove(
      int entryTier,
      short entryIndex,
      int itemIndex,
      int lastItemIndex)
    {
      if (entryTier != -1)
      {
        StringValueInt32CollectionImplementation.StringTier tier = this._tiers[entryTier];
        int index = tier[(int) entryIndex];
        if (index < 0)
          tier[(int) entryIndex] = -(itemIndex + 1);
        else
          this._tiers[index].Item = -(itemIndex + 1);
      }
      if (lastItemIndex != 0)
        return;
      this._size = this._capacity = this._released = 0;
      this._tiers = StringValueInt32CollectionImplementation._emptyArray;
      this._firstTier = (StringValueInt32CollectionImplementation.StringTier) null;
    }

    internal override void Clear()
    {
      this._size = this._capacity = this._released = 0;
      this._tiers = StringValueInt32CollectionImplementation._emptyArray;
      this._firstTier = (StringValueInt32CollectionImplementation.StringTier) null;
    }

    internal override unsafe int IndexOfEntryTier(string key, int index, int count)
    {
      int index1 = 0;
      IntPtr num1;
      if (key == null)
      {
        num1 = IntPtr.Zero;
      }
      else
      {
        fixed (char* chPtr = key)
          num1 = (IntPtr) chPtr;
      }
      char* chPtr1 = (char*) (num1 + index * 2);
      int num2 = 0;
      StringValueInt32CollectionImplementation.StringTier stringTier = this._firstTier;
      for (int index2 = 0; index2 < count; ++index2)
      {
        num2 = index1;
        int index3 = (int) chPtr1[index2];
        if ((index1 = stringTier[index3]) == 0)
          return -1;
        if (index2 + 1 == count)
          return index1;
        if (index1 < 0)
          return -1;
        stringTier = this._tiers[index1];
      }
      return -1;
    }

    internal override unsafe int IndexOfEntry(
      int tierIndex,
      string key,
      int index,
      int count,
      int itemCount)
    {
      if (tierIndex == 0)
        return this.IndexOfEntry(key, index, count, itemCount);
      IntPtr num1;
      if (key == null)
      {
        num1 = IntPtr.Zero;
      }
      else
      {
        fixed (char* chPtr = key)
          num1 = (IntPtr) chPtr;
      }
      char* chPtr1 = (char*) (num1 + index * 2);
      int num2 = 0;
      StringValueInt32CollectionImplementation.StringTier tier = this._tiers[tierIndex];
      for (int index1 = 0; index1 < count; ++index1)
      {
        num2 = tierIndex;
        int index2 = (int) chPtr1[index1];
        if ((tierIndex = tier[index2]) == 0)
          return -1;
        if (tierIndex < 0)
        {
          if (index1 + 1 != count)
            return -1;
          tierIndex *= -1;
          tierIndex += -1;
          return tierIndex < itemCount ? tierIndex : -1;
        }
        tier = this._tiers[tierIndex];
      }
      if (tier.Item != 0 && tier != this._firstTier && tier.Parent == num2)
        return tier.Item * -1 - 1;
      return -1;
    }

    internal override unsafe int IndexOfEntry(string key, int index, int count, int itemCount)
    {
      int index1 = 0;
      IntPtr num1;
      if (key == null)
      {
        num1 = IntPtr.Zero;
      }
      else
      {
        fixed (char* chPtr = key)
          num1 = (IntPtr) chPtr;
      }
      char* chPtr1 = (char*) (num1 + index * 2);
      int num2 = 0;
      StringValueInt32CollectionImplementation.StringTier stringTier = this._firstTier;
      for (int index2 = 0; index2 < count; ++index2)
      {
        num2 = index1;
        int index3 = (int) chPtr1[index2];
        if ((index1 = stringTier[index3]) == 0)
          return -1;
        if (index1 < 0)
        {
          if (index2 + 1 != count)
            return -1;
          int num3 = index1 * -1 - 1;
          return num3 < itemCount ? num3 : -1;
        }
        stringTier = this._tiers[index1];
      }
      if (stringTier.Item != 0 && stringTier != this._firstTier && stringTier.Parent == num2)
        return stringTier.Item * -1 - 1;
      return -1;
    }

    internal override unsafe int IndexOfEntry(
      string key,
      int index,
      char seperator,
      int itemCount,
      out int consumed)
    {
      consumed = 0;
      int num1 = key.Length - index;
      int index1 = 0;
      IntPtr num2;
      if (key == null)
      {
        num2 = IntPtr.Zero;
      }
      else
      {
        fixed (char* chPtr = key)
          num2 = (IntPtr) chPtr;
      }
      char* chPtr1 = (char*) (num2 + index * 2);
      int num3 = 0;
      StringValueInt32CollectionImplementation.StringTier stringTier = this._firstTier;
      for (int index2 = 0; index2 < num1; ++index2)
      {
        num3 = index1;
        int index3 = (int) chPtr1[index2];
        ++consumed;
        if (index3 != (int) seperator)
        {
          if ((index1 = stringTier[index3]) == 0)
            return -1;
          if (index1 < 0)
          {
            if (index2 + 1 != num1)
            {
              if ((int) chPtr1[index2 + 1] != (int) seperator)
                return -1;
              ++consumed;
            }
            int num4 = index1 * -1 - 1;
            return num4 < itemCount ? num4 : -1;
          }
          stringTier = this._tiers[index1];
        }
        else
          break;
      }
      if (stringTier.Item != 0 && stringTier != this._firstTier && stringTier.Parent == num3)
        return stringTier.Item * -1 - 1;
      return -1;
    }

    internal override void EnsureCapacity(int min)
    {
      if (this._capacity >= min)
        return;
      int num = this._capacity == 0 ? 4 : this._capacity * 2;
      if (num < min)
        num = min;
      this.SetCapacity(num);
    }

    internal override int EnsureSlot(int start)
    {
      if (this._released == 0)
      {
        if (this._size == this._capacity)
          this.EnsureCapacity(this._size + 1);
        return this._size++;
      }
      for (int index = start; index < this._size; ++index)
      {
        if (this._tiers[index] == null)
        {
          --this._released;
          return index;
        }
      }
      throw new InvalidOperationException();
    }

    private sealed class StringTier
    {
      private static int[] EmptyPartition = new int[33];
      private int _item;
      private int _parent;
      private int _parentIndex;
      private int _index;
      private int _terminators;
      private int _routes;
      private int[][] _table;
      private int _assigned;

      public int Item
      {
        get => this._item;
        set => this._item = value;
      }

      public int Parent
      {
        get => this._parent;
        set => this._parent = value;
      }

      public int ParentIndex
      {
        get => this._parentIndex;
        set => this._parentIndex = value;
      }

      public int Index
      {
        get => this._index;
        set => this._index = value;
      }

      public int Terminators => this._terminators;

      public int Routes => this._routes;

      [IndexerName("TableRoutes")]
      public int this[int index]
      {
        get => this._table[index >> 5][index & 31];
        set => this.SetValue(index, value);
      }

      public StringTier(int parent, int parentIndex, int index)
      {
        this._parent = parent;
        this._parentIndex = parentIndex;
        this._index = index;
        this._table = new int[8][];
        for (int index1 = 0; index1 < this._table.Length; ++index1)
          this._table[index1] = StringValueInt32CollectionImplementation.StringTier.EmptyPartition;
      }

      public StringTier(
        StringValueInt16CollectionImplementation.StringTier source)
      {
        this._item = (int) source.Item;
        this._parent = (int) source.Parent;
        this._parentIndex = (int) source.ParentIndex;
        this._index = (int) source.Index;
        this._terminators = (int) source.Terminators;
        this._routes = (int) source.Routes;
        this._assigned = source.Assigned;
        this._table = new int[8][];
        for (int index1 = 0; index1 < this._table.Length; ++index1)
        {
          if (source.Table[index1] == StringValueInt16CollectionImplementation.StringTier.EmptyPartition)
          {
            this._table[index1] = StringValueInt32CollectionImplementation.StringTier.EmptyPartition;
          }
          else
          {
            short[] numArray1 = source.Table[index1];
            int[] numArray2 = new int[33];
            for (int index2 = 0; index2 < numArray1.Length; ++index2)
              numArray2[index2] = (int) numArray1[index2];
            this._table[index1] = numArray2;
          }
        }
      }

      public int[] GetPartition(int partition) => (this._assigned & 1 << partition) == 0 ? (int[]) null : this._table[partition];

      private void SetValue(int index, int value)
      {
        int index1 = index >> 5;
        index &= 31;
        int num = 0;
        int[] numArray;
        if ((this._assigned & 1 << index1) == 0)
        {
          if (value == 0)
            return;
          this._assigned |= 1 << index1;
          numArray = this._table[index1] = new int[33];
        }
        else
        {
          num = (numArray = this._table[index1])[index];
          if (num == value)
            return;
        }
        if (value == 0)
        {
          if (num < 0)
            --this._terminators;
          --this._routes;
          if (--numArray[32] == 0)
          {
            this._table[index1] = StringValueInt32CollectionImplementation.StringTier.EmptyPartition;
            this._assigned &= ~(1 << index1);
          }
          else
            numArray[index] = value;
        }
        else
        {
          if (num != 0)
          {
            if (num < 0)
            {
              if (value > 0)
                --this._terminators;
            }
            else if (value < 0)
              ++this._terminators;
          }
          else
          {
            if (value < 0)
              ++this._terminators;
            ++numArray[32];
            ++this._routes;
          }
          numArray[index] = value;
        }
      }
    }
  }
}

// Decompiled with JetBrains decompiler
// Type: System.Collections.Generic.StringValueInt16CollectionImplementation
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Runtime.CompilerServices;

namespace System.Collections.Generic
{
  internal sealed class StringValueInt16CollectionImplementation : 
    StringValueCollectionImplementation
  {
    private static StringValueInt16CollectionImplementation.StringTier[] _emptyArray = new StringValueInt16CollectionImplementation.StringTier[0];
    internal StringValueInt16CollectionImplementation.StringTier _firstTier;
    internal StringValueInt16CollectionImplementation.StringTier[] _tiers;
    internal int _released;
    internal int _capacity;
    internal int _size;

    internal override int Size => this._tiers.Length;

    internal override int MaxCapacity => 32759;

    public StringValueInt16CollectionImplementation() => this._tiers = StringValueInt16CollectionImplementation._emptyArray;

    public StringValueInt16CollectionImplementation(int capacity) => this._tiers = new StringValueInt16CollectionImplementation.StringTier[this._capacity = capacity];

    internal override string GetEntryKey(int index, int tier, short entryIndex)
    {
      int num = 1024;
      char[] charStorage = StringValueCollectionImplementation.CharStorage;
      string empty = string.Empty;
      StringValueInt16CollectionImplementation.StringTier tier1 = this._tiers[tier];
      index = num;
      charStorage[--index] = (char) entryIndex;
      int parentIndex = (int) tier1.ParentIndex;
      while (tier1.Parent != (short) -1)
      {
        charStorage[--index] = (char) parentIndex;
        if (index == 0)
        {
          empty += new string(charStorage);
          index = num;
        }
        tier1 = this._tiers[(int) tier1.Parent];
        parentIndex = (int) tier1.ParentIndex;
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
        StringValueInt16CollectionImplementation.StringTier[] destinationArray = new StringValueInt16CollectionImplementation.StringTier[value];
        if (this._size > 0)
          Array.Copy((Array) this._tiers, 0, (Array) destinationArray, 0, this._size);
        this._tiers = destinationArray;
      }
      else
      {
        this._tiers = StringValueInt16CollectionImplementation._emptyArray;
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
        this._tiers[this._size] = this._firstTier = new StringValueInt16CollectionImplementation.StringTier((short) -1, (short) -1, this._size);
        ++this._size;
      }
      StringValueInt16CollectionImplementation.StringTier stringTier1 = this._firstTier;
      StringValueInt16CollectionImplementation.StringTier stringTier2 = (StringValueInt16CollectionImplementation.StringTier) null;
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
        short index4;
        if ((index4 = stringTier1[num2]) == (short) 0)
        {
          index2 = this.EnsureSlot(index2);
          index4 = stringTier1[num2] = (short) index2;
          this._tiers[index2] = new StringValueInt16CollectionImplementation.StringTier(stringTier1.Index, (short) num2, index2);
        }
        else if (index4 < (short) 0)
        {
          short num3 = index4;
          index2 = this.EnsureSlot(index2);
          index4 = stringTier1[num2] = (short) index2;
          (this._tiers[index2] = new StringValueInt16CollectionImplementation.StringTier(stringTier1.Index, (short) num2, index2)).Item = num3;
        }
        stringTier1 = this._tiers[(int) index4];
      }
      int index5;
      short index6;
      if ((index6 = stringTier1[index5 = (int) chPtr1[index1]]) != (short) 0)
      {
        if (index6 < (short) 0 || (stringTier2 = this._tiers[(int) index6]).Item != (short) 0)
        {
          if (overwrite)
          {
            resultIndex = (index6 < (short) 0 ? (int) index6 : (int) stringTier2.Item) * -1 - 1;
            resultTier = -1;
            return true;
          }
          resultIndex = 0;
          resultTier = -1;
          return false;
        }
        stringTier1[index5] = index6;
        stringTier2.Item = (short) -(itemCount + 1);
      }
      else
        stringTier1[index5] = (short) -(itemCount + 1);
      resultIndex = (int) (short) index5;
      resultTier = (int) stringTier1.Index;
      return true;
    }

    internal override int BeginRemove(string key, int entryTier, short entryIndex)
    {
      StringValueInt16CollectionImplementation.StringTier tier1 = this._tiers[entryTier];
      short index = tier1[(int) entryIndex];
      short num;
      if (index < (short) 0)
      {
        num = (short) ((int) index * -1 - 1);
        tier1[(int) entryIndex] = (short) 0;
        StringValueInt16CollectionImplementation.StringTier tier2;
        for (StringValueInt16CollectionImplementation.StringTier stringTier = tier1; stringTier.Parent != (short) -1 && stringTier.Routes == (short) 0; stringTier = tier2)
        {
          tier2 = this._tiers[(int) stringTier.Parent];
          if (stringTier.Item != (short) 0)
          {
            tier2[(int) stringTier.ParentIndex] = stringTier.Item;
            stringTier.Item = (short) 0;
          }
          else
            tier2[(int) stringTier.ParentIndex] = (short) 0;
          this._tiers[(int) stringTier.Index] = (StringValueInt16CollectionImplementation.StringTier) null;
          ++this._released;
        }
      }
      else
      {
        StringValueInt16CollectionImplementation.StringTier tier3 = this._tiers[(int) index];
        num = (short) ((int) tier3.Item * -1 - 1);
        tier3.Item = (short) 0;
      }
      return (int) num;
    }

    internal override void EndRemove(
      int entryTier,
      short entryIndex,
      int itemIndex,
      int lastItemIndex)
    {
      if (entryTier != -1)
      {
        StringValueInt16CollectionImplementation.StringTier tier = this._tiers[entryTier];
        short index = tier[(int) entryIndex];
        if (index < (short) 0)
          tier[(int) entryIndex] = (short) -(itemIndex + 1);
        else
          this._tiers[(int) index].Item = (short) -(itemIndex + 1);
      }
      if (lastItemIndex != 0)
        return;
      this._size = this._capacity = this._released = 0;
      this._tiers = StringValueInt16CollectionImplementation._emptyArray;
      this._firstTier = (StringValueInt16CollectionImplementation.StringTier) null;
    }

    internal override void Clear()
    {
      this._size = this._capacity = this._released = 0;
      this._tiers = StringValueInt16CollectionImplementation._emptyArray;
      this._firstTier = (StringValueInt16CollectionImplementation.StringTier) null;
    }

    internal override unsafe int IndexOfEntryTier(string key, int index, int count)
    {
      short index1 = 0;
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
      short num2 = 0;
      StringValueInt16CollectionImplementation.StringTier stringTier = this._firstTier;
      for (int index2 = 0; index2 < count; ++index2)
      {
        num2 = index1;
        int index3 = (int) chPtr1[index2];
        if ((index1 = stringTier[index3]) == (short) 0)
          return -1;
        if (index2 + 1 == count)
          return (int) index1;
        if (index1 < (short) 0)
          return -1;
        stringTier = this._tiers[(int) index1];
      }
      return -1;
    }

    internal override unsafe int IndexOfEntry(
      int rawTierIndex,
      string key,
      int index,
      int count,
      int itemCount)
    {
      short index1 = (short) rawTierIndex;
      if (index1 == (short) 0)
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
      char* chPtr1 = (char*) (num1 +  index * 2);
      short num2 = 0;
      StringValueInt16CollectionImplementation.StringTier tier = this._tiers[(int) index1];
      for (int index2 = 0; index2 < count; ++index2)
      {
        num2 = index1;
        int index3 = (int) chPtr1[index2];
        if ((index1 = tier[index3]) == (short) 0)
          return -1;
        if (index1 < (short) 0)
        {
          if (index2 + 1 != count)
            return -1;
          short num3 = (short) ((int) (short) ((int) index1 * -1) - 1);
          return (int) num3 < itemCount ? (int) num3 : -1;
        }
        tier = this._tiers[(int) index1];
      }
      if (tier.Item != (short) 0 && tier != this._firstTier && (int) tier.Parent == (int) num2)
        return (int) tier.Item * -1 - 1;
      return -1;
    }

    internal override unsafe int IndexOfEntry(string key, int index, int count, int itemCount)
    {
      short index1 = 0;
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
      short num2 = 0;
      StringValueInt16CollectionImplementation.StringTier stringTier = this._firstTier;
      for (int index2 = 0; index2 < count; ++index2)
      {
        num2 = index1;
        int index3 = (int) chPtr1[index2];
        if ((index1 = stringTier[index3]) == (short) 0)
          return -1;
        if (index1 < (short) 0)
        {
          if (index2 + 1 != count)
            return -1;
          short num3 = (short) ((int) (short) ((int) index1 * -1) - 1);
          return (int) num3 < itemCount ? (int) num3 : -1;
        }
        stringTier = this._tiers[(int) index1];
      }
      if (stringTier.Item != (short) 0 && stringTier != this._firstTier && (int) stringTier.Parent == (int) num2)
        return (int) stringTier.Item * -1 - 1;
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
      short index1 = 0;
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
      short num3 = 0;
      StringValueInt16CollectionImplementation.StringTier stringTier = this._firstTier;
      for (int index2 = 0; index2 < num1; ++index2)
      {
        num3 = index1;
        int index3 = (int) chPtr1[index2];
        ++consumed;
        if (index3 != (int) seperator)
        {
          if ((index1 = stringTier[index3]) == (short) 0)
            return -1;
          if (index1 < (short) 0)
          {
            if (index2 + 1 != num1)
            {
              if ((int) chPtr1[index2 + 1] != (int) seperator)
                return -1;
              ++consumed;
            }
            short num4 = (short) ((int) (short) ((int) index1 * -1) - 1);
            return (int) num4 < itemCount ? (int) num4 : -1;
          }
          stringTier = this._tiers[(int) index1];
        }
        else
          break;
      }
      if (stringTier.Item != (short) 0 && stringTier != this._firstTier && (int) stringTier.Parent == (int) num3)
        return (int) stringTier.Item * -1 - 1;
      return -1;
    }

    internal override void EnsureCapacity(int min)
    {
      if (this._capacity >= min)
        return;
      int num = this._capacity == 0 ? 4 : this._capacity * 2;
      if (num < min)
        num = min;
      if (num > this.MaxCapacity)
        num = this.MaxCapacity;
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

    internal sealed class StringTier
    {
      internal static short[] EmptyPartition = new short[33];
      private short _item;
      private short _parent;
      private short _parentIndex;
      private short _index;
      private short _terminators;
      private short _routes;
      private short[][] _table;
      private int _assigned;

      public short Item
      {
        get => this._item;
        set => this._item = value;
      }

      public short Parent
      {
        get => this._parent;
        set => this._parent = value;
      }

      public short ParentIndex
      {
        get => this._parentIndex;
        set => this._parentIndex = value;
      }

      public short Index
      {
        get => this._index;
        set => this._index = value;
      }

      public short Terminators => this._terminators;

      public short Routes => this._routes;

      public int Assigned => this._assigned;

      public short[][] Table => this._table;

      [IndexerName("TableRoutes")]
      public short this[int index]
      {
        get => this._table[index >> 5][index & 31];
        set => this.SetValue(index, value);
      }

      public StringTier(short parent, short parentIndex, int index)
      {
        this._parent = parent;
        this._parentIndex = parentIndex;
        this._index = (short) index;
        this._table = new short[8][];
        for (int index1 = 0; index1 < this._table.Length; ++index1)
          this._table[index1] = StringValueInt16CollectionImplementation.StringTier.EmptyPartition;
      }

      public short[] GetPartition(int partition) => (this._assigned & 1 << partition) == 0 ? (short[]) null : this._table[partition];

      private void SetValue(int index, short value)
      {
        int index1 = index >> 5;
        index &= 31;
        short num = 0;
        short[] numArray;
        if ((this._assigned & 1 << index1) == 0)
        {
          if (value == (short) 0)
            return;
          this._assigned |= 1 << index1;
          numArray = this._table[index1] = new short[33];
        }
        else
        {
          num = (numArray = this._table[index1])[index];
          if ((int) num == (int) value)
            return;
        }
        if (value == (short) 0)
        {
          if (num < (short) 0)
            --this._terminators;
          --this._routes;
          if (--numArray[32] == (short) 0)
          {
            this._table[index1] = StringValueInt16CollectionImplementation.StringTier.EmptyPartition;
            this._assigned &= ~(1 << index1);
          }
          else
            numArray[index] = value;
        }
        else
        {
          if (num != (short) 0)
          {
            if (num < (short) 0)
            {
              if (value > (short) 0)
                --this._terminators;
            }
            else if (value < (short) 0)
              ++this._terminators;
          }
          else
          {
            if (value < (short) 0)
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

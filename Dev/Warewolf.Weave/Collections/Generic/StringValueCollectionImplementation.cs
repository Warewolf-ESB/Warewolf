// Decompiled with JetBrains decompiler
// Type: System.Collections.Generic.StringValueCollectionImplementation
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

namespace System.Collections.Generic
{
  internal abstract class StringValueCollectionImplementation
  {
    protected const int TotalLength = 256;
    protected const int PartitionShift = 5;
    protected const int PartitionLength = 32;
    protected const int PartitionModulo = 31;
    protected const int TotalPartitions = 8;
    protected const int PartitionCapacity = 33;
    protected const short Unused = 0;
    protected const short NegativeOne = -1;
    protected const int Int32Unused = 0;
    protected const int Int32NegativeOne = -1;
    protected const int CharStorageCapacity = 1024;
    protected static char[] CharStorage = new char[1024];

    internal abstract int Size { get; }

    internal abstract int MaxCapacity { get; }

    internal abstract string GetEntryKey(int index, int tier, short entryIndex);

    internal abstract bool Add(
      string key,
      bool overwrite,
      int itemCount,
      out int resultTier,
      out int resultIndex);

    internal abstract int BeginRemove(string key, int entryTier, short entryIndex);

    internal abstract void EndRemove(
      int entryTier,
      short entryIndex,
      int itemIndex,
      int lastItemIndex);

    internal abstract void Clear();

    internal abstract int IndexOfEntry(string key, int index, int count, int itemCount);

    internal abstract int IndexOfEntry(
      int tierIndex,
      string key,
      int index,
      int count,
      int itemCount);

    internal abstract int IndexOfEntryTier(string key, int index, int count);

    internal abstract int IndexOfEntry(
      string key,
      int index,
      char seperator,
      int itemCount,
      out int consumed);

    internal abstract void EnsureCapacity(int min);

    internal abstract int EnsureSlot(int start);
  }
}

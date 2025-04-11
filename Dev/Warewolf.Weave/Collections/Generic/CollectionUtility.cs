// Decompiled with JetBrains decompiler
// Type: System.Collections.Generic.CollectionUtility
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

namespace System.Collections.Generic
{
  internal static class CollectionUtility
  {
    internal static readonly int[] Primes = new int[72]
    {
      3,
      7,
      11,
      17,
      23,
      29,
      37,
      47,
      59,
      71,
      89,
      107,
      131,
      163,
      197,
      239,
      293,
      353,
      431,
      521,
      631,
      761,
      919,
      1103,
      1327,
      1597,
      1931,
      2333,
      2801,
      3371,
      4049,
      4861,
      5839,
      7013,
      8419,
      10103,
      12143,
      14591,
      17519,
      21023,
      25229,
      30293,
      36353,
      43627,
      52361,
      62851,
      75431,
      90523,
      108631,
      130363,
      156437,
      187751,
      225307,
      270371,
      324449,
      389357,
      467237,
      560689,
      672827,
      807403,
      968897,
      1162687,
      1395263,
      1674319,
      2009191,
      2411033,
      2893249,
      3471899,
      4166287,
      4999559,
      5999471,
      7199369
    };

    public static void Swap<TSource>(IList<TSource> list, int a, int b)
    {
      if (a == b)
        return;
      TSource source = list[a];
      list[a] = list[b];
      list[b] = source;
    }

    public static TValue GetValueOrDefault<TKey, TValue>(
      this IDictionary<TKey, TValue> dictionary,
      TKey key)
    {
      TValue valueOrDefault;
      if (!dictionary.TryGetValue(key, out valueOrDefault))
        valueOrDefault = default (TValue);
      return valueOrDefault;
    }

    public static TValue GetValueOrDefault<TKey, TValue>(
      this IDictionary<TKey, TValue> dictionary,
      TKey key,
      TValue defaultValue)
    {
      TValue obj;
      return !dictionary.TryGetValue(key, out obj) ? defaultValue : obj;
    }

    public static Dictionary<TKey, TValue> CreateDictionary<TKey, TValue>(
      IEnumerable<TKey> keys,
      IEnumerable<TValue> values)
    {
      Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>();
      using (IEnumerator<TKey> enumerator1 = keys.GetEnumerator())
      {
        using (IEnumerator<TValue> enumerator2 = values.GetEnumerator())
        {
          while (enumerator1.MoveNext())
          {
            if (enumerator2.MoveNext())
              dictionary.Add(enumerator1.Current, enumerator2.Current);
            else
              break;
          }
        }
      }
      return dictionary;
    }

    internal static int GetPrime(int min)
    {
      if (min < 0)
        throw new ArgumentException();
      for (int index = 0; index < CollectionUtility.Primes.Length; ++index)
      {
        int prime = CollectionUtility.Primes[index];
        if (prime >= min)
          return prime;
      }
      for (int candidate = min | 1; candidate < int.MaxValue; candidate += 2)
      {
        if (CollectionUtility.IsPrime(candidate))
          return candidate;
      }
      return min;
    }

    internal static bool IsPrime(int candidate)
    {
      if ((candidate & 1) == 0)
        return candidate == 2;
      int num = (int) Math.Sqrt((double) candidate);
      for (int index = 3; index <= num; index += 2)
      {
        if (candidate % index == 0)
          return false;
      }
      return true;
    }
  }
}

// This code is distributed under MIT license. Copyright (c) 2013 George Mamaladze
// See license.txt or http://opensource.org/licenses/mit-license.php

using System.Collections.Generic;
using System.Linq;


namespace Gma.DataStructures.StringSearch
{
    public class SuffixTrie<T> : ITrie<T>
    {
        private readonly Trie<T> m_InnerTrie;
        private readonly int m_MinSuffixLength;

        public SuffixTrie(int minSuffixLength)
            : this(new Trie<T>(), minSuffixLength)
        {
        }

        private SuffixTrie(Trie<T> innerTrie, int minSuffixLength)
        {

            m_InnerTrie = innerTrie;
            m_MinSuffixLength = minSuffixLength;
        }

        public IEnumerable<T> Retrieve(string query)
        {
            return
                m_InnerTrie
                    .Retrieve(query)
                    .Distinct(new StringComparer<T>());
        }

        private string ReverseString(string input)
        {
            var array = input?.Select(c => char.IsLetter(c) ? (char.IsUpper(c) ? char.ToLower(c) : char.ToUpper(c)) : c).ToArray();
            string reversedCase = new string(array);
            return reversedCase;
        }

        public void Add(string key, T value, bool caseSensitive = true)
        {
            foreach (string suffix in GetAllSuffixes(m_MinSuffixLength, key, caseSensitive) ?? new List<string>())
            {
                m_InnerTrie.Add(suffix, value);
            }
        }

        private static IEnumerable<string> GetAllSuffixes(int minSuffixLength, string word, bool caseSensitive)
        {
            for (int i = word.Length - minSuffixLength; i >= 0; i--)
            {
                var partition = new StringPartition(word, i);
                if (caseSensitive)
                {
                    yield return partition.ToString();
                }
                else
                {
                    foreach (var str in GetAllCasePermutions(partition.ToString()).Distinct())
                    {
                        yield return str;
                    }
                }
            }
        }

        private static IEnumerable<string> GetAllCasePermutions(string s)
        {
            char[] array = s.ToLower().ToCharArray();
            int iterations = (1 << array.Length) - 1;

            for (int i = 0; i <= iterations; i++)
            {
                for (int j = 0; j < array.Length; j++)
                    array[j] = (i & (1 << j)) != 0
                                  ? char.ToUpper(array[j])
                                  : char.ToLower(array[j]);
                yield return new string(array);
            }
        }
    }
}
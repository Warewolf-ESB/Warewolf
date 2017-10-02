// This code is distributed under MIT license. Copyright (c) 2013 George Mamaladze
// See license.txt or http://opensource.org/licenses/mit-license.php

using System.Collections.Generic;
using System.Linq;


namespace Gma.DataStructures.StringSearch
{
    public class SuffixTrie<T> : ITrie<T>
    {
        private readonly Trie<T> _mInnerTrie;
        private readonly int _mMinSuffixLength;

        public SuffixTrie(int minSuffixLength)
            : this(new Trie<T>(), minSuffixLength)
        {
        }

        private SuffixTrie(Trie<T> innerTrie, int minSuffixLength)
        {

            _mInnerTrie = innerTrie;
            _mMinSuffixLength = minSuffixLength;
        }

        public IEnumerable<T> Retrieve(string query)
        {
            return
                _mInnerTrie
                    .Retrieve(query).Distinct(new StringComparer<T>());
        }

        

        public void Add(string key, T value)
        {
            var original = GetAllSuffixes(_mMinSuffixLength, key);
            var lowerCasedString = GetAllSuffixes(_mMinSuffixLength, key.ToLower());
            var upperCasedString = GetAllSuffixes(_mMinSuffixLength, key.ToUpper());
            var reversedString = GetAllSuffixes(_mMinSuffixLength, ReverseString(key));
            var suffixes =lowerCasedString?.Union(upperCasedString)?.Union(original.Union(reversedString));
            foreach (string suffix in suffixes?? new List<string>())
            {
                _mInnerTrie.Add(suffix, value);
            }

        }
        private string ReverseString(string input)
        {
            var array = input?.Select(c => char.IsLetter(c) ? (char.IsUpper(c) ? char.ToLower(c) : char.ToUpper(c)) : c).ToArray();
            string reversedCase = new string(array);
            return reversedCase;
        }

        private static IEnumerable<string> GetAllSuffixes(int minSuffixLength, string word)
        {
            for (int i = word.Length - minSuffixLength; i >= 0; i--)
            {
                var partition = new StringPartition(word, i);
                yield return partition.ToString();
            }
        }
    }
}
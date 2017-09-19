// This code is distributed under MIT license. Copyright (c) 2013 George Mamaladze
// See license.txt or http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;

namespace Gma.DataStructures.StringSearch
{
    public class TrieNode<TValue> : TrieNodeBase<TValue>
    {
        private readonly Dictionary<char, TrieNode<TValue>> m_Children;
        private readonly Queue<TValue> m_Values;

        protected TrieNode()
        {
            m_Children = new Dictionary<char, TrieNode<TValue>>();
            m_Values = new Queue<TValue>();
        }

        protected override int KeyLength
        {
            get { return 1; }
        }

        protected override IEnumerable<TrieNodeBase<TValue>> Children()
        {
            return m_Children.Values;
        }

        protected override IEnumerable<TValue> Values()
        {
            return m_Values;
        }

        protected override TrieNodeBase<TValue> GetOrCreateChild(char key)
        {
            if (!m_Children.TryGetValue(key, out TrieNode<TValue> result))
            {
                result = new TrieNode<TValue>();
                m_Children.Add(key, result);
            }
            return result;
        }

        protected override TrieNodeBase<TValue> GetChildOrNull(string query, int position)
        {
            if (query == null)
            {
                throw new ArgumentNullException("query");
            }

            return
                m_Children.TryGetValue(query[position], out TrieNode<TValue> childNode)
                    ? childNode
                    : null;
        }

        protected override void AddValue(TValue value)
        {
            m_Values.Enqueue(value);
        }
    }
}
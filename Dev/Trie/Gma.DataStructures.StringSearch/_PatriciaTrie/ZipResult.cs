// This code is distributed under MIT license. Copyright (c) 2013 George Mamaladze
// See license.txt or http://opensource.org/licenses/mit-license.php

using System.Diagnostics;

namespace Gma.DataStructures.StringSearch
{
    [DebuggerDisplay("Head: '{CommonHead}', This: '{ThisRest}', Other: '{OtherRest}', Kind: {MatchKind}")]
    public struct ZipResult
    {
        private readonly StringPartition m_CommonHead;
        private readonly StringPartition m_OtherRest;
        private readonly StringPartition m_ThisRest;

        public ZipResult(StringPartition commonHead, StringPartition thisRest, StringPartition otherRest)
        {
            m_CommonHead = commonHead;
            m_ThisRest = thisRest;
            m_OtherRest = otherRest;
        }

        public MatchKind MatchKind => m_ThisRest.Length == 0
            ? (m_OtherRest.Length == 0
                ? MatchKind.ExactMatch
                : MatchKind.IsContained)
            : (m_OtherRest.Length == 0
                ? MatchKind.Contains
                : MatchKind.Partial);

        public StringPartition OtherRest => m_OtherRest;

        public StringPartition ThisRest => m_ThisRest;

        public StringPartition CommonHead => m_CommonHead;


        public bool Equals(ZipResult other)
        {
            return 
                m_CommonHead == other.m_CommonHead && 
                m_OtherRest == other.m_OtherRest &&
                m_ThisRest == other.m_ThisRest;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is ZipResult && Equals((ZipResult) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = m_CommonHead.GetHashCode();
                hashCode = (hashCode*397) ^ m_OtherRest.GetHashCode();
                hashCode = (hashCode*397) ^ m_ThisRest.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(ZipResult left, ZipResult right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ZipResult left, ZipResult right)
        {
            return !(left == right);
        }
    }
}
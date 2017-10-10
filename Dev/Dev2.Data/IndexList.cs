/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;

namespace Dev2.Data.Binary_Objects
{
    [Serializable]
    public class IndexList
    {
        private int _maxValue;

        public int MaxValue
        {
            get { return _maxValue; }
            set
            {
                _maxValue = value;
            }
        }

        public int MinValue { get; set; }

        public HashSet<int> Gaps { get; private set; }

        private IndexList() { }

        public IndexList(HashSet<int> gaps, int maxValue)
            : this(gaps, maxValue, 1)
        {
        }

        public IndexList(HashSet<int> gaps, int maxValue, int minValue)
        {
            if(gaps == null)
            {
                gaps = new HashSet<int>();
            }

            Gaps = gaps;
            MinValue = minValue;
            MaxValue = maxValue;
        }

        public int GetMaxIndex()
        {
            int result = MaxValue;
            while(Gaps.Contains(result) && result >= 1)
            {
                result--;
            }
            return result;
        }

        public int Count()
        {

            if(MinValue > 1)
            {
                var res = MaxValue - MinValue;

                return res;
            }

            // Travis.Frisinger - Count bug change
            int result = MaxValue - Gaps.Count;

            return result;
        }
    }
}


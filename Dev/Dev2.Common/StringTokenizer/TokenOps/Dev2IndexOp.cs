/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces.StringTokenizer.Interfaces;
using System;
using System.Text;

namespace Dev2.Common
{
    internal class Dev2IndexOp : IDev2SplitOp
    {
        internal Dev2IndexOp(int index)
        {
            ToIndex = index;
        }

        public int ToIndex { get; private set; }

        public bool IsFinalOp()
        {
            return false;
        }

        public bool CanUseEnumerator(bool isReversed)
        {
            return false;
        }

        public string ExecuteOperation(char[] candidate, int startIdx, bool isReversed)
        {
            var result = new StringBuilder();

            int start = startIdx;
            int end = startIdx + ToIndex;

            // Avoid boundry over-run
            if (end > candidate.Length)
            {
                end = candidate.Length;
            }

            if (isReversed)
            {
                start = startIdx - ToIndex + 1;
                if (start < 0) start = 0;
                end = startIdx + 1;
            }

            for (int i = start; i < end; i++)
            {
                result.Append(candidate[i]);
            }

            return result.ToString();
        }

        public string ExecuteOperation(CharEnumerator candidate, int startIdx, int len, bool isReversed)
        {
            throw new NotImplementedException();
        }

        public int OpLength()
        {
            return 0;
        }
    }
}
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Common;
using Dev2.Common.Interfaces.StringTokenizer.Interfaces;
using System;
using System.IO;
using System.Text;

namespace Dev2.Common
{
    class Dev2EoFOp : IDev2SplitOp
    {
        public bool IsFinalOp() => false;

        public int OpLength() => 0;

        public bool CanUseEnumerator(bool isReversed) => false;

        public string ExecuteOperation(char[] candidate, int startIdx, bool isReversed)
        {
            var result = new StringBuilder();

            var start = startIdx;
            var end = candidate.Length;

            if (isReversed)
            {
                start = 0;
                end = startIdx + 1;
            }

            for (int i = start; i < end; i++)
            {
                result.Append(candidate[i]);
            }

            return result.ToString();
        }

        public string ExecuteOperation(CharEnumerator parts, int startIdx, int len, bool isReversed)
        {
            throw new NotImplementedException();
        }

        public string ExecuteOperation(StreamReader reader, int startIdx, int len, bool isReversed)
        {
            throw new NotImplementedException();
        }

        public string ExecuteOperation(string sourceString, int startIdx, int len, bool isReversed)
        {
            return sourceString.Substring(startIdx);
        }

        public string ExecuteOperation(ref StringBuilder sourceString, int startIdx, int len, bool isReversed)
        {
            if (isReversed)
            {
                return sourceString.Substring(0, len);
            }
            return sourceString.Substring(startIdx,len-startIdx);
        }
    }
}
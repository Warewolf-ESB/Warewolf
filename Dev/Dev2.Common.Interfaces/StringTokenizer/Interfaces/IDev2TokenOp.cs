/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;

namespace Dev2.Common.Interfaces.StringTokenizer.Interfaces
{
    public interface IDev2SplitOp
    {
        bool IsFinalOp();

        string ExecuteOperation(char[] candidate, int startIdx, bool isReversed);

        string ExecuteOperation(CharEnumerator parts, int startIdx, int len, bool isReversed);

        int OpLength();

        bool CanUseEnumerator(bool isReversed);
    }
}
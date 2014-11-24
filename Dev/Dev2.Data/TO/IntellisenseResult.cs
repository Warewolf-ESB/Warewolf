
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Data.Enums;
using Dev2.Data.Interfaces;

namespace Dev2.DataList.Contract
{
    public class IntellisenseResult : IIntellisenseResult {

        public int StartIndex { get; private set; }

        public int EndIndex { get; private set; }

        public IDataListVerifyPart Option { get; private set; }

        public string Message { get; private set; }

        public enIntellisenseResultType Type { get; private set; }

        public enIntellisenseErrorCode ErrorCode { get; private set; }

        public bool IsClosedRegion { get; private set; }
        
        internal IntellisenseResult(int startIdx, int endIdx, IDataListVerifyPart option, string message, enIntellisenseResultType typeOf, enIntellisenseErrorCode errCode, bool isClosed) {

            StartIndex = startIdx;
            EndIndex = endIdx;
            Option = option;
            Message = message;
            Type = typeOf;
            ErrorCode = errCode;
            IsClosedRegion = isClosed;
        }
    }
}

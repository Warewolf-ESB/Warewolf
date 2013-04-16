using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

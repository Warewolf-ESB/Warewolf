using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dev2.DataList.Contract
{
    public interface IIntellisenseResult {

        int StartIndex { get; }

        int EndIndex { get; }

        IDataListVerifyPart Option { get; }

        string Message { get; }

        enIntellisenseResultType Type { get; }

        enIntellisenseErrorCode ErrorCode { get; }

        bool IsClosedRegion { get; }
    }
}

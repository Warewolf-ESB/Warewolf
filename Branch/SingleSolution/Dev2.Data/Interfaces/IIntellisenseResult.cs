using Dev2.Data.Enums;
using Dev2.Data.Interfaces;

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

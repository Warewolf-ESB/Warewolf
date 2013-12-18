using Dev2.Data.Enums;

namespace Dev2.Data.Interfaces
{
    public interface IDev2DataLangaugeParseError  {

        int StartIndex { get; }

        int EndIndex { get; }

        string Message { get; }

        enIntellisenseErrorCode ErrorCode { get; }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dev2.DataList.Contract
{
    public interface IDev2DataLangaugeParseError  {

        int StartIndex { get; }

        int EndIndex { get; }

        string Message { get; }

        enIntellisenseErrorCode ErrorCode { get; }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dev2.DataList.Contract
{
    public class Dev2DataLanguageParseError : Exception, IDev2DataLangaugeParseError
    {

        private readonly int _startIdx;
        private readonly int _endIdx;
        private readonly enIntellisenseErrorCode _errCode;

        public Dev2DataLanguageParseError(string msg, int startIdx, int endIdx, enIntellisenseErrorCode code)
            : base(msg)
        {
            _startIdx = startIdx;
            _endIdx = endIdx;
            _errCode = code;
        }

        public int StartIndex
        {
            get
            {
                return _startIdx;
            }
        }

        public int EndIndex
        {
            get
            {
                return _endIdx;
            }
        }

        public string Error
        {
            get
            {
                return base.Message;
            }
        }

        public enIntellisenseErrorCode ErrorCode
        {
            get
            {
                return _errCode;
            }
        }
    }
}


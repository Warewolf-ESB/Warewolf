
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Dev2.Data.Enums;
using Dev2.Data.Interfaces;

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


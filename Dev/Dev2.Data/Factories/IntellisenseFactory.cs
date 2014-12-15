
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

namespace Dev2.DataList.Contract {
    public class IntellisenseFactory {

        public static IIntellisenseResult CreateErrorResult(int start, int end, IDataListVerifyPart opt, string msg, enIntellisenseErrorCode code, bool isClosed) {
            return new IntellisenseResult(start, end, opt, msg, enIntellisenseResultType.Error, code, isClosed);
        }

        public static IIntellisenseResult CreateSelectableResult(int start, int end, IDataListVerifyPart opt, string msg) {
            return new IntellisenseResult(start, end, opt, msg, enIntellisenseResultType.Selectable, enIntellisenseErrorCode.None, true);
        }

        public static IIntellisenseResult CreateCalculateIntellisenseResult(int start, int end, string displayName, string description, string errorMessage)
        {
            return new IntellisenseResult(start, end, new DateTimeVerifyPart(displayName, description), errorMessage, String.IsNullOrEmpty(errorMessage) ? enIntellisenseResultType.Selectable : enIntellisenseResultType.Error, String.IsNullOrEmpty(errorMessage) ? enIntellisenseErrorCode.None : enIntellisenseErrorCode.SyntaxError, true);
        }

        public static IIntellisenseResult CreateDateTimeResult(IDataListVerifyPart opt)
        {
            return new IntellisenseResult(0, 0, opt, "", enIntellisenseResultType.Selectable, enIntellisenseErrorCode.None, true);
        }

        public static IDataListVerifyPart CreateDateTimePart(string displayValue, string description)
        {
            return new DateTimeVerifyPart(displayValue, description);
        }

        public static IDataListVerifyPart CreateDataListValidationRecordsetPart(string recordset, string field)
        {
            return new DataListVerifyPart(recordset, field);
        }

        public static IDataListVerifyPart CreateDataListValidationRecordsetPart(string recordset, string field, bool useRawPartsForDisplayValue)
        {
            return new DataListVerifyPart(recordset, field, useRawPartsForDisplayValue);
        }


        public static IDataListVerifyPart CreateDataListValidationRecordsetPart(string recordset, string field, string desc) {
            return new DataListVerifyPart(recordset, field, desc);
        }

        public static IDataListVerifyPart CreateDataListValidationRecordsetPart(string recordset, string field, string desc, string index) {
            return new DataListVerifyPart(recordset, field, desc, index);
        }

        public static IDataListVerifyPart CreateDataListValidationScalarPart(string scalar) {
            return new DataListVerifyPart(null, scalar);
        }

        public static IDataListVerifyPart CreateDataListValidationScalarPart(string scalar, string desc) {
            return new DataListVerifyPart(null, scalar, desc);
        }
    }
}

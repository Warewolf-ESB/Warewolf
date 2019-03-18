#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Data.Interfaces;
using Dev2.Data.Parsers;
using Dev2.Data.Util;
using Dev2.DataList.Contract;

namespace Dev2.ViewModels.Workflow
{
    public static class WorkflowDesignerDataPartUtils
    {
        public static string RemoveRecordSetBrace(string recordSet)
        {
            string fullyFormattedStringValue;
            if (recordSet.Contains("(") && recordSet.Contains(")"))
            {
                fullyFormattedStringValue = recordSet.Remove(recordSet.IndexOf("(", StringComparison.Ordinal));
            }
            else
            {
                return recordSet;
            }
            return fullyFormattedStringValue;
        }

        public static void BuildDataPart(string dataPartFieldData, Dictionary<IDataListVerifyPart, string> unique) => BuildDataPart(dataPartFieldData, unique, false);
        public static void BuildDataPart(string dataPartFieldData, Dictionary<IDataListVerifyPart, string> unique, bool isJsonObjectSource)
        {
            var dataLanguageParser = new Dev2DataLanguageParser();
            dataPartFieldData = DataListUtil.StripBracketsFromValue(dataPartFieldData);
            IDataListVerifyPart verifyPart;
        
            if (isJsonObjectSource)
            {
                var isValid = IsJSonInputValid(dataPartFieldData, dataLanguageParser);
                if (isValid)
                {
                    verifyPart = IntellisenseFactory.CreateJsonPart(dataPartFieldData);
                    AddDataVerifyPart(verifyPart, verifyPart.DisplayValue, unique);
                }
            }
            else
            {
                AddDataUniquePart(dataPartFieldData, dataLanguageParser, unique);
            }
        }

        static void AddDataUniquePart(string dataPartFieldData, Dev2DataLanguageParser dataLanguageParser, Dictionary<IDataListVerifyPart, string> unique)
        {
            IDataListVerifyPart verifyPart = null;
            var fieldList = dataPartFieldData.Split('.');
            string fullyFormattedStringValue = "";
            if (fieldList.Length > 1 && !String.IsNullOrEmpty(fieldList[0]))
            {
                AddRecsetContainingAField(dataLanguageParser, unique, ref verifyPart, fieldList, ref fullyFormattedStringValue);
            }
            else
            {
                if (fieldList.Length == 1 && !String.IsNullOrEmpty(fieldList[0]))
                {
                    AddScalarOrRecset(dataPartFieldData, dataLanguageParser, unique, ref verifyPart, fieldList, ref fullyFormattedStringValue);
                }
            }
        }

        private static void AddScalarOrRecset(string dataPartFieldData, Dev2DataLanguageParser dataLanguageParser, Dictionary<IDataListVerifyPart, string> unique, ref IDataListVerifyPart verifyPart, string[] fieldList, ref string fullyFormattedStringValue)
        {
            // If the workflow field is simply a scalar or a record set without a child
            if (dataPartFieldData.EndsWith(")") && dataPartFieldData == fieldList[0])
            {
                if (dataPartFieldData.Contains("("))
                {
                    fullyFormattedStringValue = RemoveRecordSetBrace(fieldList[0]);
                    var intellisenseResult = dataLanguageParser.ValidateName(fullyFormattedStringValue, "");
                    if (intellisenseResult == null)
                    {
                        verifyPart = IntellisenseFactory.CreateDataListValidationRecordsetPart(fullyFormattedStringValue, String.Empty);
                        AddDataVerifyPart(verifyPart, verifyPart.DisplayValue, unique);
                    }
                }
            }
            else
            {
                var intellisenseResult = dataLanguageParser.ValidateName(dataPartFieldData, "");
                if (intellisenseResult == null)
                {
                    verifyPart = IntellisenseFactory.CreateDataListValidationScalarPart(RemoveRecordSetBrace(dataPartFieldData));
                    AddDataVerifyPart(verifyPart, verifyPart.DisplayValue, unique);
                }
            }
        }

        static void AddRecsetContainingAField(Dev2DataLanguageParser dataLanguageParser, Dictionary<IDataListVerifyPart, string> unique, ref IDataListVerifyPart verifyPart, string[] fieldList, ref string fullyFormattedStringValue)
        {
            // If it's a RecordSet Containing a field
            var recAdded = false;

            foreach (var item in fieldList)
            {
                if (item.EndsWith(")") && item == fieldList[0])
                {
                    AddField(dataLanguageParser, unique, ref verifyPart, ref fullyFormattedStringValue, ref recAdded, item);
                }
                else if (item == fieldList[1] && !(item.EndsWith(")") && item.Contains(")")))
                {
                    verifyPart = AddField(dataLanguageParser, unique, verifyPart, fieldList, recAdded, item);
                }
                else
                {
                    break;
                }
            }
        }

        static IDataListVerifyPart AddField(Dev2DataLanguageParser dataLanguageParser, Dictionary<IDataListVerifyPart, string> unique, IDataListVerifyPart verifyPart, string[] fieldList, bool recAdded, string item)
        {
            // If it's a field to a record set
            var intellisenseResult = dataLanguageParser.ValidateName(item, "");
            if (intellisenseResult == null && recAdded)
            {
                verifyPart = IntellisenseFactory.CreateDataListValidationRecordsetPart(
                        RemoveRecordSetBrace(fieldList.ElementAt(0)), item);
                AddDataVerifyPart(verifyPart, verifyPart.DisplayValue, unique);
            }

            return verifyPart;
        }

        static void AddField(Dev2DataLanguageParser dataLanguageParser, Dictionary<IDataListVerifyPart, string> unique, ref IDataListVerifyPart verifyPart, ref string fullyFormattedStringValue, ref bool recAdded, string item)
        {
            if (item.Contains("("))
            {
                fullyFormattedStringValue = RemoveRecordSetBrace(item);
                var intellisenseResult = dataLanguageParser.ValidateName(fullyFormattedStringValue, "");
                if (intellisenseResult == null)
                {
                    recAdded = true;
                    verifyPart =
                         IntellisenseFactory.CreateDataListValidationRecordsetPart(fullyFormattedStringValue,
                             String.Empty);
                    AddDataVerifyPart(verifyPart, verifyPart.DisplayValue, unique);
                }
            }
        }

        static bool IsJSonInputValid(string dataPartFieldData, Dev2DataLanguageParser dataLanguageParser)
        {
            var isValid = false;
            if (dataPartFieldData.Length < 1 || dataPartFieldData[0] != '@')
            {
                return isValid;
            }

            var removeBrace = dataPartFieldData.Contains("()") ? dataPartFieldData.Replace("()", "") : RemoveRecordSetBrace(dataPartFieldData);
            var replaceAtSign = removeBrace.Replace("@", "");
            var intellisenseResult = dataLanguageParser.ValidateName(string.IsNullOrEmpty(replaceAtSign) ? dataPartFieldData : replaceAtSign, "");
            if (intellisenseResult == null)
            {
                isValid = true;
            }
            else
            {
                isValid = isValidObject(intellisenseResult);
                if (!isValid)
                {
                    isValid = isValidParts();
                }
            }
            return isValid;


            bool isValidObject(IIntellisenseResult resultError)
            {
                var indexOfAtSign = dataPartFieldData.IndexOf("@", StringComparison.Ordinal);
                if (indexOfAtSign == 0 && (dataPartFieldData.Length >= 2) && char.IsLetter(dataPartFieldData[1]) && !resultError.Message.Contains("invalid char"))
                {
                    return true;
                }
                return false;
            }
            bool isValidParts()
            {
                if (dataPartFieldData.Contains("."))
                {
                    var fields = dataPartFieldData.Substring(1).Split('.');
                    if (fields.All(p => !string.IsNullOrEmpty(p) && char.IsLetter(p[0])))
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public  static void AddDataVerifyPart(IDataListVerifyPart part, string nameOfPart, Dictionary<IDataListVerifyPart, string> unique)
        {
            if (!unique.ContainsValue(nameOfPart))
            {
                unique.Add(part, nameOfPart);
            }
        }
    }
}

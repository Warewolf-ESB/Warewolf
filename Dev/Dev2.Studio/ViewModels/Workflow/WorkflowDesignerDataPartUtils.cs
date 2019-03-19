#pragma warning disable
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

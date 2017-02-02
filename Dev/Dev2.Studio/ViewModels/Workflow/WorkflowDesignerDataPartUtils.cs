/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
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


        public static void BuildDataPart(string dataPartFieldData, Dictionary<IDataListVerifyPart, string> unique, bool isJsonObjectSource = false)
        {
            Dev2DataLanguageParser dataLanguageParser = new Dev2DataLanguageParser();
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
                string[] fieldList = dataPartFieldData.Split('.');
                string fullyFormattedStringValue;
                if (fieldList.Length > 1 && !String.IsNullOrEmpty(fieldList[0]))
                {
                    // If it's a RecordSet Containing a field
                    bool recAdded = false;

                    foreach (var item in fieldList)
                    {
                        if (item.EndsWith(")") && item == fieldList[0])
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
                        else if (item == fieldList[1] && !(item.EndsWith(")") && item.Contains(")")))
                        {
                            // If it's a field to a record set
                            var intellisenseResult = dataLanguageParser.ValidateName(item, "");
                            if (intellisenseResult == null && recAdded)
                            {
                                verifyPart = IntellisenseFactory.CreateDataListValidationRecordsetPart(
                                        RemoveRecordSetBrace(fieldList.ElementAt(0)), item);
                                AddDataVerifyPart(verifyPart, verifyPart.DisplayValue, unique);
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                else if (fieldList.Length == 1 && !String.IsNullOrEmpty(fieldList[0]))
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
            }
        }

       private static bool IsJSonInputValid(string dataPartFieldData, Dev2DataLanguageParser dataLanguageParser)
       {
           var isValid = false;
           var removeBrace = dataPartFieldData.Contains("()") ? dataPartFieldData.Replace("()", "") : RemoveRecordSetBrace(dataPartFieldData);
           var replaceAtSign = removeBrace.Replace("@", "");
           var intellisenseResult = dataLanguageParser.ValidateName(string.IsNullOrEmpty(replaceAtSign) ? dataPartFieldData : replaceAtSign, "");
           if (intellisenseResult == null)
                isValid = true;
            else
            {
                var indexOfAtSign = dataPartFieldData.IndexOf("@", StringComparison.Ordinal);
                if (dataPartFieldData.Contains("@") && (indexOfAtSign == 0) && (indexOfAtSign + 1 >= dataPartFieldData.Length))
                {
                    if (!intellisenseResult.Message.Contains("invalid char"))
                        if (char.IsLetter(dataPartFieldData[1]))
                            isValid = true;
                }

                if (dataPartFieldData.Contains('.'))
                {
                    var fields = dataPartFieldData.Replace("@", "").Split('.');
                    if(fields.All(p => !string.IsNullOrEmpty(p) && char.IsLetter(p[0])))
                        isValid = true;
                }
            }
            return isValid;
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

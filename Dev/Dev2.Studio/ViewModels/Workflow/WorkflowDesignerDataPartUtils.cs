
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
using System.Collections.Generic;
using System.Linq;
using Dev2.Data.Interfaces;
using Dev2.Data.Parsers;
using Dev2.Data.Util;
using Dev2.DataList.Contract;

namespace Dev2.ViewModels.Workflow
{
   internal static class WorkflowDesignerDataPartUtils
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


     public static void BuildDataPart(string dataPartFieldData, Dictionary<IDataListVerifyPart, string> unique)
       {
           Dev2DataLanguageParser dataLanguageParser = new Dev2DataLanguageParser();

           dataPartFieldData = DataListUtil.StripBracketsFromValue(dataPartFieldData);
           IDataListVerifyPart verifyPart;
           string fullyFormattedStringValue;
           string[] fieldList = dataPartFieldData.Split('.');
           if (fieldList.Count() > 1 && !String.IsNullOrEmpty(fieldList[0]))
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
           else if (fieldList.Count() == 1 && !String.IsNullOrEmpty(fieldList[0]))
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


      public  static void AddDataVerifyPart(IDataListVerifyPart part, string nameOfPart, Dictionary<IDataListVerifyPart, string> unique)
        {
            if (!unique.ContainsValue(nameOfPart))
            {
                unique.Add(part, nameOfPart);
            }
        }
    }
}

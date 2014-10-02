
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
using System.Text.RegularExpressions;
using Dev2.Common.Interfaces.DataList.Contract;
using Dev2.Data.Interfaces;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.Builders;
using Dev2.DataList.Contract.Value_Objects;

namespace Dev2.Data.Operations
{
    public class Dev2ReplaceOperation : IDev2ReplaceOperation
    {
        private const RegexOptions NoneCompiled = RegexOptions.None | RegexOptions.Compiled;
        private const RegexOptions IgnoreCaseCompiled = RegexOptions.IgnoreCase | RegexOptions.Compiled;

        #region Ctor

        // REVIEW - This needs to be an internal constructor accessed via a factory!
        internal Dev2ReplaceOperation()
        {
        }

        #endregion 

        #region Methods

        public IDev2DataListUpsertPayloadBuilder<string> Replace(Guid exIdx, string expression, string oldString, string newString, bool caseMatch, IDev2DataListUpsertPayloadBuilder<string> payloadBuilder, out ErrorResultTO errors, out int ReplaceCount, out IBinaryDataListEntry entryToReplaceIn)
        {
            ReplaceCount = 0;
            oldString = oldString.Replace("\\", "\\\\");
            // ReSharper disable RedundantAssignment
            ErrorResultTO allErrors = new ErrorResultTO();
            errors = new ErrorResultTO();
            // ReSharper restore RedundantAssignment
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();
            entryToReplaceIn = compiler.Evaluate(exIdx, enActionType.User, expression, false, out errors);
            allErrors.MergeErrors(errors);

            if (entryToReplaceIn != null)
            {
                RegexOptions regexOptions;
                if (caseMatch)
                {
                    regexOptions = NoneCompiled;
                }
                else
                {
                    regexOptions = IgnoreCaseCompiled;
                }                
                
                //Massimo Guerrera -  Added the Regex.Escape to escape certain characters - Bug 9937
                Regex regex = new Regex(Regex.Escape(oldString), regexOptions);                

                IDev2DataListEvaluateIterator itr = Dev2ValueObjectFactory.CreateEvaluateIterator(entryToReplaceIn);
                while (itr.HasMoreRecords())
                {
                    IList<IBinaryDataListItem> rowList = itr.FetchNextRowData();
                    if(rowList != null)
                    {
                        foreach(IBinaryDataListItem binaryDataListItem in rowList)
                        {
                            int tmpCount = ReplaceCount;
                            string tmpVal = binaryDataListItem.TheValue;
                            if (tmpVal.Contains("\\") && oldString.Contains("\\"))
                            {
                                tmpVal = tmpVal.Replace("\\","\\\\");
                            }

                            ReplaceCount += regex.Matches(tmpVal).Count;
                            if(ReplaceCount > tmpCount)
                            {
                                if(entryToReplaceIn.IsRecordset)
                                {
                                    string recsetDisplayValue =
                                    DataListUtil.CreateRecordsetDisplayValue(entryToReplaceIn.Namespace,binaryDataListItem.FieldName,binaryDataListItem.ItemCollectionIndex.ToString());
                                    expression = string.Concat("[[", recsetDisplayValue, "]]");
                                }

                                var replaceValue = regex.Replace(tmpVal, newString);
                                payloadBuilder.Add(expression, replaceValue);
                            }
                        }
                    }
                }
                payloadBuilder.FlushIterationFrame();
            }
            else
            {
                allErrors.AddError("DataList entry is null.");
            }

            errors = allErrors;

            return payloadBuilder;
        }

        #endregion
    }
}

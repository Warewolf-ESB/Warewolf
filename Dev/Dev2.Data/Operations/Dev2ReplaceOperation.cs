using Dev2.Data.Interfaces;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.Builders;
using Dev2.DataList.Contract.Value_Objects;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

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

                Regex regex = new Regex(oldString, regexOptions);

                IDev2DataListEvaluateIterator itr = Dev2ValueObjectFactory.CreateEvaluateIterator(entryToReplaceIn);
                while (itr.HasMoreRecords())
                {
                    IList<IBinaryDataListItem> rowList = itr.FetchNextRowData();
                    if(rowList != null)
                    {
                        foreach(IBinaryDataListItem binaryDataListItem in rowList)
                        {
                            int tmpCount = ReplaceCount;
                            ReplaceCount += regex.Matches(binaryDataListItem.TheValue).Count;
                            if(ReplaceCount > tmpCount)
                            {
                                if(entryToReplaceIn.IsRecordset)
                                {
                                    string recsetDisplayValue =
                                    DataListUtil.CreateRecordsetDisplayValue(entryToReplaceIn.Namespace,binaryDataListItem.FieldName,binaryDataListItem.ItemCollectionIndex.ToString());
                                    expression = string.Concat("[[", recsetDisplayValue, "]]");
                                }

                                payloadBuilder.Add(expression, regex.Replace(binaryDataListItem.TheValue, newString));
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

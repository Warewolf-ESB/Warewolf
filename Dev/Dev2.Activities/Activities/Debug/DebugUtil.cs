using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using System;

namespace Dev2.Activities.Debug
{
    public static class DebugUtil
    {
        public static DebugOutputBase EvaluateEmptyRecordsetBeforeAddingToDebugOutput(string expression, string labelText, Guid executionID)
        {
            ErrorResultTO errors;
            string error;
            IBinaryDataListEntry expressionsEntry;
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();
            var dataList = compiler.FetchBinaryDataList(executionID, out errors);

            if(DataListUtil.IsValueRecordset(expression))
            {
                var found = dataList.TryGetEntry(DataListUtil.ExtractRecordsetNameFromValue(expression), out expressionsEntry, out error);
                if((found && expressionsEntry.IsEmpty()) || !found)
                {
                    return new DebugItemStaticDataParams("", expression, labelText);
                }
                expressionsEntry = compiler.Evaluate(executionID, enActionType.User, expression, false, out errors);
                return new DebugItemVariableParams(expression, labelText, expressionsEntry, executionID);
            }

            expressionsEntry = compiler.Evaluate(executionID, enActionType.User, expression, false, out errors);
            return new DebugItemVariableParams(expression, labelText, expressionsEntry, executionID);
        }
    }
}

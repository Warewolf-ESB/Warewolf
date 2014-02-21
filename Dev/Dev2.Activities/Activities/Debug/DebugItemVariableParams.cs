using System;
using System.Collections.Generic;
using Dev2.Data.TO;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.Diagnostics;

namespace Dev2.Activities.Debug
{
    public class DebugItemVariableParams : DebugOutputBase
    {
        readonly string _expression;
        readonly string _labelText;
        readonly IBinaryDataListEntry _valueEntry;
        readonly Guid _executionId;
        readonly bool _groupedItem;
        DebugOutputTO _debugOutputTO;
        string _groupName;

        public DebugItemVariableParams(string expression, string labelText, IBinaryDataListEntry valueEntry, Guid executionId, bool groupedItem = false)
        {
            _expression = expression;
            _labelText = labelText;
            _valueEntry = valueEntry;
            _executionId = executionId;
            _groupedItem = groupedItem;
        }

        public DebugItemVariableParams(DebugOutputTO debugOutputTO, string labelText = "Result", string groupName = "")
        {
            _debugOutputTO = debugOutputTO;
            _labelText = labelText;
            _groupName = groupName;
        }

        public string Expression
        {
            get
            {
                return _expression;
            }
        }
        public override string LabelText
        {
            get
            {
                return _labelText;
            }
        }
        public IBinaryDataListEntry ValueEntry
        {
            get
            {
                return _valueEntry;
            }
        }
        public Guid ExecutionId
        {
            get
            {
                return _executionId;
            }
        }

        public bool GroupedItem
        {
            get
            {
                return _groupedItem;
            }
        }


        public DebugOutputTO DebugOutputTO
        {
            get
            {
                return _debugOutputTO;
            }
        }

        public override List<DebugItemResult> GetDebugItemResult()
        {
            List<DebugItemResult> debugItemsResults;
            if(DebugOutputTO != null)
            {
                debugItemsResults = CreateDebugItemFromDebugOutputTO(DebugOutputTO, LabelText, _groupName);
            }
            else
            {
                if(ValueEntry != null)
                {
                    debugItemsResults = CreateDebugItemsFromEntry(Expression, ValueEntry, ExecutionId, enDev2ArgumentType.Input, LabelText);
                }
                else
                {
                    debugItemsResults = new List<DebugItemResult>{
                 new DebugItemResult
                    {
                        Type = DebugItemResultType.Variable, 
                        Value = "",
                        Label = LabelText,
                        Variable = Expression
                    }
                    };
                }
            }
            return debugItemsResults;
        }
    }
}

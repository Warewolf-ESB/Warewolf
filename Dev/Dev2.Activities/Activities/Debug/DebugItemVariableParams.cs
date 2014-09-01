using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Data.TO;
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
        readonly DebugTO _debugTO;
        readonly List<string> _regions;

        public DebugItemVariableParams(string expression, string labelText, IBinaryDataListEntry valueEntry, Guid executionId, bool groupedItem = false)
        {
            _expression = expression;
            _labelText = labelText;
            _valueEntry = valueEntry;
            _executionId = executionId;
            _groupedItem = groupedItem;
        }

        public DebugItemVariableParams(DebugTO debugTO, List<string> regions = null)
        {
            _debugTO = debugTO;
            _labelText = "";
            _regions = regions;
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


        public DebugTO DebugTO
        {
            get
            {
                return _debugTO;
            }
        }

        public override List<IDebugItemResult> GetDebugItemResult()
        {
            List<IDebugItemResult> debugItemsResults;
            if(DebugTO != null)
            {
                debugItemsResults = CreateDebugItemForOutput(DebugTO, LabelText, _regions);
            }
            else
            {
                if(ValueEntry != null)
                {
                    debugItemsResults = CreateDebugItemsFromEntry(Expression, ValueEntry, ExecutionId, enDev2ArgumentType.Input, LabelText);
                }
                else
                {
                    debugItemsResults = new List<IDebugItemResult>{
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

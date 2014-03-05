using System;
using System.Collections.Generic;
using Dev2.DataList.Contract;
using Dev2.Diagnostics;

namespace Dev2.Activities.Debug
{

    public class DebugOutputParams : DebugOutputBase
    {
        readonly string _expression;
        readonly string _value;
        readonly Guid _dlId;
        readonly int _indexToUpsertTo;
        readonly string _labelText;

        public DebugOutputParams(string expression, string value, Guid dlId, int indexToUpsertTo)
        {
            _expression = expression;
            _value = value;
            _dlId = dlId;
            _indexToUpsertTo = indexToUpsertTo;
            _labelText = "";
        }

        public string Expression
        {
            get
            {
                return _expression;
            }
        }
        public string Value
        {
            get
            {
                return _value;
            }
        }
        public Guid DlId
        {
            get
            {
                return _dlId;
            }
        }
        public int IndexToUpsertTo
        {
            get
            {
                return _indexToUpsertTo;
            }
        }
        public override string LabelText
        {
            get
            {
                return _labelText;
            }
        }

        public override List<DebugItemResult> GetDebugItemResult()
        {
            List<DebugItemResult> debugItemsResults = CreateDebugItemsFromString(Expression, Value, DlId, IndexToUpsertTo, LabelText, enDev2ArgumentType.Output);
            return debugItemsResults;
        }
    }
}

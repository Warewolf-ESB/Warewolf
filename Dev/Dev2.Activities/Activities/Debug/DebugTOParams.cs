using Dev2.Data.TO;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.Diagnostics;
using System;
using System.Collections.Generic;

namespace Dev2.Activities.Debug
{
    public class DebugTOParams : DebugOutputBase
    {

        readonly string _labelText;
        readonly DebugTO _debugTO;
        readonly bool _isInput;
        readonly string _leftLabel;
        readonly string _rightLabel;
        readonly List<string> _regions;

        public DebugTOParams(DebugTO debugTO, bool isInput, string leftLabel = "", string rightLabel = "", List<string> regions = null)
        {
            _debugTO = debugTO;
            _isInput = isInput;
            _leftLabel = leftLabel;
            _rightLabel = rightLabel;
            _labelText = "";
            _regions = regions;

        }

        public override string LabelText
        {
            get
            {
                return _labelText;
            }
        }

        public DebugTO DebugTO
        {
            get
            {
                return _debugTO;
            }
        }

        public override List<DebugItemResult> GetDebugItemResult()
        {
            List<DebugItemResult> debugItemsResults;
            if(_isInput)
            {
                debugItemsResults = CreateDebugItemForInput(DebugTO, LabelText, _leftLabel, _rightLabel, _regions);
            }
            else
            {
                debugItemsResults = CreateDebugItemForOutput(DebugTO, LabelText, _regions);
            }

            return debugItemsResults;
        }
    }
}

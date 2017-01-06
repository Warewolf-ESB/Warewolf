using System.Collections.Generic;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Data.Util;
using Dev2.Diagnostics;
using Warewolf.Storage;

namespace Dev2.Activities.Debug
{
    public class DebugItemWarewolfRecordset : DebugOutputBase
    {
        readonly DataStorage.WarewolfRecordset _warewolfRecordset;
        readonly string _labelText;
        string _operand;
        readonly string _variable;
        private readonly bool _mockSelected;

        public DebugItemWarewolfRecordset(DataStorage.WarewolfRecordset warewolfRecordset, string variable, string labelText, string operand, bool mockSelected = false)
        {
            _warewolfRecordset = warewolfRecordset;
            _labelText = labelText;
            _operand = operand;
            _variable = variable;
            Type = DebugItemResultType.Variable;
            _mockSelected = mockSelected;
        }
        
        public override string LabelText => _labelText;

        public string Variable => _variable;

        public DebugItemResultType Type { get; }

        public override List<IDebugItemResult> GetDebugItemResult()
        {

            var debugItemsResults = BuildDebugItemFromAtomList();
            return debugItemsResults;
        }

        List<IDebugItemResult> BuildDebugItemFromAtomList()
        {
            var results = new List<IDebugItemResult>();
           
            
            foreach (var item in _warewolfRecordset.Data)
            {
                if (item.Key == "WarewolfPositionColumn")
                {
                    continue;
                }

                var grpIdx = 0;
                foreach (var warewolfAtom in item.Value)
                {
                    var index = _warewolfRecordset.Data["WarewolfPositionColumn"][grpIdx];
                    var position = ExecutionEnvironment.WarewolfAtomToString(index);
                    grpIdx++;
                    string displayExpression = DataListUtil.AddBracketsToValueIfNotExist(DataListUtil.CreateRecordsetDisplayValue(DataListUtil.ExtractRecordsetNameFromValue(_variable),item.Key,position));
                    var debugType = DebugItemResultType.Value;
                    if (DataListUtil.IsEvaluated(displayExpression))
                    {
                        _operand = "=";
                        debugType = DebugItemResultType.Variable;
                    }
                    else
                    {
                        displayExpression = null;
                    }
                    if (!warewolfAtom.IsNothing)
                    {
                        results.Add(new DebugItemResult
                        {
                            Type = debugType,
                            Label = _labelText,
                            Variable = displayExpression,
                            Operator = _operand,
                            GroupName = _variable,
                            Value = ExecutionEnvironment.WarewolfAtomToString(warewolfAtom),
                            GroupIndex = grpIdx,
                            MockSelected = _mockSelected
                        });
                    }
                }
            }
            return results;
        }
    }
}
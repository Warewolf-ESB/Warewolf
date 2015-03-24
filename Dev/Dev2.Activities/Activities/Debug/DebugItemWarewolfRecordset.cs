using System.Collections.Generic;
using System.Linq;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Data.Util;
using Dev2.Diagnostics;
using Warewolf.Storage;

namespace Dev2.Activities.Debug
{
    public class DebugItemWarewolfRecordset : DebugOutputBase
    {
        readonly DataASTMutable.WarewolfRecordset _warewolfRecordset;
        readonly string _labelText;
        string _operand;
        readonly string _variable;
        readonly DebugItemResultType _type;

        public DebugItemWarewolfRecordset(DataASTMutable.WarewolfRecordset warewolfRecordset, string variable, string labelText, string operand)
        {
            _warewolfRecordset = warewolfRecordset;
            _labelText = labelText;
            _operand = operand;
            _variable = variable;
            _type = DebugItemResultType.Variable;
        }
        
        public override string LabelText
        {
            get
            {
                return _labelText;
            }
        }

        public string Variable
        {
            get
            {
                return _variable;
            }
        }
        public DebugItemResultType Type
        {
            get
            {
                return _type;
            }
        }

        public override List<IDebugItemResult> GetDebugItemResult()
        {

            var debugItemsResults = BuildDebugItemFromAtomList();
            return debugItemsResults;
        }

        List<IDebugItemResult> BuildDebugItemFromAtomList()
        {
            var results = new List<IDebugItemResult>();
            var grpIdx = 0;
            foreach (var item in _warewolfRecordset.Data)
            {
                if (item.Key == "WarewolfPositionColumn")
                {
                    continue;
                }
                
                foreach (var warewolfAtom in item.Value)
                {
                    grpIdx++;
                    var index = _warewolfRecordset.Data["WarewolfPositionColumn"][grpIdx-1];
                    var position = ExecutionEnvironment.WarewolfAtomToString(index);
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
                    results.Add(new DebugItemResult
                    {
                        Type = debugType,
                        Label = _labelText,
                        Variable = displayExpression,
                        Operator = _operand,
                        GroupName = _variable,
                        Value = ExecutionEnvironment.WarewolfAtomToString(warewolfAtom),
                        GroupIndex = grpIdx
                    });
                }
            }
            return results;
        }
    }
}
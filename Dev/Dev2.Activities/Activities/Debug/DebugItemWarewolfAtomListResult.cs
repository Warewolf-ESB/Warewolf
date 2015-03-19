using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Diagnostics;
using Dev2.Data.Util;

namespace Dev2.Activities.Debug
{
    public class DebugItemWarewolfAtomListResult : DebugOutputBase
    {
        readonly WarewolfDataEvaluationCommon.WarewolfEvalResult.WarewolfAtomListresult _warewolfAtomListresult;
        readonly string _labelText;
        readonly string _operand;
        readonly string _variable;
        readonly DebugItemResultType _type;

        public DebugItemWarewolfAtomListResult(WarewolfDataEvaluationCommon.WarewolfEvalResult.WarewolfAtomListresult warewolfAtomListresult, string variable, string labelText, string operand)
        {
            _warewolfAtomListresult = warewolfAtomListresult;
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
            string groupName = null;
            int grpIdx = 0;
            foreach (var item in _warewolfAtomListresult.Item)
            {


                string displayExpression = _variable;
                string rawExpression = _variable;
                if (displayExpression.Contains("().") || displayExpression.Contains("(*)."))
                {
                    grpIdx++;
                    groupName = displayExpression;
                    displayExpression = rawExpression;
                }
                else
                {
                    
                        string indexRegionFromRecordset = DataListUtil.ExtractIndexRegionFromRecordset(displayExpression);
                        int indexForRecset;
                        int.TryParse(indexRegionFromRecordset, out indexForRecset);

                        if (indexForRecset > 0)
                        {
                            int indexOfOpenningBracket = displayExpression.IndexOf("(", StringComparison.Ordinal) + 1;
                            string group = displayExpression.Substring(0, indexOfOpenningBracket) + "*" + displayExpression.Substring(indexOfOpenningBracket + indexRegionFromRecordset.Length);
                            grpIdx++;
                            groupName = @group;
                        }
                }

                var debugOperator = "";
                var debugType = DebugItemResultType.Value;
                if (DataListUtil.IsEvaluated(displayExpression))
                {
                    debugOperator = "=";
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
                    Operator = debugOperator,
                    GroupName = groupName,
                    Value = Warewolf.Storage.Environment.WarewolfAtomToString(item),
                    GroupIndex = grpIdx
                });
            }
            return results;
        }
    }
}
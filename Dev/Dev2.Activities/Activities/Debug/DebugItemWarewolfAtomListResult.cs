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
        readonly string _rightLabel;
        readonly string _leftLabel;
        readonly WarewolfDataEvaluationCommon.WarewolfEvalResult _oldValue;
        readonly string _assignedToVariableName;

        public DebugItemWarewolfAtomListResult(WarewolfDataEvaluationCommon.WarewolfEvalResult.WarewolfAtomListresult warewolfAtomListresult, WarewolfDataEvaluationCommon.WarewolfEvalResult oldResult, string assignedToVariableName, string variable, string leftLabelText, string rightLabelText, string operand)
        {
            _labelText = "";
            _operand = operand;
            _variable = variable;
            _type = DebugItemResultType.Variable;
            _rightLabel = rightLabelText;
            _leftLabel = leftLabelText;
            _warewolfAtomListresult = warewolfAtomListresult;
            _oldValue = oldResult;
            _assignedToVariableName = assignedToVariableName;
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
            if (!string.IsNullOrEmpty(_leftLabel))
            {
                string groupName = null;
                int grpIdx = 0;
                if(_warewolfAtomListresult != null)
                {
                    foreach (var item in _warewolfAtomListresult.Item)
                    {
                        string displayExpression = _variable;
                        string rawExpression = _variable;
                        if (displayExpression.Contains("().") || displayExpression.Contains("(*)."))
                        {
                            grpIdx++;
                            groupName = rawExpression;
                            displayExpression = DataListUtil.AddBracketsToValueIfNotExist(DataListUtil.CreateRecordsetDisplayValue(DataListUtil.ExtractRecordsetNameFromValue(_variable), DataListUtil.ExtractFieldNameFromValue(_variable), grpIdx.ToString()));
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
                            Label = _leftLabel,
                            Variable = DataListUtil.IsEvaluated(displayExpression) ? displayExpression : null,
                            Operator = debugOperator,
                            GroupName = groupName,
                            Value = Warewolf.Storage.ExecutionEnvironment.WarewolfAtomToString(item),
                            GroupIndex = grpIdx
                        });
                    }
                }
                else
                {
                    results.Add(new DebugItemResult
                    {
                        Type = DebugItemResultType.Variable,
                        Label = _leftLabel,
                        Variable = DataListUtil.IsEvaluated(Variable) ? Variable : null,
                        Operator = _operand,
                        GroupName = null,
                        Value = "",
                        GroupIndex = grpIdx
                    });
                }
            }

            if (!string.IsNullOrEmpty(_rightLabel))
            {
                if (_oldValue.IsWarewolfAtomResult)
                {
                    var scalarResult = _oldValue as WarewolfDataEvaluationCommon.WarewolfEvalResult.WarewolfAtomResult;
                    if(scalarResult != null)
                    {
                        results.Add(new DebugItemResult
                        {
                            Type = DebugItemResultType.Variable,
                            Label = _rightLabel,
                            Variable = DataListUtil.IsEvaluated(_assignedToVariableName)?_assignedToVariableName:null,
                            Operator = string.IsNullOrEmpty(_operand) ? "" : "=",
                            GroupName = Variable,
                            Value = Warewolf.Storage.ExecutionEnvironment.WarewolfAtomToString(scalarResult.Item),
                            GroupIndex = 0
                        });
                    }
                }
                if (_oldValue.IsWarewolfAtomListresult)
                {
                    var recSetResult = _oldValue as WarewolfDataEvaluationCommon.WarewolfEvalResult.WarewolfAtomListresult;
                    string groupName = null;
                    int grpIdx = 0;
                    if(recSetResult != null)
                    {
                        foreach (var item in recSetResult.Item)
                        {
                            string displayExpression = _assignedToVariableName;
                            string rawExpression = _assignedToVariableName;
                            if (displayExpression.Contains("().") || displayExpression.Contains("(*)."))
                            {
                                grpIdx++;
                                groupName = rawExpression;
                                displayExpression = DataListUtil.AddBracketsToValueIfNotExist(DataListUtil.CreateRecordsetDisplayValue(DataListUtil.ExtractRecordsetNameFromValue(_assignedToVariableName), DataListUtil.ExtractFieldNameFromValue(_assignedToVariableName), grpIdx.ToString()));
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
                                Label = _rightLabel,
                                Variable = DataListUtil.IsEvaluated(displayExpression) ? displayExpression : null,
                                Operator = debugOperator,
                                GroupName = groupName,
                                Value = Warewolf.Storage.ExecutionEnvironment.WarewolfAtomToString(item),
                                GroupIndex = grpIdx
                            });
                        }
                    }
                }
            }
            return results;
        }
    }
}
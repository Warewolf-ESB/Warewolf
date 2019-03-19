#pragma warning disable
using System;
using System.Collections.Generic;
using System.Globalization;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Data.Interfaces.Enums;
using Dev2.Data.Parsers;
using Dev2.Data.Util;
using Dev2.Diagnostics;
using Warewolf.Storage;

namespace Dev2.Activities.Debug
{
    public class DebugItemWarewolfAtomListResult : DebugOutputBase
    {
        readonly CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult _warewolfAtomListresult;
        readonly string _labelText;
        readonly string _operand;
        readonly bool _isCalculate;
        readonly string _variable;
        readonly DebugItemResultType _type;
        readonly string _rightLabel;
        readonly string _leftLabel;
        readonly CommonFunctions.WarewolfEvalResult _oldValue;
        readonly string _assignedToVariableName;
        readonly string _newValue;
        readonly bool _mockSelected;

        public DebugItemWarewolfAtomListResult(CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult warewolfAtomListresult, CommonFunctions.WarewolfEvalResult oldResult, string assignedToVariableName, string variable, string leftLabelText, string rightLabelText, string operand)
            : this(warewolfAtomListresult, oldResult, assignedToVariableName, variable, leftLabelText, rightLabelText, operand, false, false)
        {
        }

        public DebugItemWarewolfAtomListResult(CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult warewolfAtomListresult, CommonFunctions.WarewolfEvalResult oldResult, string assignedToVariableName, string variable, string leftLabelText, string rightLabelText, string operand, bool isCalculate, bool mockSelected)
        {
            _labelText = "";
            _operand = operand;
            _isCalculate = isCalculate;
            _variable = variable;
            _type = DebugItemResultType.Variable;
            _rightLabel = rightLabelText;
            _leftLabel = leftLabelText;
            _warewolfAtomListresult = warewolfAtomListresult;
            _oldValue = oldResult;
            _assignedToVariableName = assignedToVariableName;
            _mockSelected = mockSelected;
        }

        public DebugItemWarewolfAtomListResult(CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult warewolfAtomListresult, string newValue, string assignedToVariableName, string variable, string leftLabelText, string rightLabelText, string operand)
            : this(warewolfAtomListresult, newValue, assignedToVariableName, variable, leftLabelText, rightLabelText, operand, false, false)
        {
        }

        public DebugItemWarewolfAtomListResult(CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult warewolfAtomListresult, string newValue, string assignedToVariableName, string variable, string leftLabelText, string rightLabelText, string operand, bool isCalculate, bool mockSelected)
        {
            _labelText = "";
            _operand = operand;
            _isCalculate = isCalculate;
            _variable = variable;
            _type = DebugItemResultType.Variable;
            _rightLabel = rightLabelText;
            _leftLabel = leftLabelText;
            _warewolfAtomListresult = warewolfAtomListresult;
            _newValue = newValue;
            _oldValue = null;
            _assignedToVariableName = assignedToVariableName;
            _mockSelected = mockSelected;
        }


        public override string LabelText => _labelText;

        public string Variable => _variable;

        public DebugItemResultType Type => _type;

        public override List<IDebugItemResult> GetDebugItemResult()
        {

            var debugItemsResults = BuildDebugItemFromAtomList();
            return debugItemsResults;
        }

        List<IDebugItemResult> BuildDebugItemFromAtomList()
        {
            var results = new List<IDebugItemResult>();
            HasLeftLabel(results);
            HasRightLabel(results);
            NoLabels(results);
            return results;
        }

        void NoLabels(List<IDebugItemResult> results)
        {
            if (string.IsNullOrEmpty(_rightLabel) && string.IsNullOrEmpty(_leftLabel))
            {
                if (_warewolfAtomListresult != null)
                {
                    AtomListResultNoLabel(results);
                }
                else
                {
                    results.Add(new DebugItemResult
                    {
                        Type = DebugItemResultType.Variable,
                        Variable = DataListUtil.IsEvaluated(Variable) ? Variable : null,
                        Operator = _operand,
                        GroupName = null,
                        Value = "",
                        GroupIndex = 0,
                        MockSelected = _mockSelected
                    });
                }
            }
        }

        void AtomListResultNoLabel(List<IDebugItemResult> results)
        {
            string groupName = null;
            var grpIdx = 0;
            foreach (var item in _warewolfAtomListresult.Item)
            {
                var displayExpression = _variable;
                var rawExpression = _variable;
                if (displayExpression.Contains("()") || displayExpression.Contains("(*)"))
                {
                    grpIdx++;
                    groupName = rawExpression;
                    if (!rawExpression.StartsWith("[[@"))
                    {
                        displayExpression = GetObjectDisplayExpression(grpIdx);
                    }
                    else
                    {
                        var varName = DataListUtil.ExtractRecordsetNameFromValue(_variable);
                        displayExpression = DataListUtil.AddBracketsToValueIfNotExist(string.Concat(varName, "(", grpIdx, ")"));
                    }
                }
                else
                {
                    var indexRegionFromRecordset = DataListUtil.ExtractIndexRegionFromRecordset(displayExpression);
                    int.TryParse(indexRegionFromRecordset, out int indexForRecset);

                    if (indexForRecset > 0)
                    {
                        var indexOfOpenningBracket = displayExpression.IndexOf("(", StringComparison.Ordinal) + 1;
                        var group = displayExpression.Substring(0, indexOfOpenningBracket) + "*" + displayExpression.Substring(indexOfOpenningBracket + indexRegionFromRecordset.Length);
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
                    if (_isCalculate)
                    {
                        displayExpression = groupName ?? displayExpression;
                    }
                }
                else
                {
                    displayExpression = null;
                }
                results.Add(new DebugItemResult
                {
                    Type = debugType,
                    Variable = DataListUtil.IsEvaluated(displayExpression) ? displayExpression : null,
                    Operator = debugOperator,
                    GroupName = groupName,
                    Value = ExecutionEnvironment.WarewolfAtomToString(item),
                    GroupIndex = grpIdx,
                    MockSelected = _mockSelected
                });
            }
        }

        private string GetObjectDisplayExpression(int grpIdx)
        {
            string displayExpression = DataListUtil.AddBracketsToValueIfNotExist(DataListUtil.CreateRecordsetDisplayValue(DataListUtil.ExtractRecordsetNameFromValue(_variable), DataListUtil.ExtractFieldNameOnlyFromValue(DataListUtil.AddBracketsToValueIfNotExist(_variable)), grpIdx.ToString()));
            if (DataListUtil.GetRecordsetIndexType(_variable) == enRecordsetIndexType.Star)
            {
                displayExpression += _variable.Replace(DataListUtil.ReplaceRecordsetIndexWithStar(displayExpression), "");
            }
            else
            {
                if (DataListUtil.GetRecordsetIndexType(_variable) == enRecordsetIndexType.Blank)
                {
                    displayExpression += _variable.Replace(DataListUtil.ReplaceRecordsetIndexWithBlank(displayExpression), "");
                }
            }

            return displayExpression;
        }

        void HasRightLabel(List<IDebugItemResult> results)
        {
            if (!string.IsNullOrEmpty(_rightLabel))
            {
                if (_oldValue != null)
                {
                    ProcessOldValue(results);
                }
                if (_oldValue == null)
                {
                    var debugItemResult = new DebugItemResult
                    {
                        Type = DebugItemResultType.Variable,
                        Label = _rightLabel,
                        Variable = null,
                        Operator = "=",
                        GroupName = null,
                        Value = _newValue,
                        GroupIndex = 0,
                        MockSelected = _mockSelected
                    };
                    results.Add(debugItemResult);
                }
            }
        }

        void ProcessOldValue(List<IDebugItemResult> results)
        {
            if (_oldValue.IsWarewolfAtomResult)
            {
                if (_oldValue is CommonFunctions.WarewolfEvalResult.WarewolfAtomResult scalarResult)
                {
                    results.Add(new DebugItemResult
                    {
                        Type = DebugItemResultType.Variable,
                        Label = _rightLabel,
                        Variable = DataListUtil.IsEvaluated(_assignedToVariableName) ? _assignedToVariableName : null,
                        Operator = string.IsNullOrEmpty(_operand) ? "" : "=",
                        GroupName = null,
                        Value = ExecutionEnvironment.WarewolfAtomToString(scalarResult.Item),
                        GroupIndex = 0,
                        MockSelected = _mockSelected
                    });
                }
            }
            else
            {
                if (_oldValue.IsWarewolfAtomListresult)
                {
                    OldValueAtomListResult(results);
                }
            }
        }

        void OldValueAtomListResult(List<IDebugItemResult> results)
        {
            var recSetResult = _oldValue as CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult;
            string groupName = null;
            var grpIdx = 0;
            if (recSetResult != null)
            {
                foreach (var item in recSetResult.Item)
                {
                    var displayExpression = _assignedToVariableName;
                    var rawExpression = _assignedToVariableName;
                    if (displayExpression.Contains("().") || displayExpression.Contains("(*)."))
                    {
                        RecordsetGroupName(ref grpIdx, ref displayExpression, rawExpression, ref groupName);
                    }
                    else
                    {
                        ObjectGroupName(ref grpIdx, displayExpression, ref groupName);
                    }

                    var debugOperator = "";
                    var debugType = DebugItemResultType.Value;
                    if (DataListUtil.IsEvaluated(displayExpression))
                    {
                        debugOperator = "=";
                        debugType = DebugItemResultType.Variable;
                        displayExpression = _isCalculate ? (groupName ?? displayExpression) : displayExpression;
                    }
                    else
                    {
                        displayExpression = null;
                    }
                    var debugItemResult = new DebugItemResult
                    {
                        Type = debugType,
                        Label = _rightLabel,
                        Variable = DataListUtil.IsEvaluated(displayExpression) ? displayExpression : null,
                        Operator = debugOperator,
                        GroupName = groupName,
                        Value = ExecutionEnvironment.WarewolfAtomToString(item),
                        GroupIndex = grpIdx,
                        MockSelected = _mockSelected
                    };
                    results.Add(debugItemResult);
                }
            }
        }

        static void ObjectGroupName(ref int grpIdx, string displayExpression, ref string groupName)
        {
            var indexRegionFromRecordset = DataListUtil.ExtractIndexRegionFromRecordset(displayExpression);
            int.TryParse(indexRegionFromRecordset, out int indexForRecset);

            if (indexForRecset > 0)
            {
                var indexOfOpenningBracket = displayExpression.IndexOf("(", StringComparison.Ordinal) + 1;
                var group = displayExpression.Substring(0, indexOfOpenningBracket) + "*" + displayExpression.Substring(indexOfOpenningBracket + indexRegionFromRecordset.Length);
                grpIdx++;
                groupName = @group;
            }
        }

        private void RecordsetGroupName(ref int grpIdx, ref string displayExpression, string rawExpression, ref string groupName)
        {
            grpIdx++;
            groupName = rawExpression;
            var dataLanguageParser = new Dev2DataLanguageParser();
            var vals = dataLanguageParser.ParseForActivityDataItems(_assignedToVariableName);
            if (vals != null)
            {
                foreach (var val in vals)
                {
                    var repVal = DataListUtil.CreateRecordsetDisplayValue(DataListUtil.ExtractRecordsetNameFromValue(val), DataListUtil.ExtractFieldNameFromValue(val), grpIdx.ToString());
                    displayExpression = _assignedToVariableName.Replace(val, repVal);
                }
            }
        }

        void HasLeftLabel(List<IDebugItemResult> results)
        {
            if (!string.IsNullOrEmpty(_leftLabel))
            {
                string groupName = null;
                var grpIdx = 0;
                if (_warewolfAtomListresult != null)
                {
                    AddEachWarewolfAtomListResult(results, ref groupName, ref grpIdx);
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
                        GroupIndex = grpIdx,
                        MockSelected = _mockSelected
                    });
                }
            }
        }

        private void AddEachWarewolfAtomListResult(List<IDebugItemResult> results, ref string groupName, ref int grpIdx)
        {
            foreach (var atomItem in _warewolfAtomListresult.Item)
            {
                var displayExpression = _variable;
                var rawExpression = _variable;
                var item = atomItem.ToString();
                displayExpression = GetGroupName(displayExpression, rawExpression, ref grpIdx, ref item, ref groupName);

                var debugOperator = "";
                var debugType = DebugItemResultType.Value;
                if (DataListUtil.IsEvaluated(displayExpression))
                {
                    debugOperator = string.IsNullOrEmpty(item) ? "" : "=";
                    debugType = DebugItemResultType.Variable;
                    if (_isCalculate)
                    {
                        displayExpression = groupName ?? displayExpression;
                    }
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
                    Value = item,
                    GroupIndex = grpIdx,
                    MockSelected = _mockSelected
                });
            }
        }

        string GetGroupName(string displayExpression, string rawExpression, ref int grpIdx, ref string item, ref string groupName)
        {
            var expr = displayExpression;
            if (displayExpression.Contains("().") || displayExpression.Contains("(*)."))
            {
                grpIdx++;
                var index = grpIdx.ToString(CultureInfo.InvariantCulture);
                if (rawExpression.Contains(".WarewolfPositionColumn"))
                {
                    index = item;
                    item = "";
                }
                groupName = rawExpression.Replace(".WarewolfPositionColumn", "");
                expr = DataListUtil.AddBracketsToValueIfNotExist(DataListUtil.CreateRecordsetDisplayValue(DataListUtil.ExtractRecordsetNameFromValue(_variable), DataListUtil.ExtractFieldNameFromValue(_variable), index)).Replace(".WarewolfPositionColumn", "");
            }
            else
            {
                var indexRegionFromRecordset = DataListUtil.ExtractIndexRegionFromRecordset(expr);
                int.TryParse(indexRegionFromRecordset, out int indexForRecset);

                if (indexForRecset > 0)
                {
                    var indexOfOpenningBracket = expr.IndexOf("(", StringComparison.Ordinal) + 1;
                    var group = expr.Substring(0, indexOfOpenningBracket) + "*" + expr.Substring(indexOfOpenningBracket + indexRegionFromRecordset.Length);
                    grpIdx++;
                    groupName = @group;
                }
            }
            return expr;
        }
    }
}
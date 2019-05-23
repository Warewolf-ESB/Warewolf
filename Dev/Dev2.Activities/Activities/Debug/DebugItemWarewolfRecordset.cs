#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

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
        readonly bool _mockSelected;

        public DebugItemWarewolfRecordset(DataStorage.WarewolfRecordset warewolfRecordset, string variable, string labelText, string operand)
            : this(warewolfRecordset, variable, labelText, operand, false)
        {
        }

        public DebugItemWarewolfRecordset(DataStorage.WarewolfRecordset warewolfRecordset, string variable, string labelText, string operand, bool mockSelected)
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
                    var displayExpression = DataListUtil.AddBracketsToValueIfNotExist(DataListUtil.CreateRecordsetDisplayValue(DataListUtil.ExtractRecordsetNameFromValue(_variable),item.Key,position));
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
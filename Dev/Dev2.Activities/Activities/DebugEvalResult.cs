using System;
using System.Collections.Generic;
using Dev2.Activities.Debug;
using Dev2.Common;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Data;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Warewolf.Storage;
using WarewolfParserInterop;

namespace Dev2.Activities
{
    public class DebugEvalResult : DebugOutputBase
    {
        string _inputVariable;
        string _positionInput;
        readonly string _label;
        readonly WarewolfDataEvaluationCommon.WarewolfEvalResult _evalResult;

        public DebugEvalResult(string inputVariable, string label, IExecutionEnvironment environment, bool isDataMerge = false)
        {
            _inputVariable = inputVariable;
            _positionInput = "";
            _label = label;
            try
            {
                if (DataListUtils.IsValueRecordset(_inputVariable))
                {
                    if (DataListUtil.GetRecordsetIndexType(_inputVariable) == enRecordsetIndexType.Blank)
                    {
                        var length = environment.GetLength(DataListUtil.ExtractRecordsetNameFromValue(_inputVariable));
                        _inputVariable = DataListUtil.ReplaceRecordsetBlankWithIndex(_inputVariable, length);
                    }                    
                }
                if (isDataMerge)
                {
                    var evalForDataMerge = environment.EvalForDataMerge(_inputVariable);
                    var innerIterator = new WarewolfListIterator();
                    var innerListOfIters = new List<WarewolfIterator>();
                    foreach (var listOfIterator in evalForDataMerge)
                    {
                        var inIterator = new WarewolfIterator(listOfIterator);
                        innerIterator.AddVariableToIterateOn(inIterator);
                        innerListOfIters.Add(inIterator);
                    }
                    var atomList = new List<DataASTMutable.WarewolfAtom>();
                    while (innerIterator.HasMoreData())
                    {
                        var stringToUse = "";
                        foreach (var warewolfIterator in innerListOfIters)
                        {
                            stringToUse += warewolfIterator.GetNextValue();
                        }
                        atomList.Add(DataASTMutable.WarewolfAtom.NewDataString(stringToUse));
                    }
                    var finalString = string.Join("", atomList);
                    _evalResult = WarewolfDataEvaluationCommon.WarewolfEvalResult.NewWarewolfAtomListresult(new WarewolfAtomList<DataASTMutable.WarewolfAtom>(DataASTMutable.WarewolfAtom.Nothing, atomList));
                    if (DataListUtil.IsFullyEvaluated(finalString))
                    {
                        _evalResult = environment.Eval(finalString);
                    }
                }
                else
                {
                    _inputVariable = environment.EvalToExpression(_inputVariable);
                    _evalResult = environment.Eval(_inputVariable);
                }
                if (_inputVariable.Contains(".WarewolfPositionColumn")) _positionInput = _inputVariable.Replace(".WarewolfPositionColumn", ""); 

            }
            catch(Exception e)
            {
                Dev2Logger.Log.Error(e.Message,e);
                _evalResult = WarewolfDataEvaluationCommon.WarewolfEvalResult.NewWarewolfAtomResult(DataASTMutable.WarewolfAtom.Nothing);
            }
            
        }

        #region Overrides of DebugOutputBase

        public override string LabelText
        {
            get
            {
                return _label;
            }
        }

        public override List<IDebugItemResult> GetDebugItemResult()
        {
            if (_evalResult.IsWarewolfAtomResult)
            {
                var scalarResult = _evalResult as WarewolfDataEvaluationCommon.WarewolfEvalResult.WarewolfAtomResult;
                if (scalarResult != null && !scalarResult.Item.IsNothing)
                {
                    var warewolfAtomToString = ExecutionEnvironment.WarewolfAtomToString(scalarResult.Item);
                    if (warewolfAtomToString == _inputVariable && DataListUtils.IsEvaluated(_inputVariable))
                    {
                        warewolfAtomToString = null;
                    }
                    if (!DataListUtils.IsEvaluated(_inputVariable))
                    {
                        _inputVariable = null;
                    }
                    return new DebugItemWarewolfAtomResult(warewolfAtomToString, _inputVariable, LabelText).GetDebugItemResult();
                }
            }
            else if (_evalResult.IsWarewolfAtomListresult)
            {
                var listResult = _evalResult as WarewolfDataEvaluationCommon.WarewolfEvalResult.WarewolfAtomListresult;
                if (listResult != null)
                {
                    return new DebugItemWarewolfAtomListResult(listResult, "", "", _inputVariable, LabelText, "", "=").GetDebugItemResult();
                }
            }
            else if (_evalResult.IsWarewolfRecordSetResult)
            {
                var listResult = _evalResult as WarewolfDataEvaluationCommon.WarewolfEvalResult.WarewolfRecordSetResult;
                if (listResult != null)
                {
                    List<IDebugItemResult> res = new List<IDebugItemResult>();
                    foreach(var item in listResult.Item.Data)
                    {
                        if (item.Key != WarewolfDataEvaluationCommon.PositionColumn)
                        {
                            var data = WarewolfDataEvaluationCommon.WarewolfEvalResult.NewWarewolfAtomListresult(item.Value) as WarewolfDataEvaluationCommon.WarewolfEvalResult.WarewolfAtomListresult;
                            res.AddRange(new DebugItemWarewolfAtomListResult(data, "", "", ExecutionEnvironment.ConvertToColumnWithStar(_inputVariable, item.Key), LabelText, "", "=").GetDebugItemResult());
                        }
                     }
                    return res;
                }
            }

            return new DebugItemStaticDataParams("",_inputVariable,LabelText).GetDebugItemResult();
        }

        #endregion
    }
}
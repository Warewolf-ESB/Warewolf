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
// ReSharper disable MemberCanBePrivate.Global

namespace Dev2.Activities
{
    public class DebugEvalResult : DebugOutputBase
    {
        string _inputVariable;
        CommonFunctions.WarewolfEvalResult _evalResult;
        private readonly bool _isCalculate;

        public DebugEvalResult(string inputVariable, string label, IExecutionEnvironment environment,int update, bool isDataMerge = false,bool isCalculate=false, bool mockSelected = false)
        {
            _inputVariable = inputVariable.Trim();
            LabelText = label;
            _isCalculate = isCalculate;
            MockSelected = mockSelected;
            try
            {
                if (ExecutionEnvironment.IsRecordsetIdentifier(_inputVariable) && DataListUtil.IsEvaluated(_inputVariable))
                {
                    if (DataListUtil.GetRecordsetIndexType(_inputVariable) == enRecordsetIndexType.Blank)
                    {
                        var length = environment.GetLength(DataListUtil.ExtractRecordsetNameFromValue(_inputVariable));
                        _inputVariable = DataListUtil.ReplaceRecordsetBlankWithIndex(_inputVariable, length);
                    }                    
                }
                if (isDataMerge)
                {
                    DataMergeItem(environment, update);
                }
                else
                {
                    RegularItem(environment, update, isCalculate);
                }

            }
            catch(Exception e)
            {
                Dev2Logger.Error(e.Message,e);
                _evalResult = CommonFunctions.WarewolfEvalResult.NewWarewolfAtomResult(DataStorage.WarewolfAtom.Nothing);
            }
            
        }

        private void RegularItem(IExecutionEnvironment environment, int update, bool isCalculate)
        {
            var evalToExpression = environment.EvalToExpression(_inputVariable, update);
            if(DataListUtil.IsEvaluated(evalToExpression))
            {
                _inputVariable = evalToExpression;
            }
            _evalResult = environment.Eval(_inputVariable, update);
            string cleanExpression;
            var isCalcExpression = DataListUtil.IsCalcEvaluation(_inputVariable, out cleanExpression);
            if(isCalcExpression && !isCalculate)
            {
                if(_evalResult.IsWarewolfAtomResult)
                {
                    var atomResult = _evalResult as CommonFunctions.WarewolfEvalResult.WarewolfAtomResult;
                    if(atomResult != null)
                    {
                        var res = atomResult.Item.ToString();
                        string resValue;
                        DataListUtil.IsCalcEvaluation(res, out resValue);
                        _evalResult = CommonFunctions.WarewolfEvalResult.NewWarewolfAtomResult(DataStorage.WarewolfAtom.NewDataString(resValue));
                    }
                }
                _inputVariable = cleanExpression;
            }
        }

        private void DataMergeItem(IExecutionEnvironment environment, int update)
        {
            var evalForDataMerge = environment.EvalForDataMerge(_inputVariable, update);
            var innerIterator = new WarewolfListIterator();
            var innerListOfIters = new List<WarewolfIterator>();
            foreach(var listOfIterator in evalForDataMerge)
            {
                var inIterator = new WarewolfIterator(listOfIterator);
                innerIterator.AddVariableToIterateOn(inIterator);
                innerListOfIters.Add(inIterator);
            }
            var atomList = new List<DataStorage.WarewolfAtom>();
            while(innerIterator.HasMoreData())
            {
                var stringToUse = "";
                // ReSharper disable once LoopCanBeConvertedToQuery
                foreach(var warewolfIterator in innerListOfIters)
                {
                    stringToUse += warewolfIterator.GetNextValue();
                }
                atomList.Add(DataStorage.WarewolfAtom.NewDataString(stringToUse));
            }
            var finalString = string.Join("", atomList);
            _evalResult = CommonFunctions.WarewolfEvalResult.NewWarewolfAtomListresult(new WarewolfAtomList<DataStorage.WarewolfAtom>(DataStorage.WarewolfAtom.Nothing, atomList));
            if(DataListUtil.IsFullyEvaluated(finalString))
            {
                _inputVariable = finalString;
                _evalResult = environment.Eval(finalString, update);
            }
            else
            {
                var evalToExpression = environment.EvalToExpression(_inputVariable, update);
                if(DataListUtil.IsEvaluated(evalToExpression))
                {
                    _inputVariable = evalToExpression;
                }
            }
        }

        #region Overrides of DebugOutputBase

        public override string LabelText { get; }
        public bool MockSelected { get; }

        public override List<IDebugItemResult> GetDebugItemResult()
        {
            if (_evalResult.IsWarewolfAtomResult)
            {
                var scalarResult = _evalResult as CommonFunctions.WarewolfEvalResult.WarewolfAtomResult;
                if (scalarResult != null && !scalarResult.Item.IsNothing)
                {
                    var warewolfAtomToString = ExecutionEnvironment.WarewolfAtomToString(scalarResult.Item);
                    if (warewolfAtomToString == _inputVariable && DataListUtil.IsEvaluated(_inputVariable))
                    {
                        warewolfAtomToString = null;
                    }
                    if (!DataListUtil.IsEvaluated(_inputVariable))
                    {
                        _inputVariable = null;
                    }
                    return new DebugItemWarewolfAtomResult(warewolfAtomToString, _inputVariable, LabelText, MockSelected).GetDebugItemResult();
                }
            }
            else if (_evalResult.IsWarewolfAtomListresult)
            {
                var listResult = _evalResult as CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult;
                if (listResult != null)
                {
                    return new DebugItemWarewolfAtomListResult(listResult, "", "", _inputVariable, LabelText, "", "=", _isCalculate, MockSelected).GetDebugItemResult();
                }
            }
            else if (_evalResult.IsWarewolfRecordSetResult)
            {
                var listResult = _evalResult as CommonFunctions.WarewolfEvalResult.WarewolfRecordSetResult;
                if (listResult != null)
                {
                    return new DebugItemWarewolfRecordset(listResult.Item, _inputVariable, LabelText,  "=", MockSelected).GetDebugItemResult();
                }
            }

            return new DebugItemStaticDataParams("",_inputVariable,LabelText, MockSelected).GetDebugItemResult();
        }

        #endregion
    }
}
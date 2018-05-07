using System;
using System.Collections.Generic;
using Dev2.Activities.Debug;
using Dev2.Common;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Data;
using Dev2.Data.Interfaces.Enums;
using Dev2.Data.Util;
using Warewolf.Storage;
using Warewolf.Storage.Interfaces;
using WarewolfParserInterop;


namespace Dev2.Activities
{
    public class DebugEvalResult : DebugOutputBase
    {
        string _inputVariable;
        readonly string _operand;
        CommonFunctions.WarewolfEvalResult _evalResult;
        readonly bool _isCalculate;

        public DebugEvalResult(string inputVariable, string label, IExecutionEnvironment environment, int update, string operand)
            : this(inputVariable, label, environment, update, operand, false, false, false)
        {
        }

        public DebugEvalResult(string inputVariable, string label, IExecutionEnvironment environment, int update, string operand, bool isDataMerge, bool isCalculate, bool mockSelected)
            : this(inputVariable, label, environment, update, isDataMerge, isCalculate, mockSelected)
        {
            _operand = operand;
        }

        public DebugEvalResult(string inputVariable, string label, IExecutionEnvironment environment, int update)
            : this(inputVariable, label, environment, update, false, false, false)
        {
        }

        public DebugEvalResult(string inputVariable, string label, IExecutionEnvironment environment, int update, bool isDataMerge)
            : this(inputVariable, label, environment, update, isDataMerge, false, false)
        {
        }

        public DebugEvalResult(string inputVariable, string label, IExecutionEnvironment environment, int update, bool isDataMerge, bool isCalculate)
            : this(inputVariable, label, environment, update, isDataMerge, isCalculate, false)
        {
        }

        public DebugEvalResult(string inputVariable, string label, IExecutionEnvironment environment, int update, bool isDataMerge, bool isCalculate, bool mockSelected)
        {
            _inputVariable = inputVariable?.Trim();
            LabelText = label;
            _isCalculate = isCalculate;
            MockSelected = mockSelected;
            try
            {
                if (ExecutionEnvironment.IsRecordsetIdentifier(_inputVariable) && DataListUtil.IsEvaluated(_inputVariable) && DataListUtil.GetRecordsetIndexType(_inputVariable) == enRecordsetIndexType.Blank)
                {
                    var length = environment.GetLength(DataListUtil.ExtractRecordsetNameFromValue(_inputVariable));
                    _inputVariable = DataListUtil.ReplaceRecordsetBlankWithIndex(_inputVariable, length);
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
            catch (Exception e)
            {
                Dev2Logger.Error(e.Message,e, GlobalConstants.WarewolfError);
                _evalResult = CommonFunctions.WarewolfEvalResult.NewWarewolfAtomResult(DataStorage.WarewolfAtom.Nothing);
            }

        }

        void RegularItem(IExecutionEnvironment environment, int update, bool isCalculate)
        {
            var evalToExpression = environment.EvalToExpression(_inputVariable, update);
            if (DataListUtil.IsEvaluated(evalToExpression))
            {
                _inputVariable = evalToExpression;
            }
            _evalResult = environment.Eval(_inputVariable, update);
            var isCalcExpression = DataListUtil.IsCalcEvaluation(_inputVariable, out string cleanExpression);
            if (isCalcExpression && !isCalculate)
            {
                if (_evalResult.IsWarewolfAtomResult && _evalResult is CommonFunctions.WarewolfEvalResult.WarewolfAtomResult atomResult)
                {
                    var res = atomResult.Item.ToString();
                    DataListUtil.IsCalcEvaluation(res, out string resValue);
                    _evalResult = CommonFunctions.WarewolfEvalResult.NewWarewolfAtomResult(DataStorage.WarewolfAtom.NewDataString(resValue));
                }

                _inputVariable = cleanExpression;
            }
        }

        void DataMergeItem(IExecutionEnvironment environment, int update)
        {
            var evalForDataMerge = environment.EvalForDataMerge(_inputVariable, update);
            var innerIterator = new WarewolfListIterator();
            var innerListOfIters = new List<WarewolfIterator>();
            foreach (var listOfIterator in evalForDataMerge)
            {
                var inIterator = new WarewolfIterator(listOfIterator);
                innerIterator.AddVariableToIterateOn(inIterator);
                innerListOfIters.Add(inIterator);
            }
            var atomList = new List<DataStorage.WarewolfAtom>();
            while (innerIterator.HasMoreData())
            {
                var stringToUse = "";

                foreach (var warewolfIterator in innerListOfIters)
                {
                    stringToUse += warewolfIterator.GetNextValue();
                }
                atomList.Add(DataStorage.WarewolfAtom.NewDataString(stringToUse));
            }
            var finalString = string.Join("", atomList);
            _evalResult = CommonFunctions.WarewolfEvalResult.NewWarewolfAtomListresult(new WarewolfAtomList<DataStorage.WarewolfAtom>(DataStorage.WarewolfAtom.Nothing, atomList));
            if (DataListUtil.IsFullyEvaluated(finalString))
            {
                _inputVariable = finalString;
                _evalResult = environment.Eval(finalString, update);
            }
            else
            {
                var evalToExpression = environment.EvalToExpression(_inputVariable, update);
                if (DataListUtil.IsEvaluated(evalToExpression))
                {
                    _inputVariable = evalToExpression;
                }
            }
        }

        public override string LabelText { get; }
        public bool MockSelected { get; }

        public override List<IDebugItemResult> GetDebugItemResult()
        {
            if (_evalResult.IsWarewolfAtomResult)
            {
                if (_evalResult is CommonFunctions.WarewolfEvalResult.WarewolfAtomResult scalarResult && !scalarResult.Item.IsNothing)
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
                    return new DebugItemWarewolfAtomResult(warewolfAtomToString, _inputVariable, LabelText, _operand, MockSelected).GetDebugItemResult();
                }
            }
            else if (_evalResult.IsWarewolfAtomListresult)
            {
                if (_evalResult is CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult listResult)
                {
                    return new DebugItemWarewolfAtomListResult(listResult, "", "", _inputVariable, LabelText, "", "=", _isCalculate, MockSelected).GetDebugItemResult();
                }
            }
            else if (_evalResult.IsWarewolfRecordSetResult)
            {
                if (_evalResult is CommonFunctions.WarewolfEvalResult.WarewolfRecordSetResult listResult)
                {
                    return new DebugItemWarewolfRecordset(listResult.Item, _inputVariable, LabelText, "=", MockSelected).GetDebugItemResult();
                }
            } else { return new DebugItemStaticDataParams("", _inputVariable, LabelText, MockSelected).GetDebugItemResult(); }

            return new DebugItemStaticDataParams("", _inputVariable, LabelText, MockSelected).GetDebugItemResult();
        }        
    }
}
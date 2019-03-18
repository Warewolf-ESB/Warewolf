#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
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

#pragma warning disable S1541 // Methods and properties should not be too complex
        public override List<IDebugItemResult> GetDebugItemResult()
#pragma warning restore S1541 // Methods and properties should not be too complex
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
            else if (_evalResult.IsWarewolfAtomListresult && _evalResult is CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult atomListResult)
            {
                return new DebugItemWarewolfAtomListResult(atomListResult, "", "", _inputVariable, LabelText, "", "=", _isCalculate, MockSelected).GetDebugItemResult();
            }
            else if (_evalResult.IsWarewolfRecordSetResult && _evalResult is CommonFunctions.WarewolfEvalResult.WarewolfRecordSetResult recordsetResult)
            {
                return new DebugItemWarewolfRecordset(recordsetResult.Item, _inputVariable, LabelText, "=", MockSelected).GetDebugItemResult();
            }
            else
            {
                return new DebugItemStaticDataParams("", _inputVariable, LabelText, MockSelected).GetDebugItemResult();
            }

            return new DebugItemStaticDataParams("", _inputVariable, LabelText, MockSelected).GetDebugItemResult();
        }        
    }
}
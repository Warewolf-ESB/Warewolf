using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces;
using Newtonsoft.Json.Linq;
using WarewolfParserInterop;

namespace Warewolf.Storage
{
    public class ScopedEnvironment : IExecutionEnvironment
    {
        private readonly IExecutionEnvironment _inner;
        private readonly string _datasource;
        private readonly string _alias;

        public ScopedEnvironment(IExecutionEnvironment inner, string datasource, string alias)
        {
            _inner = inner;
            _datasource = datasource;
            _alias = alias;
        }

        #region Implementation of IExecutionEnvironment

        public CommonFunctions.WarewolfEvalResult Eval(string exp, int update, bool throwsifnotexists = true)
        {
            var updateDatasource = UpdateDataSourceWithIterativeValue(_datasource, update);
            return _inner.Eval(ReplaceUpdate(exp, updateDatasource), update, throwsifnotexists);
        }

        public CommonFunctions.WarewolfEvalResult EvalStrict(string exp, int update)
        {
            var updateDatasource = UpdateDataSourceWithIterativeValue(_datasource, update);
            return _inner.EvalStrict(ReplaceUpdate(exp, updateDatasource), update);
        }

        public void Assign(string exp, string value, int update)
        {
            var updateDatasource = UpdateDataSourceWithIterativeValue(_datasource, update);
            _inner.Assign(ReplaceUpdate(exp, updateDatasource), ReplaceUpdate(value, updateDatasource), 0);
        }

        private string ReplaceUpdate(string value, string magic)
        {
            return value.Replace(_alias, magic);
        }

        private string UpdateDataSourceWithIterativeValue(string datasource, int update)
        {
            return WarewolfDataEvaluationCommon.languageExpressionToString( WarewolfDataEvaluationCommon.parseLanguageExpression(datasource,update));
        }

        public void AssignWithFrame(IAssignValue value, int update)
        {
            var updateDatasource = UpdateDataSourceWithIterativeValue(_datasource, update);
            _inner.AssignWithFrame(new AssignValue(ReplaceUpdate(value.Name, _datasource), ReplaceUpdate(value.Value, _datasource)), update);
        }

        public int GetLength(string recordSetName)
        {
            return _inner.GetLength(recordSetName);
        }

        public int GetCount(string recordSetName)
        {
            return _inner.GetCount(recordSetName);
        }

        public IList<int> EvalRecordSetIndexes(string recordsetName, int update)
        {
            var updateDatasource = UpdateDataSourceWithIterativeValue(_datasource, update);
            return _inner.EvalRecordSetIndexes(ReplaceUpdate(recordsetName, updateDatasource), 0);
        }

        public bool HasRecordSet(string recordsetName)
        {
            return _inner.HasRecordSet(recordsetName);
        }

        public IList<string> EvalAsListOfStrings(string expression, int update)
        {
            var updateDatasource = UpdateDataSourceWithIterativeValue(_datasource, update);
            return _inner.EvalAsListOfStrings(ReplaceUpdate(expression, updateDatasource), 0);
        }

        public void EvalAssignFromNestedStar(string exp, CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult recsetResult, int update)
        {
            var updateDatasource = UpdateDataSourceWithIterativeValue(_datasource, update);
            _inner.EvalAssignFromNestedStar(ReplaceUpdate(exp, updateDatasource), recsetResult, 0);
        }

        public void EvalAssignFromNestedLast(string exp, CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult recsetResult, int update)
        {
            var updateDatasource = UpdateDataSourceWithIterativeValue(_datasource, update);
            _inner.EvalAssignFromNestedLast(ReplaceUpdate(exp, updateDatasource), recsetResult, 0);
        }

        public void EvalAssignFromNestedNumeric(string rawValue, CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult recsetResult, int update)
        {
            var updateDatasource = UpdateDataSourceWithIterativeValue(_datasource, update);
            _inner.EvalAssignFromNestedNumeric(ReplaceUpdate(rawValue, updateDatasource), recsetResult, 0);
        }

        public void EvalDelete(string exp, int update)
        {
            var updateDatasource = UpdateDataSourceWithIterativeValue(_datasource, update);
            _inner.EvalDelete(ReplaceUpdate(exp, updateDatasource), 0);
        }

        public void CommitAssign()
        {
            _inner.CommitAssign();
        }

        public void SortRecordSet(string sortField, bool descOrder, int update)
        {
            var updateDatasource = UpdateDataSourceWithIterativeValue(_datasource, update);
            _inner.SortRecordSet(ReplaceUpdate(sortField, updateDatasource), descOrder, 0);
        }

        public string ToStar(string expression)
        {
            return _inner.ToStar(expression.Replace(_alias, _datasource));
        }

        public IEnumerable<DataASTMutable.WarewolfAtom> EvalAsList(string searchCriteria, int update, bool throwsifnotexists = false)
        {
            var updateDatasource = UpdateDataSourceWithIterativeValue(_datasource, update);
            return _inner.EvalAsList(ReplaceUpdate(searchCriteria, updateDatasource), 0, throwsifnotexists);
        }

        public IEnumerable<int> EvalWhere(string expression, Func<DataASTMutable.WarewolfAtom, bool> clause, int update)
        {
            var updateDatasource = UpdateDataSourceWithIterativeValue(_datasource, update);
            return _inner.EvalWhere(ReplaceUpdate(expression, updateDatasource), clause, 0);
        }

        public void ApplyUpdate(string expression, Func<DataASTMutable.WarewolfAtom, DataASTMutable.WarewolfAtom> clause, int update)
        {
            var updateDatasource = UpdateDataSourceWithIterativeValue(_datasource, update);
            _inner.ApplyUpdate(ReplaceUpdate(expression, updateDatasource), clause, 0);
        }

        public HashSet<string> Errors
        {
            get
            {
                return _inner.Errors;
            }
        }
        public HashSet<string> AllErrors
        {
            get
            {
                return _inner.AllErrors;
            }
        }

        public void AddError(string error)
        {
            _inner.AddError(error);
        }

        public void AssignDataShape(string p)
        {
            _inner.AssignDataShape(p);
        }

        public string FetchErrors()
        {
            return _inner.FetchErrors();
        }

        public bool HasErrors()
        {
            return _inner.HasErrors();
        }

        public string EvalToExpression(string exp, int update)
        {
            var updateDatasource = UpdateDataSourceWithIterativeValue(_datasource, update);
            return _inner.EvalToExpression(ReplaceUpdate(exp, updateDatasource), 0);
        }

        public IEnumerable<CommonFunctions.WarewolfEvalResult> EvalForDataMerge(string exp, int update)
        {
            var updateDatasource = UpdateDataSourceWithIterativeValue(_datasource, update);
            return _inner.EvalForDataMerge(ReplaceUpdate(exp, updateDatasource), 0);
        }

        public void AssignUnique(IEnumerable<string> distinctList, IEnumerable<string> valueList, IEnumerable<string> resList, int update)
        {
            // consider not allow unique bob bob bob in select and apply
            _inner.AssignUnique(distinctList, valueList, resList, update);
        }

        public CommonFunctions.WarewolfEvalResult EvalForJson(string exp)
        {
            return _inner.EvalForJson(exp.Replace(_alias, _datasource));
        }

        public void AddToJsonObjects(string exp, JContainer jContainer)
        {
            _inner.AddToJsonObjects(exp, jContainer);
        }

        public void AssignJson(IEnumerable<IAssignValue> values, int update)
        {
            _inner.AssignJson(values, update);
        }

        public void AssignJson(IAssignValue value, int update)
        {
            _inner.AssignJson(value, update);
        }

        #endregion
    }
}
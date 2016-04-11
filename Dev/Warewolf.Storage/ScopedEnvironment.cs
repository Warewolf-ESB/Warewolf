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
            update = Update(exp, update);
            return _inner.Eval(exp.Replace(_alias, _datasource), update, throwsifnotexists);
        }

        public CommonFunctions.WarewolfEvalResult EvalStrict(string exp, int update)
        {
            update = Update(exp, update);
            return _inner.EvalStrict(exp.Replace(_alias, _datasource), update);
        }

        public void Assign(string exp, string value, int update)
        {
            update = Update(exp, update);
            _inner.Assign(ReplaceAliasWithDatasource(exp), value, update);
        }

        private string ReplaceAliasWithDatasource(string exp)
        {
            return exp.Replace(_alias, _datasource);
        }

        private int Update(string exp, int update)
        {
            update = exp.Contains(_alias) ? update : 0;
            return update;
        }

        public void AssignWithFrame(IAssignValue value, int update)
        {
            update = Update(value.Name, update);
            _inner.AssignWithFrame(new AssignValue(value.Name.Replace(_alias, _datasource), value.Value.Replace(_alias, _datasource)), update);
        }

        public int GetLength(string recordSetName)
        {
            return _inner.GetLength(ReplaceAliasWithDatasource(recordSetName));
        }

        public int GetCount(string recordSetName)
        {
            return _inner.GetCount(ReplaceAliasWithDatasource(recordSetName));
        }

        public IList<int> EvalRecordSetIndexes(string recordsetName, int update)
        {
            update = Update(recordsetName, update);
            return _inner.EvalRecordSetIndexes(ReplaceAliasWithDatasource(recordsetName), update);
        }

        public bool HasRecordSet(string recordsetName)
        {
            return _inner.HasRecordSet(ReplaceAliasWithDatasource(recordsetName));
        }

        public IList<string> EvalAsListOfStrings(string expression, int update)
        {
            update = Update(expression, update);
            return _inner.EvalAsListOfStrings(expression.Replace(_alias, _datasource), update);
        }

        public void EvalAssignFromNestedStar(string exp, CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult recsetResult, int update)
        {
            update = Update(exp, update);
            _inner.EvalAssignFromNestedStar(exp.Replace(_alias, _datasource), recsetResult, update);
        }

        public void EvalAssignFromNestedLast(string exp, CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult recsetResult, int update)
        {
            update = Update(exp, update);
            _inner.EvalAssignFromNestedLast(ReplaceAliasWithDatasource(exp), recsetResult, update);
        }

        public void EvalAssignFromNestedNumeric(string rawValue, CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult recsetResult, int update)
        {
            update = Update(rawValue, update);
            _inner.EvalAssignFromNestedNumeric(rawValue.Replace(_alias, _datasource), recsetResult, update);
        }

        public void EvalDelete(string exp, int update)
        {
            update = Update(exp, update);
            _inner.EvalDelete(exp.Replace(_alias, _datasource), update);
        }

        public void CommitAssign()
        {
            _inner.CommitAssign();
        }

        public void SortRecordSet(string sortField, bool descOrder, int update)
        {
            update = Update(sortField, update);
            _inner.SortRecordSet(ReplaceAliasWithDatasource(sortField), descOrder, update);
        }

        public string ToStar(string expression)
        {
            return _inner.ToStar(expression.Replace(_alias, _datasource));
        }

        public IEnumerable<DataASTMutable.WarewolfAtom> EvalAsList(string searchCriteria, int update, bool throwsifnotexists = false)
        {
            update = Update(searchCriteria, update);
            return _inner.EvalAsList(ReplaceAliasWithDatasource(searchCriteria), update, throwsifnotexists);
        }

        public IEnumerable<int> EvalWhere(string expression, Func<DataASTMutable.WarewolfAtom, bool> clause, int update)
        {
            update = Update(expression, update);
            return _inner.EvalWhere(expression.Replace(_alias, _datasource), clause, update);
        }

        public void ApplyUpdate(string expression, Func<DataASTMutable.WarewolfAtom, DataASTMutable.WarewolfAtom> clause, int update)
        {
            update = Update(expression, update);
            _inner.ApplyUpdate(expression.Replace(_alias, _datasource), clause, update);
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
            _inner.AssignDataShape(ReplaceAliasWithDatasource(p));
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
            update = Update(exp, update);
            return _inner.EvalToExpression(exp.Replace(_alias, _datasource), update);
        }

        public IEnumerable<CommonFunctions.WarewolfEvalResult> EvalForDataMerge(string exp, int update)
        {
            update = Update(exp, update);
            return _inner.EvalForDataMerge(exp.Replace(_alias, _datasource), update);
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

        public void AddToJsonObjects(string bob, JContainer jContainer)
        {
            _inner.AddToJsonObjects(bob, jContainer);
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
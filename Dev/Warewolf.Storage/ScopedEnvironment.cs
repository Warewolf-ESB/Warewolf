using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces;
using Newtonsoft.Json.Linq;
using Warewolf.Storage.Interfaces;
using WarewolfParserInterop;

namespace Warewolf.Storage
{
    public class ScopedEnvironment : IExecutionEnvironment
    {
        private readonly IExecutionEnvironment _inner;
        private string _datasource;
        private readonly string _alias;
        private readonly Func<string, int, string, string> _doReplace;
        public ScopedEnvironment(IExecutionEnvironment inner, string datasource, string alias)
        {
            _inner = inner;
            _datasource = datasource;
            _alias = alias;
            _doReplace = UpdateDataSourceWithIterativeValueFunction;
        }

        #region Implementation of IExecutionEnvironment

        public CommonFunctions.WarewolfEvalResult Eval(string exp, int update, bool throwsifnotexists = false,bool shouldEscape=false)
        {
            return _inner.Eval(UpdateDataSourceWithIterativeValue(_datasource, update, exp), update, throwsifnotexists,shouldEscape);
        }

        public CommonFunctions.WarewolfEvalResult EvalStrict(string exp, int update)
        {

            return _inner.EvalStrict(UpdateDataSourceWithIterativeValue(_datasource, update, exp), update);
        }

        public void Assign(string exp, string value, int update)
        {

            _inner.Assign(UpdateDataSourceWithIterativeValue(_datasource, update, exp), UpdateDataSourceWithIterativeValue(_datasource, update, value), 0);
        }

        private string UpdateDataSourceWithIterativeValueFunction(string datasource, int update, string exp)
        {
            return exp.Replace(_alias, datasource);
        }

        private string UpdateDataSourceWithIterativeValue(string datasource, int update, string exp)
        {
            var magic = _doReplace(datasource, update, exp);
            return magic;

        }

        public void AssignWithFrame(IAssignValue value, int update)
        {
            var name = UpdateDataSourceWithIterativeValue(_datasource, update, value.Name);
            var valuerep = UpdateDataSourceWithIterativeValue(_datasource, update, value.Value);
            _inner.AssignWithFrame(new AssignValue(name, valuerep), update);
        }

        public void AssignStrict(string exp, string value, int update)
        {
            var name = UpdateDataSourceWithIterativeValue(_datasource, update, exp);
            var valuerep = UpdateDataSourceWithIterativeValue(_datasource, update, value);
            _inner.AssignStrict(name, valuerep, update);
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

            return _inner.EvalRecordSetIndexes(UpdateDataSourceWithIterativeValue(_datasource, update, recordsetName), 0);
        }

        public bool HasRecordSet(string recordsetName)
        {
            return _inner.HasRecordSet(recordsetName);
        }

        public IList<string> EvalAsListOfStrings(string expression, int update)
        {
            return _inner.EvalAsListOfStrings(UpdateDataSourceWithIterativeValue(_datasource, update, expression), 0);
        }

        public void EvalAssignFromNestedStar(string exp, CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult recsetResult, int update)
        {

            _inner.EvalAssignFromNestedStar(UpdateDataSourceWithIterativeValue(_datasource, update, exp), recsetResult, 0);
        }

        public void EvalAssignFromNestedLast(string exp, CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult recsetResult, int update)
        {

            _inner.EvalAssignFromNestedLast(UpdateDataSourceWithIterativeValue(_datasource, update, exp), recsetResult, 0);
        }

        public void EvalAssignFromNestedNumeric(string rawValue, CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult recsetResult, int update)
        {

            _inner.EvalAssignFromNestedNumeric(UpdateDataSourceWithIterativeValue(_datasource, update, rawValue), recsetResult, 0);
        }

        public void EvalDelete(string exp, int update)
        {

            _inner.EvalDelete(UpdateDataSourceWithIterativeValue(_datasource, update, exp), 0);
        }

        public void CommitAssign()
        {
            _inner.CommitAssign();
        }

        public void SortRecordSet(string sortField, bool descOrder, int update)
        {

            _inner.SortRecordSet(UpdateDataSourceWithIterativeValue(_datasource, update, sortField), descOrder, 0);
        }

        public string ToStar(string expression)
        {
            return _inner.ToStar(expression.Replace(_alias, _datasource));
        }

        public IEnumerable<DataStorage.WarewolfAtom> EvalAsList(string searchCriteria, int update, bool throwsifnotexists = false)
        {

            return _inner.EvalAsList(UpdateDataSourceWithIterativeValue(_datasource, update, searchCriteria), 0, throwsifnotexists);
        }

        public IEnumerable<int> EvalWhere(string expression, Func<DataStorage.WarewolfAtom, bool> clause, int update)
        {

            return _inner.EvalWhere(UpdateDataSourceWithIterativeValue(_datasource, update, expression), clause, 0);
        }

        public void ApplyUpdate(string expression, Func<DataStorage.WarewolfAtom, DataStorage.WarewolfAtom> clause, int update)
        {
            var s = UpdateDataSourceWithIterativeValue(_datasource, update, expression);
            if (s.Contains("[[@"))
            {
                var res = _inner.Eval(s, 0);
                if (res.IsWarewolfAtomResult)
                {
                    var atom = res as CommonFunctions.WarewolfEvalResult.WarewolfAtomResult;
                    if (atom != null)
                    {
                        var resClause = clause.Invoke(atom.Item);
                        _inner.AssignJson(new AssignValue(s, resClause.ToString()), 0);
                    }
                }
            }
            else
            {
                _inner.ApplyUpdate(s, clause, 0);
            }
        }

        public HashSet<string> Errors => _inner.Errors;

        public HashSet<string> AllErrors => _inner.AllErrors;

        public void AddError(string error)
        {
            _inner.AddError(error);
        }

        public void AssignDataShape(string p)
        {
            _inner.AssignDataShape(p.Replace(_alias, _datasource));
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

            return _inner.EvalToExpression(exp, 0);
        }

        public IEnumerable<CommonFunctions.WarewolfEvalResult> EvalForDataMerge(string exp, int update)
        {
            return _inner.EvalForDataMerge(UpdateDataSourceWithIterativeValue(_datasource, update, exp), 0);
        }

        public void AssignUnique(IEnumerable<string> distinctList, IEnumerable<string> valueList, IEnumerable<string> resList, int update)
        {
            // consider not allow unique bob bob bob in select and apply
            _inner.AssignUnique(distinctList, valueList, resList, update);
        }

        public CommonFunctions.WarewolfEvalResult EvalForJson(string exp,bool shouldEscape=false)
        {
            return _inner.EvalForJson(exp.Replace(_alias, _datasource), shouldEscape);
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

        public JContainer EvalJContainer(string exp)
        {
            return _inner.EvalJContainer(exp);
        }

        public List<string> GetIndexes(string exp)
        {
           return _inner.GetIndexes(exp);
        }

        public int GetObjectLength(string recordSetName)
        {
            return _inner.GetObjectLength(recordSetName);
        }

        #endregion

        public void SetDataSource(string ds)
        {
            _datasource = ds;
        }

       
    }
}
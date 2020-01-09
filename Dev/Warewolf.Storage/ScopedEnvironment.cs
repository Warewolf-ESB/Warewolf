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
        readonly IExecutionEnvironment _inner;
        string _datasource;
        readonly string _alias;
        readonly Func<string, int, string, string> _doReplace;
        public ScopedEnvironment(IExecutionEnvironment inner, string datasource, string alias)
        {
            _inner = inner;
            _datasource = datasource;
            _alias = alias;
            _doReplace = UpdateDataSourceWithIterativeValueFunction;
        }

        #region Implementation of IExecutionEnvironment

        public CommonFunctions.WarewolfEvalResult Eval(string exp, int update) => Eval(exp, update, false, false);

        public CommonFunctions.WarewolfEvalResult Eval(string exp, int update, bool throwsifnotexists) => Eval(exp, update, throwsifnotexists, false);

        public CommonFunctions.WarewolfEvalResult Eval(string exp, int update, bool throwsifnotexists, bool shouldEscape) => _inner.Eval(UpdateDataSourceWithIterativeValue(_datasource, update, exp), update, throwsifnotexists, shouldEscape);

        public CommonFunctions.WarewolfEvalResult EvalStrict(string exp, int update) => _inner.EvalStrict(UpdateDataSourceWithIterativeValue(_datasource, update, exp), update);

        public void Assign(string exp, string value, int update)
        {

            _inner.Assign(UpdateDataSourceWithIterativeValue(_datasource, update, exp), UpdateDataSourceWithIterativeValue(_datasource, update, value), 0);
        }

        string UpdateDataSourceWithIterativeValueFunction(string datasource, int update, string exp) => exp.Replace(_alias, datasource);

        string UpdateDataSourceWithIterativeValue(string datasource, int update, string exp)
        {
            var magic = _doReplace(datasource, update, exp);
            return magic;

        }
		public void AssignWithFrame(IEnumerable<IAssignValue> values, int update)
		{
			foreach (var value in values)
			{
				AssignWithFrame(value, update);
			}
		}
		public void AssignWithFrame(IAssignValue values, int update)
        {
            var name = UpdateDataSourceWithIterativeValue(_datasource, update, values.Name);
            var valuerep = UpdateDataSourceWithIterativeValue(_datasource, update, values.Value);
            _inner.AssignWithFrame(new AssignValue(name, valuerep), update);
        }

        public void AssignStrict(string exp, string value, int update)
        {
            var name = UpdateDataSourceWithIterativeValue(_datasource, update, exp);
            var valuerep = UpdateDataSourceWithIterativeValue(_datasource, update, value);
            _inner.AssignStrict(name, valuerep, update);
        }

        public int GetLength(string recordSetName) => _inner.GetLength(recordSetName);

        public int GetCount(string recordSetName) => _inner.GetCount(recordSetName);

        public IList<int> EvalRecordSetIndexes(string recordsetName, int update) => _inner.EvalRecordSetIndexes(UpdateDataSourceWithIterativeValue(_datasource, update, recordsetName), 0);

        public bool HasRecordSet(string recordsetName) => _inner.HasRecordSet(recordsetName);

        public IList<string> EvalAsListOfStrings(string expression, int update) => _inner.EvalAsListOfStrings(UpdateDataSourceWithIterativeValue(_datasource, update, expression), 0);

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

        public string ToStar(string expression) => _inner.ToStar(expression.Replace(_alias, _datasource));
        public IEnumerable<DataStorage.WarewolfAtom> EvalAsList(string expression, int update) => EvalAsList(expression, update, false);

        public IEnumerable<DataStorage.WarewolfAtom> EvalAsList(string expression, int update, bool throwsifnotexists) => _inner.EvalAsList(UpdateDataSourceWithIterativeValue(_datasource, update, expression), 0, throwsifnotexists);

        public IEnumerable<int> EvalWhere(string expression, Func<DataStorage.WarewolfAtom, bool> clause, int update) => _inner.EvalWhere(UpdateDataSourceWithIterativeValue(_datasource, update, expression), clause, 0);

        public void ApplyUpdate(string expression, Func<DataStorage.WarewolfAtom, DataStorage.WarewolfAtom> clause, int update)
        {
            var s = UpdateDataSourceWithIterativeValue(_datasource, update, expression);
            if (s.Contains("[[@"))
            {
                var res = _inner.Eval(s, 0);
                if (res.IsWarewolfAtomResult && res is CommonFunctions.WarewolfEvalResult.WarewolfAtomResult atom)
                {
                    var resClause = clause.Invoke(atom.Item);
                    _inner.AssignJson(new AssignValue(s, resClause.ToString()), 0);
                }

            }
            else
            {
                _inner.ApplyUpdate(s, clause, 0);
            }
        }

        public HashSet<string> Errors => _inner.Errors;

        public HashSet<string> AllErrors => _inner.AllErrors;

        public Guid Id => _inner.Id;
        public Guid ParentId => _inner.ParentId;

        public void AddError(string error)
        {
            _inner.AddError(error);
        }

        public void AssignDataShape(string p)
        {
            _inner.AssignDataShape(p.Replace(_alias, _datasource));
        }

        public string FetchErrors() => _inner.FetchErrors();

        public bool HasErrors() => _inner.HasErrors();

        public string EvalToExpression(string exp, int update) => _inner.EvalToExpression(exp, 0);

        public IEnumerable<CommonFunctions.WarewolfEvalResult> EvalForDataMerge(string exp, int update) => _inner.EvalForDataMerge(UpdateDataSourceWithIterativeValue(_datasource, update, exp), 0);

        public void AssignUnique(IEnumerable<string> distinctList, IEnumerable<string> valueList, IEnumerable<string> resList, int update)
        {
            _inner.AssignUnique(distinctList, valueList, resList, update);
        }

        public CommonFunctions.WarewolfEvalResult EvalForJson(string exp) => EvalForJson(exp, false);

        public CommonFunctions.WarewolfEvalResult EvalForJson(string exp, bool shouldEscape) => _inner.EvalForJson(exp.Replace(_alias, _datasource), shouldEscape);

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

        public JContainer EvalJContainer(string exp) => _inner.EvalJContainer(exp);

        public List<string> GetIndexes(string exp) => _inner.GetIndexes(exp);

        public int GetObjectLength(string recordSetName) => _inner.GetObjectLength(recordSetName);

        #endregion

        public void SetDataSource(string ds)
        {
            _datasource = ds;
        }

        public IEnumerable<Tuple<string, DataStorage.WarewolfAtom>[]> EvalAsTable(string recordsetExpression, int update)
        {
            return _inner.EvalAsTable(recordsetExpression, update);
        }

        public IEnumerable<Tuple<string, DataStorage.WarewolfAtom>[]> EvalAsTable(string recordsetExpression, int update, bool throwsifnotexists)
        {
            return _inner.EvalAsTable(recordsetExpression, update, throwsifnotexists);
	}
        public string ToJson()
        {
            return "";
        }

        public IExecutionEnvironment Snapshot()
        {
            return _inner.Snapshot();
        }

        public void FromJson(string serializedEnv)
        {
            
        }
    }
}

#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Warewolf.Data;

namespace Warewolf.Storage.Interfaces
{
    public interface IExecutionEnvironment
    {
        Guid Id { get; }
        Guid ParentId { get; }
        CommonFunctions.WarewolfEvalResult Eval(string exp, int update);

        CommonFunctions.WarewolfEvalResult Eval(string exp, int update, bool throwsifnotexists);

        CommonFunctions.WarewolfEvalResult Eval(string exp, int update, bool throwsifnotexists, bool shouldEscape);

        CommonFunctions.WarewolfEvalResult EvalStrict(string exp, int update);

        void Assign(string exp, string value, int update);
        void AssignString(string exp, string value, int update);

        void AssignStrict(string exp, string value, int update);
		void AssignWithFrame(IEnumerable<IAssignValue> values, int update);

		void AssignWithFrame(IAssignValue values, int update);

        int GetLength(string recordSetName);

        int GetCount(string recordSetName);

        IList<int> EvalRecordSetIndexes(string recordsetName, int update);

        bool HasRecordSet(string recordsetName);

        IList<string> EvalAsListOfStrings(string expression, int update);

        void EvalAssignFromNestedStar(string exp, CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult recsetResult, int update);

        void EvalAssignFromNestedLast(string exp, CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult recsetResult, int update);

        void EvalAssignFromNestedNumeric(string rawValue, CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult recsetResult, int update);

        void EvalDelete(string exp, int update);

        void CommitAssign();

        void SortRecordSet(string sortField, bool descOrder, int update);

        string ToStar(string expression);
        IEnumerable<Tuple<string, DataStorage.WarewolfAtom>[]> EvalAsTable(string recordsetExpression, int update);
        IEnumerable<Tuple<string, DataStorage.WarewolfAtom>[]> EvalAsTable(string recordsetExpression, int update, bool throwsifnotexists);

		IEnumerable<DataStorage.WarewolfAtom> EvalAsList(string expression, int update);
		IEnumerable<DataStorage.WarewolfAtom> EvalAsList(string expression, int update, bool throwsifnotexists);

        IEnumerable<int> EvalWhere(string expression, Func<DataStorage.WarewolfAtom, bool> clause, int update);

        void ApplyUpdate(string expression, Func<DataStorage.WarewolfAtom, DataStorage.WarewolfAtom> clause, int update);

        HashSet<string> Errors { get; }
        HashSet<string> AllErrors { get; }

        void AddError(string error);

        void AssignDataShape(string p);

        string FetchErrors();

        bool HasErrors();

        string EvalToExpression(string exp, int update);

        IEnumerable<CommonFunctions.WarewolfEvalResult> EvalForDataMerge(string exp, int update);

        void AssignUnique(IEnumerable<string> distinctList, IEnumerable<string> valueList, IEnumerable<string> resList, int update);

        CommonFunctions.WarewolfEvalResult EvalForJson(string exp);

        CommonFunctions.WarewolfEvalResult EvalForJson(string exp, bool shouldEscape);

        void AddToJsonObjects(string exp, JContainer jContainer);

        void AssignJson(IEnumerable<IAssignValue> values, int update);

        void AssignJson(IAssignValue value, int update);

        JContainer EvalJContainer(string exp);

        List<string> GetIndexes(string exp);
        int GetObjectLength(string recordSetName);

        string ToJson();
        void FromJson(string serializedEnv);
        IExecutionEnvironment Snapshot();
        IEnumerable<(string scalarName, DataStorage.WarewolfAtom scalar)> EvalAllScalars();
        IEnumerable<(string recSetName, DataStorage.WarewolfRecordset recSet)> EvalAllRecordsets();
        IEnumerable<(string objectName, JContainer jObject)> EvalAllObjects();
        string EvalResultToString(CommonFunctions.WarewolfEvalResult result);
    }
}

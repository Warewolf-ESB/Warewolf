using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces;
using Newtonsoft.Json.Linq;

namespace Warewolf.Storage.Interfaces
{
    public interface IExecutionEnvironment
    {
        CommonFunctions.WarewolfEvalResult Eval(string exp, int update);

        CommonFunctions.WarewolfEvalResult Eval(string exp, int update, bool throwsifnotexists);

        CommonFunctions.WarewolfEvalResult Eval(string exp, int update, bool throwsifnotexists, bool shouldEscape);

        CommonFunctions.WarewolfEvalResult EvalStrict(string exp, int update);

        void Assign(string exp, string value, int update);

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
    }
}

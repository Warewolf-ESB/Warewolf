#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
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

using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Common.Interfaces;

namespace Warewolf.Storage
{

    public interface IExecutionEnvironment
    {
        WarewolfDataEvaluationCommon.WarewolfEvalResult Eval(string exp);

        bool Assign(string exp, string value);

        bool MultiAssign(IEnumerable<IAssignValue> values);

        bool AssignWithFrame(IAssignValue values);

        int GetEvaluationResultAsInt(string exp);
        int GetLength(string recordSetName);
        int GetCount(string recordSetName);
        IList<int> EvalRecordSetIndexes(string recordsetName);

        bool HasRecordSet(string recordsetName);

        IList<string> EvalAsListOfStrings(string expression);

        void EvalAssignFromNestedStar(string exp, WarewolfDataEvaluationCommon.WarewolfEvalResult.WarewolfAtomListresult recsetResult);

        void EvalAssignFromNestedLast(string exp, WarewolfDataEvaluationCommon.WarewolfEvalResult.WarewolfAtomListresult recsetResult);

        void EvalAssignFromNestedNumeric(string rawValue, WarewolfDataEvaluationCommon.WarewolfEvalResult.WarewolfAtomListresult recsetResult);

        void EvalDelete(string exp);

        void CommitAssign();
    }
    public class ExecutionEnvironment : IExecutionEnvironment
    {
        DataASTMutable.WarewolfEnvironment _env;
    
        public  ExecutionEnvironment()
        {
            _env = PublicFunctions.CreateEnv("");
        }

        public WarewolfDataEvaluationCommon.WarewolfEvalResult Eval(string exp)
        {
            return PublicFunctions.EvalEnvExpression(exp, _env);
        }

        public bool Assign(string exp,string value)
        {
            var envTemp =  PublicFunctions.EvalAssign(exp,value, _env);
            _env = envTemp;
            return true; //todo : decide on whether to catch here of just send exceptions on
        }


        public bool MultiAssign(IEnumerable<IAssignValue> values  )
        {
            var envTemp = PublicFunctions.EvalMultiAssign(values, _env);
            _env = envTemp;
            return true; //todo : decide on whether to catch here of just send exceptions on
        }

        public bool AssignWithFrame(IAssignValue values)
        {
            var envTemp = PublicFunctions.EvalAssignWithFrame(values, _env);
            _env = envTemp;
            return true; //todo : decide on whether to catch here of just send exceptions on
        }

        public int GetEvaluationResultAsInt(string exp)
        {
            var result = Eval(exp);
            if(result.IsWarewolfAtomResult)
            {
                // ReSharper disable PossibleNullReferenceException
                var x = (result as WarewolfDataEvaluationCommon.WarewolfEvalResult.WarewolfAtomResult).Item;
                // ReSharper restore PossibleNullReferenceException
                if(x.IsInt)
                {
                    var resultvalue = x as DataASTMutable.WarewolfAtom.Int;
                    // ReSharper disable PossibleNullReferenceException
                    return resultvalue.Item;
                    // ReSharper restore PossibleNullReferenceException
                }
                return 0;
            }
            else
            {
                // ReSharper disable PossibleNullReferenceException
                var x = (result as WarewolfDataEvaluationCommon.WarewolfEvalResult.WarewolfAtomListresult).Item.Last();
                // ReSharper restore PossibleNullReferenceException
                // ReSharper restore PossibleNullReferenceException
                if (x.IsInt)
                {
                    var resultvalue = x as DataASTMutable.WarewolfAtom.Int;
                    // ReSharper disable PossibleNullReferenceException
                    return resultvalue.Item;
                    // ReSharper restore PossibleNullReferenceException
                }
                return 0;
            }
        }

        public int GetLength(string recordSetName)
        {
            return _env.RecordSets[recordSetName].LastIndex;
        }

        public int GetCount(string recordSetName)
        {
            return _env.RecordSets[recordSetName].Count;
        }

        public IList<int> EvalRecordSetIndexes(string recordsetName)
        {
           
            return PublicFunctions.getIndexes(recordsetName,_env).ToList() ;
        }

        public bool HasRecordSet(string recordsetName)
        {
            var x = WarewolfDataEvaluationCommon.ParseLanguageExpression(recordsetName);
            if(x.IsRecordSetNameExpression)
            {
                var recsetName = x as LanguageAST.LanguageExpression.RecordSetNameExpression;
                // ReSharper disable PossibleNullReferenceException
                return _env.RecordSets.ContainsKey(recsetName.Item.Name);
                // ReSharper restore PossibleNullReferenceException
            }
            return false;
            
        }

        public IList<string> EvalAsListOfStrings(string expression)
        {
            var result = Eval(expression);
            if (result.IsWarewolfAtomResult)
            {
                // ReSharper disable PossibleNullReferenceException
                var x = (result as WarewolfDataEvaluationCommon.WarewolfEvalResult.WarewolfAtomResult).Item;
                // ReSharper restore PossibleNullReferenceException
                return new List<string> { WarewolfAtomToString(x) };
            }
            else
            {
                // ReSharper disable PossibleNullReferenceException
                // ReSharper disable PossibleNullReferenceException
                var x = (result as WarewolfDataEvaluationCommon.WarewolfEvalResult.WarewolfAtomListresult).Item;
                // ReSharper restore PossibleNullReferenceException
                return x.Select(WarewolfAtomToString).ToList();
            }
        }

        public static  string WarewolfAtomToString(DataASTMutable.WarewolfAtom a)
        {
            return PublicFunctions.AtomtoString(a);
        }

        public static string WarewolfEvalResultToString(WarewolfDataEvaluationCommon.WarewolfEvalResult result)
        {
         
            if (result.IsWarewolfAtomResult)
            {
                // ReSharper disable PossibleNullReferenceException
                var x = (result as WarewolfDataEvaluationCommon.WarewolfEvalResult.WarewolfAtomResult).Item;
                // ReSharper restore PossibleNullReferenceException
                return WarewolfAtomToString(x);
            }
            else
            {
                // ReSharper disable PossibleNullReferenceException
                var x = (result as WarewolfDataEvaluationCommon.WarewolfEvalResult.WarewolfAtomListresult).Item;
                StringBuilder res = new StringBuilder(); 
                for(int index  = 0; index < x.Count; index++)
                {
                    var warewolfAtom = x[index];
                    if(index==x.Count-1)
                    {
                        res.Append(warewolfAtom);
                    }
                    else
                    {
                        res.Append(warewolfAtom).Append(",");
                    }
                }
                return res.ToString();
            }
        }

        public void EvalAssignFromNestedStar(string exp, WarewolfDataEvaluationCommon.WarewolfEvalResult.WarewolfAtomListresult recsetResult)
        {
            for(int index = 0; index < recsetResult.Item.Count; index++)
            {
                var warewolfAtom = recsetResult.Item[index];
                Assign(exp.Replace("*", (index+1).ToString()), WarewolfAtomToString(warewolfAtom));
            }
        }

        public void EvalAssignFromNestedLast(string exp, WarewolfDataEvaluationCommon.WarewolfEvalResult.WarewolfAtomListresult recsetResult)
        {
            foreach(var warewolfAtom in recsetResult.Item)
            {
                Assign(exp.Replace("*", ""), WarewolfAtomToString(warewolfAtom));
            }
        }

        public void EvalAssignFromNestedNumeric(string exp, WarewolfDataEvaluationCommon.WarewolfEvalResult.WarewolfAtomListresult recsetResult)
        {
            if( recsetResult.Item.Any())
            Assign(exp, WarewolfAtomToString(recsetResult.Item.Last()));
        }

        public void EvalDelete(string exp)
        {
            _env =  PublicFunctions.EvalDelete(exp, _env);
        }

        public void CommitAssign()
        {
            _env = PublicFunctions.RemoveFraming(_env);
        }
    }
}

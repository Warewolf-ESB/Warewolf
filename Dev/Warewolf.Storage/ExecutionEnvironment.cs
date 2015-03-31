using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Dev2.Common.Common;
using Dev2.Common.Interfaces;
using WarewolfParserInterop;

namespace Warewolf.Storage
{

    public interface IExecutionEnvironment
    {
        WarewolfDataEvaluationCommon.WarewolfEvalResult Eval(string exp);

        WarewolfDataEvaluationCommon.WarewolfEvalResult EvalStrict(string exp);
        void Assign(string exp, string value);

        void MultiAssign(IEnumerable<IAssignValue> values);

        void AssignWithFrame(IAssignValue values);

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

        void SortRecordSet(string sortField, bool descOrder);

        string ToStar(string expression);

        IEnumerable<DataASTMutable.WarewolfAtom> EvalAsList(string searchCriteria);

        IEnumerable<int> EnvalWhere(string expression, Func<DataASTMutable.WarewolfAtom, bool> clause);

        void ApplyUpdate(string expression, Func<DataASTMutable.WarewolfAtom, DataASTMutable.WarewolfAtom> clause);

        IList<string> Errors { get; } 
        void AddError(string error);


    }
    public class ExecutionEnvironment : IExecutionEnvironment
    {
        DataASTMutable.WarewolfEnvironment _env;
    
        public  ExecutionEnvironment()
        {
            _env = PublicFunctions.CreateEnv("");
            Errors = new List<string>();
        }

        public WarewolfDataEvaluationCommon.WarewolfEvalResult Eval(string exp)
        {
            if (string.IsNullOrEmpty(exp))
            {
                return WarewolfDataEvaluationCommon.WarewolfEvalResult.NewWarewolfAtomResult(DataASTMutable.WarewolfAtom.Nothing);
            }
            return PublicFunctions.EvalEnvExpression(exp, _env);
            
        }

        public WarewolfDataEvaluationCommon.WarewolfEvalResult EvalStrict(string exp)
        {
            var res =  Eval(exp);
            if(IsNothing( res))
                throw new NullValueInVariableException("The expression"+exp+"has no value assigned",exp);
            return res;
        }

        public void Assign(string exp, string value)
        {
            if (string.IsNullOrEmpty(exp))
            {
                return ;
            }

            
            var envTemp =  PublicFunctions.EvalAssignWithFrame( new AssignValue( exp,value), _env);
            
            _env = envTemp;
            CommitAssign();
           
        }


        public void MultiAssign(IEnumerable<IAssignValue> values)
        {
            try
            {
            var envTemp = PublicFunctions.EvalMultiAssign(values, _env);
            _env = envTemp;
        }
            catch(Exception err)
            {

               Errors.Add(err.Message);
               throw;
            }
          

        }

        public void AssignWithFrame(IAssignValue values)
        {
            try
            {
            if (string.IsNullOrEmpty(values.Name))
            {
                return ;
            }

            var envTemp = PublicFunctions.EvalAssignWithFrame(values, _env);
            _env = envTemp;
            }
            catch (Exception err)
            {

                Errors.Add(err.Message);
                throw;
            }
        
  
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
                // ReSharper disable PossibleNullReferenceException
                // ReSharper disable PossibleNullReferenceException
                var warewolfAtomListresult = result as WarewolfDataEvaluationCommon.WarewolfEvalResult.WarewolfAtomListresult;
                if(warewolfAtomListresult != null)
                {
                    var x = warewolfAtomListresult.Item;
                    // ReSharper restore PossibleNullReferenceException
                    return x.Select(WarewolfAtomToString).ToList();
                }
                throw new Exception("bob");
            }

        public static  string WarewolfAtomToString(DataASTMutable.WarewolfAtom a)
        {
            return PublicFunctions.AtomtoString(a);
        }

        public static bool IsRecordSetName(string a)
        {
            try
            {
                var x = WarewolfDataEvaluationCommon.ParseLanguageExpression(a);
                return x.IsRecordSetNameExpression;
            }
            catch(Exception)
            {
                return false;
                
            }
        }

        public static bool IsValidVariableExpression(string expression, out string errorMessage)
        {
            errorMessage = "";
            try
            {
                var x = WarewolfDataEvaluationCommon.ParseLanguageExpression(expression);
                if (x.IsRecordSetExpression || x.IsScalarExpression)
                {
                    return true;
                }
            }
            catch (Exception e)
            {
                errorMessage = e.Message;
                return false;
            }
            return false;
        }

        public static string WarewolfEvalResultToString(WarewolfDataEvaluationCommon.WarewolfEvalResult result)
        {
         
            if (result.IsWarewolfAtomResult)
            {
                // ReSharper disable PossibleNullReferenceException
                var warewolfAtomResult = result as WarewolfDataEvaluationCommon.WarewolfEvalResult.WarewolfAtomResult;
                if(warewolfAtomResult != null)
                {
                    var x = warewolfAtomResult.Item;
                // ReSharper restore PossibleNullReferenceException
                return WarewolfAtomToString(x);
            }
                throw new Exception("null when f# said it should not be");
            }
                // ReSharper disable RedundantIfElseBlock
            else
                // ReSharper restore RedundantIfElseBlock
            {
                // ReSharper disable PossibleNullReferenceException
                var warewolfAtomListresult = result as WarewolfDataEvaluationCommon.WarewolfEvalResult.WarewolfAtomListresult;
                if(warewolfAtomListresult != null)
                {
                    var x = warewolfAtomListresult.Item;
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
                throw new Exception("null when f# said it should not be");
            }
        }

        public void EvalAssignFromNestedStar(string exp, WarewolfDataEvaluationCommon.WarewolfEvalResult.WarewolfAtomListresult recsetResult)
        {
            for(int index = 0; index < recsetResult.Item.Count; index++)
            {
                var warewolfAtom = recsetResult.Item[index];
                Assign(exp.Replace("*", (index+1).ToString(CultureInfo.InvariantCulture)), WarewolfAtomToString(warewolfAtom));
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

        public void SortRecordSet(string sortField, bool descOrder)
        {

            _env = PublicFunctions.SortRecset(sortField, descOrder, _env);
        }

        public string ToStar(string expression)
        {
            var exp = WarewolfDataEvaluationCommon.ParseLanguageExpression(expression);
            if(exp.IsRecordSetExpression)
            {
                var rec = exp as LanguageAST.LanguageExpression.RecordSetExpression;
                if(rec != null)
                {
                return "[["+rec.Item.Name+"(*)."+rec.Item.Column+"]]";
            }
            }
            return expression;
        }

        public IEnumerable<DataASTMutable.WarewolfAtom> EvalAsList(string expression)
        {
            var result = Eval(expression);
            if (result.IsWarewolfAtomResult)
            {
                // ReSharper disable PossibleNullReferenceException
                var warewolfAtomResult = result as WarewolfDataEvaluationCommon.WarewolfEvalResult.WarewolfAtomResult;
                if(warewolfAtomResult != null)
                {
                    var x = warewolfAtomResult.Item;
                // ReSharper restore PossibleNullReferenceException
                return new List<DataASTMutable.WarewolfAtom> { x };
            }
                throw new Exception("null when f# said it should not be");
            }
                // ReSharper disable RedundantIfElseBlock
            else
                // ReSharper restore RedundantIfElseBlock
            {
                // ReSharper disable PossibleNullReferenceException
                // ReSharper disable PossibleNullReferenceException
                var x = (result as WarewolfDataEvaluationCommon.WarewolfEvalResult.WarewolfAtomListresult).Item;
                // ReSharper restore PossibleNullReferenceException
                return x.ToList();
            }
        }

        public IEnumerable<int> EnvalWhere (string expression , Func<DataASTMutable.WarewolfAtom,bool> clause)
        {
            return PublicFunctions.EvalWhere(expression, _env, clause);
        }

        public void ApplyUpdate(string expression, Func<DataASTMutable.WarewolfAtom, DataASTMutable.WarewolfAtom> clause)
        {
            var temp = PublicFunctions.EvalUpdate(expression, _env,clause);
            _env = temp;

        }

        public IList<string> Errors { get; private set; }

        public void AddError(string error)
        {
            Errors.Add(error);
        }

        public static string ConvertToIndex(string outputVar, int i)
        {
            var output =  WarewolfDataEvaluationCommon.ParseLanguageExpression(outputVar);
            if(output.IsRecordSetExpression)
            {
                
                var outputidentifier = (output as LanguageAST.LanguageExpression.RecordSetExpression).Item;
                if(Equals(outputidentifier.Index, LanguageAST.Index.Star))
                return "[[" + outputidentifier.Name + "(" + i+ ")." +outputidentifier.Column+ "]]";
            }
            return outputVar;
        }

        public static bool IsRecordsetIdentifier(string assignVar)
        {
            try
            {
                var x = WarewolfDataEvaluationCommon.ParseLanguageExpression(assignVar);
                return x.IsRecordSetExpression;
            }
            catch (Exception)
            {
                return false;

            }
        }

        public static bool IsScalar(string assignVar)
        {
            try
            {
                var x = WarewolfDataEvaluationCommon.ParseLanguageExpression(assignVar);
                return x.IsScalarExpression;
            }
            catch (Exception)
            {
                return false;

            }
        }

        public static bool IsNothing(WarewolfDataEvaluationCommon.WarewolfEvalResult evalInp1)
        {
            return WarewolfDataEvaluationCommon.IsNothing(evalInp1);
        }
    }
}

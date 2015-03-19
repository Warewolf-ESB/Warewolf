using System.Collections.Generic;
using Dev2.Common.Interfaces;
using WarewolfParserInterop;

namespace Warewolf.Storage
{

    public interface IEnvironment
    {
        WarewolfDataEvaluationCommon.WarewolfEvalResult Eval(string exp);

        bool Assign(string exp, string value);

        bool MultiAssign(IEnumerable<IAssignValue> values);
    }
    public class  Environment : IEnvironment
    {
        DataASTMutable.WarewolfEnvironment _env;

        public  Environment()
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

        public static  string WarewolfAtomToString(DataASTMutable.WarewolfAtom a)
        {
            return PublicFunctions.AtomtoString(a);
        }
    }
}

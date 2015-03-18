using System.Collections.Generic;
using WarewolfParserInterop;

namespace Warewolf.Storage
{


    public class  Environment
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


        public bool MultiAssign(IList<IAssignValue> values  )
        {
            var envTemp = PublicFunctions.EvalMultiAssign(values, _env);
            _env = envTemp;
            return true; //todo : decide on whether to catch here of just send exceptions on
        }
    }
}

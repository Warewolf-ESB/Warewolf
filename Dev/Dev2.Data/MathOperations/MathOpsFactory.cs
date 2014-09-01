using System.Collections.Generic;
using System.Text;
using Dev2.Common.Interfaces;
using Dev2.Data.MathOperations;

namespace Dev2.MathOperations {
    public static class MathOpsFactory {

        private static string _mathFnDataList = string.Empty;
        private static IList<string> _rawMathFnList = new List<string>();

        public static string FetchMathFnDataList() {
            // we need to init it ;)
            if (_mathFnDataList == string.Empty) {
                InitMathFnRawData();
            }
            return _mathFnDataList;
        }

        public static IList<string> FetchMathFnStringList() {
            // we need to init it ;)
            if (_rawMathFnList.Count == 0) {
                InitMathFnRawData();
            }

            return _rawMathFnList;
        }

        public static IFunctionEvaluator CreateFunctionEvaluator() {
            return new FunctionEvaluator();
        }

        public static IEvaluationFunction CreateEvaluationExpressionTO(string expression) {
            return new EvaluationFunctionTO(expression);
        }

        public static IFunction CreateFunction(string functionName, IList<string> arguments, IList<string> argumentDescriptions, string description) {
            return new Function(functionName, arguments, argumentDescriptions, description);
        }

        public static IFunction CreateFunction() {
            return new Function();
        }

        public static IFrameworkRepository<IFunction> FunctionRepository() {
            return new FunctionRepository();
        }


        private static void InitMathFnRawData(){
            IFrameworkRepository<IFunction> repo = FunctionRepository();
            repo.Load();
            ICollection<IFunction> fns = repo.All();
            StringBuilder tmp = new StringBuilder("<DL>");

            // build list
            foreach (IFunction f in fns) {
                _rawMathFnList.Add(f.FunctionName);
                tmp.Append("<");
                tmp.Append(f.FunctionName);
                tmp.Append("/>");
            }
            tmp.Append("</DL>");

            _mathFnDataList = tmp.ToString();
        }
    }
}

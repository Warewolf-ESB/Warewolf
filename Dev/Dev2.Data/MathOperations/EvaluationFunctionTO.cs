using Dev2.MathOperations;

namespace Dev2.Data.MathOperations {
    public class EvaluationFunctionTO : IEvaluationFunction {

        private readonly string _function;

        internal EvaluationFunctionTO(string function) {
            _function = function;
        }

        public string Function {
            get { return _function; }
        }
    }
}

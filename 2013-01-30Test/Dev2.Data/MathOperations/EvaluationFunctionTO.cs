using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dev2.MathOperations {
    public class EvaluationFunctionTO : IEvaluationFunction {

        private string _function;

        internal EvaluationFunctionTO(string function) {
            _function = function;
        }

        public string Function {
            get { return _function; }
        }
    }
}

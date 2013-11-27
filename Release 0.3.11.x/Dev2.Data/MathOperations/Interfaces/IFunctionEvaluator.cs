using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.DataList.Contract;

namespace Dev2.MathOperations {

    public interface IFunctionEvaluator {
        // You have broken one of the MOST CORE RULES OF THIS INDUSTRY - NEVER CHANGE AN INTERFACE DEF ONCE IT IS SET!!!!!!!!
        // When was it changed, is the comment above implied since I opened the file????!!!!!!! (nice to add the extra '!!!!!!!')
        string EvaluateFunction(IEvaluationFunction expressionTO, Guid dlID, out ErrorResultTO errors);
        bool TryEvaluateFunction(IEvaluationFunction expressionTO, out string evaluation, out string error);
        bool TryEvaluateFunction(string expression, out string evaluation, out string error);
        bool TryEvaluateFunction<T>(List<T> value, string expression, out string evaluation, out string error) where T : IConvertible;
    }
}

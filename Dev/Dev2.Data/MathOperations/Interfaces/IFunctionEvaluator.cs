using System;
using System.Collections.Generic;
using Dev2.DataList.Contract;

namespace Dev2.MathOperations {

    public interface IFunctionEvaluator {
        string EvaluateFunction(IEvaluationFunction expressionTO, Guid dlID, out ErrorResultTO errors);
        bool TryEvaluateFunction(IEvaluationFunction expressionTO, out string evaluation, out string error);
        bool TryEvaluateFunction(string expression, out string evaluation, out string error);
        bool TryEvaluateFunction<T>(List<T> value, string expression, out string evaluation, out string error) where T : IConvertible;
    }
}

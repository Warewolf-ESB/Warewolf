using System.Collections.Generic;
using Dev2.Studio.Core.Models;

namespace Dev2.Studio.Core.Interfaces {
    public interface IDecisionType {
        string DecisionTypeName { get; set; }
        List<OperatorType> OperatorTypes { get; set; }
        string StringDecorator { get; }
        string FunctionName { get; }
        bool IsValid { get; }
        string GetExpression();
        string BuildStringExpression(string functionName, string decorator, string expression, string op, object value, object endvalue);
    }
}

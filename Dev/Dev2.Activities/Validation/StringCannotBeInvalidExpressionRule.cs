using System;
using Dev2.Providers.Errors;
using Dev2.Providers.Validation.Rules;

namespace Dev2.Validation
{
    public class StringCannotBeInvalidExpressionRule : Rule<string>
    {
        public StringCannotBeInvalidExpressionRule(string valueToCheck, Action onInvalid = null)
            : base(valueToCheck, onInvalid)
        {
        }

        public override IActionableErrorInfo Check()
        {
            string outputValue;
            return ValueToCheck.TryParseVariables(out outputValue, OnInvalid);
        }

    }
}
using System;
using Dev2.Providers.Errors;

namespace Dev2.Providers.Validation.Rules
{
    public class IsNumericRule : Rule<string>
    {
        public IsNumericRule(Func<string> getValue)
            : base(getValue)
        {
            ErrorText = "must be a whole number";
        }

        public override IActionableErrorInfo Check()
        {
            int value;
            return !int.TryParse(GetValue(), out value) ? CreatError() : null;
        }
    }
}



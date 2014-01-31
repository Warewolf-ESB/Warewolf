using System;
using Dev2.Providers.Errors;

namespace Dev2.Providers.Validation.Rules
{
    public class IsNumericRule : Rule<string>
    {
        public IsNumericRule(Func<string> getValue)
            : base(getValue)
        {
        }

        public override IActionableErrorInfo Check()
        {
            int value;
            if(!int.TryParse(GetValue(), out value))
            {
                return new ActionableErrorInfo(DoError)
                {
                    Message = LabelText + " value must be a whole number."
                };
            }
            return null;
        }
    }
}



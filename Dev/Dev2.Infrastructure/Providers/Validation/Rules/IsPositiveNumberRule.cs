using System;
using Dev2.Providers.Errors;

namespace Dev2.Providers.Validation.Rules
{
    public class IsPositiveNumberRule : Rule<string>
    {
        public IsPositiveNumberRule(Func<string> getValue)
            : base(getValue)
        {
        }

        public override IActionableErrorInfo Check()
        {
            int value;
            if(int.TryParse(GetValue(), out value))
            {
                if(value < 0)
                {
                    return new ActionableErrorInfo(DoError)
                    {
                        Message = LabelText + " value must be a positive whole number."
                    };
                }
            }
            return null;
        }
    }
}



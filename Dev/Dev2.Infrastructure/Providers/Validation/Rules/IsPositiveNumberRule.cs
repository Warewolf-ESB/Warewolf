using System;
using Dev2.Providers.Errors;

namespace Dev2.Providers.Validation.Rules
{
    public class IsPositiveNumberRule : Rule<string>
    {
        public IsPositiveNumberRule(Func<string> getValue, Action onInvalid = null)
            : base(getValue, onInvalid)
        {
        }

        public override IActionableErrorInfo Check()
        {
            int value;
            if(int.TryParse(GetValue(), out value))
            {
                if(value < 0)
                {
                    return new ActionableErrorInfo(OnInvalid)
                    {
                        Message = "The value must be a positive whole number.",
                        FixData = "Please provide a positive whole number for this field."
                    };
                }
            }
            return null;
        }
    }
}



using System;
using Dev2.Providers.Errors;

namespace Dev2.Providers.Validation.Rules
{
    public class IsPositiveNumberRule : Rule<string>
    {
        public IsPositiveNumberRule(Func<string> getValue)
            : base(getValue)
        {
            ErrorText = "must be a positive whole number";
        }

        public override IActionableErrorInfo Check()
        {
            var isValid = false;
            int value;
            if(int.TryParse(GetValue(), out value))
            {
                if(value >= 0)
                {
                    isValid = true;
                }
            }
            return isValid ? null : CreatError();
        }
    }
}



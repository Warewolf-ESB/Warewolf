using System;
using Dev2.Providers.Errors;

namespace Dev2.Providers.Validation.Rules
{
    public class IsPositiveNumberRule : Rule<string>
    {
        public IsPositiveNumberRule(Func<string> getValue)
            : base(getValue)
        {
            ErrorText = "must be a real number";
        }

        public override IActionableErrorInfo Check()
        {
            var isValid = false;
            int x;
            var value = GetValue();
            if(int.TryParse(value, out x))
            {
                if(x >= 0)
                {
                    isValid = true;
                }
            }
            return isValid ? null : CreatError();
        }
    }
}



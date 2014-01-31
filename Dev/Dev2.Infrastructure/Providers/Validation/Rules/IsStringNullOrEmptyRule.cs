using System;
using Dev2.Providers.Errors;

namespace Dev2.Providers.Validation.Rules
{
    public class IsStringNullOrEmptyRule : Rule<string>
    {
        public IsStringNullOrEmptyRule(Func<string> getValue)
            : base(getValue)
        {
        }

        public override IActionableErrorInfo Check()
        {
            var value = GetValue();
            if(string.IsNullOrEmpty(value))
            {
                return new ActionableErrorInfo(DoError)
                {
                    Message = LabelText + " value cannot be empty or null."
                };
            }
            return null;
        }
    }
}
using System;
using Dev2.Providers.Errors;

namespace Dev2.Providers.Validation.Rules
{
    public class IsStringNullOrEmptyRule : Rule<string>
    {
        public IsStringNullOrEmptyRule(Func<string> getValue, Action onInvalid = null)
            : base(getValue, onInvalid)
        {
        }

        public override IActionableErrorInfo Check()
        {
            var value = GetValue();
            if(string.IsNullOrEmpty(value))
            {
                return new ActionableErrorInfo(OnInvalid)
                {
                    Message = "The value cannot be empty or null.",
                    FixData = "Please provide a value for this field."
                };
            }
            return null;
        }
    }
}
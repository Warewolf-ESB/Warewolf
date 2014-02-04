using System;
using Dev2.Providers.Errors;

namespace Dev2.Providers.Validation.Rules
{
    public class IsStringNullOrEmptyRule : Rule<string>
    {
        public IsStringNullOrEmptyRule(Func<string> getValue)
            : base(getValue)
        {
            ErrorText = "cannot be empty or null";
        }

        public override IActionableErrorInfo Check()
        {
            var value = GetValue();
            return string.IsNullOrEmpty(value) ? CreatError() : null;
        }
    }
}
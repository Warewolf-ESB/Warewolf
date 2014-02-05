using System;
using Dev2.Providers.Errors;

namespace Dev2.Providers.Validation.Rules
{
    public class IsStringEmptyRule : Rule<string>
    {
        public IsStringEmptyRule(Func<string> getValue)
            : base(getValue)
        {
            ErrorText = "cannot be empty";
        }

        public override IActionableErrorInfo Check()
        {
            var value = GetValue();
            return string.IsNullOrEmpty(value) ? CreatError() : null;
        }
    }
}
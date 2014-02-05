using System;
using Dev2.Providers.Errors;

namespace Dev2.Providers.Validation.Rules
{
    public class IsStringEmptyOrWhiteSpaceRule : Rule<string>
    {
        public IsStringEmptyOrWhiteSpaceRule(Func<string> getValue)
            : base(getValue)
        {
            ErrorText = "cannot be empty or only white space";
        }

        public override IActionableErrorInfo Check()
        {
            var value = GetValue();
            return string.IsNullOrWhiteSpace(value) ? CreatError() : null;
        }
    }
}
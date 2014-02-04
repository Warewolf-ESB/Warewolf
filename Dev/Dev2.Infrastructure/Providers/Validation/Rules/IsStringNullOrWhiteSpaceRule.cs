using System;
using Dev2.Providers.Errors;

namespace Dev2.Providers.Validation.Rules
{
    public class IsStringNullOrWhiteSpaceRule : Rule<string>
    {
        public IsStringNullOrWhiteSpaceRule(Func<string> getValue)
            : base(getValue)
        {
            ErrorText = "cannot be empty, null or white space only";
        }

        public override IActionableErrorInfo Check()
        {
            var value = GetValue();
            return string.IsNullOrWhiteSpace(value) ? CreatError() : null;
        }
    }
}
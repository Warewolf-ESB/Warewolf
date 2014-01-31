using System;
using Dev2.Providers.Errors;

namespace Dev2.Providers.Validation.Rules
{
    public class IsStringNullOrWhiteSpaceRule : Rule<string>
    {
        public IsStringNullOrWhiteSpaceRule(Func<string> getValue)
            : base(getValue)
        {
        }

        public override IActionableErrorInfo Check()
        {
            var value = GetValue();
            if(string.IsNullOrWhiteSpace(value))
            {
                return new ActionableErrorInfo(DoError)
                {
                    Message = LabelText + " value cannot be empty, null or white space only."
                };
            }
            return null;
        }
    }
}
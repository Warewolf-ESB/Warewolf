using System;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;

namespace Dev2.Providers.Validation.Rules
{
    public class IsRequiredWhenOtherIsNotEmptyRule : Rule<string>
    {
        readonly Func<string> _otherValue;

        public IsRequiredWhenOtherIsNotEmptyRule(Func<string> getValue, Func<string> otherValue)
            : base(getValue)
        {
            VerifyArgument.IsNotNull("otherValue", otherValue);
            _otherValue = otherValue;
            ErrorText = "cannot be empty";
        }

        public override IActionableErrorInfo Check()
        {
            var otherValue = _otherValue();
            if(!string.IsNullOrEmpty(otherValue))
            {
                var value = GetValue();
                return string.IsNullOrEmpty(value) ? CreatError() : null;
            }
            return null;
        }
    }
}
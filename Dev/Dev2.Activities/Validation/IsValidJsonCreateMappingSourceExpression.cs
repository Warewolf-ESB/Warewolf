using System;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Providers.Errors;
using Dev2.Providers.Validation.Rules;
using Dev2.TO;

namespace Dev2.Validation
{
    public class IsValidJsonCreateMappingSourceExpression : Rule<string>
    {
        public IsValidJsonCreateMappingSourceExpression(Func<string> getValue)
            : base(getValue)
        {
        }

        public override IActionableErrorInfo Check()
        {
            var to = GetValue.Invoke();
            ErrorText = JsonMappingCompoundTo.ValidateInput(to);
            if (string.IsNullOrEmpty(ErrorText))
            {
                return null;
            }

            return new ActionableErrorInfo
            {
                ErrorType = ErrorType.Critical,
                Message = ErrorText,
            };
        }
    }
}
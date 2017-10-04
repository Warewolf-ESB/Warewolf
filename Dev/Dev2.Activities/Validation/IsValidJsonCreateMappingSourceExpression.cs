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

        /*
        protected override bool IsValid(JsonMappingTo item)
        {
            try
            {
                if (string.IsNullOrEmpty(JsonMappingCompoundTo.IsValidJsonMappingInput(
                    item.SourceName,
                    item.DestinationName)))
                    return true;

            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }
         * */

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
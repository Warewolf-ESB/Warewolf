using System;
using Dev2.Providers.Errors;

namespace Dev2.Providers.Validation.Rules
{
    public class ValueCannotBeNullRule : Rule
    {
        public ValueCannotBeNullRule(object valueToCheck, Action onInvalid = null)
            : base(valueToCheck, onInvalid)
        {
        }

        public override IActionableErrorInfo Check()
        {
            if(ValueToCheck == null)
            {
                return new ActionableErrorInfo(OnInvalid)
                {
                    Message = "The value cannot be null.",
                    FixData = "Please provide a value for this field."
                };
            }
            return null;
        }
    }
}
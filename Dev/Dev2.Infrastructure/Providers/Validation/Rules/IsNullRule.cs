using System;
using Dev2.Providers.Errors;

namespace Dev2.Providers.Validation.Rules
{
    public class IsNullRule : Rule<object>
    {
        public IsNullRule(Func<object> getValue, Action onInvalid = null)
            : base(getValue, onInvalid)
        {
        }

        public override IActionableErrorInfo Check()
        {
            var value = GetValue();
            if(value == null)
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
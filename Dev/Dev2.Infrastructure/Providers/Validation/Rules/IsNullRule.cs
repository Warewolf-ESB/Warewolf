using System;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;

namespace Dev2.Providers.Validation.Rules
{
    public class IsNullRule : Rule<object>
    {
        public IsNullRule(Func<object> getValue)
            : base(getValue)
        {
            ErrorText = "cannot be null";
        }

        public override IActionableErrorInfo Check()
        {
            var value = GetValue();
            return value == null ? CreatError() : null;
        }
    }
}
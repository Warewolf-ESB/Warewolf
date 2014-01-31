using System;
using Dev2.Providers.Errors;

namespace Dev2.Providers.Validation.Rules
{
    public abstract class Rule<T> : RuleBase
    {
        protected readonly Func<T> GetValue;

        protected Rule(Func<T> getValue)
        {
            VerifyArgument.IsNotNull("getValue", getValue);
            GetValue = getValue;
        }
    }

    public abstract class RuleBase
    {
        protected RuleBase()
        {
            LabelText = "The";
        }

        public abstract IActionableErrorInfo Check();

        public string LabelText { get; set; }
        public Action DoError { get; set; }
    }
}
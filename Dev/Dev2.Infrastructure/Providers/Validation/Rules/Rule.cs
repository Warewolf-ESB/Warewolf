using System;
using Dev2.Providers.Errors;

namespace Dev2.Providers.Validation.Rules
{
    public abstract class Rule<T> : RuleBase
    {
        protected readonly Func<T> GetValue;
        protected Action OnInvalid { get; private set; }

        protected Rule(Func<T> getValue, Action onInvalid)
        {
            VerifyArgument.IsNotNull("getValue", getValue);
            GetValue = getValue;
            OnInvalid = onInvalid;
        }
    }

    public abstract class RuleBase
    {
        public abstract IActionableErrorInfo Check();
    }
}
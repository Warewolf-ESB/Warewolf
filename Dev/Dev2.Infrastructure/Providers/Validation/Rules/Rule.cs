using System;
using Dev2.Providers.Errors;

namespace Dev2.Providers.Validation.Rules
{
    public abstract class Rule<T> : RuleBase
    {
        public T ValueToCheck { get; private set; }
        public Action OnInvalid { get; private set; }

        protected Rule(T valueToCheck, Action onInvalid)
        {
            ValueToCheck = valueToCheck;
            OnInvalid = onInvalid;
        }
    }

    public abstract class Rule : Rule<object>
    {
        protected Rule(object valueToCheck, Action onInvalid)
            : base(valueToCheck, onInvalid)
        {
        }
    }

    public abstract class RuleBase
    {
        public abstract IActionableErrorInfo Check();
    }
}
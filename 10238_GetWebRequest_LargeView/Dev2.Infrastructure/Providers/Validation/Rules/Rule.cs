using Dev2.Providers.Errors;

namespace Dev2.Providers.Validation.Rules
{
    public abstract class Rule<T>:RuleBase
    {
        public T ValueToCheck { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        protected Rule(T valueToCheck)
        {
            ValueToCheck = valueToCheck;
        }
    }

    public abstract class Rule : Rule<object>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        protected Rule(object valueToCheck)
            : base(valueToCheck)
        {
        }
    }

    public abstract class RuleBase
    {
        public abstract IErrorInfo Check();
    }
}
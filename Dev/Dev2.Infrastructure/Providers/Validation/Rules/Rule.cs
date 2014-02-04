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
            ErrorText = "value is invalid.";
        }

        public string LabelText { get; set; }
        public string ErrorText { get; set; }
        public Action DoError { get; set; }

        public abstract IActionableErrorInfo Check();

        protected IActionableErrorInfo CreatError()
        {
            return new ActionableErrorInfo(DoError)
            {
                Message = string.Format("{0} {1}", LabelText, ErrorText)
            };
        }
    }
}
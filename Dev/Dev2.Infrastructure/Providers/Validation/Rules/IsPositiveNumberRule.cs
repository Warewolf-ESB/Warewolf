using System;
using Dev2.Providers.Errors;

namespace Dev2.Providers.Validation.Rules
{
    public class IsPositiveNumberRule : Rule
    {
        #region Overrides of Rule

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public IsPositiveNumberRule(string valueToCheck, Action onInvalid = null)
            : base(valueToCheck, onInvalid)
        {
        }

        public override IActionableErrorInfo Check()
        {
            int value;
            if(int.TryParse(ValueToCheck.ToString(), out value))
            {
                if(value < 0)
                {
                    return new ActionableErrorInfo(OnInvalid)
                    {
                        Message = "The value must be a positive whole number.",
                        FixData = "Please provide a positive whole number for this field."
                    };
                }
            }
            return null;
        }

        #endregion
    }
}



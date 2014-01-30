using System;
using Dev2.Providers.Errors;

namespace Dev2.Providers.Validation.Rules
{


    public class IsNumericRule : Rule
    {
        #region Overrides of Rule

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public IsNumericRule(string valueToCheck, Action onInvalid = null)
            : base(valueToCheck, onInvalid)
        {
        }

        public override IActionableErrorInfo Check()
        {
            decimal value;
            if(decimal.TryParse(ValueToCheck.ToString(), out value))
            {
                return new ActionableErrorInfo(OnInvalid)
                {
                    Message = "The value must be a whole number.",
                    FixData = "Please provide a whole number for this field."
                };
            }
            return null;
        }

        #endregion
    }
}



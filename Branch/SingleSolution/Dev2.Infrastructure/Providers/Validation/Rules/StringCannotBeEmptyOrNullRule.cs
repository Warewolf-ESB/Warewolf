using Dev2.Providers.Errors;

namespace Dev2.Providers.Validation.Rules
{
    public class StringCannotBeEmptyOrNullRule : Rule
    {
        #region Overrides of Rule

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public StringCannotBeEmptyOrNullRule(object valueToCheck)
            : base(valueToCheck)
        {
        }

        public override IErrorInfo Check()
        {
            if(ValueToCheck == null)
            {
                return new ErrorInfo
                {
                    Message = "The value cannot be null.",
                    FixData = "Please provide a value for this field."
                };
            }
            return null;
        }

        #endregion
    }
}
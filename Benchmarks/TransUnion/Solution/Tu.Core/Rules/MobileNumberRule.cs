using Tu.Extensions;

namespace Tu.Rules
{
    public class MobileNumberRule : Rule
    {
        public MobileNumberRule(IValidator validator, string fieldName)
            : base(validator, fieldName)
        {
        }

        public override bool IsValid(object obj)
        {
            var value = obj.ToStringSafe();

            return IsValid(() => Validator.IsNullOrEmpty(value), "")
                   || (
                          IsValid(() => Validator.IsLengthEqual(value, 10), "Length Out Of Range")
                          && IsValid(() => Validator.IsNumeric(value), "Is Not Numeric")
                      );
        }
    }
}
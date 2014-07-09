using Tu.Extensions;

namespace Tu.Rules
{
    public class EmailAddressRule : Rule
    {
        public EmailAddressRule(IValidator validator, string fieldName)
            : base(validator, fieldName)
        {
        }

        public override bool IsValid(object obj)
        {
            var value = obj.ToStringSafe();

            return IsValid(() => Validator.IsNullOrEmpty(value), "")
                   || (
                          IsValid(() => Validator.IsLengthLessThanOrEqualTo(value, 50), "Length Out Of Range")
                          && IsValid(() => Validator.IsValidEmailAddress(value), "Is Invalid")
                      );
        }
    }
}
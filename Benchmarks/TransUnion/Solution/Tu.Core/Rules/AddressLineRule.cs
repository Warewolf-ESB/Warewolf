using Tu.Extensions;

namespace Tu.Rules
{
    public class AddressLineRule : Rule
    {
        public AddressLineRule(IValidator validator, string fieldName)
            : base(validator, fieldName)
        {
        }

        public override bool IsValid(object obj)
        {
            var value = obj.ToStringSafe();

            return IsValid(() => Validator.IsNullOrEmpty(value), "")
                   || IsValid(() => Validator.IsLengthLessThanOrEqualTo(value, 100), "Length Out Of Range");
        }
    }
}
using Tu.Extensions;

namespace Tu.Rules
{
    public class PostalCodeRule : Rule
    {
        public PostalCodeRule(IValidator validator, string fieldName)
            : base(validator, fieldName)
        {
        }

        public override bool IsValid(object obj)
        {
            var value = obj.ToStringSafe();

            return IsValid(() => Validator.IsNullOrEmpty(value), "")
                   || IsValid(() => Validator.IsLengthLessThanOrEqualTo(value, 6), "Length Out Of Range");
        }
    }
}
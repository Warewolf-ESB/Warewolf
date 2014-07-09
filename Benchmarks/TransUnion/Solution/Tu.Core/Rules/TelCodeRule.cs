using Tu.Extensions;

namespace Tu.Rules
{
    public class TelCodeRule : Rule
    {
        public TelCodeRule(IValidator validator, string fieldName)
            : base(validator, fieldName)
        {
        }

        public override bool IsValid(object obj)
        {
            var value = obj.ToStringSafe();

            return IsValid(() => Validator.IsNullOrEmpty(value), "")
                   || IsValid(() => Validator.IsLengthEqual(value, 3), "Length Not In Range");
        }
    }
}
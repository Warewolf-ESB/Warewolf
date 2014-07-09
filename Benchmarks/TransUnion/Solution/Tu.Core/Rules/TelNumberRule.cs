using Tu.Extensions;

namespace Tu.Rules
{
    public class TelNumberRule : Rule
    {
        public TelNumberRule(IValidator validator, string fieldName)
            : base(validator, fieldName)
        {
        }

        public override bool IsValid(object obj)
        {
            var value = obj.ToStringSafe();

            return IsValid(() => Validator.IsNullOrEmpty(value), "")
                   || IsValid(() => Validator.IsLengthEqual(value, 7), "Length Not In Range");
        }
    }
}
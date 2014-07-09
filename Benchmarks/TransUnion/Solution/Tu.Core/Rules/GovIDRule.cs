using Tu.Extensions;

namespace Tu.Rules
{
    public class GovIDRule : Rule
    {
        public GovIDRule(IValidator validator, string fieldName)
            : base(validator, fieldName)
        {
        }

        public override bool IsValid(object obj)
        {
            var value = obj.ToStringSafe();

            return IsValid(() => !Validator.IsNullOrEmpty(value), "Is Empty")
                   && IsValid(() => Validator.IsLengthEqual(value, 13), "Wrong Length")
                   && IsValid(() => Validator.IsNumeric(value), "Contains Non-Numeric Values")
                   && IsValid(() => !Validator.StartsWith(value, "0000"), "Starts with 4 or More Zeros");
        }
    }
}
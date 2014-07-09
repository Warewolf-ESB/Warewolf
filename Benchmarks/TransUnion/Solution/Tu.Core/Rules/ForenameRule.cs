using Tu.Extensions;

namespace Tu.Rules
{
    public class ForenameRule : Rule
    {
        public ForenameRule(IValidator validator, string fieldName)
            : base(validator, fieldName)
        {
        }

        public override bool IsValid(object obj)
        {
            var value = obj.ToStringSafe();

            return IsValid(() => !Validator.IsNullOrEmpty(value), "Are Null")
                   && IsValid(() => !Validator.IsNumeric(value), "Contains Numeric Values")
                   && IsValid(() => Validator.IsUpper(value), "Is Not Uppercase")
                   && IsValid(() => Validator.IsCountOfLessThanOrEqualTo(value, '-', 1), "Contains Too Many Hyphens")
                   && IsValid(() => Validator.IsCountOfLessThanOrEqualTo(value, '\'', 1), "Contains Too Many Apostrophes")
                   && IsValid(() => !Validator.ContainsSpecialChars(value), "Contains Special Characters")
                   && IsValid(() => Validator.IsLengthGreaterThan(value, 1), "Length Not In Allowed Range")
                   && IsValid(() => Validator.IsLengthLessThanOrEqualTo(value, 50), "Length Not In Allowed Range");
        }
    }
}
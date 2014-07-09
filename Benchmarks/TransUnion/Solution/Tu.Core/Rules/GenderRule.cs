using Tu.Extensions;

namespace Tu.Rules
{
    public class GenderRule : Rule
    {
        public GenderRule(IValidator validator, string fieldName)
            : base(validator, fieldName)
        {
        }

        public override bool IsValid(object obj)
        {
            var value = obj.ToStringSafe();

            return IsValid(() => Validator.IsNullOrEmpty(value), "")
                   || IsValid(() => Validator.IsLengthEqual(value, 1), "Length Out Of Range");
        }
    }
}
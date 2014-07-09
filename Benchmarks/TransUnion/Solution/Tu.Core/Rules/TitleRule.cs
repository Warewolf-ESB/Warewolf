using Tu.Extensions;

namespace Tu.Rules
{
    public class TitleRule : Rule
    {
        public TitleRule(IValidator validator, string fieldName)
            : base(validator, fieldName)
        {
        }

        public override bool IsValid(object obj)
        {
            var value = obj.ToStringSafe();
            return IsValid(() => Validator.IsLengthLessThanOrEqualTo(value, 50), "Length Too Long");
        }
    }
}
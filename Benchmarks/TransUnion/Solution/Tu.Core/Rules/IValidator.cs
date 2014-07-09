namespace Tu.Rules
{
    public interface IValidator
    {
        bool IsNullOrEmpty(string value);

        bool IsLengthEqual(string value, int length);

        bool IsLengthLessThanOrEqualTo(string value, int length);

        bool IsLengthGreaterThan(string value, int length);

        bool IsNumeric(string value);

        bool IsUpper(string value);

        bool StartsWith(string value, string prefix);

        bool IsCountOfLessThanOrEqualTo(string value, char c, int maxCount);

        bool ContainsSpecialChars(string value);

        bool IsValidEmailAddress(string value);
    }
}
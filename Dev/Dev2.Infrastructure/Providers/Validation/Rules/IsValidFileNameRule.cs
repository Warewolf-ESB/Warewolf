using System;
using System.Text.RegularExpressions;

namespace Dev2.Providers.Validation.Rules
{
    public class IsValidFileNameRule : IsValidCollectionRule
    {
        // see http://stackoverflow.com/questions/12039679/regular-expression-on-filename
        static readonly Regex TheRegex = new Regex(@"^[a-z]+(?:[ -][a-z]+)*\s+\d+H\s+[a-z]+\s+\d{2}-\d{2}-\d{4}\.dwg$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public IsValidFileNameRule(Func<string> getValue, char splitToken = ';')
            : base(getValue, "file name", splitToken)
        {
        }

        protected override bool IsValid(string item)
        {
            var result = TheRegex.IsMatch(item);
            return result;
        }
    }
}
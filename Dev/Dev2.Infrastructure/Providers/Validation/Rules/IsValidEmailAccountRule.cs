using System;
using Dev2.Common.ExtMethods;

namespace Dev2.Providers.Validation.Rules
{
    public class IsValidEmailAccountRule : IsValidCollectionRule
    {
        public IsValidEmailAccountRule(Func<string> getValue, char splitToken = ';')
            : base(getValue, "email account", splitToken)
        {
        }

        protected override bool IsValid(string item)
        {
            return IsDomainAccount(item) || item.IsEmail();
        }

        static bool IsDomainAccount(string value)
        {
            var parts = value.Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
            return parts.Length == 2;
        }
    }
}

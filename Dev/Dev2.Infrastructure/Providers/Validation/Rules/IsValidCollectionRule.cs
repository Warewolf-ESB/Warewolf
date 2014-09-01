using System;
using System.Linq;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;

namespace Dev2.Providers.Validation.Rules
{
    public abstract class IsValidCollectionRule : Rule<string>
    {
        readonly char[] _separator;

        protected IsValidCollectionRule(Func<string> getValue, string itemName, char splitToken)
            : base(getValue)
        {
            VerifyArgument.IsNotNull("itemName", itemName);
            VerifyArgument.IsNotNull("splitToken", splitToken);
            ErrorText = "contains an invalid " + itemName;
            _separator = new[] { splitToken };
        }

        public override IActionableErrorInfo Check()
        {
            var value = GetValue();

            if(!string.IsNullOrEmpty(value))
            {
                var items = value.Split(_separator, StringSplitOptions.RemoveEmptyEntries);
                if(items.Count(item => !IsValid(item)) > 0)
                {
                    return CreatError();
                }
            }
            return null;
        }

        protected abstract bool IsValid(string item);
    }
}
using System;
using Dev2.Providers.Errors;

namespace Dev2.Providers.Validation.Rules
{
    public class IsSingleCharRule : Rule<string>
    {
        public IsSingleCharRule(Func<string> getValue)
            : base(getValue)
        {
            ErrorText = "must be a single character";
        }

        public override IActionableErrorInfo Check()
        {
            var value = GetValue();
            if(!string.IsNullOrEmpty(value))
            {
                if(value.Length > 1)
                {
                    return CreatError();
                }
            }
            return null;
        }
    }
}
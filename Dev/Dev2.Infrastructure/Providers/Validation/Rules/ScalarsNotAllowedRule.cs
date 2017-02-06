using System;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Warewolf.Resource.Errors;

namespace Dev2.Providers.Validation.Rules
{
    public class ScalarsNotAllowedRule : Rule<string>
    {

        public ScalarsNotAllowedRule(Func<string> getValue)
            : base(getValue)
        {
            ErrorText = ErrorResource.CannotHaveScalars;
        }

        public override IActionableErrorInfo Check()
        {
            var value = GetValue();

            string[] fields = value.Split(',');
            for(int i = 0; i < fields.Length; i++)
            {
                if(!fields[i].Contains("(") && !fields[i].Contains(")"))
                {
                    return CreatError();
                }
            }

            return null;
        }

    }
}
using System;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Data.Util;
using Dev2.Providers.Validation.Rules;
using Warewolf.Resource.Errors;

namespace Dev2.Validation
{
    public class ShouldNotBeVariableRule : Rule<string>
    {
        public ShouldNotBeVariableRule(Func<string> getValue)
            : base(getValue)
        {
            ErrorText = ErrorResource.CannotBeVariable;
        }

        public override IActionableErrorInfo Check()
        {
            var value = GetValue();
            return IsVariable(value) ? CreatError() : null;
        }

        static bool IsVariable(string value)
        {
            return DataListUtil.IsFullyEvaluated(value);
        }
    }
}
using System;
using Dev2.Providers.Errors;
using Dev2.Providers.Validation.Rules;

namespace Dev2.Infrastructure.Tests.Providers.Validation.Rules
{
    public class TestRuleBase : Rule<object>
    {
        public TestRuleBase(Func<object> getValue)
            : base(getValue)
        {
        }

        public IActionableErrorInfo CheckResult { get; set; }

        public override IActionableErrorInfo Check()
        {
            return CheckResult;
        }

        public IActionableErrorInfo TestCreatError()
        {
            return CreatError();
        }
    }
}
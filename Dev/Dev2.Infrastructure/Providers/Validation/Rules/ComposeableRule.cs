using System;
using Dev2.Providers.Errors;

namespace Dev2.Providers.Validation.Rules
{
    public class ComposeableRule<T> : RuleBase
    {
        readonly Rule<T> _baseRule;
        Func<IActionableErrorInfo> _check;
        public ComposeableRule(Rule<T> baseRule)
        {
            VerifyArgument.IsNotNull("baseRule", baseRule);
            _baseRule = baseRule;
            _check = _baseRule.Check;
           
        }

        public ComposeableRule<T> And(Rule<T> andRule)
        {
            
            VerifyArgument.IsNotNull("baseRule", andRule);
            var b = _check;
            _check = ()=>
                {
                    var a= b();
                    if (a != null)
                        return a;
                    return andRule.Check();
                };
            return this;
        }

        public ComposeableRule<T> Or(Rule<T> orRule)
        {

            VerifyArgument.IsNotNull("orRule", orRule);
            var b = _check;
            _check = () =>
                {
                    var a = b();
                    if (a == null)
                        return null;
                    return orRule.Check();
                };
            return this;
        }

        public override IActionableErrorInfo Check()
        {
            return  _check();
        }
    }
}
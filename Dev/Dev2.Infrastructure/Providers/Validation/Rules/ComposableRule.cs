/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;

namespace Dev2.Providers.Validation.Rules
{
    public class ComposableRule<T> : RuleBase
    {
        private readonly Rule<T> _baseRule;
        private Func<IActionableErrorInfo> _check;

        public ComposableRule(Rule<T> baseRule)
        {
            VerifyArgument.IsNotNull("baseRule", baseRule);
            _baseRule = baseRule;
            _check = _baseRule.Check;
        }

        public ComposableRule<T> And(Rule<T> andRule)
        {
            VerifyArgument.IsNotNull("andRule", andRule);
            Func<IActionableErrorInfo> b = _check;
            _check = () =>
            {
                IActionableErrorInfo a = b();

                if (a != null)
                    return a;
                return andRule.Check();
            };
            return this;
        }

        public ComposableRule<T> Or(Rule<T> orRule)
        {
            VerifyArgument.IsNotNull("orRule", orRule);
            Func<IActionableErrorInfo> b = _check;
            _check = () =>
            {
                IActionableErrorInfo a = b();
                if (a == null)
                {
                    return orRule.Check();
                }
                return a;
            };
            return this;
        }

        public override IActionableErrorInfo Check()
        {
            return _check();
        }
    }
}
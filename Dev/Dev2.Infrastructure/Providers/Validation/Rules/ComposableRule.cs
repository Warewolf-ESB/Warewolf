/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
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
        readonly Rule<T> _baseRule;
        Func<IActionableErrorInfo> _check;
        public ComposableRule(Rule<T> baseRule)
        {
            VerifyArgument.IsNotNull("baseRule", baseRule);
            _baseRule = baseRule;
            _check = _baseRule.Check;

        }

        public ComposableRule<T> Or(Rule<T> orRule)
        {

            VerifyArgument.IsNotNull("orRule", orRule);
            var b = _check;
            _check = () =>
                {
                    var a = b();
                    if (a == null)
                    {
                        orRule.DoError = DoError;
                        _baseRule.DoError = DoError;
                        var actionableErrorInfo = orRule.Check();
                        return actionableErrorInfo;
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


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

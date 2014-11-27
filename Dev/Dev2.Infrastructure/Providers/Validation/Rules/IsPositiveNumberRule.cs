
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
    public class IsPositiveNumberRule : Rule<string>
    {
        public IsPositiveNumberRule(Func<string> getValue)
            : base(getValue)
        {
            ErrorText = "must be a real number";
        }

        public override IActionableErrorInfo Check()
        {
            var isValid = false;
            int x;
            var value = GetValue();
            if(int.TryParse(value, out x))
            {
                if(x >= 0)
                {
                    isValid = true;
                }
            }
            return isValid ? null : CreatError();
        }
    }
}



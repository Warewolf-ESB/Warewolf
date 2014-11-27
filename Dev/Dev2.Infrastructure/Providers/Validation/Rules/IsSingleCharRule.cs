
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

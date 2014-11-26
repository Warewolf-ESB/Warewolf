
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
using System.Linq;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;

namespace Dev2.Providers.Validation.Rules
{
    public abstract class IsValidCollectionRule : Rule<string>
    {
        readonly char[] _separator;

        protected IsValidCollectionRule(Func<string> getValue, string itemName, char splitToken)
            : base(getValue)
        {
            VerifyArgument.IsNotNull("itemName", itemName);
            VerifyArgument.IsNotNull("splitToken", splitToken);
            ErrorText = "contains an invalid " + itemName;
            _separator = new[] { splitToken };
        }

        public override IActionableErrorInfo Check()
        {
            var value = GetValue();

            if(!string.IsNullOrEmpty(value))
            {
                var items = value.Split(_separator, StringSplitOptions.RemoveEmptyEntries);
                if(items.Count(item => !IsValid(item)) > 0)
                {
                    return CreatError();
                }
            }
            return null;
        }

        protected abstract bool IsValid(string item);
    }
}

#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

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

            var fields = value.Split(',');
            for (int i = 0; i < fields.Length; i++)
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

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
using Dev2.DataList.Contract;
using Dev2.Providers.Validation.Rules;

namespace Dev2.Validation
{
    public class IsSingleValueRule : Rule<string>
    {
        public IsSingleValueRule(Func<string> getValue)
            : base(getValue)
        {
            ErrorText = "result field only allows a single result";
        }



        #region Overrides of RuleBase

        public override IActionableErrorInfo Check()
        {
            string exp = GetValue();
            if (!String.IsNullOrEmpty(exp))
            {
                var regions = DataListCleaningUtils.SplitIntoRegions(exp);
                if (regions.Count > 1)
                    return CreatError();
                return null;
            }
            return null;
        }

        #endregion

        public static void ApplyIsSingleValueRule(string value, ErrorResultTO errors)
        {
            var rule = new IsSingleValueRule(() => value);
            var single = rule.Check();
            if (single != null)
                errors.AddError(single.Message);
        }
    }
}

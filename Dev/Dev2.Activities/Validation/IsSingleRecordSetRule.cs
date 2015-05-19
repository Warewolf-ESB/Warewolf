
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
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.Providers.Validation.Rules;

namespace Dev2.Validation
{
    public class IsSingleRecordSetRule : Rule<string>
      {
        public IsSingleRecordSetRule(Func<string> getValue)
            : base(getValue)
        {
            ErrorText = "sort field is invalid. You may only sort on a single RecordSet columns";
        }



        #region Overrides of RuleBase

        public override IActionableErrorInfo Check()
        {
            string exp = GetValue();
            if (!string.IsNullOrEmpty(exp))
            {
                var regions = DataListCleaningUtils.SplitIntoRegions(exp);
                if (regions.Count > 1)
                    return CreatError();
                if (regions.Count == 1 && !DataListUtil.IsValueRecordsetWithFields(regions[0]))
                    return CreatError(); 
                return null;
            }
            return null;
        }

        #endregion
      }
}

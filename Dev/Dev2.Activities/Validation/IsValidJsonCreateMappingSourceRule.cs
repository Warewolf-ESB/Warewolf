
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Providers.Errors;
using Dev2.Providers.Validation.Rules;
using Dev2.TO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Dev2.Validation
{
    public class IsValidJsonCreateMappingInputRule : Rule<JsonMappingTo>
    {
        public IsValidJsonCreateMappingInputRule(Func<JsonMappingTo> getValue)
            : base(getValue)
        {
        }

        /*
        protected override bool IsValid(JsonMappingTo item)
        {
            try
            {
                if (string.IsNullOrEmpty(JsonMappingCompoundTo.IsValidJsonMappingInput(
                    item.SourceName,
                    item.DestinationName)))
                    return true;

            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }
         * */

        public override IActionableErrorInfo Check()
        {
            var to = GetValue.Invoke();
            ErrorText = JsonMappingCompoundTo.IsValidJsonMappingInput(to.SourceName, to.DestinationName);
            if (string.IsNullOrEmpty(ErrorText))
                return null;
            else
                return new ActionableErrorInfo
                {
                    ErrorType = Common.Interfaces.Infrastructure.Providers.Errors.ErrorType.Critical,
                    Message = ErrorText,
                };
        }

    }
}

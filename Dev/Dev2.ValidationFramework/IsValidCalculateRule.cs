
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Dev2.Data.Util;
using Dev2.Providers.Errors;
using Dev2.Providers.Validation.Rules;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.InterfaceImplementors;
using System;
using System.Linq;

namespace Dev2.ValidationFramework
{
    public class IsValidCalculateRule : Rule<string>
    {
        readonly IIntellisenseProvider _intellisenseProvider;

        public IsValidCalculateRule(Func<string> getValue)
            : base(getValue)
        {
            _intellisenseProvider = new CalculateIntellisenseProvider();
        }

        public override IActionableErrorInfo Check()
        {
            var value = GetValue();
            bool isValid = true;

            string calculationExpression;
            if(DataListUtil.IsCalcEvaluation(value, out calculationExpression))
            {

                IntellisenseProviderContext context = new IntellisenseProviderContext
                {
                    CaretPosition = value.Length,
                    InputText = value,
                    IsInCalculateMode = true,
                    DesiredResultSet = IntellisenseDesiredResultSet.ClosestMatch
                };

                var results = _intellisenseProvider.GetIntellisenseResults(context);

                if(results.Any(e => e.IsError))
                {
                    isValid = false;
                    ErrorText = results.First(e => e.IsError).Description;
                }
            }

            if(!isValid)
            {
                return new ActionableErrorInfo(DoError)
                {
                    Message = string.Format("{0} {1}", LabelText, ErrorText)
                };
            }

            return null;
        }
    }
}

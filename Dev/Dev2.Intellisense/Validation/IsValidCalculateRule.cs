using Dev2.Data.Util;
using Dev2.Providers.Errors;
using Dev2.Providers.Validation.Rules;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.InterfaceImplementors;
using System;
using System.Linq;

namespace Dev2.Intellisense.Validation
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
                value = calculationExpression;
            }

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
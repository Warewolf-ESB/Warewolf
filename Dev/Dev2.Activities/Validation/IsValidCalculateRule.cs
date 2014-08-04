using System;
using System.Parsing.Intellisense;
using Dev2.Calculate;
using Dev2.Data.Util;
using Dev2.Providers.Errors;
using Dev2.Providers.Validation.Rules;

namespace Dev2.Validation
{
    public class IsValidCalculateRule : Rule<string>
    {
        private readonly ISyntaxTreeBuilderHelper _syntaxBuilder;

        public IsValidCalculateRule(Func<string> getValue)
            : base(getValue)
        {
            _syntaxBuilder = new SyntaxTreeBuilderHelper();
        }


        public override IActionableErrorInfo Check()
        {
            var value = GetValue();

            string calculationExpression;
            if (DataListUtil.IsCalcEvaluation(value, out calculationExpression))
            {
                value = calculationExpression;
            }
            Token[] tokens;
            _syntaxBuilder.Build(value, false, out tokens);

            if (_syntaxBuilder.EventLog != null && _syntaxBuilder.HasEventLogs)
            {

                return new ActionableErrorInfo(DoError) { Message = "Syntax Error An error occurred while parsing { " + value + " } It appears to be malformed" };
            }


            return null;
        }
    }
}
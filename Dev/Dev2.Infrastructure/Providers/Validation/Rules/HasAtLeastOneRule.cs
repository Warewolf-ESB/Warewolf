using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Providers.Errors;

namespace Dev2.Providers.Validation.Rules
{
    public class HasAtLeastOneRule : Rule<string>
    {
        readonly Func<string>[] _otherValues;

        public HasAtLeastOneRule(Func<string> getValue, params Func<string>[] otherValues)
            : base(getValue)
        {
            _otherValues = otherValues;
        }

        public override IActionableErrorInfo Check()
        {
            var values = new List<string> { GetValue() };
            if(_otherValues != null)
            {
                values.AddRange(_otherValues.Select(otherValue => otherValue()));
            }

            return values.Any(value => !string.IsNullOrEmpty(value)) ? null : new ActionableErrorInfo(DoError)
            {
                Message = string.Format("Please supply at least one of the following: {0}", LabelText)
            };
        }
    }
}
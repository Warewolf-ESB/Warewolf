using System.Collections.Generic;
using Dev2.Providers.Errors;
using Dev2.Providers.Validation;
using Dev2.Providers.Validation.Rules;

namespace Dev2.TO
{
    public abstract class ValidatedObject : ObservableObject, IPerformsValidation
    {
        Dictionary<string, List<IActionableErrorInfo>> _errors;

        public string Error { get { return string.Empty; } }

        public string this[string columnName] { get { return null; } }

        public Dictionary<string, List<IActionableErrorInfo>> Errors { get { return _errors ?? (_errors = new Dictionary<string, List<IActionableErrorInfo>>()); } set { OnPropertyChanged(ref _errors, value); } }

        public abstract void Validate();

        public bool Validate(string propertyName, RuleSet ruleSet)
        {
            if(ruleSet == null)
            {
                Errors[propertyName] = new List<IActionableErrorInfo>();
            }
            else
            {
                var errorsTos = ruleSet.ValidateRules();
                Errors[propertyName] = errorsTos;
            }

            // ReSharper disable ExplicitCallerInfoArgument
            OnPropertyChanged("Errors");
            // ReSharper restore ExplicitCallerInfoArgument

            List<IActionableErrorInfo> errorList;
            if(Errors.TryGetValue(propertyName, out errorList))
            {
                return errorList.Count == 0;
            }
            return false;
        }

        public virtual bool Validate(string propertyName)
        {
            var ruleSet = GetRuleSet(propertyName);
            return Validate(propertyName, ruleSet);
        }

        protected abstract RuleSet GetRuleSet(string propertyName);
    }
}
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Common.Interfaces.Infrastructure.Providers.Validation;
using System.Collections.Generic;

namespace Dev2.Common
{
    public abstract class ValidatableObject : ObservableObject, IPerformsValidation
    {
        Dictionary<string, List<IActionableErrorInfo>> _errors;

        public string Error => string.Empty;

        public string this[string columnName] => null;

        public Dictionary<string, List<IActionableErrorInfo>> Errors { get => _errors ?? (_errors = new Dictionary<string, List<IActionableErrorInfo>>()); set => OnPropertyChanged(ref _errors, value); }

        public bool Validate(string propertyName, IRuleSet ruleSet)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                return true;
            }

            if (ruleSet == null)
            {
                Errors[propertyName] = new List<IActionableErrorInfo>();
            }
            else
            {
                var errorsTos = ruleSet.ValidateRules();
                Errors[propertyName] = errorsTos;
            }


            OnPropertyChanged("Errors");


            if (Errors.TryGetValue(propertyName, out List<IActionableErrorInfo> errorList))
            {
                return errorList.Count == 0;
            }
            return false;
        }

        public virtual bool Validate(string propertyName, string datalist)
        {
            var ruleSet = GetRuleSet(propertyName, datalist);
            return Validate(propertyName, ruleSet);
        }

        public abstract IRuleSet GetRuleSet(string propertyName, string datalist);
    }
}

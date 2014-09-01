using System.Collections.Generic;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Common.Interfaces.Infrastructure.Providers.Validation;

namespace Dev2.TO
{
    public abstract class ValidatedObject : ObservableObject, IPerformsValidation
    {
        Dictionary<string, List<IActionableErrorInfo>> _errors;

        public string Error { get { return string.Empty; } }

        public string this[string columnName] { get { return null; } }

        public Dictionary<string, List<IActionableErrorInfo>> Errors { get { return _errors ?? (_errors = new Dictionary<string, List<IActionableErrorInfo>>()); } set { OnPropertyChanged(ref _errors, value); } }

        public bool Validate(string propertyName, IRuleSet ruleSet)
        {
            if(string.IsNullOrEmpty(propertyName))
            {
                return true;
            }

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

        public virtual bool Validate(string propertyName, string datalist)
        {
            var ruleSet = GetRuleSet(propertyName, datalist);
            return Validate(propertyName, ruleSet);
        }

        public abstract IRuleSet GetRuleSet(string propertyName, string datalist);
    }
}
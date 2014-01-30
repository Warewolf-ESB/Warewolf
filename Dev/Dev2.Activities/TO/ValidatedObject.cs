using System.Collections.Generic;
using Dev2.Providers.Errors;
using Dev2.Providers.Validation;
using Dev2.Providers.Validation.Rules;

namespace Dev2.TO
{
    public abstract class ValidatedObject : ObservableObject, IPerformsValidation
    {
        Dictionary<string, List<IActionableErrorInfo>> _errors = new Dictionary<string, List<IActionableErrorInfo>>();

        public string Error { get { return string.Empty; } }

        public string this[string columnName] { get { return null; } }

        public Dictionary<string, List<IActionableErrorInfo>> Errors { get { return _errors; } set { OnPropertyChanged(ref _errors, value); } }

        public abstract void Validate();

        public bool Validate(string propertyName, RuleSet ruleSet)
        {
            if(ruleSet == null)
            {
                Errors[propertyName] = new List<IActionableErrorInfo>();
            }

            OnPropertyChanged("Errors");

            List<IActionableErrorInfo> errorList;
            if(Errors.TryGetValue(propertyName, out errorList))
            {
                return errorList.Count == 0;
            }
            return false;
        }

        public bool Validate(string propertyName)
        {
            return false;
        }
    }
}
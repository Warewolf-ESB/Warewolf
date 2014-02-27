using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using Dev2.DataList.Contract;
using Dev2.Interfaces;
using Dev2.Providers.Errors;
using Dev2.Providers.Validation;
using Dev2.Providers.Validation.Rules;
using Dev2.Util;

namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
    /// <summary>
    /// Used for activties
    /// </summary>
    public class ActivityDTO : INotifyPropertyChanged, IDev2TOFn, IPerformsValidation
    {
        string _fieldName;
        string _fieldValue;
        int _indexNumber;
        List<string> _outList;
        Dictionary<string, List<IActionableErrorInfo>> _errors = new Dictionary<string, List<IActionableErrorInfo>>();
        string _errorMessage;

        bool _isFieldNameFocused;

        public ActivityDTO()
            : this("[[Variable]]", "Expression", 0)
        {
        }

        public ActivityDTO(string fieldName, string fieldValue, int indexNumber, bool inserted = false)
        {
            Inserted = inserted;
            FieldName = fieldName;
            FieldValue = fieldValue;
            IndexNumber = indexNumber;
            _outList = new List<string>();
        }

        public string WatermarkTextVariable { get; set; }

        public string WatermarkTextValue { get; set; }

        void RaiseCanAddRemoveChanged()
        {
            OnPropertyChanged("CanRemove");
            OnPropertyChanged("CanAdd");
        }

        [FindMissing]
        public string FieldName
        {
            get
            {
                return _fieldName;
            }
            set
            {
                if(_fieldName != value)
                {
                _fieldName = value;
                OnPropertyChanged("FieldName");
                RaiseCanAddRemoveChanged();
                }
                //DO NOT call validation here as it will cause issues during serialization
            }
        }

        void ValidateFieldName()
        {
            Validate("FieldName", GetFieldNameRuleSet());
        }

        RuleSet GetFieldNameRuleSet()
        {
            var ruleSet = new RuleSet();
            return ruleSet;
        }

        [FindMissing]
        public string FieldValue
        {
            get
            {
                return _fieldValue;
            }
            set
            {
                if(_fieldValue != value)
                {
                    _fieldValue = value;
                    OnPropertyChanged("FieldValue");
                    RaiseCanAddRemoveChanged();
                }
                //DO NOT call validation here as it will cause issues during serialization
            }
        }

        public int IndexNumber
        {
            get
            {
                return _indexNumber;
            }
            set
            {
                _indexNumber = value;
                OnPropertyChanged("IndexNumber");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            if(PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }


        public bool CanRemove()
        {
            bool result = string.IsNullOrEmpty(FieldName) && string.IsNullOrEmpty(FieldValue);
            return result;
        }


        public bool CanAdd()
        {
            bool result = !(string.IsNullOrEmpty(FieldName) && string.IsNullOrEmpty(FieldValue));
            return result;
        }

        public void ClearRow()
        {
            FieldName = string.Empty;
            FieldValue = string.Empty;
        }

        public bool Inserted { get; set; }

        public List<string> OutList
        {
            get
            {
                return _outList;
            }
            set
            {
                _outList = value;
            }
        }


        public Dictionary<string, List<IActionableErrorInfo>> Errors
        {
            get
            {
                return _errors;
            }
            set
            {
                _errors = value;
                OnPropertyChanged("Errors");
            }
        }

        public OutputTO ConvertToOutputTO()
        {
            return DataListFactory.CreateOutputTO(FieldName, OutList);
        }

        public bool Validate(string propertyName, IRuleSet ruleSet)
        {
            if(ruleSet == null)
            {
                Errors[propertyName] = new List<IActionableErrorInfo>();
            }
            else
            {
                var errorsTos = ruleSet.ValidateRules();
                var actionableErrorInfos = errorsTos.ConvertAll<IActionableErrorInfo>(input => new ActionableErrorInfo(input, () =>
                {
                    IsFieldNameFocused = true;
                }));
                Errors[propertyName] = actionableErrorInfos;
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
            RuleSet ruleSet = null;
            switch(propertyName)
            {
                case "FieldName":
                    ruleSet = GetFieldNameRuleSet();
                    break;
                case "FieldValue":
                    break;
            }
            return Validate(propertyName, ruleSet);
        }

        /// <summary>
        /// Validates the property name with the default rule set in <value>ActivityDTO</value>
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public bool Validate(Expression<Func<string>> property)
        {
            var propertyName = GetPropertyName(property);
            return Validate(propertyName);
        }

        /// <summary>
        /// Validates the property name with the default rule set in <value>ActivityDTO</value>
        /// </summary>
        /// <param name="property">Property to validate</param>
        /// <param name="ruleSet">Ruleset to use during validation</param>
        /// <returns></returns>
        public bool Validate(Expression<Func<string>> property, RuleSet ruleSet)
        {
            var propertyName = GetPropertyName(property);
            return Validate(propertyName, ruleSet);
        }

        #region Implementation of IDataErrorInfo

        /// <summary>
        /// Gets the error message for the property with the given name.
        /// </summary>
        /// <returns>
        /// The error message for the property. The default is an empty string ("").
        /// </returns>
        /// <param name="columnName">The name of the property whose error message to get. </param>
        public string this[string columnName]
        {
            get
            {
                //This is commented out as it is an example of how to do on the fly validation. This is probably be used when we implement the validation rules for this class.
                //                string item;
                //                if(PerformPropertyValidation(columnName, out item)) return item;
                Errors = new Dictionary<string, List<IActionableErrorInfo>>();
                return null;
            }
        }

        //This is not use as it is an example of how to do on the fly validation. This is probably be used when we implement the validation rules for this class and will be used in the method above.
        bool PerformPropertyValidation(string columnName, out string item)
        {
            item = string.Empty;
            if(columnName == "FieldName")
            {
                ValidateFieldName();
                {
                    item = GetErrorMessage(columnName);
                    return true;
                }
            }
            if(columnName == "FieldValue")
            {
                Validate("FieldValue", null);
                {
                    item = GetErrorMessage(columnName);
                    return true;
                }
            }
            return false;
        }

        string GetErrorMessage(string columnName)
        {
            var errorMessages = from error in Errors
                                where error.Key == columnName
                                select error.Value;
            var errorMessage = "";
            errorMessages.ToList().ForEach(list => { errorMessage = String.Join(Environment.NewLine, list.Select(errorInfo => errorInfo.Message)); });
            ErrorMessage = errorMessage;
            return errorMessage;
        }

        /// <summary>
        /// Gets an error message indicating what is wrong with this object.
        /// </summary>
        /// <returns>
        /// An error message indicating what is wrong with this object. The default is an empty string ("").
        /// </returns>
        public string Error { get; private set; }

        public string ErrorMessage
        {
            get
            {
                return _errorMessage;
            }
            set
            {
                _errorMessage = value;
                OnPropertyChanged("ErrorMessage");
            }
        }

        public bool HasError
        {
            get
            {
                var errorCount = Errors.SelectMany(pair => pair.Value).Count();
                return errorCount != 0;
            }
        }

        public bool IsFieldNameFocused
        {
            get
            {
                return _isFieldNameFocused;
            }
            set
            {
                _isFieldNameFocused = value;
                OnPropertyChanged("IsFieldNameFocused");
            }
        }
        #endregion

        static string GetPropertyName<TU>(Expression<Func<TU>> propertyName)
        {
            if(propertyName.NodeType != ExpressionType.Lambda)
            {
                throw new ArgumentException(@"Value must be a lamda expression", "propertyName");
            }

            var body = propertyName.Body as MemberExpression;

            if(body == null)
            {
                throw new ArgumentException("Must have body");
            }
            if(body.Member == null)
            {
                throw new ArgumentException("Body must have Member");
            }
            return body.Member.Name;
        }
    }
}
